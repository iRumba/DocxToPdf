# Тестирование

## Юнит-тесты
Проект не требует сложного тестирования на данном этапе.

### Server
- Тестирование endpoint /api/convert через WebApplicationFactory
- Проверка валидации (неверный формат, превышение размера)
- Mock для LibreOffice (замена Process.Start на тестовый)

### Client
- Компонентные тесты с Vitest
- Проверка отображения состояний (загрузка, ошибка, успех)

## E2E-тесты
- Не реализованы на данном этапе

## Ручное тестирование
```bash
# Проверка health-check
curl http://localhost:5000/api/health

# Проверка конвертации
curl -X POST http://localhost:5000/api/convert -F "file=@test.docx" -o result.pdf
```
