######################  build stage  ###########################
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

COPY ["api.csproj", "."]
RUN dotnet restore

COPY . .
RUN dotnet publish -c Release -o /app/publish

######################  runtime stage  #########################
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS runtime
WORKDIR /app

COPY --from=build /app/publish .
COPY entrypoint.sh /entrypoint.sh
RUN chmod +x /entrypoint.sh

ENV ASPNETCORE_URLS=http://+:8080 \
    DB_HOST=db \
    DB_PORT=5432

EXPOSE 8080
ENTRYPOINT ["/entrypoint.sh"]
