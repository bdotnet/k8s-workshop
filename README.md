# k8s-workshop

Basic Todo application with ASP.NET Core 2.1 Razor Pages as Front end & ASP.NET Core 2.1 WEB API as backend

Built using Visual Studio for Mac and Visual Studio Code. Container images are created using Docker for Mac. Application was tested against minikube running within a Mac machine. All the below mentioned steps should work everywhere irrespective of Windows/Linux/Mac.

## Prerequisites 

* Visual Studio 2017 or Visual Studio for Mac or Visual Studio Code
* .NET Core SDK 2.1
* Docker for Windows/Docker for Mac
* minikube
* kubectl

## Step 1 : Running aspnet core 2.1 application

* Clone the repository
* Run the **k8s-todo-api** app first by running the command `dotnet run` from **k8s-todo-api** folder 
* Browse the swagger ui for the api using http://localhost:5000/swagger/index.html 
* Run the **k8s-todo-web** app by running the command `dotnet run` from **k8s-todo-web** folder 
* Browse the application using http://localhost:3000 

## Step 2 : Dockerizing the aspnet core 2.1 application

### Dockerizing the API application

* Navigate to the `k8s-todo-api` folder and create a `Dockerfile` and paste the below contents
    <pre><code>
    FROM microsoft/dotnet:2.1-aspnetcore-runtime AS base
    WORKDIR /app
    ENV ASPNETCORE_URLS http://*:5000
    EXPOSE 5000
     
    FROM microsoft/dotnet:2.1-sdk AS build
    WORKDIR /src
    COPY k8s-todo-api.csproj k8s-todo-api/
    RUN dotnet restore k8s-todo-api/k8s-todo-api.csproj
    WORKDIR /src/k8s-todo-api
    COPY . .
    RUN dotnet build k8s-todo-api.csproj -c Release -o /app

    FROM build AS publish
    RUN dotnet publish k8s-todo-api.csproj -c Release -o /app

    FROM base AS final
    WORKDIR /app
    COPY --from=publish /app .
    ENTRYPOINT ["dotnet", "k8s-todo-api.dll"]
    </code></pre>
* Create the docker image

    Run the below command to build the docker image
    <pre><code> docker build -t k8s-todo-api:latest . </code></pre>

* Run the API app in a container
  
    Run the below command to run the image in a container
    <pre><code> docker run -p 5000:5000 k8s-todo-api:latest </code></pre>
    
### Dockerizing the Web Application

* Navigate to the `k8s-todo-web` folder and create a new Dockerfile

<pre><code>
    
    FROM microsoft/dotnet:2.1-aspnetcore-runtime AS base
    WORKDIR /app
    ENV ASPNETCORE_URLS http://*:3000
    EXPOSE 3000

    FROM microsoft/dotnet:2.1-sdk AS build
    WORKDIR /src
    COPY k8s-todo-web.csproj k8s-todo-web/
    RUN dotnet restore k8s-todo-web/k8s-todo-web.csproj
    WORKDIR /src/k8s-todo-web
    COPY . .
    RUN dotnet build k8s-todo-web.csproj -c Release -o /app

    FROM build AS publish
    RUN dotnet publish k8s-todo-web.csproj -c Release -o /app

    FROM base AS final
    WORKDIR /app
    COPY --from=publish /app .
    ENTRYPOINT ["dotnet", "k8s-todo-web.dll"]
    
   </code></pre>
  
  * Make the below code changes before building the docker image
  
  1. Remove the Endpoint value from the appSettings.json file. appSettings.json should look like below:
  
  <pre><code>
  {
  "Logging": {
    "LogLevel": {
      "Default": "Warning"
    }
  },
  "TodoApiService": {
    "EndpointUri": ""
  },
  "AllowedHosts": "*"
}
</code></pre>

2. Remove the `UseUrls` from the WebHostBuilder configuration in `Program.cs` file. The CreateWebHostBuilder method should 
look like below

<pre><code>
public static IWebHostBuilder CreateWebHostBuilder(string[] args) =>
            WebHost.CreateDefaultBuilder(args)
                .UseStartup<Startup>();
    }
