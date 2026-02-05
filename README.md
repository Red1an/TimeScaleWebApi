# TimeScaleWebApi
Тестовое задание для infotecs. WebAPI приложение для работы с timescale.
 данными
## Что делает проект
- Загружает CSV-файлы (Date;ExecutionTime;Value)
- Валидирует данные (диапазон дат, неотрицательные значения, лимит строк ≤ 10 000)
- Считает интегральные метрики и сохраняет в таблицу Results
- Позволяет получать:
  - список результатов с фильтрами
  - последние 10 значений по имени файла

## Технологии
- .NET 9
- ASP.NET Core Web API
- EF Core 9 + Npgsql
- PostgreSQL
- Swagger

## Эндпоинты
- POST   /api/Data/upload → загрузка и обработка CSV
- GET    /api/Data/results → список результатов (фильтры: filename, DateFrom, DateTo, avgValueFrom, avgValueTo, avgExecutionFrom, avgExecutionTo)
- GET    /api/Data/{filename}/latest → последние 10 записей по файлу (сортировка по Дате)
