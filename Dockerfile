# Use official .NET SDK to build the app
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /app

# Copy project file and restore dependencies
COPY goluxai.csproj ./
RUN dotnet restore

# Copy all source files and build the application
COPY . ./
RUN dotnet publish -c Release -o /out

# Use lightweight .NET runtime for deployment
FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app
COPY --from=build /out .

# Expose port 8080 for Railway
EXPOSE 8080
CMD ["dotnet", "goluxai.dll"]
