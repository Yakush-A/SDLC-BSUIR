# Описание дизайна программного обеспечения

Для модуля «Генератор отчётов листов персонажей» (Sheet Export) проекта Live D&D List

Версия 0.1

Подготовил(а) А.П. Якуш

БГУИР, кафедра электронных вычислительных машин

07.11.2026

## Оглавление

<!-- TOC -->
* [1. Введение](#1-introduction)
  * [1.1 Назначение документа](#11-document-purpose)
  * [1.2 Область применения](#12-subject-scope)
  * [1.3 Определения, акронимы и сокращения](#13-definitions-acronyms-and-abbreviations)
  * [1.4 Ссылки](#14-references)
  * [1.5 Обзор документа](#15-document-overview)
* [2. Обзор дизайна](#2-design-overview)
  * [2.1 Задачи стейкхолдеров](#21-stakeholder-concerns)
  * [2.2 Выбранные точки зрения](#22-selected-viewpoints)
* [3. Проектные представления](#3-design-views)
* [4. Решения](#4-decisions)
* [5. Приложения](#5-appendixes)
<!-- TOC -->

## История изменений

| Name | Date | Reason For Changes | Version |
|------|------|--------------------|---------|
| А.П. Якуш | 07.11.2026 | Первая редакция на основе SRS v0.1 (системные требования, приоритеты, решения ADR-0001 … 0006) | 0.1 |
|      |      |                    |         |

---

## 1. Введение {#1-introduction}

Документ описывает, как будет устроен модуль экспорта листа персонажа в PDF внутри приложения Live D&D List: компоненты, интерфейсы, данные, алгоритмы и решения. Он основан на согласованном SRS и является руководством для разработчиков.

### 1.1 Назначение документа {#11-document-purpose}

SDD задаёт структуру реализации достаточно подробно, чтобы разработчик мог писать код без дополнительных архитектурных вопросов (предписывающая архитектура). Читатели: разработчик (реализация), тестировщик (проверка по трассировке к SRS), Заказчик (контроль, что дизайн закрывает его требования), преподаватель. Связанные документы: SRS (`srs/srs.md`), протоколы решений (`adr/`), план реализации (`plan/plan.md`).

### 1.2 Область применения {#12-subject-scope}

Проектируется модуль Sheet Export версии 0.1 и его интеграция в Live D&D List (backend Java 21, Spring Boot 4.0.2, frontend React + TypeScript). Модуль создаёт PDF с листом персонажа D&D 5e в структуре из трёх страниц, позволяет сохранить его в каталог, выбранный пользователем, и предоставляет точку расширения для других игровых систем.

**Входит:** пакет `dev.ushki.sheetexport` (ядро), адаптер D&D 5e и REST-контроллер в проекте, кнопка и логика сохранения во frontend, встроенные шрифты, ресурсы подписей.

**Не входит:** адаптеры других игровых систем, хранение PDF на сервере, изменение схемы БД, редактирование листа в PDF, печать.

Бизнес-цели BO-1 … BO-4 и ограничения CON-1 … CON-7 берутся из SRS (раздел 1.2 и 2.3).

### 1.3 Определения, акронимы и сокращения {#13-definitions-acronyms-and-abbreviations}

| Term | Definition |
|------|------------|
| ADR | Architectural Decision Record — протокол архитектурного решения |
| API | Application Programming Interface — набор определений и протоколов для создания и интеграции прикладного ПО |
| Frame (рамка) | Прямоугольная область страницы, в которую выводится блок; может ссылаться на рамку продолжения |
| Layout (раскладка) | Вычисление положения текста, таблиц и изображений на страницах до записи PDF |
| Port / Adapter | Интерфейс ядра (порт) и реализация для конкретной системы (адаптер) |
| SDD | Software Design Document — документ, описывающий назначение, требования и характер ПО |
| SheetModel | Нейтральная модель листа: страницы, блоки, поля, таблицы, текст, изображения |
| SRS | Software Requirements Specification — документ, описывающий назначение, требования и характер ПО |
| SSRF | Server-Side Request Forgery — подделка запроса со стороны сервера |
| WP | Work Package — пакет работ плана реализации |
| Провайдер листа | Реализация порта `SheetDefinitionProvider`, создающая `SheetModel` для одной игровой системы |
| Страница продолжения | Дополнительная страница, создаваемая при переполнении блока |

### 1.4 Ссылки {#14-references}

| № | Источник | Тип | Расположение |
|---|----------|-----|--------------|
| 1 | SRS модуля Sheet Export, версия 0.1 | Нормативная | `srs/srs.md` |
| 2 | Протоколы архитектурных решений ADR-0001 … ADR-0006 | Нормативная | `adr/` |
| 3 | План реализации и переписка с Заказчиком | Справочная | `plan/plan.md`, `correspondence/` |
| 4 | Репозиторий Live D&D List (код, `README.md`, `CONTRIBUTING.md`) | Нормативная | https://github.com/Margit-The-Fell-Omen/liveDndList |
| 5 | Apache PDFBox 3.x, документация | Справочная | https://pdfbox.apache.org |
| 6 | File System Access API (WICG / MDN) | Справочная | https://developer.mozilla.org/docs/Web/API/File_System_API |
| 7 | RFC 6266, RFC 5987 (Content-Disposition, кодирование имени файла) | Справочная | IETF |
| 8 | SIL Open Font License 1.1 | Справочная | https://openfontlicense.org |
| 9 | Лабораторная работа №4, БГУИР | Нормативная | `Лабораторная_работа_4.pdf` |

### 1.5 Обзор документа {#15-document-overview}

Раздел 2 содержит задачи стейкхолдеров и выбранные точки зрения. Раздел 3 описывает проектные представления с ID вида `DV-XXX-NN`, ссылками на требования SRS (`REQ-…`) и решения (`DEC-…`). Раздел 4 фиксирует решения (подробности в `adr/`). Раздел 5 содержит приложения: приоритеты, трассировку, таблицу соответствия данных блокам листа, конфигурацию и риски. Диаграммы дизайна выполнены в UML (PlantUML, исходники `.puml` в `sdd/diagrams/`); графика SRS остаётся в SysML v2. При изменении документа добавляется строка в историю изменений и увеличивается версия.

---

## 2. Обзор дизайна {#2-design-overview}

Модуль построен по схеме «порты и адаптеры» (ADR-0002): ядро `dev.ushki.sheetexport` ничего не знает о проекте и D&D, оно превращает нейтральную `SheetModel` в PDF в два этапа: раскладка (`LayoutEngine`, ADR-0001) и запись (`PdfRenderer`). Данные D&D преобразует тонкий адаптер `DndSheetProvider` внутри проекта. Frontend открывает диалог сохранения до запроса и пишет полученный PDF в выбранный файл (ADR-0003). Генерация синхронная, с ограничением параллелизма (ADR-0004), портрет загружается через защитный клиент (ADR-0005), шрифты встроены (ADR-0006).

### 2.1 Задачи стейкхолдеров {#21-stakeholder-concerns}

| Стейкхолдер | Задачи | Адресующие точки зрения |
|-------------|--------|-------------------------|
| Игрок | Получить корректный PDF быстро и сохранить, куда нужно; понятные ошибки | Interface, Interaction, State dynamics, Algorithm |
| Мастер игры | Прочитать файл в любом просмотрщике; кириллица без искажений | Algorithm, Resources |
| Заказчик | Модуль встроен без сбоев; изоляция и расширяемость; контроль сроков | Context, Composition, Dependency, Patterns |
| Разработчик | Понятные границы, простое добавление новой системы | Logical, Structure, Interface, Dependency |
| Тестировщик | Проверяемость: геометрия, текст, нагрузка, безопасность | Algorithm, Interface, Concurrency |
| Сопровождающий / оператор | Развёртывание без новых сервисов, метрики, предсказуемые ресурсы | Deployment, Resources, Concurrency |

### 2.2 Выбранные точки зрения {#22-selected-viewpoints}

| Точка зрения | Применимость | Что адресует | Язык | Представления |
|--------------|--------------|--------------|------|---------------|
| Контекст | Используется | Границы, акторы, внешние системы | Контекстная диаграмма (SysML v2 из SRS) | DV-CTX-01 |
| Композиция | Используется | Состав и размещение компонентов | Диаграмма компонентов, пакетов | DV-CMP-01 |
| Логическое | Используется | Типы и их связи | Диаграмма классов UML | DV-LOG-01 |
| Физическое | Не применяется: новое оборудование не требуется, физическая топология не меняется | — | — | — |
| Структура | Используется | Внутренняя организация модуля, порты | Таблица пакетов и портов | DV-STR-01 |
| Зависимости | Используется | Направление зависимостей, правила изоляции | Диаграмма пакетов (SysML v2 из SRS) | DV-DEP-01 |
| Информация | Используется | Модель листа, соответствие данным проекта | Словарь данных, таблица соответствия | DV-INF-01 |
| Интерфейс | Используется | REST-контракт, SPI провайдера, UI-компонент | Спецификация API | DV-IFC-01, DV-IFC-02, DV-IFC-03 |
| Взаимодействие | Используется | Последовательность вызовов, ошибки | Диаграмма последовательности | DV-INT-01 |
| Алгоритм | Используется | Раскладка, производные значения, загрузка портрета | Псевдокод, таблицы решений | DV-ALG-01, DV-ALG-02, DV-ALG-03 |
| Динамика состояний | Используется | Состояния кнопки экспорта | Диаграмма состояний (SysML v2 из SRS) | DV-STA-01 |
| Параллелизм | Используется | Ограничение одновременных генераций | Описание | DV-CON-01 |
| Паттерны | Используется | Применённые приёмы проектирования | Таблица | DV-PAT-01 |
| Развёртывание | Используется | Размещение на узлах, настройки nginx и JVM | Диаграмма развёртывания | DV-DPL-01 |
| Ресурсы | Используется | Память, потоки, шрифты, сеть | Таблица ресурсов и лимитов | DV-RES-01 |

---

## 3. Проектные представления {#3-design-views}

### DV-CTX-01-context

- **ID**: DV-CTX-01-context
- **Title**: Контекст модуля
- **Viewpoint**: Контекст
- **Representation**: Модуль встроен в Live D&D List. Акторы: Игрок (запускает экспорт, сохраняет файл), Мастер игры (получает файл вне системы). Внешние элементы: хост портрета (HTTP GET), файловая система пользователя (запись файла через браузер). Подробная диаграмма — `srs/diagrams/01-context.png`, варианты использования — `srs/diagrams/02-use-cases.png`.
- **More Information**: SRS 2.1, 2.2; REQ-INT-004, REQ-INT-010; DEC-002.

### DV-CMP-01-composition

- **ID**: DV-CMP-01-composition
- **Title**: Композиция: проект, модуль, frontend
- **Viewpoint**: Композиция
- **Representation**:

| Компонент | Расположение | Ответственность | Новое / существующее |
|-----------|--------------|-----------------|----------------------|
| `ExportPdfButton`, `pdfExport.ts` | `frontend/src/components/character/`, `frontend/src/services/` | Кнопка, диалог сохранения, запрос, запись файла, уведомления | Новое (интеграция в `CharacterHeader.tsx`) |
| `CharacterExportController` | `dev.ushki.livedndlist.controller` | Маршрут `GET /api/v1/characters/{id}/export/pdf`, заголовки ответа | Новое |
| `DndSheetProvider` | `dev.ushki.livedndlist.export` | Адаптер: `CharacterResponse` → `SheetModel` | Новое |
| `dev.ushki.sheetexport` (`api`, `model`, `layout`, `render`, `image`, `spi`) | Тот же Maven-модуль, отдельный корневой пакет | Ядро: раскладка, рендер, шрифты, загрузка портрета, реестр провайдеров | Новое |
| `CharacterService`, `GlobalExceptionHandler`, JWT-фильтр | Проект | Данные персонажа с проверкой владельца, формат ошибок, авторизация | Существующее, без изменений |

  Диаграмма изоляции: `srs/diagrams/03-module-architecture.png`.
- **More Information**: REQ-REUSE-001, REQ-REUSE-003, REQ-INT-001, REQ-INT-003; DEC-002.

### DV-LOG-01-logical

- **ID**: DV-LOG-01-logical
- **Title**: Логическая модель ядра
- **Viewpoint**: Логическое
- **Representation**: ![Классы ядра](diagrams/class-model.png)

  Ключевые абстракции: `SheetExportService` (фасад), `SheetDefinitionProvider` (порт), `SheetModel` → `PageSpec` → `Block` (`FieldBlock`, `ScoreBlock`, `ChecklistBlock`, `TableBlock`, `TextBlock`, `ImageBlock`), `PageTemplate` с `Frame`, `LayoutEngine`, `PdfRenderer`, `FontRegistry`, `LabelBundle`, `ImageLoader`/`UrlGuard`. Ядро содержит только типы блоков и рамок и не использует термины D&D.
- **More Information**: REQ-REUSE-002, REQ-FUNC-015 … 020; DEC-001, DEC-002.

### DV-STR-01-structure

- **ID**: DV-STR-01-structure
- **Title**: Структура пакетов и порты ядра
- **Viewpoint**: Структура
- **Representation**:

| Пакет | Содержимое | Может зависеть от |
|-------|------------|-------------------|
| `api` | `SheetExportService`, `SheetDefinitionProvider` (порт), `ExportRequest`, `PdfDocument`, `SheetExportException` | `model`, `layout`, `render`, `image` |
| `model` | `SheetModel`, `PageSpec`, типы `Block` | только JDK |
| `layout` | `LayoutEngine`, `PageTemplate`, `Frame`, `TextWrapper`, `Paginator` | `model`, метрики шрифтов из `render` через интерфейс `TextMetrics` |
| `render` | `PdfRenderer`, `FontRegistry`, `LabelBundle` | `layout`, `model`, **PDFBox (только здесь)** |
| `image` | `ImageLoader`, `UrlGuard`, `ImageScaler` | JDK, `httpclient5` |
| `spi` | `SheetExportConfiguration` (`@Configuration`), реестр провайдеров | `api` |

  Порты: **входной** — `SheetExportService.export(ExportRequest)`; **выходной** — `SheetDefinitionProvider.build(ExportRequest)`. Подключение: `@Import(SheetExportConfiguration.class)` в конфигурации проекта; провайдеры регистрируются как Spring-бины и собираются в реестр по `systemId()`.
- **More Information**: REQ-REUSE-001 … 004; DEC-002.

### DV-DEP-01-dependencies

- **ID**: DV-DEP-01-dependencies
- **Title**: Направление зависимостей и правила изоляции
- **Viewpoint**: Зависимости
- **Representation**:

| Правило | Проверка |
|---------|----------|
| `dev.ushki.sheetexport..` не зависит от `dev.ushki.livedndlist..` | ArchUnit `IsolationTest` |
| `org.apache.pdfbox..` импортируется только в `sheetexport.render` | ArchUnit `IsolationTest` |
| Проект зависит только от `sheetexport.api` и `sheetexport.spi` | ArchUnit `IsolationTest` |
| Внешние зависимости ядра: PDFBox 3.x, `httpclient5` (версия из BOM Spring Boot), Noto Sans (ресурсы) | `docs/licenses.md`, осмотр `pom.xml` |
| Тестовые зависимости: ArchUnit | `pom.xml` |

  Зависимости идут направленно: проект → `api`/`spi` модуля; адаптер `DndSheetProvider` → порт `SheetDefinitionProvider`; модуль → проект: нет.
- **More Information**: REQ-REUSE-001, REQ-COMP-002; DEC-001, DEC-002; диаграмма `srs/diagrams/03-module-architecture.png`.

### DV-INF-01-information

- **ID**: DV-INF-01-information
- **Title**: Нейтральная модель листа и соответствие данным проекта
- **Viewpoint**: Информация
- **Representation**: Модуль ничего не хранит (CON-7). Данные передаются в памяти в виде `SheetModel`. Соответствие данных проекта блокам листа (адаптер D&D) приведено в Приложении C; типы блоков:

| Блок | Содержание | Правила |
|------|------------|---------|
| `FieldBlock` | подпись и значение | пустое значение выводится как «—» (REQ-FUNC-022) |
| `ScoreBlock` | подпись, значение, модификатор | одна рамка на характеристику |
| `ChecklistBlock` | элементы: отметка, подпись, значение | используется для спасбросков и навыков |
| `TableBlock` | колонки, строки, `repeatHeader` | строки не разрываются между страницами, заголовок повторяется |
| `TextBlock` | подпись, текст, рамка продолжения | перенос по словам, остаток уходит в рамку продолжения |
| `ImageBlock` | изображение или заглушка, режим `fit` | пропорции сохраняются |
| `PageSpec` | шаблон страницы, бегущий заголовок, блоки | страница 3 присутствует, только если есть заклинания |

- **More Information**: REQ-FUNC-009 … 014, REQ-FUNC-022, REQ-FUNC-023; Приложение C.

### DV-IFC-01-rest-api

- **ID**: DV-IFC-01-rest-api
- **Title**: REST-контракт экспорта
- **Viewpoint**: Интерфейс
- **Representation**:

  **Экспорт одного листа**

  `GET /api/v1/characters/{id}/export/pdf?lang=en|ru&mode=color|bw`

  | Элемент | Описание |
  |---------|----------|
  | Авторизация | `Authorization: Bearer <JWT>` (как в остальных маршрутах), владелец персонажа |
  | Параметры | `lang` (по умолчанию `en`; `ru` — после WP-16a), `mode` (по умолчанию `color`; `bw` — стретч WP-18) |
  | 200 | `Content-Type: application/pdf`; `Content-Disposition: attachment; filename="<ASCII-имя>.pdf"; filename*=UTF-8''<процент-кодированное имя>`; `Cache-Control: no-store`; тело начинается с `%PDF-` |
  | 401 | нет или недействителен токен (формат проекта) |
  | 404 | персонаж не найден или не принадлежит пользователю (как в проекте) |
  | 429/503 | сервер занят (нет разрешения на генерацию за 10 с) |
  | 500 | ошибка генерации, тело `ApiResponse` без частичного PDF |

  Кириллица в имени файла передаётся только через `filename*` (RFC 5987); в `filename` — ASCII-вариант с заменой не-ASCII символов на `_`.

  **Пакетный экспорт (WP-17)**: `POST /api/v1/characters/export/zip`, тело `{"ids":[...]}` (не более 20), ответ `application/zip` с PDF каждого персонажа пользователя; чужие и несуществующие `id` пропускаются, список пропусков возвращается в заголовке `X-Skipped-Ids`.
- **More Information**: REQ-INT-004, REQ-INT-009, REQ-FUNC-005, REQ-SEC-001, REQ-REL-003, REQ-FUNC-024, REQ-FUNC-025; DEC-004.

### DV-IFC-02-spi

- **ID**: DV-IFC-02-spi
- **Title**: SPI провайдера листа (точка расширения)
- **Viewpoint**: Интерфейс
- **Representation**:

  ```java
  public interface SheetDefinitionProvider {
    String systemId();                       // например, "dnd5e"
    SheetModel build(ExportRequest request); // не возвращает null, бросает SheetExportException
  }

  public record ExportRequest(String systemId, String sourceId, String requester,
                              Locale locale, ExportOptions options) {}

  public record PdfDocument(byte[] bytes, int pageCount, String suggestedFileName) {}
  ```

  Контракт: провайдер не выполняет рендер, не знает о PDF и шрифтах; пустые данные возвращает как пустые значения; доступ к данным и проверка владельца — на его стороне (в D&D провайдер вызывает `CharacterService.getCharacterSheet(id, username)`). Добавление системы: один класс `@Component`, реализующий интерфейс, плюс ресурс подписей.
- **More Information**: REQ-REUSE-003, REQ-MAINT-002; DEC-002.

### DV-IFC-03-ui

- **ID**: DV-IFC-03-ui
- **Title**: Интерфейс frontend
- **Viewpoint**: Интерфейс
- **Representation**: компонент `ExportPdfButton` использует общий `Button` и стили темы; размещается в правом верхнем углу `CharacterHeader` (рисунок `srs/diagrams/05-ui-header.png`). Свойства: `characterId`, `characterName`, `onBeforeExport` (сбрасывает отложенное сохранение полей). Функция `exportCharacterPdf(id, handle)` в `services/` выполняет запрос с обновлением токена (отдельный вариант `fetchWithAuth`, возвращающий `Blob`, так как существующий вариант разбирает JSON). Уведомления через `useToast`. Подписи кнопки: «Export to PDF» / «Generating…».
- **More Information**: REQ-INT-005 … 009, REQ-FUNC-021; DEC-003; макеты `srs/diagrams/05-ui-*.png`.

### DV-INT-01-interaction

- **ID**: DV-INT-01-interaction
- **Title**: Сценарий экспорта: успех, отмена, ошибка
- **Viewpoint**: Взаимодействие
- **Representation**: ![Последовательность экспорта](diagrams/seq-export.png)

  Ключевой порядок: диалог сохранения → сохранение правок → запрос → генерация → запись файла. Ошибка после выбора файла приводит к `abort()` записи и уведомлению; файл не создаётся.
- **More Information**: REQ-FUNC-003 … 008, REQ-FUNC-021, REQ-REL-001, REQ-REL-003; DEC-003, DEC-004.

### DV-ALG-01-layout

- **ID**: DV-ALG-01-layout
- **Title**: Раскладка: перенос текста, рамки, пагинация
- **Viewpoint**: Алгоритм
- **Representation**:

  ```
  layout(model, template):
    pages = []
    for spec in model.pages:
      page = newPage(spec.template, runningHeader)
      for block in spec.blocks:
        frame = page.frame(block.frameId)
        switch block.type:
          TEXT:  lines = wrap(normalize(block.text), frame.width, font)
                 while lines not empty:
                   fit = lines.take(frame.freeHeight / lineHeight)
                   frame.place(fit)
                   if lines not empty:
                     frame = nextFrame(block.overflowTo, page)   // доп. рамка или страница продолжения
          TABLE: for row in block.rows:
                   if row.height > frame.freeHeight: frame = nextFrame(...) + repeat header
                   frame.place(row)                               // строка не разрывается
          IMAGE: scale = min(frame.w/img.w, frame.h/img.h); frame.placeCentered(img, scale)
          FIELD/SCORE/CHECKLIST: frame.place(clipToFrame(text))   // длинные значения переносятся
      pages += page
    if |pages| > maxPages: throw SheetExportException(TOO_LARGE)
    number pages "N из M", set footer
    return pages

  wrap(text, width, font):
    for each paragraph:
      for each word:
        while width(word) > width: cut word at the largest prefix that fits (+ "-")   // слово длиннее строки
        if width(line + " " + word) <= width: append; else newline
    return lines
  ```

  Правила нормализации: `\r\n` → `\n`; табуляция → 4 пробела; управляющие символы и нулевой символ удаляются; символ вне шрифта → U+FFFD или «?» (REQ-FUNC-019); суррогатные пары обрабатываются как один символ. Сложность: O(n) по символам текста; ширина измеряется метриками шрифта (`TextMetrics`), поэтому раскладка тестируется без PDF.
- **More Information**: REQ-FUNC-014 … 020, REQ-REL-002, REQ-PERF-001, REQ-PERF-002; DEC-001. Пределы: не более 100 страниц (OQ-2).

### DV-ALG-02-derived-values

- **ID**: DV-ALG-02-derived-values
- **Title**: Производные значения листа D&D
- **Viewpoint**: Алгоритм
- **Representation**: вычисляются адаптером `DndSheetProvider`, чтобы ядро оставалось нейтральным.

| Значение | Правило |
|----------|---------|
| Модификатор характеристики | берётся из `AbilityScoresResponse` (`...Modifier`); если пусто, то `floor((значение − 10) / 2)` |
| Спасбросок | модификатор + (бонус мастерства, если характеристика в `savingThrowProficiencies`) |
| Навык | `SkillResponse.totalBonus`; отметки владения и экспертности из `proficient`, `expertise` |
| Пассивная мудрость (восприятие) | 10 + `totalBonus` навыка «Восприятие» |
| Сложность спасброска заклинаний | 8 + бонус мастерства + модификатор `spellcastingAbility` |
| Бонус атаки заклинанием | бонус мастерства + модификатор `spellcastingAbility` |
| Бонус атаки оружием | модификатор Силы (или Ловкости, если в `properties` есть «finesse» или оружие дальнобойное) + бонус мастерства (допущение: владение оружием предполагается, OQ-8) |
| Названия расы, предыстории, классов | разрешаются через существующие сервисы (с кэшем); если не найдено, ключ преобразуется в читаемый вид |
| «Класс заклинателя» | первый класс из `classesInfo` с заданной `spellcastingAbility` (допущение) |

- **More Information**: REQ-FUNC-009 … 013; Приложение C.

### DV-ALG-03-portrait

- **ID**: DV-ALG-03-portrait
- **Title**: Безопасная загрузка и масштабирование портрета
- **Viewpoint**: Алгоритм
- **Representation**:

  ```
  load(url):
    if url empty -> Placeholder
    u = parse(url); require scheme in {http, https}, port in {80, 443}
    for hop in 0..3:
      ips = resolve(u.host)                  // свой DnsResolver: проверка при подключении
      require all(ips) not in {loopback, anyLocal, siteLocal, linkLocal, multicast, 169.254/16, fc00::/7}
      resp = GET(u, timeout=5s, maxBytes=5MB, followRedirects=false)
      if resp.redirect: u = resolve(resp.location); continue
      break
    sniff magic bytes: PNG | JPEG else Placeholder
    read size via ImageReader; require w*h <= 25_000_000
    img = decode; if max(w,h) > 1500: downscale to 1500
    return img                              // любая ошибка -> Placeholder, экспорт продолжается
  ```

  Рендер: изображение вписывается в рамку «Внешность» 65 × 80 мм (`scale = min(frame.w/img.w, frame.h/img.h)`), по центру, без искажения.
- **More Information**: REQ-INT-010, REQ-SEC-002, REQ-SEC-003, REQ-REL-001, REQ-FUNC-020, REQ-PERF-003; DEC-005.

### DV-STA-01-button-states

- **ID**: DV-STA-01-button-states
- **Title**: Состояния кнопки экспорта
- **Viewpoint**: Динамика состояний
- **Representation**: состояния `idle → choosingLocation → preparing → generating → writing → idle`, ветви `UserCancelled`, `Error → failed → Dismiss → idle`. Повторный `ClickExport` в состояниях, кроме `idle`, игнорируется. Диаграмма: `srs/diagrams/04-export-flow.png`. При отсутствии `showSaveFilePicker` состояние `choosingLocation` пропускается, запись заменяется скачиванием `<a download>`.
- **More Information**: REQ-INT-007, REQ-FUNC-006, REQ-FUNC-007; DEC-003.

### DV-CON-01-concurrency

- **ID**: DV-CON-01-concurrency
- **Title**: Параллелизм и синхронизация
- **Viewpoint**: Параллелизм
- **Representation**: сервисы модуля без изменяемого общего состояния (stateless); на каждый запрос создаётся свой `PDDocument`, `LaidOutDocument` и список страниц. Общие неизменяемые данные: байты шрифтов (загружаются один раз). Ограничение одновременных генераций: `Semaphore(min(4, availableProcessors))`, ожидание разрешения до 10 с, затем ответ 503; общий таймаут генерации 30 с (прерывание и ответ 500/504). Загрузка портрета выполняется в потоке запроса с собственными таймаутами.
- **More Information**: REQ-PERF-004, REQ-REL-004; DEC-004.

### DV-PAT-01-patterns

- **ID**: DV-PAT-01-patterns
- **Title**: Применённые паттерны
- **Viewpoint**: Паттерны
- **Representation**:

| Паттерн | Где | Зачем |
|---------|-----|-------|
| Ports and Adapters | ядро ↔ `DndSheetProvider` | изоляция, расширение (REQ-REUSE-*) |
| Strategy | `SheetDefinitionProvider` по `systemId` | разные игровые системы |
| Facade | `SheetExportService` | единая точка входа для проекта |
| Registry | реестр провайдеров | выбор провайдера по идентификатору |
| Composite | `SheetModel` → `PageSpec` → `Block` | единообразная обработка блоков |
| Two-phase rendering | `LayoutEngine` → `PdfRenderer` | тестируемая раскладка независимо от библиотеки |
| Guard | `UrlGuard` | защита от SSRF |
| Null Object | `Placeholder` вместо портрета | устойчивость к сбоям |

- **More Information**: DEC-001, DEC-002, DEC-005.

### DV-DPL-01-deployment

- **ID**: DV-DPL-01-deployment
- **Title**: Развёртывание
- **Viewpoint**: Развёртывание
- **Representation**: ![Развёртывание](diagrams/deployment.png)

  Новых контейнеров и сервисов нет: модуль входит в `livedndlist.jar`. Изменения окружения: (1) `java.awt.headless=true` для backend (через `JAVA_TOOL_OPTIONS` или `ENTRYPOINT`); (2) `proxy_read_timeout 60s` в nginx для `/api/` (значение по умолчанию уже 60 с, проверить и зафиксировать); (3) шрифты в ресурсах JAR. Сборка: существующий Dockerfile без шагов.
- **More Information**: REQ-INT-001, REQ-INT-002, REQ-PORT-001; DEC-004, DEC-006.

### DV-RES-01-resources

- **ID**: DV-RES-01-resources
- **Title**: Использование ресурсов
- **Viewpoint**: Ресурсы
- **Representation**:

| Ресурс | Использование | Лимит / мера |
|--------|---------------|--------------|
| Память (куча) | документ PDF, изображения, текст | типичный лист до 32 МБ, предельный до 128 МБ; изображение уменьшается до 1500 px; PDF собирается в памяти |
| Потоки | поток запроса + ожидание семафора | не более `min(4, CPU)` одновременных генераций |
| CPU | раскладка и сжатие PDF | время: типичный до 3 с, предельный до 15 с (REQ-PERF-001, 002) |
| Сеть | загрузка портрета | таймаут 5 с, 5 МБ, 3 перенаправления |
| Диск | не используется (PDF не пишется на сервере) | временные файлы не создаются |
| Шрифты | Noto Sans Regular/Bold/Italic в ресурсах JAR | подмножество встраивается в PDF (REQ-PERF-003) |
| Размер файла | PDF | типичный до 2 МБ, предельный до 20 МБ |

- **More Information**: REQ-PERF-001 … 004, REQ-REL-004; DEC-004, DEC-005, DEC-006.

---

## 4. Решения {#4-decisions}

Полные протоколы по шаблону MADR находятся в каталоге `adr/`.

- **ID**: DEC-001-pdf-library
- **Title**: Использовать Apache PDFBox и собственный движок раскладки
- **Context**: Нужна библиотека PDF с кириллицей, переносом и пагинацией при лицензии, совместимой с UNLICENSE, и тестируемой раскладке.
- **Options**: PDFBox + собственный LayoutEngine; OpenPDF; OpenHTMLtoPDF.
- **Outcome**: «PDFBox + собственный LayoutEngine», потому что раскладка остаётся в нашем коде и проверяется без PDF, а библиотека заменяется переписыванием только пакета `render`.
- **More Information**: `adr/ADR-0001-pdf-library.md`; DV-LOG-01, DV-ALG-01.

- **ID**: DEC-002-module-isolation
- **Title**: Изолировать модуль через нейтральную модель и порт провайдера
- **Context**: Требуются изоляция от проекта и расширение на другие системы без изменения ядра.
- **Options**: порты и адаптеры в одном Maven-модуле; отдельный Maven-подмодуль; экспорт напрямую из DTO.
- **Outcome**: «Порты и адаптеры в одном Maven-модуле, пакет `dev.ushki.sheetexport`», потому что требования выполняются без изменения сборки, граница проверяется ArchUnit.
- **More Information**: `adr/ADR-0002-module-isolation.md`; DV-CMP-01, DV-STR-01, DV-DEP-01.

- **ID**: DEC-003-save-location
- **Title**: Сохранять через File System Access API с запасным скачиванием
- **Context**: Выбор каталога в момент экспорта возможен только средствами браузера.
- **Options**: `showSaveFilePicker` + `<a download>`; только `<a download>`; `showDirectoryPicker`; Electron/Tauri.
- **Outcome**: «`showSaveFilePicker` с запасным `<a download>`», потому что только он даёт выбор каталога и имени при каждом экспорте и работает везде.
- **More Information**: `adr/ADR-0003-save-location.md`; DV-INT-01, DV-STA-01.

- **ID**: DEC-004-sync-export
- **Title**: Выдавать PDF синхронным ответом с ограничением параллелизма
- **Context**: Времена генерации небольшие, файлы не хранятся; в `notes.txt` была заметка про async.
- **Options**: синхронный GET; асинхронная задача; потоковая отдача.
- **Outcome**: «Синхронный GET с ограничением параллелизма», потому что укладывается в REQ-PERF-* и не требует хранилища задач.
- **More Information**: `adr/ADR-0004-sync-export.md`; DV-CON-01, DV-RES-01.

- **ID**: DEC-005-portrait-loading
- **Title**: Загружать портрет через проверяющий клиент с защитой от SSRF
- **Context**: Адрес вводит пользователь, запрос выполняет сервер.
- **Options**: проверка в DNS-резолвере; белый список доменов; загрузка браузером; без портрета.
- **Outcome**: «Проверка адресов в DNS-резолвере собственного клиента», потому что закрывает подмену DNS и не требует списка доменов.
- **More Information**: `adr/ADR-0005-portrait-loading.md`; DV-ALG-03.

- **ID**: DEC-006-fonts
- **Title**: Встроить шрифты Noto Sans, в PDF — подмножества
- **Context**: Alpine без системных шрифтов; нужна кириллица и одинаковый результат на всех ОС.
- **Options**: Noto Sans в ресурсах; DejaVu Sans; системные шрифты; загрузка при запуске.
- **Outcome**: «Noto Sans в ресурсах», потому что покрывает кириллицу и латиницу при лицензии OFL.
- **More Information**: `adr/ADR-0006-fonts.md`; DV-RES-01.

---

## 5. Приложения {#5-appendixes}

### Приложение A. Приоритеты функциональности

Шкала (чем меньше число, тем выше приоритет):

| Priority | Meaning |
|----------|---------|
| 10 | Critical (must have): без этого релиз не имеет смысла |
| 20 | High: важно для качества результата |
| 30 | Medium: желательно в релизе |
| 40 | Low: делать, если остаётся время |
| 50 | Nice to have: откладывается в backlog |

| Work package | Feature | Requirements | Priority |
|--------------|---------|--------------|----------|
| WP-01 | PoC: PDF with Cyrillic, portrait, long text | REQ-POC-001 | 10 |
| WP-02 | Neutral SheetModel, provider port, registry, isolation test | REQ-REUSE-001 … 004 | 10 |
| WP-03 | Embedded fonts and glyph fallback | REQ-FUNC-018, 019 | 10 |
| WP-04a | Layout: text wrapping, frames, overflow | REQ-FUNC-014, 016 | 10 |
| WP-04b | Layout: tables, pagination, continuation pages | REQ-FUNC-015, 017 | 10 |
| WP-05 | REST endpoint and integration | REQ-INT-001 … 004, 009, REQ-SEC-001, REQ-REL-003 | 10 |
| WP-06 | D&D adapter and derived values | REQ-FUNC-009 … 013, 022 | 10 |
| WP-11 | Frontend: export button, states, toasts | REQ-INT-005 … 008, REQ-FUNC-001, 021 | 10 |
| WP-12 | Save dialog (File System Access API) and flush of pending edits | REQ-FUNC-003 … 008 | 10 |
| WP-07 | Page 1 template (main sheet) | REQ-FUNC-009 … 012, 015, 023 | 20 |
| WP-08 | Page 2 template | REQ-FUNC-014, 015, 020, 023 | 20 |
| WP-09 | Portrait: secure loader, scaling, placeholder | REQ-INT-010, REQ-SEC-002, 003, REQ-REL-001, REQ-FUNC-020 | 20 |
| WP-13a | Test fixtures and fuzz harness | REQ-REL-002 | 20 |
| WP-13b | Performance (JMX), cross-OS check, regression | REQ-PERF-001 … 004, REQ-PORT-001 | 20 |
| WP-16a | PDF language selector and label bundles | REQ-FUNC-024 | 20 |
| WP-16b | Russian label translations | REQ-FUNC-024 | 20 |
| WP-10 | Page 3: spells by level | REQ-FUNC-013 | 30 |
| WP-10b | Spell descriptions in PDF | REQ-FUNC-013 | 30 |
| WP-14 | Module docs and license list | REQ-MAINT-002, REQ-COMP-002 | 30 |
| WP-17 | Batch export of characters to ZIP | REQ-FUNC-025 | 30 |
| WP-18 | Black-and-white print mode | REQ-FUNC-026 | 40 |
| WP-15 | Observability: metrics and logs | REQ-OBS-001, 002 | 50 |

Оценки, итерации и стоимость — в `plan/plan.md`.

### Приложение B. Трассировка требований к проектным представлениям

| Требования SRS | Проектные представления | Решения |
|----------------|--------------------------|---------|
| REQ-INT-001 … 003 (запуск, тесты, совместимость) | DV-CMP-01, DV-DPL-01 | DEC-002 |
| REQ-INT-004, 009 (REST, авторизация) | DV-IFC-01, DV-INT-01 | DEC-004 |
| REQ-INT-005 … 008 (кнопка, состояния) | DV-IFC-03, DV-STA-01 | DEC-003 |
| REQ-INT-010, REQ-SEC-002, 003, REQ-REL-001 (портрет) | DV-ALG-03, DV-RES-01 | DEC-005 |
| REQ-FUNC-001 … 008 (запуск, выбор каталога, свежие данные) | DV-INT-01, DV-STA-01, DV-IFC-03 | DEC-003 |
| REQ-FUNC-009 … 013, 022, 023 (содержимое) | DV-INF-01, DV-ALG-02, Приложение C | DEC-002 |
| REQ-FUNC-014 … 020 (форматирование) | DV-ALG-01, DV-LOG-01, DV-RES-01 | DEC-001, DEC-006 |
| REQ-FUNC-021 (сообщения) | DV-IFC-03, DV-INT-01 | DEC-003 |
| REQ-PERF-001 … 004 | DV-CON-01, DV-RES-01, DV-ALG-01 | DEC-004 |
| REQ-SEC-001, 004, 005 | DV-IFC-01, DV-IFC-02, DV-INF-01 | DEC-002 |
| REQ-REL-002 … 004 | DV-ALG-01, DV-CON-01, DV-IFC-01 | DEC-001, DEC-004 |
| REQ-OBS-001, 002 | DV-RES-01 (метрики Micrometer, журнал) | — |
| REQ-COMP-001 … 004 | DV-DEP-01, DV-RES-01, Приложение D | DEC-001, DEC-006 |
| REQ-REUSE-001 … 004, REQ-MAINT-001, 002 | DV-STR-01, DV-DEP-01, DV-IFC-02, DV-PAT-01 | DEC-002 |
| REQ-PORT-001 | DV-RES-01, DV-DPL-01 | DEC-006 |

### Приложение C. Соответствие данных проекта блокам листа (адаптер D&D 5e)

| Данные проекта (`CharacterResponse`) | Блок листа | Страница |
|--------------------------------------|------------|----------|
| `name` | Имя персонажа; бегущий заголовок | 1, 2, 3 |
| `raceKey`, `backgroundKey`, `alignment`, `experiencePoints` | Раса, предыстория, мировоззрение, опыт | 1 |
| `classesInfo[]` (`classKey`, `level`), `totalLevel` | Класс и уровень | 1 |
| имя пользователя из запроса | Игрок | 1 |
| `abilityScores` | Характеристики (6 блоков) | 1 |
| `savingThrowProficiencies`, модификаторы, `proficiencyBonus` | Спасброски, бонус мастерства | 1 |
| `skills[]` | Навыки (18 строк) | 1 |
| `skills[PERCEPTION].totalBonus` | Пассивная мудрость (восприятие) | 1 |
| `armorClass`, `initiative`, `speed` | КБ, инициатива, скорость | 1 |
| `maxHitPoints`, `currentHitPoints`, `temporaryHitPoints` | Хиты | 1 |
| `hitDice`, `deathSaveSuccesses`, `deathSaveFailures` | Кости хитов, спасброски от смерти | 1 |
| `equipment[]` с заданным `damage` | Атаки (таблица) | 1 |
| `equipment[]` (все предметы) | Снаряжение (таблица) | 1 → продолжение |
| `copper`, `silver`, `electrum`, `gold`, `platinum` | Монеты | 1 |
| `personalityTraits`, `ideals`, `bonds`, `flaws` | Черты, идеалы, привязанности, слабости | 1 |
| `featuresAndTraits` | Умения и особенности; остаток в «Дополнительные умения» | 1, 2 |
| `portraitUrl` | Внешность (портрет) | 2 |
| `backstory` | Предыстория | 2 |
| `notes` | Союзники, организации и заметки | 2 |
| `spells[]` по `level` 0–9, `spellcastingAbility` | Заклинания по уровням, характеристика заклинаний | 3 |
| *(нет данных)* | Вдохновение, прочие владения и языки, возраст/рост/вес/глаза/кожа/волосы, символ фракции, сокровища, ячейки заклинаний | 1, 2, 3 — пустые рамки |

### Приложение D. Конфигурация модуля

| Свойство | По умолчанию | Назначение |
|----------|--------------|------------|
| `sheetexport.max-pages` | 100 | предел страниц (OQ-2) |
| `sheetexport.max-concurrent` | `min(4, CPU)` | число одновременных генераций |
| `sheetexport.queue-timeout` | 10s | ожидание разрешения |
| `sheetexport.generation-timeout` | 30s | общий таймаут генерации |
| `sheetexport.image.timeout` | 5s | таймаут загрузки портрета |
| `sheetexport.image.max-bytes` | 5MB | предел размера загрузки |
| `sheetexport.image.max-redirects` | 3 | перенаправления |
| `sheetexport.image.max-pixels` | 25000000 | предел размера в пикселях |
| `sheetexport.image.allow-private` | false | только для тестов |
| `sheetexport.default-locale` | en | язык подписей |

### Приложение E. Риски дизайна

| ID | Риск | Вероятность / влияние | Мера |
|----|------|-----------------------|------|
| R-1 | Раскладка трудоёмче оценки | средняя / высокое | прототип WP-01; триггер пересмотра ADR-0001 при превышении 28 ч |
| R-2 | `ImageIO` в Alpine JRE без GUI работает иначе | низкая / среднее | `java.awt.headless=true`, прогон прототипа в контейнере |
| R-3 | Нет `showSaveFilePicker` в части браузеров | высокая / среднее | запасное скачивание, OQ-1 |
| R-4 | Изменение DTO персонажа ломает адаптер | средняя / среднее | контрактный тест адаптера, `CharacterResponse` проверяется в CI |
| R-5 | Неверные допущения о бонусе атаки (OQ-8) | средняя / низкое | согласование с Заказчиком, правило вынесено в одну функцию |
| R-6 | Часть изменений Заказчика поздно в цикле | высокая / среднее | резерв 10% в каждой итерации, приоритеты 30–50 откладываются в backlog |
