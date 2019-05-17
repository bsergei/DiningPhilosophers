FROM microsoft/dotnet:2.1-aspnetcore-runtime AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM microsoft/dotnet:2.1-sdk AS build
WORKDIR /src
COPY ["DiningPhilosophersWeb/DiningPhilosophersWeb.csproj", "DiningPhilosophersWeb/"]
RUN dotnet restore "DiningPhilosophersWeb/DiningPhilosophersWeb.csproj"
COPY . .
WORKDIR "/src/DiningPhilosophersWeb"
RUN dotnet build "DiningPhilosophersWeb.csproj" -c Release -o /app

FROM build AS publish
RUN dotnet publish "DiningPhilosophersWeb.csproj" -c Release -o /app

FROM base AS final
WORKDIR /app
COPY --from=publish /app .
ENTRYPOINT ["dotnet", "DiningPhilosophersWeb.dll"]