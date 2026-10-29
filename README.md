# Сервис задач DemoApi (DevOps)

Учебный REST API сервис управления проектами и задачами по курсу **«Инструменты DevOps»** (.NET 10, ASP.NET Core, PostgreSQL 18, Docker, Docker Compose, Prometheus).

## Основные эндпоинты сервиса

| Метод и маршрут | Назначение |
|---|---|
| `GET /` | Информация о сервисе, версии (`APP_VERSION`) и имени хоста/контейнера |
| `GET /api/projects` | Список всех проектов |
| `GET /api/projects/{id}` | Получение проекта по идентификатору |
| `POST /api/projects` | Создание нового проекта (`400` при пустом имени, `409` при дубликате) |
| `PUT /api/projects/{id}` | Переименование проекта (`200 OK`, `400 Bad Request`, `404 Not Found`, `409 Conflict`) |
| `DELETE /api/projects/{id}` | Удаление проекта вместе с его задачами |
| `GET /api/projects/{id}/stats` | Статистика по задачам проекта (`total`, `todo`, `inProgress`, `done`, `overdue`) |
| `GET /api/tasks` | Список задач с фильтрацией по `?projectId=` и `&status=` |
| `GET /api/tasks/{id}` | Получение задачи по идентификатору |
| `POST /api/tasks` | Создание новой задачи в проекте (начальный статус `Todo`) |
| `PUT /api/tasks/{id}` | Обновление названия, описания и срока задачи |
| `PATCH /api/tasks/{id}/status` | Смена статуса задачи (`Todo -> InProgress -> Done` и на шаг назад) |
| `DELETE /api/tasks/{id}` | Удаление задачи |
| `GET /health/live` | Liveness-проверка жизнеспособности процесса |
| `GET /health/ready` | Readiness-проверка готовности сервиса и доступности БД PostgreSQL |
| `GET /metrics` | Метрики в формате Prometheus |

---

## Как запустить (Docker Compose)

1. Создайте локальный файл переменных окружения `.env` из шаблона:
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

## Как разрабатывать локально (Visual Studio 2026)

1. Поднимите контейнер базы данных PostgreSQL в фоне:
   ```powershell
   docker compose up -d --wait db
   ```
2. Откройте решение `DemoApi.slnx` в **Visual Studio 2026** и нажмите **F5** (или выполните `dotnet run --project src/DemoApi`).
3. Документация **Swagger UI** будет доступна по адресу `http://localhost:5080/swagger`.

---

## Как запустить тесты

Тесты (`xUnit` + `WebApplicationFactory`) проверяют бизнес-правила и HTTP API поверх PostgreSQL:

```powershell
# 1. Убедитесь, что контейнер БД запущен
docker compose up -d --wait db

# 2. Запустите все модульные и интеграционные тесты
dotnet test
```
