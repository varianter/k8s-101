

# start build stage
FROM --platform=$BUILDPLATFORM mcr.microsoft.com/dotnet/sdk:8.0-alpine AS build
ARG TARGETARCH
WORKDIR /src
# copy csproj and restore as distinct layers
COPY simple-api.csproj .
RUN dotnet restore -a $TARGETARCH
# copy and publish app and libraries
COPY . .
RUN dotnet publish -a $TARGETARCH -c Release --no-restore -o /app
# final stage and image
FROM mcr.microsoft.com/dotnet/aspnet:8.0-alpine
WORKDIR /app
COPY --from=build /app .
USER $APP_UID
ENV ASPNETCORE_URLS=http://+:8080
ENTRYPOINT ["dotnet", "simple-api.dll"]
EXPOSE 8080