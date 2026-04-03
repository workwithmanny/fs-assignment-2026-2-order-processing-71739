FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY ["CustomerPortal/CustomerPortal.csproj", "CustomerPortal/"]
RUN dotnet restore "CustomerPortal/CustomerPortal.csproj"
COPY . .
WORKDIR "/src/CustomerPortal"
RUN dotnet build "./CustomerPortal.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "./CustomerPortal.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "CustomerPortal.dll"]
