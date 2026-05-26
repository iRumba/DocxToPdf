# Инструменты проекта

## .NET 10
- **Назначение:** Backend (ASP.NET Core Minimal API)
- **Версия:** 10.0.300
- **Docker-образ:** mcr.microsoft.com/dotnet/sdk:10.0 (build), mcr.microsoft.com/dotnet/aspnet:10.0 (runtime)

## React + Vite + TypeScript
- **Назначение:** Frontend SPA
- **Версии:** React 19, Vite 9, TypeScript 5.x
- **Docker-образ:** node:26-alpine (build), nginx:alpine (runtime)

## LibreOffice
- **Назначение:** Конвертация DOCX → PDF
- **Лицензия:** MPL 2.0 (свободная, open-source)
- **Установка:** apt-get install -y --no-install-recommends libreoffice
- **Использование:** soffice --headless --convert-to pdf <input> --outdir <dir>

## Docker
- **Версия:** 29.4.3
- **Docker Compose:** v5.1.4

## .NET Aspire
- **Назначение:** Оркестрация микросервисов в разработке
- **Шаблоны:** aspire-apphost, aspire-servicedefaults
