# k8s-workshop
Demo application used in the kubernetes workshop

## Running aspnet core 2.1 application

* Clone the repository
* Run the **k8s-todo-api** app first by running the command from k8s-todo-api folder `dotnet run`
* Browse the swagger ui for the api using http://localhost:5000/swagger/index.html 
* Run the **k8s-todo-web** app by running the command from k8s-todo-web folder `dotnet run`
* Browse the application using http://localhost:3000 

## Dockerizing the aspnet core 2.1 application

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

## Orchestrating WEB & API app execution in containers

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
    
## Pushing docker images to docker hub

* Connect your local docker host with the docker hub by logging in to your account `docker login`

* Push the api app image
  
  `docker push <your-repo-name>/k8s-todo-api:latest`

* Push the web app image

  `docker push <your-repo-name>/k8s-todo-web:latest`