</code></pre>

* Build the image for k8s-todo-web application
<pre><code>
docker build -t k8s-todo-web:latest . 
</code></pre>

**At this point if you run the web application it might not work properly, as it needs to invoke the API application correctly to load the todo information**

### Orchestrating WEB & API app execution in containers

* Navigate to the root folder of the repo and create a `Docker-Compose.yml` file

<pre><code>
version: '3'

services:
  todoapi:
    image: todoapi
    ports: 
      - "5000:5000"
    build:
      context: ./k8s-todo-api
      dockerfile: Dockerfile

  todoweb:
    image: todoweb
    ports: 
      - "3000:3000"
    build:
      context: ./k8s-todo-web
      dockerfile: Dockerfile
</code></pre>

* Create another file called `docker-compose-override.yml` with the below contents. This file is required to inject the appSettings value dynamically
<pre><code>

version: '3'

services:
  todoapi:
    environment:
      - ASPNETCORE_ENVIRONMENT=Development
    ports:
      - "5000"

  todoweb:
    environment:
      - ASPNETCORE_ENVIRONMENT=Development
      - TodoApiService__EndpointUri=todoapi:5000
    ports:
      - "3000"
</code></pre>

* Running both containers

Run the below command to bring up both the containers in order (api container will be up before creating the web container)
<pre><code> docker-compose up </code></pre>
    
### Pushing docker images to docker hub

* Connect your local docker host with the docker hub by logging in to your account `docker login`

* Push the api app image
  
  `docker push <your-repo-name>/k8s-todo-api:latest`

* Push the web app image

  `docker push <your-repo-name>/k8s-todo-web:latest`
  
## Step : 3 Deploying to Kubernetes

* Ensure you have the below tools installed in your machine to work with kubernetes(k8s) locally

1. [minikube](https://kubernetes.io/docs/setup/minikube/)
2. [kubectl](https://kubernetes.io/docs/tasks/tools/install-kubectl/)

**TO understand the various basic/important commandlets in kubectl refer : [k8s-getting-started.md](https://gist.github.com/svswaminathan/571fda1c5fbbfcd1d05d50793031f0ac)**

* Navigate to the root folder and Create a new configmap file called `k8s-todo-configmap.yml` to store the runtime configuration values to be supplied to aspnetcore's `appSettings.json`

<pre><code>

apiVersion: v1
kind: ConfigMap
metadata:
  name: k8s-todo-configmap
  namespace: default
data:
  aspNetCoreEnvironment: Development
  todoApiServiceEndpointUri: todoapi:5000
  
</code></pre>

* Apply the config map to the minikube

`kubectl create --filename k8s-todo-configmap --record`

* Navigate to the root folder of the repo and Create a new file `k8s-todo-deployment.yml`

<pre><code>
apiVersion: apps/v1
kind: Deployment
metadata:
  name: todoapi
  labels:
    app: todoapi
spec:
  replicas: 3
  selector:
    matchLabels:
      app: todoapi
  strategy:
    rollingUpdate:
      maxSurge: 1
      maxUnavailable: 1
  minReadySeconds: 5
  template:
    metadata:
      labels:
        app: todoapi
    spec:
      containers:
      - name: todoapi
        image: <your-repo-name>/k8s-todo-api:latest
        ports:
        - containerPort: 5000
        env:
        - name: ASPNETCORE_ENVIRONMENT
          valueFrom:
            configMapKeyRef:
              name: k8s-todo-configmap
              key: aspNetCoreEnvironment
</code></pre>

* Deploy the API app to minikube

    Run the below command to create a deployment using the yml file created above

    `kubectl create --filename k8s-todo-deployment.yml --record`

* Expose the API app as k8s service

    Run the below command to expose the deployment as service

    `kubectl expose deployment/todoapi --type=NodePort --port 5000`

* List all the services and find out the port to connect to the API

    `kubectl get services`

* Browse the API by combining the output of `minikube ip` command and the NodePort assigned to the service. for e.g., in my local the services were exposed at http://192.168.99.100:32335/swagger/index.html 




