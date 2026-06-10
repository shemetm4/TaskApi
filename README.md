# TaskApi

Minimal API для управления задачами (.NET 9, PostgreSQL, RabbitMQ).

## Запуск

1. Поднять инфраструктуру (Postgres + RabbitMQ):
```
   docker compose up -d
```
   Дождаться готовности: `docker compose ps` → оба сервиса `healthy`.

2. Применить миграции:
```
   cd TaskApi
   dotnet ef database update
```

3. Запустить приложение:
```
   dotnet run
```
   API: http://localhost:5126
   RabbitMQ UI: http://localhost:15672 (guest/guest)

## Тесты

```
dotnet test
```

## Эндпоинты

**Создать задачу**
```
POST /tasks
Content-Type: application/json
{
  "title": "string",
  "priority": "Low | Medium | High"
}
```

**Получить все задачи**
```
GET /tasks
```

**Завершить задачу** (ставит CompletedAt, отправляет событие в RabbitMQ)
```
PUT /tasks/{id}/complete
```

**Удалить задачу**
```
DELETE /tasks/{id}
```