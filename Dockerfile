FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY . .
RUN dotnet restore
RUN dotnet publish -c Release -o /app

FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app
COPY --from=build /app .

RUN mkdir -p /app/data
RUN chmod 777 /app/data

EXPOSE 80
ENTRYPOINT ["dotnet", "TaskManagerApi.dll"] 