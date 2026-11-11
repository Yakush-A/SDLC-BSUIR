package SheetExportContext {
    doc /* Контекст модуля экспорта листа персонажа в PDF (проект Live D&D List) */

    part def Player;            // владелец листа персонажа
    part def GameMaster;        // получает PDF от игрока (вне системы)
    part def PortraitHost;      // внешний хост изображения портрета (portraitUrl)
    part def UserFileSystem;    // каталог, выбранный пользователем

    part def Frontend {
        part header : CharacterHeader;
        part exportButton : ExportButton;
    }
    part def Backend {
        part characterApi : CharacterController;
        part sheetAdapter : DndSheetAdapter;
        part exportModule : SheetExportModule;
    }
    part def LiveDndList {
        part frontend : Frontend;
        part backend  : Backend;
    }

    interface def HttpPdfExport {
        doc /* GET /api/v1/characters/{id}/export/pdf, JWT, ответ application/pdf */
    }
    interface def PortraitDownload {
        doc /* HTTP GET portraitUrl, таймаут и лимит размера */
    }

    part system : LiveDndList;
    part player : Player;
    part gm : GameMaster;
    part portraitHost : PortraitHost;
    part fs : UserFileSystem;

    connection pressButton connect player to system.frontend.exportButton;
    connection pdfRequest : HttpPdfExport
        connect system.frontend.exportButton to system.backend.characterApi;
    connection portraitLoad : PortraitDownload
        connect system.backend.exportModule to portraitHost;
    connection saveFile connect system.frontend to fs;
    connection shareFile connect player to gm;
}
