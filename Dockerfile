# Use official .NET SDK to build the app
FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build
WORKDIR /app

# Copy everything (to handle multi-project solutions)
COPY . ./

# Explicitly restore using the correct project or solution file
RUN dotnet restore goluxai.csproj

# Build the project
RUN dotnet publish goluxai.csproj -c Release -o /out

# Use lightweight .NET runtime for deployment
FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app
COPY --from=build /out .

# Expose port 8080 for Railway
EXPOSE 8080
CMD ["dotnet", "goluxai.dll"]
