package SheetExportUseCases {
    part def Player; part def Developer;
    part def SheetExportModuleSubject;

    use case def ExportSheetToPdf {
        subject module : SheetExportModuleSubject;
        actor player : Player;
        objective { doc /* Получить корректно отформатированный PDF листа персонажа */ }
        include use case chooseLocation : ChooseSaveLocation;
        include use case flushEdits : SaveLatestEdits;
        extend use case showError : ShowExportError;
    }
    use case def ChooseSaveLocation  { actor player : Player; }
    use case def SaveLatestEdits     { actor player : Player; }
    use case def ShowExportError     { actor player : Player; }
    use case def RegisterGameSystemExporter {
        actor developer : Developer;
        objective { doc /* Добавить экспорт листа другой игровой системы без изменения ядра модуля */ }
    }
}
