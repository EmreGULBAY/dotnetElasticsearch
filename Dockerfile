# Use the official ASP.NET Core runtime as a parent image
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 80

# Use the official ASP.NET Core SDK image to build the application
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["SampleApi.csproj", "./"]
RUN dotnet restore "./SampleApi.csproj"
COPY . .
WORKDIR "/src/"
RUN dotnet build "SampleApi.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "SampleApi.csproj" -c Release -o /app/publish

# Copy the build output to the runtime image
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "SampleApi.dll"]
