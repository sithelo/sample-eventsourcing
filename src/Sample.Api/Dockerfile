FROM mcr.microsoft.com/dotnet/aspnet:6.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src
COPY ["src/Sample.Api/Sample.Api.csproj", "Sample.Api/"]
RUN dotnet restore "src/Sample.Api/Sample.Api.csproj"
COPY . .
WORKDIR "/src/Sample.Api"
RUN dotnet build "Sample.Api.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Sample.Api.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Sample.Api.dll"]
