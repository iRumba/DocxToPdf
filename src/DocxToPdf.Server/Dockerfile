FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Copy project files
COPY src/DocxToPdf.Server/DocxToPdf.Server.csproj DocxToPdf.Server/
RUN dotnet restore DocxToPdf.Server/DocxToPdf.Server.csproj

COPY src/DocxToPdf.Server/ DocxToPdf.Server/
WORKDIR /src/DocxToPdf.Server
RUN dotnet publish -c Release -o /app

# Runtime image with LibreOffice
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
RUN apt-get update && \
    apt-get install -y --no-install-recommends libreoffice-writer && \
    apt-get clean && \
    rm -rf /var/lib/apt/lists/*

WORKDIR /app
COPY --from=build /app .

ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080

ENTRYPOINT ["dotnet", "DocxToPdf.Server.dll"]
