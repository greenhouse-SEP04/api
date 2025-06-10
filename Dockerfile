############  build stage  ################################
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# copy csproj, restore first for better build caching
COPY ["api.csproj", "."]
RUN dotnet restore

# copy the remaining source and publish
COPY . .
RUN dotnet publish -c Release -o /app/publish

############  runtime stage  ##############################
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS runtime
WORKDIR /app

COPY --from=build /app/publish .

# listen on 8080 inside the container
ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080

ENTRYPOINT ["dotnet", "api.dll"]
