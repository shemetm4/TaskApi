\# TaskApi



Minimal API для управления задачами (.NET 9, PostgreSQL, RabbitMQ).



\## Запуск



1\. Поднять инфраструктуру (Postgres + RabbitMQ):

```

&#x20;  docker compose up -d

```

&#x20;  Дождаться готовности: `docker compose ps` → оба сервиса `healthy`.



2\. Применить миграции:

```

&#x20;  cd TaskApi

&#x20;  dotnet ef database update

```



3\. Запустить приложение:

```

&#x20;  dotnet run

```

&#x20;  API: http://localhost:5126

&#x20;  RabbitMQ UI: http://localhost:15672 (guest/guest)



\## Тесты



```

dotnet test

```



\## Эндпоинты



\*\*Создать задачу\*\*

```

POST /tasks

Content-Type: application/json

{

&#x20; "title": "string",

&#x20; "priority": "Low | Medium | High"

}

```



\*\*Получить все задачи\*\*

```

GET /tasks

```



\*\*Завершить задачу\*\* (ставит CompletedAt, отправляет событие в RabbitMQ)

```

PUT /tasks/{id}/complete

```



\*\*Удалить задачу\*\*

```

DELETE /tasks/{id}

```

