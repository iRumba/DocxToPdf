# Развёртывание

## Окружения

| Окружение | URL | Способ развёртывания |
|-----------|-----|---------------------|
| Локальное | `http://localhost:{{port}}` | Docker Compose |
| Staging | {{staging_url}} | {{staging_deploy_method}} |
| Продакшн | {{production_url}} | {{production_deploy_method}} |

## Процесс развёртывания

### 1. Сборка

```bash
{{build_command}}
```

### 2. Тестирование

```bash
{{test_command}}
```

### 3. Деплой

```bash
{{deploy_command}}
```

## Docker образы

- **Registry:** {{docker_registry}}
- **Image name:** {{docker_image_name}}
- **Tags:** `latest`, `{{version_tag_format}}`

## Переменные окружения

Перед деплоем убедись, что все необходимые переменные окружения установлены.
Полный список — в [secrets.md](secrets.md).

---

*Подробнее о CI/CD: ссылка на конфигурацию CI*
