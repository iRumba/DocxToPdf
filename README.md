# DocxToPdf

Конвертация DOCX в PDF через LibreOffice Headless.

## Архитектура

- **Server** — ASP.NET Core Minimal API (.NET 10). Принимает DOCX, конвертирует через LibreOffice, возвращает PDF.
- **Client** — React + Vite + TypeScript. Drag & drop загрузка, скачивание результата.
- **Aspire AppHost** — .NET Aspire оркестрация для локальной разработки.

## Запуск

### Docker Compose
```bash
docker compose up
```

### .NET Aspire
```bash
dotnet run --project src/DocxToPdf.AppHost
```

## Технологии

- .NET 10
- React + Vite + TypeScript
- LibreOffice (headless)
- Docker Compose
- .NET Aspire
