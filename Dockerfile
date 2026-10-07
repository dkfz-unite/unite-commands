FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
ENV ASPNETCORE_HTTP_PORTS=80
ENV DOTNET_SYSTEM_GLOBALIZATION_INVARIANT=1
ENV ASPNETCORE_hostBuilder:reloadConfigOnChange=false
WORKDIR /app
EXPOSE 80

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS restore
WORKDIR /src
COPY ["Unite.Commands.Web/Unite.Commands.Web.csproj", "Unite.Commands.Web/"]
RUN dotnet restore "Unite.Commands.Web/Unite.Commands.Web.csproj"

FROM restore AS build
COPY . .
WORKDIR "/src/Unite.Commands.Web"
RUN dotnet build --no-restore "Unite.Commands.Web.csproj" -c Release

FROM build AS publish
RUN dotnet publish --no-build "Unite.Commands.Web.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Unite.Commands.Web.dll"]
