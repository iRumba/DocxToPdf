# Архитектура DocxToPdf

## Общая схема

```
┌─────────────────────────────────────────────────────────────┐
│  Browser (React SPA)                                        │
│  ┌──────────────┐    POST /api/convert (multipart)          │
│  │  Drop Zone    │ ──────────────────────────────────────┐  │
│  │  + File Info  │                                       │  │
│  └──────────────┘                                       │  │
│         │                                                │  │
│         ▼                                                │  │
│  ┌──────────────┐    GET /api/convert/{id}/download       │  │
│  │  Download     │ ◄────────────────────────────────────┘  │
│  └──────────────┘                                          │
└──────────────────────────────────────────────────────────────┘
                                │
                                ▼
┌──────────────────────────────────────────────────────────────┐
│  ASP.NET Core Minimal API Server (.NET 10)                  │
│                                                              │
│  POST /api/convert                                           │
│    ├── Validate file (.docx, size ≤ 50MB)                    │
│    ├── Save to temp directory                                │
│    ├── Run: soffice --headless --convert-to pdf              │
│    ├── Read PDF from temp                                    │
│    ├── Return File(pdfBytes, "application/pdf")              │
│    └── Cleanup temp (finally)                                │
│                                                              │
│  GET /api/health                                             │
│    └── Return { status: "healthy" }                          │
└──────────────────────────────────────────────────────────────┘
```

## Компоненты

### Backend (DocxToPdf.Server)
- **Технология:** ASP.NET Core Minimal API, .NET 10
- **Порты:** 8080 (внутренний), 5000 (внешний в Docker Compose)
- **Зависимости:** только ASP.NET Core (без внешних NuGet-пакетов)
- **Конвертация:** вызов LibreOffice через Process.Start

### Frontend (DocxToPdf.Client)
- **Технология:** React 19 + Vite 9 + TypeScript
- **Порты:** 5173 (dev), 80 (production/nginx)
- **Библиотеки:** react-dropzone, axios
- **Сборка:** Vite → dist → nginx

### Оркестрация

#### Docker Compose
- Два сервиса: server + client
- client зависит от server (depends_on)
- nginx-proxy для проксирования /api/ на сервер

#### .NET Aspire
- AppHost запускает server (AddProject) и client (AddNpmApp)
- ServiceDefaults: OpenTelemetry, Health Checks, Resilience
- Dashboard для мониторинга

## Поток данных

1. Пользователь выбирает/перетаскивает .docx файл в браузере
2. React-приложение отображает информацию о файле
3. Пользователь нажимает "Convert to PDF"
4. Файл отправляется на сервер через POST /api/convert (multipart/form-data)
5. Сервер валидирует файл (расширение, размер)
6. Файл сохраняется во временную директорию
7. Запускается LibreOffice: soffice --headless --convert-to pdf
8. LibreOffice создаёт .pdf файл в той же директории
9. Сервер читает PDF и возвращает его в ответе
10. Временная директория удаляется
11. Браузер автоматически скачивает PDF
