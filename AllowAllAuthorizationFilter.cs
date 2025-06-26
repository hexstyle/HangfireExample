using Hangfire.Dashboard;

public class AllowAllAuthorizationFilter : IDashboardAuthorizationFilter
{
    public bool Authorize(DashboardContext context)
    {
        /*
            // В реальном приложении с авторизацией делаем так:
            var httpContext = context.GetHttpContext();

            // Разрешаем доступ только аутентифицированным пользователям
            return httpContext.User.Identity?.IsAuthenticated ?? false;
            // Либо же берем Claim из контекста и определяем разрешенных по ролям
        */
        
        // Всем разрешен доступ. Используется для примера.
        // В продуктивном коде быть не должно.
        return true;
    }
    }