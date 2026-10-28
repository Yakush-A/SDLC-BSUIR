package SheetExportArchitecture {
    doc /* Изоляция модуля: проект зависит от модуля, модуль НЕ зависит от проекта */

    package Project {                       // существующий код liveDndList
        part def CharacterController;
        part def CharacterService;
        part def DndCharacter;              // JPA-сущность
        part def DndSheetAdapter {          // единственная точка связи (тонкий адаптер)
            doc /* DndCharacter -> SheetModel; реализует SheetDefinitionProvider */
        }
    }

    package SheetExport {                   // изолированный модуль
        part def SheetExportService {
            doc /* Фасад: export(systemId, sourceId) -> PdfDocument */
            port api;
        }
        interface def SheetDefinitionProvider {
            doc /* Порт расширения: по одному на игровую систему */
            attribute systemId : String;
            action build { in sourceId : String; out model : SheetModel; }
        }
        part def ProviderRegistry;
        part def SheetModel {
            doc /* Нейтральная модель: заголовок, секции, поля, таблицы, текстовые блоки, изображения */
            part sections : Section[0..*];
        }
        part def Section; part def FieldGroup; part def TableBlock; part def TextBlock; part def ImageBlock;
        part def LayoutEngine {
            doc /* Перенос текста, пагинация, масштабирование изображений */
        }
        part def PdfRenderer;
        part def FontRegistry;              // встроенные шрифты с кириллицей
        part def ImageLoader;               // безопасная загрузка портрета
        part def LabelBundle;               // подписи секций (en, ru)
    }

    part def OtherSystemAdapter;            // будущее: Pathfinder и т.д.

    dependency from Project::CharacterController to SheetExport::SheetExportService;
    dependency from Project::DndSheetAdapter to SheetExport::SheetDefinitionProvider;
    dependency from Project::DndSheetAdapter to Project::DndCharacter;
    dependency from OtherSystemAdapter to SheetExport::SheetDefinitionProvider;
    dependency from SheetExport::SheetExportService to SheetExport::ProviderRegistry;
    dependency from SheetExport::SheetExportService to SheetExport::LayoutEngine;
    dependency from SheetExport::LayoutEngine to SheetExport::PdfRenderer;
    dependency from SheetExport::LayoutEngine to SheetExport::FontRegistry;
    dependency from SheetExport::LayoutEngine to SheetExport::ImageLoader;
    dependency from SheetExport::LayoutEngine to SheetExport::LabelBundle;
}
