# Use the appropriate base image for .NET 6.0
FROM mcr.microsoft.com/dotnet/aspnet:6.0 AS base
WORKDIR /app
EXPOSE 7115
EXPOSE 5225
#EXPOSE 80
#EXPOSE 443

# Use the appropriate base image for .NET 6.0 SDK for building
FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src
COPY ["StaticFileSecureCall.csproj", "."]
RUN dotnet restore "./StaticFileSecureCall.csproj"
COPY . .
WORKDIR "/src/."
RUN dotnet build "StaticFileSecureCall.csproj" -c Release -o /app/build

# Continue with the rest of the Dockerfile as before
FROM build AS publish
RUN dotnet publish "StaticFileSecureCall.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENV ASPNETCORE_ENVIRONMENT Production
ENTRYPOINT ["dotnet", "StaticFileSecureCall.dll"]
