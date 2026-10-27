# QuickDecompile

<p align="center">
  <a href="https://microsoft.com/windows"><img src="https://img.shields.io/badge/Платформа-Windows-0078D6?style=for-the-badge&logo=windows&logoColor=white" alt="Платформа"></a>
  <a href="https://dotnet.microsoft.com"><img src="https://img.shields.io/badge/.NET-8.0-512BD4?style=for-the-badge&logo=dotnet&logoColor=white" alt=".NET 8.0"></a>
  <a href="https://avaloniaui.net"><img src="https://img.shields.io/badge/UI-Avalonia_UI-8B5CF6?style=for-the-badge&logo=avalonia" alt="Avalonia UI"></a>
  <a href="https://github.com/KreziBro/QuickDecompile"><img src="https://img.shields.io/badge/Форматы-.NET_%7C_Java_%7C_Android-00B359?style=for-the-badge" alt="Форматы"></a>
  <a href="LICENSE"><img src="https://img.shields.io/badge/Лицензия-MIT-gray?style=for-the-badge" alt="Лицензия"></a>
</p>

Ультрабыстрый и легкий десктопный просмотрщик декомпилированного кода. Работает как «Блокнот для декомпиляции»: никаких тяжелых IDE, длительного создания проектов и громоздких меню — достаточно сделать правый клик по скомпилированному файлу в Проводнике Windows, чтобы мгновенно просмотреть чистый исходный код.

Создан на базе современного фреймворка Avalonia UI и проверенных декомпиляторов (ICSharpCode.Decompiler, Vineflower, jadx), обеспечивая плавный рендеринг, синтаксическую подсветку и моментальный отклик интерфейса.

## Поддерживаемые форматы

* **.NET**: `.dll`, `.exe` (высокоточная декомпиляция C# через ICSharpCode.Decompiler)
* **Java**: `.class`, `.jar`, `.war` (декомпиляция байткода JVM через Vineflower)
* **Android**: `.dex`, `.apk`, `.aab` (разбор мобильных пакетов и классов через jadx)

## Основные возможности

* **Интеграция с контекстным меню Windows**: Пункт «Открыть в QuickDecompile» прямо в проводнике для ассоциированных расширений файлов.
* **Мгновенный просмотр и кэширование**: Моментальный холодный старт, эффективное in-memory кэширование результатов декомпиляции для повторного открытия.
* **Система вкладок (Tabs)**: Удобная параллельная работа с несколькими файлами и сборками одновременно.
* **Интерактивное дерево файлов**: Удобная древовидная навигация по пакетам, ресурсам и классам внутри JAR, APK и DLL архивов.
* **Синтаксическая подсветка**: Четкое цветовое выделение синтаксиса C#, Java и байткода на базе AvaloniaEdit.

## Архитектура проекта

* **Core**: Модуль идентификации форматов файлов, менеджер кэширования и базовые абстракции.
* **Backends**: Изолированные адаптеры над внешними и встроенными библиотеками декомпиляции.
* **Shell**: Нативная интеграция с реестром контекстного меню Windows и IPC-мост для единого инстанса приложения (Single Instance).
* **UI**: Высокопроизводительный графический интерфейс на Avalonia UI с кастомными темами и виртуализацией списков.

## Быстрый старт

### Требования
* [.NET 8.0 SDK](https://dotnet.microsoft.com/download/dotnet/8.0) (для сборки и запуска)
* Java Runtime Environment (JRE 17+) в системном `PATH` или переменной `JAVA_HOME` (требуется для декомпиляции Java и Android)

### Внешние декомпиляторы
Для работы с Java и Android положите соответствующие JAR-пакеты рядом с исполняемым файлом `QuickDecompile.UI.exe`:

1. **Vineflower** (для `.class` и `.jar`):
   Скачайте актуальный `vineflower-X.Y.Z.jar` со страницы [Vineflower Releases](https://github.com/Vineflower/vineflower/releases), переименуйте в `vineflower.jar` и поместите в директорию с программой.
2. **jadx** (для `.apk` и `.dex`):
   Скачайте релизный архив `jadx-X.Y.Z.zip` с [jadx Releases](https://github.com/skylot/jadx/releases), извлеките `jadx-cli.jar` и сохраните как `jadx.jar` рядом с exe.

*(QuickDecompile также производит автоматический поиск по паттернам `*vineflower*.jar`, `*cfr*.jar` и `*jadx*.jar`).*

### Сборка и запуск из исходников

```bash
# Клонирование репозитория
git clone https://github.com/KreziBro/QuickDecompile.git
cd QuickDecompile

# Сборка решения
dotnet build QuickDecompile.sln

# Запуск графического интерфейса
dotnet run --project src/QuickDecompile.UI
```

Прямое открытие файла через аргументы командной строки:

```bash
dotnet run --project src/QuickDecompile.UI -- "путь/к/файлу.dll"
```

## Лицензии компонентов

* [Avalonia UI](https://github.com/AvaloniaUI/Avalonia) — MIT License
* [AvaloniaEdit](https://github.com/AvaloniaUI/AvaloniaEdit) — MIT License
* [ICSharpCode.Decompiler](https://github.com/icsharpcode/ILSpy) — MIT License
* [Vineflower](https://github.com/Vineflower/vineflower) — GPL-2.0 License
* [CFR](https://github.com/leibnitz27/cfr) — MIT License
* [jadx](https://github.com/skylot/jadx) — Apache-2.0 License

## Сообщество и поддержка

* Официальный Telegram канал разработчика: https://t.me/KreziBro_dev
* Поддержать развитие проекта на платформе Ko-fi: https://ko-fi.com/krejzibro

## Лицензия

Распространяется под лицензией MIT. Подробности в файле [LICENSE](LICENSE).
