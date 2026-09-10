FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY ["Summarix.Presentation/Summarix.Presentation.csproj", "Summarix.Presentation/"]
COPY ["Summarix.Application/Summarix.Application.csproj", "Summarix.Application/"]
COPY ["Summarix.Infrastructure/Summarix.Infrastructure.csproj", "Summarix.Infrastructure/"]
COPY ["Summarix.Domain/Summarix.Domain.csproj", "Summarix.Domain/"]

RUN dotnet restore "Summarix.Presentation/Summarix.Presentation.csproj"

COPY . .
WORKDIR "/src/Summarix.Presentation"
RUN dotnet publish "Summarix.Presentation.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app
COPY --from=build /app/publish .

ENV ASPNETCORE_HTTP_PORTS=8080

EXPOSE 8080
ENTRYPOINT ["dotnet", "Summarix.Presentation.dll"]