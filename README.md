# Task Service REST API

Учебный REST API сервис управления проектами и задачами по курсу **«Инструменты DevOps»** (.NET 10, ASP.NET Core, PostgreSQL 18, Docker, Docker Compose, Prometheus).

## Архитектура и возможности

- **Проекты (`/api/projects`)**: создание, просмотр списка, получение по ID, переименование (`PUT /api/projects/{id}`), удаление, статистика по задачам (`GET /api/projects/{id}/stats`).
- **Задачи (`/api/tasks`)**: создание, фильтрация по проекту и статусу, редактирование, контролируемая смена статуса (`Todo -> InProgress -> Done`), удаление.
- **Хранилище**: поддержка работы в оперативной памяти (`InMemory`) и в СУБД `PostgreSQL`.
- **Наблюдаемость и эксплуатация**:
  - Проверки жизнеспособности и готовности: `/health/live` и `/health/ready`
  - Метрики Prometheus: `/metrics`
  - Структурированные JSON-логи в Production

---

## Быстрый запуск через Docker Compose

1. Скопируйте файл переменных окружения (при необходимости):
   ```powershell
   Copy-Item .env.example .env
   ```
2. Соберите и запустите контейнеры сервиса и базы данных PostgreSQL:
   ```powershell
   docker compose up -d --build --wait
   ```
3. Проверьте готовность сервиса:
   ```powershell
   curl.exe http://localhost:8080/health/ready
   irm http://localhost:8080/api/projects
   ```
4. Остановка контейнеров:
   ```powershell
   # Остановка с сохранением данных БД в томе pgdata
   docker compose down

   # Остановка с полной очисткой томов данных (при пересоздании с нуля)
   docker compose down -v
   ```

---

## Локальный запуск и разработка

Требуется установленный **.NET 10 SDK**.

```powershell
# Запуск сервиса локально (Swagger UI доступен на http://localhost:5080/swagger)
dotnet run --project src/DemoApi

# Запуск модульных и интеграционных тестов
dotnet test
```

