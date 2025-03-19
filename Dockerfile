FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build
WORKDIR /app

COPY goluxai.csproj ./
RUN dotnet restore goluxai.csproj

COPY . ./
RUN dotnet publish goluxai.csproj -c Release -o out

FROM mcr.microsoft.com/dotnet/aspnet:7.0 AS runtime
WORKDIR /app

COPY --from=build /app/out .
ENV TZ=Europe/Berlin
ENV ASPNETCORE_ENVIRONMENT=Production
ENV ASPNETCORE_URLS=http://+:9876

ENTRYPOINT ["dotnet", "goluxai.dll"]
