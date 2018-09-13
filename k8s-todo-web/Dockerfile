
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
