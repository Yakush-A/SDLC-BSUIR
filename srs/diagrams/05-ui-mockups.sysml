package SheetExportUiMockups {
    doc /* GUI-прототип: изменения экрана «Лист персонажа». Рендер: 05-*.png, 06-*.png */

    part def Button { attribute label : String; attribute state : ButtonState; }
    enum def ButtonState { idle; busy; disabled; }
    part def Toast  { attribute kind : String; attribute text : String; }
    part def SaveDialog {
        attribute suggestedName : String;   // <имя персонажа>_<yyyy-MM-dd>.pdf
        attribute fileType : String = "application/pdf";
    }

    part characterHeader {
        part characterName;
        part raceField; part classLevelField; part backgroundField;
        part alignmentField; part experienceField; part applyLevelButton;
        part exportButton : Button {
            :>> label = "Export to PDF";
            doc /* правый верхний угол заголовка, иконка PDF слева от подписи */
        }
    }
    part exportBusy   : Button { :>> label = "Generating…"; :>> state = ButtonState::busy; }
    part errorToast   : Toast  { :>> kind = "error"; :>> text = "Не удалось создать PDF. Повторите попытку."; }
    part successToast : Toast  { :>> kind = "success"; :>> text = "PDF сохранён"; }
    part saveDialog   : SaveDialog { :>> suggestedName = "rfgergqg_2026-10-28.pdf"; }
}
