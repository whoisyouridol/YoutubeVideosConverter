# Base image with runtime for running the application
FROM mcr.microsoft.com/dotnet/runtime:8.0 AS base
WORKDIR /app

# Install ffmpeg in the base image
RUN apt-get update && apt-get install -y ffmpeg && apt-get clean

# Build stage for restoring and building dependencies
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src

# Copy project files
COPY ["src/YoutubeVideosConverter.Presentation/YoutubeVideosConverter.Presentation.csproj", "YoutubeVideosConverter.Presentation/"]
COPY ["src/YoutubeVideoConverter.Infrastructure.SQL/YoutubeVideoConverter.Infrastructure.SQL.csproj", "YoutubeVideoConverter.Infrastructure.SQL/"]
COPY ["src/YoutubeVideosConverter.Application/YoutubeVideosConverter.Application.csproj", "YoutubeVideosConverter.Application/"]

# Restore dependencies
RUN dotnet restore "YoutubeVideosConverter.Presentation/YoutubeVideosConverter.Presentation.csproj"

# Copy the rest of the source code
COPY . .

# Set working directory for build
WORKDIR "/src/YoutubeVideosConverter.Presentation"

# Build the application
RUN dotnet build "YoutubeVideosConverter.Presentation.csproj" -c $BUILD_CONFIGURATION -o /app/build

# Publish stage to prepare final output
FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "YoutubeVideosConverter.Presentation.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=true

# Final stage for production
FROM base AS final
WORKDIR /app

# Copy published output from the publish stage
COPY --from=publish /app/publish .

# Entry point for the application
ENTRYPOINT ["dotnet", "YoutubeVideosConverter.Presentation.dll"]
