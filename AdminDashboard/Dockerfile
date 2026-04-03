FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
WORKDIR /app
EXPOSE 8080

FROM node:18 AS node-build
WORKDIR /app
COPY AdminDashboard/ClientApp/package*.json ./
RUN npm install
COPY AdminDashboard/ClientApp/ ./
RUN npm run build

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY ["AdminDashboard/AdminDashboard.csproj", "AdminDashboard/"]
RUN dotnet restore "AdminDashboard/AdminDashboard.csproj"
COPY . .
COPY --from=node-build /app/build ./AdminDashboard/ClientApp/build
WORKDIR "/src/AdminDashboard"
RUN dotnet publish "./AdminDashboard.csproj" -c Release -o /app/publish /p:UseAppHost=false /p:SkipNpmBuild=true

FROM base AS final
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "AdminDashboard.dll"]
