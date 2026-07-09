# -----------------------------------------------
# Stage 1: Base (runtime) image
# aspnet:10.0 already defines a non-root user (APP_UID=1654) and Kestrel
# listens on 8080 by default (ASPNETCORE_HTTP_PORTS=8080). Do NOT invent 5000.
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
WORKDIR /app
EXPOSE 8080

# -----------------------------------------------
# Stage 2: Build image
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
ARG BUILD_CONFIGURATION=Release

# --- Restore layer (cache-friendly) ---------------------------------
# Copy ONLY the .csproj files first. This layer is re-run by Docker only
# when a .csproj changes, so `dotnet restore` (the slow, network-bound step)
# is cached across ordinary source edits.
# CRITICAL: every project in the restore graph must be here. API -> Application,
# Contracts, Infrastructure; Application -> Domain, Contracts; Infrastructure ->
# Application, Domain. Domain.csproj was MISSING before, so `dotnet restore`
# could not resolve the graph -> broken cache layer / restore failure.
COPY ["API/API.csproj", "API/"]
COPY ["Application/Application.csproj", "Application/"]
COPY ["Contracts/Contracts.csproj", "Contracts/"]
COPY ["Infrastructure/Infrastructure.csproj", "Infrastructure/"]
COPY ["Domain/Domain.csproj", "Domain/"]

RUN dotnet restore "API/API.csproj"

# --- Build + publish -------------------------------------------------
# Copy the rest of the source (a .dockerignore keeps bin/obj/.git out).
COPY . .

# `dotnet publish` builds under the hood, so a separate `dotnet build` step
# is redundant work + an extra layer. Publish straight to the output folder.
FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "API/API.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

# -----------------------------------------------
# Stage 3: Final image (small runtime, no SDK)
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
# Drop root: run the app as the unprivileged user shipped in the base image.
# A container escape then lands on a nobody-user, not root.
USER $APP_UID
ENTRYPOINT ["dotnet", "API.dll"]


# --- reference commands (run from the Backend/ solution folder) ------
# docker build -f API/Dockerfile -t backend:1.0 .
# docker tag backend:1.0 abdulaziz026/backend:v1.0
# docker push abdulaziz026/backend:v1.0
#
# docker run -d -p 5000:8080 --name api-backend backend:1.0
#   -> host localhost:5000 maps to the container's 8080
# GET http://localhost:5000/api/Home/dashboard
