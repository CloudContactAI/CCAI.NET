FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build

WORKDIR /sdk
COPY src/CCAI.NET/CCAI.NET.csproj ./src/CCAI.NET/
COPY integration/CCAITest.csproj ./integration/
RUN echo "" > RELEASE-NOTES-v1.4.5.md
RUN dotnet restore ./integration/CCAITest.csproj

COPY src/ ./src/
COPY integration/ ./integration/
RUN dotnet build ./integration/CCAITest.csproj -c Release -o /app/out -p:GeneratePackageOnBuild=false

FROM mcr.microsoft.com/dotnet/runtime:8.0
WORKDIR /app
COPY --from=build /app/out ./
CMD ["dotnet", "CCAITest.dll"]
