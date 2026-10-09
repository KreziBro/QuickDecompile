# QuickDecompile

Лёгкий и быстрый десктопный просмотрщик декомпилированного кода.
Работает как «Блокнот для декомпиляции» — никаких настроек проектов, никаких меню, просто правый клик на скомпилированный файл и сразу виден исходник.

## Поддерживаемые форматы
- **.NET**: `.dll`, `.exe` (декомпиляция через ICSharpCode.Decompiler)
- **Java**: `.class`, `.jar`, `.war` (декомпиляция через Vineflower)
- **Android**: `.dex`, `.apk`, `.aab` (декомпиляция через jadx)

## Архитектура
- **Core**: определение формата файла и кэш результатов.
- **Backends**: обёртки над внешними инструментами декомпиляции.
- **Shell**: интеграция с контекстным меню Windows и IPC для одного экземпляра приложения.
- **UI**: приложение на Avalonia UI с синтаксической подсветкой через AvaloniaEdit.

## Запуск

### Требования
- .NET 8.0 SDK (для сборки)
- Java Runtime Environment (JRE) в `PATH` или `JAVA_HOME` (нужен для Java и Android декомпиляции)

### Внешние декомпиляторы
Для декомпиляции Java и Android нужно скачать JAR-файлы и положить их рядом с `QuickDecompile.UI.exe`.

1. **Vineflower** (для `.class` и `.jar`):
   - Скачать последний `vineflower-X.Y.Z.jar` с [Vineflower Releases](https://github.com/Vineflower/vineflower/releases).
   - Переименовать в `vineflower.jar` и положить рядом с exe.

2. **jadx** (для `.apk` и `.dex`):
   - Скачать последний `jadx-X.Y.Z.zip` с [jadx Releases](https://github.com/skylot/jadx/releases).
   - Извлечь `jadx-cli.jar` и положить рядом с exe как `jadx.jar`.

*(QuickDecompile также автоматически ищет `*vineflower*.jar`, `*cfr*.jar` или `*jadx*.jar` в папке с exe).*

### Сборка и запуск
```bash
dotnet build QuickDecompile.sln
dotnet run --project src/QuickDecompile.UI
```

Запуск с файлом напрямую:
```bash
dotnet run --project src/QuickDecompile.UI -- "путь/к/файлу.dll"
```

## Возможности
- **Интеграция с контекстным меню**: добавляет «Открыть в QuickDecompile» прямо в контекстное меню Windows.
- **Мгновенный просмотр**: быстрый запуск и кэш результатов в памяти.
- **Вкладки**: несколько файлов одновременно.
- **Дерево файлов**: навигация по содержимому JAR, APK, DLL.

## Лицензии сторонних компонентов

- **[Avalonia UI](https://github.com/AvaloniaUI/Avalonia)**: MIT License.
- **[AvaloniaEdit](https://github.com/AvaloniaUI/AvaloniaEdit)**: MIT License.
- **[ICSharpCode.Decompiler](https://github.com/icsharpcode/ILSpy)**: MIT License.
- **[Vineflower](https://github.com/Vineflower/vineflower)**: GPL-2.0 License.
- **[CFR](https://github.com/leibnitz27/cfr)**: MIT License.
- **[jadx](https://github.com/skylot/jadx)**: Apache-2.0 License.

## MVP

Текущая версия является исключительно просмотрщиком. Вне скоупа MVP:
- Редактирование и перекомпиляция
- Деобфускация и патчинг
- Динамический анализ и отладка
- Плагины
