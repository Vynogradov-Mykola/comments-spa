# ----------------------
# 1. Сборка проекта
# ----------------------
FROM mcr.microsoft.com/dotnet/sdk:10.0.103 AS build
WORKDIR /app

# Копируем csproj и восстанавливаем зависимости
COPY Comments.Api/*.csproj ./
RUN dotnet restore

# Копируем весь проект и публикуем
COPY Comments.Api/. ./
RUN dotnet publish -c Release -o out

# ----------------------
# 2. Рантайм
# ----------------------
FROM mcr.microsoft.com/dotnet/aspnet:10.0.3 AS runtime
WORKDIR /app

# Копируем готовые файлы из стадии сборки
COPY --from=build /app/out ./

ENTRYPOINT ["dotnet", "Comments.Api.dll"]