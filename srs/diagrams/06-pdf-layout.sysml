package PdfLayout {
    doc /* Макет PDF по структуре листа D&D 5e. A4, книжная ориентация. Рендер: 06-pdf-layout.png */
    attribute pageWidthMm : Real = 210;
    attribute pageHeightMm : Real = 297;
    attribute marginMm : Real = 10;

    part def Page { part runningHeader; part footer { doc /* имя, «Страница N из M», дата */ } }

    part page1 : Page {
        doc /* Основной лист */
        part banner { part characterName; part classLevelBackgroundRaceAlignmentXpPlayer; }
        part abilityScores;                                   // 6 блоков
        part inspiration  { doc /* вручную */ }
        part proficiencyBonus; part savingThrows; part skills;
        part passiveWisdomPerception;
        part otherProficienciesAndLanguages { doc /* вручную */ }
        part armorInitiativeSpeed; part hitPoints; part temporaryHitPoints;
        part hitDice; part deathSaves;
        part attacksAndSpellcasting { doc /* таблица, заголовок повторяется */ }
        part equipmentAndCoins     { doc /* таблица + 5 видов монет */ }
        part personalityTraits; part ideals; part bonds; part flaws;
        part featuresAndTraits     { doc /* остаток уходит на страницу 2 */ }
    }
    part page2 : Page {
        doc /* Внешность и предыстория */
        part banner { part characterName; part physicalTraits { doc /* вручную */ } }
        part appearance   { doc /* портрет 65x80 мм, пропорции сохраняются */ }
        part backstory;
        part factionSymbol { doc /* вручную */ }
        part alliesOrganizationsNotes;
        part additionalFeaturesAndTraits;
        part treasure { doc /* вручную */ }
    }
    part page3 : Page {
        doc /* Заклинания; выводится, если они есть */
        part spellcastingClass; part spellcastingAbility; part spellSaveDc; part spellAttackBonus;
        part spellLevels : SpellLevelBlock[10];               // уровни 0..9
    }
    part def SpellLevelBlock { attribute level : Integer; part slots { doc /* вручную */ } part spells; }
    part continuationPage : Page {
        doc /* создаётся при переполнении любого блока; повторяет заголовок таблицы */
    }
}
