FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build-env
WORKDIR /app

COPY DotSmog/*.csproj ./DotSmog/
WORKDIR /app/DotSmog
RUN dotnet restore

COPY DotSmog/. ./
RUN dotnet publish -c Release -o out

FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app
COPY --from=build-env /app/DotSmog/out .

ENTRYPOINT ["dotnet", "DotSmog.dll"]
