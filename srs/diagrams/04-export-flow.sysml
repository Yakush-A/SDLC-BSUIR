package SheetExportFlow {
    state def ExportButtonStates {
        entry action init;
        then state idle;
        state idle;
        state choosingLocation { doc /* диалог выбора каталога и имени файла */ }
        state preparing        { doc /* отправка несохранённых правок */ }
        state generating       { doc /* запрос к серверу, кнопка заблокирована */ }
        state writing          { doc /* запись файла в выбранный каталог */ }
        state failed           { doc /* сообщение об ошибке, лист не изменён */ }

        transition idle_to_choose first idle accept ClickExport then choosingLocation;
        transition cancel first choosingLocation accept UserCancelled then idle;
        transition choose_to_prepare first choosingLocation accept LocationChosen then preparing;
        transition prepare_to_gen first preparing accept EditsSaved then generating;
        transition gen_to_write first generating accept PdfReceived then writing;
        transition write_done first writing accept FileWritten then idle;
        transition any_fail first generating accept Error then failed;
        transition write_fail first writing accept Error then failed;
        transition failed_to_idle first failed accept Dismiss then idle;
    }
}
