FROM mcr.microsoft.com/dotnet/aspnet:6.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src
COPY ["PDF.Generator.WebAPI/PDF.Generator.WebAPI.csproj", "PDF.Generator.WebAPI/"]
RUN dotnet restore "PDF.Generator.WebAPI/PDF.Generator.WebAPI.csproj"
COPY . .
WORKDIR "/src/PDF.Generator.WebAPI"
RUN dotnet build "PDF.Generator.WebAPI.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "PDF.Generator.WebAPI.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "PDF.Generator.WebAPI.dll"]
