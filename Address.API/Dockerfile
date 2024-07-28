FROM mcr.microsoft.com/dotnet/core/aspnet:3.1-buster-slim AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443
EXPOSE 63000

FROM mcr.microsoft.com/dotnet/core/sdk:3.1-buster AS build
WORKDIR /Address.API
COPY . .
RUN dotnet restore "/Address.API/src/Services/Address/Address.API/Address.API.csproj"
RUN dotnet build "/Address.API/src/Services/Address/Address.API/Address.API.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "/Address.API/src/Services/Address/Address.API/Address.API.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Address.API.dll"]