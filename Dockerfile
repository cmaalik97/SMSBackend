# Stage 1: Build the C# project
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /app

COPY *.csproj ./
RUN dotnet restore

COPY . ./
RUN dotnet publish -c Release -o /app/publish

# Stage 2: Run the C# project
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app

COPY --from=build /app/publish .

# Render.com sets PORT automatically - ASP.NET reads it here
ENV ASPNETCORE_URLS=http://+:${PORT:-8080}

# Change "Student_Management_System" to YOUR .csproj filename (without .csproj)
ENTRYPOINT ["dotnet", "Student_Management_System.dll"]
