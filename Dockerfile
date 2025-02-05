# Use the official .NET 7.0 SDK image to build the application
FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build
WORKDIR /app

# Copy the project file and restore dependencies
COPY goluxai.csproj ./
RUN dotnet restore goluxai.csproj

# Copy the remaining source code and build the application
COPY . ./
RUN dotnet publish goluxai.csproj -c Release -o out

# Use the official ASP.NET Core runtime image to run the application
FROM mcr.microsoft.com/dotnet/aspnet:7.0 AS runtime
WORKDIR /app
COPY --from=build /app/out .
ENV TZ=Europe/Berlin

# Set the entry point for the container
ENTRYPOINT ["dotnet", "goluxai.dll"]
