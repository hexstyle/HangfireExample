# Используем официальный образ SDK для сборки
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /app

# Копируем csproj и восстанавливаем зависимости
COPY *.csproj ./
RUN dotnet restore

# Копируем остальные файлы и билдим приложение
COPY . ./
RUN dotnet publish -c Release -o out

# Запускаем минимальный runtime образ
FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app
COPY --from=build /app/out .

# Открываем порт 80
EXPOSE 8080

# Запускаем приложение
ENTRYPOINT ["dotnet", "HangfireExample.dll"]
