# Use the official .NET runtime as a base image
FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app

# Copy the published output from Visual Studio
COPY out/ .

# Start the API
CMD ["dotnet", "MondayIntegration.dll"]
