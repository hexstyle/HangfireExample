## Основная идея
Hangfire можно использовать как полноценный планировщик заданий, управляемый из C# кода.

Настройки заданий, логи и текущую активность хранит в себе инстанс PostgreSQL.

Одну БД могут использовать разные приложения, регистрируя в ней свои джобы и исполняя их самостоятельно.

Джобы с одинаковыми именами перетирают друг друга, если 2 приложения подключились одновременно к одной БД. При этом hangfire воспринимает имеющиеся активные хосты как свои воркеры и распределяет нагрузку между ними. Можно проверить, запустив приложение одновременно и в докере, и в локальном отладчике. Джоб будет будет выполняться по своему расписанию, однако приложения будут чередовать друг с другом его запуск, таким образом, распределяя нагрузку между собой.

## Пример использования Hangfire
Запускается локально (https://localhost:7228/hangfire):
```
dotnet run
```
Или через docker (http://localhost:5000/hangfire):
```
docker-compose up
```

## Конфигурация
### Пакеты
Добавляем в проект пакеты Hangfire
```
dotnet add package Hangfire
dotnet add package Hangfire.AspNetCore
dotnet add package Hangfire.Console
dotnet add package Hangfire.PostgreSql
```
### Базовая настройка сервера
Настраивается как обычный Asp.Net сервис. Для подключения к СУБД лучше использовать Factory-класс, создающий подключение (прим.: [PostgresConnectionFactory.cs](PostgresConnectionFactory.cs))
```
var factory = new PostgresConnectionFactory(connString);
builder.Services.AddHangfire(config =>
{
    config.UsePostgreSqlStorage((options)=>
        options.UseConnectionFactory(factory)
    );
});
builder.Services.AddHangfireServer();
builder.Services.AddSingleton<IConnectionFactory>(factory);
```
И затем в секции app регистрируем дашборд. При этом указание Authorization обязательно (прим.: [AllowAllAuthorizationFilter.cs](AllowAllAuthorizationFilter.cs). Если не включить, то при сборке Production приложения будет выводиться 401. 
```
app.UseHangfireDashboard("/hangfire", new DashboardOptions
{
    Authorization = new[] { new AllowAllAuthorizationFilter() }
});
```
Все структуры и таблицы, необходимые ему для работы, сервер настроит сам.
### Настройка джобов
Сперва регистрируем джоб в DI Asp.Net:
```
builder.Services.AddTransient<MyJob>();
```
Затем устанавливаем для него расписание:
```
RecurringJob.AddOrUpdate<MyJob>(
    "my-job",
    job => job.Run((PerformContext)null),
    "*/5 * * * * *");
```
При этом job.Run - это не реальный вызов метода, а только его метаданные для Hangfire. Параметр PerformContext при этом предается из плагина Hangfire.Console и подставляется автоматом при каждом вызове. Таким образом, мы получаем контекст, куда можно писать лог работы джоба.
```
public class MyJob
{
    public void Run(PerformContext context)
    {
        context.WriteLine( $"Job started at {DateTime.Now}");
        Thread.Sleep(2000);
        context.WriteLine($"Job finished at {DateTime.Now}");
    }
}
```
### Обслуживание
В OpenSource версии Hangfire не имеет встроенных средств обслуживания. Но джоб, удаляющий старые логи, можно написать самостоятельно (прим.: [Jobs/MaintenanceJob.cs](Jobs/MaintenanceJob.cs)).


