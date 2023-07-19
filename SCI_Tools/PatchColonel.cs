using McMaster.Extensions.CommandLineUtils;
using SCI_Lib.Resources;
using SCI_Lib.Resources.Scripts;
using SCI_Lib.Resources.Scripts.Elements;
using SCI_Lib.Resources.Scripts.Sections;
using SCI_Lib.Resources.Vocab;
using System;

namespace SCI_Tools
{
    // patch_colonel -d D:\Dos\GAMES\Laura_Bow_RUS
    // Русификация Colonel's Bequest
    [Command("patch_colonel", Description = "Export font to images")]
    class PatchColonel : PatchCommand
    {
        protected override void Patch()
        {
            PatchScr777();

            //FindRuDuplicate();
            //PrintSaids(25);

            CreateWord("для", WordClass.Association);

            CreateWord("кукольный", WordClass.QualifyingAdjective);
            CreateWord("инвалидный", WordClass.QualifyingAdjective);
            CreateWord("летучий", WordClass.QualifyingAdjective);
            CreateWord("книжный", WordClass.QualifyingAdjective);
            CreateWord("задний", WordClass.QualifyingAdjective);
            CreateWord("барный", WordClass.QualifyingAdjective);
            CreateWord("письменный", WordClass.QualifyingAdjective);
            CreateWord("душевой", WordClass.QualifyingAdjective);

            CreateWord("каретник", WordClass.Noun);
            CreateWord("каминный", WordClass.Noun);
            CreateWord("гардеробная", WordClass.Noun);
            CreateWord("орел,орла", WordClass.Noun);
            CreateWord("бокал,стакан", WordClass.Noun);
            CreateWord("кофеварка", WordClass.Noun);
            CreateWord("посуда", WordClass.Noun);
            CreateWord("рука", WordClass.Noun);
            CreateWord("шкафчик", WordClass.Noun);
            CreateWord("кисть", WordClass.Noun);
            CreateWord("напиток", WordClass.Noun);
            CreateWord("абажур", WordClass.Noun);
            CreateWord("бильярдная", WordClass.Noun);
            CreateWord("аптечка", WordClass.Noun);
            CreateWord("кастрюля,горшок,казан,котелок", WordClass.Noun);
            CreateWord("шахта", WordClass.Noun);
            CreateWord("стойло", WordClass.Noun);
            CreateWord("кабинет", WordClass.Noun);
            CreateWord("тростник", WordClass.Noun);
            CreateWord("ветер", WordClass.Noun);

            CreateWord("по", WordClass.Proposition);

            CreateWord("смаж", WordClass.ImperativeVerb);
            CreateWord("позвони", WordClass.ImperativeVerb);
            CreateWord("качайся", WordClass.ImperativeVerb);
            CreateWord("включи", WordClass.ImperativeVerb);
            CreateWord("заведи", WordClass.ImperativeVerb);
            CreateWord("поднимись", WordClass.ImperativeVerb);
            CreateWord("сними", WordClass.ImperativeVerb);
            CreateWord("закури,кури,покури", WordClass.ImperativeVerb);
            CreateWord("задвинь", WordClass.ImperativeVerb);
            CreateWord("выключи", WordClass.ImperativeVerb);
            CreateWord("запряги", WordClass.ImperativeVerb);
            CreateWord("спустись,слезь", WordClass.ImperativeVerb);
            CreateWord("вылей", WordClass.ImperativeVerb);

            Commit();

            ReplaceWord("desk", "стол");
            ReplaceWord("case", "ящик");
            ReplaceWord("drawer", "ящик");
            ReplaceWord("find", "ищи", 2, 9, 15, 31, 46, 52, 65, 74, 235);
            ReplaceWord("on", "на");
            ReplaceWord("carpet", "ковер");

            PatchSaid(0, 8, "/окурок");
            PatchSaid(0, 13, "/вентиль");

            PatchSaid(2, 0, "загляни<в/гробница");
            PatchSaid(2, 8, "загляни,(иди<по)/дорожка");
            PatchSaid(2, 12, "[<вниз,на][/земля]"); // посмотри вниз, осмотри землю, посмотри на землю

            PatchSaid(3, 11, "используй/масленка");
            PatchSaid(3, 12, "смаж/колокол");
            PatchSaid(3, 13, "полей/(колокол<масло),(масло<колокол)");
            PatchSaid(3, 17, "(кольцо,позвони)<используй<трость/(веревка,кольцо,колокол)");
            PatchSaid(3, 19, "потяни,позвони/трость<(кольцо,веревка,колокол)");
            PatchSaid(3, 20, "потяни,позвони/(кольцо,веревка,колокол)<трость");
            PatchSaid(3, 21, "(позвони,потяни)<используй<*/(веревка,кольцо,колокол)");
            PatchSaid(3, 23, "выдерни/(кольцо,веревка)/*");
            PatchSaid(3, 24, "позвони/колокол");
            PatchSaid(3, 28, "загляни/мышь<летучий");
            PatchSaid(3, 29, "лови,возьми/мышь<летучий");
            PatchSaid(3, 30, "загляни/лестница");
            PatchSaid(3, 31, "(заберись,поднимись,спустись)[/лестница<по,башня]");

            PatchSaid(4, 2, "/сад[<розовый]");

            PatchSaid(5, 3, "/дорожка");
            PatchSaid(5, 7, "возьми/роза,листва");
            PatchSaid(5, 14, "загляни/урна,постамент");

            PatchSaid(6, 17, "возьми/кресло[<качалка]");
            PatchSaid(6, 18, "качайся,садись[/кресло]");
            PatchSaid(6, 19, "загляни/кресло[<качалка]");
            PatchSaid(6, 27, "загляни<(под,за)/курятник");
            PatchSaid(6, 28, "загляни/курятник");

            PatchSaid(7, 1, "<под/мост");
            PatchSaid(7, 2, "<через/поток,вода");
            PatchSaid(7, 6, "перейди/мост");
            PatchSaid(7, 7, "иди[<через]/мост");
            PatchSaid(7, 8, "заберись[<на]/дерево,бревно");
            PatchSaid(7, 9, "прыгай[<через][/мост,поток]");
            PatchSaid(7, 10, "осмотри/мост");

            PatchSaid(9, 5, "/поле");
            PatchSaid(9, 22, "загляни<(в,сквозь)/окно,комната");

            PatchSaid(10, 5, "<вверх");

            PatchSaid(12, 5, "<вверх");
            PatchSaid(12, 7, "постучи<в/погреб<дверь");
            PatchSaid(12, 15, "возьми/фонарь");
            PatchSaid(12, 16, "осмотри/фонарь");
            PatchSaid(12, 20, "(войди,иди,залезь)<в/будка");
            AddSynonym(12, "лампа", "фонарь");

            PatchScr14();
            PatchSaid(14, 2, "/дорожка");
            PatchSaid(14, 3, "возьми,оторви,развяжи,загляни/веревка");
            PatchSaid(14, 8, "заберись<(в,на)/качели");
            PatchSaid(14, 17, "загляни<(в,сквозь)/окно,дом");
            PatchSaid(14, 23, "загляни/дом");

            PatchSaid(15, 4, "<вверх");

            PatchSaid(16, 3, "<вниз");
            PatchSaid(16, 4, "<вверх");
            PatchSaid(16, 6, "возьми/фонарь");
            PatchSaid(16, 7, "загляни/фонарь");
            PatchSaid(16, 23, "подними,(загляни<под)/коврик,(дверца<коврик)");
            RemoveSynonym(16, "лампа", "зажги");
            AddSynonym(16, "лампа", "фонарь");

            PatchSaid(17, 3, "<вверх");

            PatchSaid(18, 5, "/дорожка");

            PatchSaid(20, 2, "/дом");
            PatchSaid(20, 11, "загляни<(в,сквозь)/окно,дом");
            AddSynonym(20, "каретник", "дом");

            PatchSaid(21, 4, "/фонарь");
            PatchSaid(21, 6, "загляни/дом");
            RemoveSynonym(21, "walk", "дорожка");
            RemoveSynonym(21, "зажги", "лампа");
            AddSynonym(21, "лампа", "фонарь");

            PatchSaid(22, 3, "/дверца,фонарь");
            AddSynonym(22, "лампа", "фонарь");
            RemoveSynonym(22, "зажги", "лампа");

            PatchSaid(23, 1, "подними");
            PatchSaid(23, 13, "<в/колодец");
            PatchSaid(23, 16, "/дверь,погреб[/погреб]");
            PatchSaid(23, 17, "/дверца,фонарь");
            PatchSaid(23, 25, "отпусти/рукоять,ведро");
            PatchSaid(23, 26, "намотай,крути/рукоять,веревка");
            PatchSaid(23, 28, "отпусти/рукоять");
            PatchSaid(23, 33, "подними/ведро,веревка");
            RemoveSynonym(23, "зажги", "лампа");
            AddSynonym(23, "лампа", "фонарь");

            PatchSaid(24, 4, "/лестница");
            PatchSaid(24, 9, "загляни/адвокат");
            PatchSaid(24, 10, "поговори/адвокат");
            RemoveSynonym(24, "fellow", "адвокат");

            PatchSaid(25, 13, "(загляни,используй)/монокль/след"); // используй монокль на следе
            PatchSaid(25, 14, "загляни/след/монокль"); // осмотри след через монокль
            PatchSaid(25, 16, "загляни/скалка");
            PatchSaid(25, 17, "возьми/скалка");

            PatchSaid(27, 3, "загляни/дом<игровой");

            PatchSaid(28, 5, "/дверца,фонарь");
            PatchSaid(28, 7, "искупайся,нырни");
            AddSynonym(28, "орел", "птица");
            AddSynonym(28, "лампа", "фонарь");

            PatchSaid(31, 6, "/кинжал");
            PatchSaid(31, 8, "/шкаф<верх");
            PatchSaid(31, 12, "<над/камин");
            PatchSaid(31, 13, "/полка<каминный");
            PatchSaid(31, 31, "крути/глобус");
            PatchSaid(31, 34, "загляни/томагавк");
            PatchSaid(31, 35, "возьми/томагавк");
            PatchSaid(31, 38, "открой,(загляни<в)/стол,(ящик<стол)");
            PatchSaid(31, 45, "потрогай/коробка");
            PatchSaid(31, 47, "загляни/коробка");
            RemoveSynonyms(31, "почитай");
            AddSynonym(31, "орел", "птица");
            AddSynonym(31, "кабинет", "комната");

            PatchSaid(32, 1, "*/лифт");
            PatchSaid(32, 2, "лифт[<открой][/*]");
            PatchSaid(32, 4, "/ворота,(дверь[<!*])");
            PatchSaid(32, 5, "/шахта");
            PatchSaid(32, 6, "открой/ворота,(дверь[<!*])");
            PatchSaid(32, 7, "закрой/ворота,(дверь[<!*])");
            PatchSaid(32, 22, "/лошадь");
            PatchSaid(32, 24, "<за/картина");
            PatchSaid(32, 36, "подними,поставь/кресло");
            PatchSaid(32, 44, "загляни/шкаф<книжный");
            RemoveSynonym(32, "полка", "bookcase");

            PatchSaid(33, 10, "(потяни,нажми)[<на]/часы");
            PatchSaid(33, 13, "(нажми,потяни)[<на]/зеркало");
            PatchSaid(33, 18, "открой,(загляни<в)/часы");
            PatchSaid(33, 20, "загляни[<на]/часы,время");
            PatchSaid(33, 22, "загляни<в/зеркало");
            PatchSaid(33, 25, "загляни/абажур");
            RemoveSynonym(33, "closet", "шкаф");

            PatchSaid(34, 14, "загляни/(мужчина,картина)<глаз"); // осмотри глаза картины/мужчины
            PatchSaid(34, 15, "загляни/глаз[<мужчина]");  // осмотри мужчине глаза
            PatchSaid(34, 16, "<за/картина");
            PatchSaid(34, 30, "открой/кофеварка");
            PatchSaid(34, 31, "загляни<в/кофеварка");
            PatchSaid(34, 33, "возьми/кофеварка");
            PatchSaid(34, 34, "загляни/кофеварка");

            PatchSaid(35, 7, "/ковер");
            PatchSaid(35, 5, "/посуда");
            PatchSaid(35, 9, "мой/рука");
            PatchSaid(35, 10, "возьми/посуда");
            PatchSaid(35, 21, "возьми/кость");
            PatchSaid(35, 29, "загляни[<на]/кофейник");
            PatchSaid(35, 32, "спроси/кухарка/кость<о");
            PatchSaid(35, 33, "спроси/лил/кость<о");

            PatchSaid(36, 1, "спроси/*/игра<бильярд<в"); // спроси руди об игре в бильярд
            PatchSaid(36, 19, "играй,вставь,включи,заведи/виктрола,музыка,пластинка[/(виктрола,(проигрыватель<пластинка))]");
            PatchSaid(36, 21, "открой,(загляни<в)/виктрола,шкафчик,(проигрыватель<пластинка)");
            PatchSaid(36, 22, "загляни/виктрола,шкафчик,(проигрыватель<пластинка)");
            PatchSaid(36, 28, "загляни/шкаф<книжный");
            PatchSaid(36, 41, "загляни/рукоять,механизм[<заводной][/пианино]");
            PatchSaid(36, 42, "крути,заведи/пианино,рукоять,механизм[<заводной][/пианино]");

            PatchSaid(37, 0, "возьми,крути,вытащи/птиц");
            PatchSaid(37, 6, "открой,подними,(загляни<в)/шлем");
            PatchSaid(37, 7, "(нажми,двигай,крути,открой,(загляни<в))>");
            PatchSaid(37, 16, "/кисть<правый");
            PatchSaid(37, 17, "/кисть<левый");
            PatchSaid(37, 18, "/кисть");
            PatchSaid(37, 29, "/дверца<секретный");
            PatchSaid(37, 30, "/дверца<задний");
            PatchSaid(37, 35, "/птиц");
            PatchSaid(37, 38, "смаж>");
            PatchSaid(37, 39, "/шлем");
            PatchSaid(37, 41, "потяни>");
            PatchSaid(37, 44, "надень>");
            PatchSaid(37, 45, "(заберись,поднимись)[<по]/лестница");
            PatchSaid(37, 52, "(потяни,нажми)[<на]/часы");
            PatchSaid(37, 53, "(нажми,потяни)[<на]/зеркало");
            PatchSaid(37, 54, "крути/зеркало");
            PatchSaid(37, 56, "(спрячься,заберись)<в/доспех,костюм");
            PatchSaid(37, 58, "возьми/вентиль");
            AddSynonym(37, "орел", "птица");
            RemoveSynonym(37, "сундук", "тело");

            PatchSaid(38, 5, "садись<(на,за)/(стойка[<барный]),табурет");
            PatchSaid(38, 6, "загляни<(под,за)/монумент");
            PatchSaid(38, 7, "загляни<под,за,в/стойка[<барный]");
            PatchSaid(38, 8, "загляни/стойка[<барный]");
            PatchSaid(38, 20, "загляни/(герти,девочка)<глаз");
            PatchSaid(38, 24, "выпей,налей,возьми/графин,алкоголь,напиток");
            PatchSaid(38, 29, "загляни<в/стакан[/!*]");
            PatchSaid(38, 30, "возьми/стакан");
            PatchSaid(38, 31, "используй/монокль/стакан"); // используй монокль на стакане
            PatchSaid(38, 32, "загляни/стакан[<на]/монокль"); // осмотри стакан через монокль
            PatchSaid(38, 33, "загляни/отпечаток/стакан");
            PatchSaid(38, 34, "загляни/стакан/монокль>");
            PatchSaid(38, 35, "загляни/стакан");
            PatchSaid(38, 38, "/полли[<!*]"); // покорми попугая !(дай печенье попугаю)
            PatchSaid(38, 40, "/полли<*>"); // дай печенье попугаю
            PatchSaid(38, 45, "/*<печенье"); // дай печенье попугаю
            PatchSaid(38, 49, "поговори<с"); // поговори с полли

            PatchSaid(41, 5, "<в/кровать<шкаф");
            PatchSaid(41, 6, "/кровать<шкаф");
            PatchSaid(41, 9, "открой/шкаф");
            PatchSaid(41, 11, "сними,возьми/покрывало");
            PatchSaid(41, 12, "открой/кровать,дверца");
            PatchSaid(41, 13, "опусти,потяни/кровать");
            PatchSaid(41, 14, "закрой,вставь,нажми,подними/кровать[<вверх]");
            PatchSaid(41, 17, "загляни[<!*]/комод");
            PatchSaid(41, 22, "загляни/кровать");
            RemoveSynonyms(41, "коробка");

            PatchSaid(42, 3, "/полка[<каминный]");
            PatchSaid(42, 4, "<в/лифт");
            PatchSaid(42, 5, "/лифт[<!*]");
            PatchSaid(42, 6, "/лифт<дверь");
            PatchSaid(42, 8, "/кресло[<инвалидный]");
            PatchSaid(42, 14, "возьми,двигай,нажми/кресло[<инвалидный]");
            PatchSaid(42, 18, "садись<в/кресло[<инвалидный]");
            PatchSaid(42, 19, "открой,войди,иди/лифт[<дверь]");
            PatchSaid(42, 20, "кури/сигара");
            PatchSaid(42, 21, "садись<в/кресло[<инвалидный]");
            PatchSaid(42, 22, "нажми,двигай/(кресло[<инвалидный]),анри");
            PatchSaid(42, 29, "открой,(загляни<в)/стол<туалетный");
            PatchSaid(42, 30, "загляни/стол<туалетный");
            PatchSaid(42, 31, "ищи,(загляни<в)/пушка,ствол");
            PatchSaid(42, 34, "загляни/кресло[<инвалидный]");
            PatchSaid(42, 41, "(загляни<в),открой/комод");
            PatchSaid(42, 42, "загляни/комод");
            PatchSaid(42, 47, "загляни<в/шкаф");
            RemoveSynonym(42, "полка", "mantel");
            RemoveSynonym(42, "closet", "шкаф");
            RemoveSynonyms(42, "коробка");

            PatchSaid(43, 4, "/абажур");
            PatchSaid(43, 22, "загляни/дверца[<!*]");
            PatchSaid(43, 23, "ищи,открой,(загляни<в)/комод");
            PatchSaid(43, 24, "загляни/комод");
            RemoveSynonyms(43, "коробка");
            RemoveSynonym(43, "closet", "шкаф");
            RemoveSynonym(43, "лампа", "зажги");

            PatchSaid(44, 6, "/полка[<каминный]");
            PatchSaid(44, 8, "<в/стол<туалетный");
            PatchSaid(44, 9, "/стол<туалетный");
            PatchSaid(44, 10, "открой/стол<туалетный");
            PatchSaid(44, 14, "открой/(дверца[<!*]),желоб");
            PatchSaid(44, 17, "закрой/(дверца[<!*]),желоб");
            PatchSaid(44, 18, "загляни[<на]/комод");
            PatchSaid(44, 21, "загляни[<на,в]/отражение");
            PatchSaid(44, 28, "загляни/анри<глаз");
            PatchSaid(44, 31, "загляни/девочка<глаз");
            PatchSaid(44, 37, "поменяй,надень/одежда");
            PatchSaid(44, 38, "обыщи/одежда");
            PatchSaid(44, 43, "загляни/дверца[<!*]");
            RemoveSynonyms(44, "коробка");
            RemoveSynonym(44, "ворота", "желоб");
            AddSynonym(44, "шахта", "желоб");

            PatchSaid(45, 4, "двигай/шкаф,(домик[<кукольный])");
            PatchSaid(45, 7, "загляни[<на]/комод");
            PatchSaid(45, 11, "загляни<в/домик[<кукольный]");
            PatchSaid(45, 12, "загляни/домик[<кукольный]");
            PatchSaid(45, 17, "загляни<в/стакан");
            PatchSaid(45, 18, "загляни/стакан,напиток,платок");
            PatchSaid(45, 20, "/напиток,стакан");
            RemoveSynonyms(45, "коробка");
            RemoveSynonyms(45, "мешок");
            RemoveSynonyms(45, "nursery");
            RemoveSynonyms(45, "closet");

            PatchSaid(46, 12, "загляни[<!*]/комод");
            PatchSaid(46, 17, "(иди,заберись,перепрыгни)[<в]/окно");
            PatchSaid(46, 21, "загляни<вниз");
            PatchSaid(46, 22, "загляни<(в,из)/окно");
            RemoveSynonyms(46, "коробка");
            RemoveSynonyms(46, "мешок");

            PatchSaid(47, 4, "/дверь/ванная");
            PatchSaid(47, 6, "/птиц");
            PatchSaid(47, 7, "/абажур");
            PatchSaid(47, 8, "возьми,взломай,оторви,крути,вытащи/птиц");
            PatchSaid(47, 16, "открой,(загляни<в)/дверца,кладовка");
            PatchSaid(47, 17, "загляни/кладовка,(дверца<кладовка),(дверца[<!*])");
            RemoveSynonyms(47, "downstair");
            AddSynonym(47, "орел", "птица");

            PatchSaid(48, 6, "/ковер,земля,кровь,пятно");
            PatchSaid(48, 14, "загляни/девочка<глаз");
            PatchSaid(48, 20, "загляни[<на]/комод");
            PatchSaid(48, 21, "открой,(загляни<в)/стол[<письменный]");
            PatchSaid(48, 22, "загляни[<!*]/стол<письменный");
            PatchSaid(48, 25, "загляни/стол[<!*]>");
            PatchSaid(48, 26, "загляни[<!*]/стол<письменный");
            RemoveSynonyms(48, "чемодан");
            RemoveSynonyms(48, "коробка");

            PatchScr49();
            PatchSaid(49, 2, "/бильярдная");
            PatchSaid(49, 4, "/гостевой,(комната<гостевой)");

            PatchSaid(50, 4, "/гостевой,(комната<гостевой)");
            PatchSaid(50, 25, "загляни/след/монокль"); // осмотри след через монокль

            PatchSaid(52, 1, "/*<панель,дверца>");
            PatchSaid(52, 2, "открой/рукоять");
            PatchSaid(52, 4, "крути/рукоять");
            PatchSaid(52, 20, "возьми,вытащи/кирпич[/стена]");
            PatchSaid(52, 21, "разбей,сломай/стена[<кирпич]");
            PatchSaid(52, 22, "возьми,вытащи,двигай/тело[/куча[<из]]");
            PatchSaid(52, 30, "загляни/желоб");
            PatchSaid(52, 31, "(войди,иди,заберись)[<(в)]/желоб");
            PatchSaid(52, 32, "загляни/дверца,панель[<потайной]");
            PatchSaid(52, 34, "загляни/стена[<!*]");
            PatchSaid(52, 39, "загляни/ванная,прачечная");

            PatchSaid(53, 3, "/лестница");
            PatchSaid(53, 10, "возьми/напиток");
            PatchSaid(53, 17, "загляни[<на]/стол");
            PatchSaid(53, 20, "мой,включи/рука,вода");
            PatchSaid(53, 26, "потяни/цепочка,ручка");
            PatchSaid(53, 32, "спроси/дворецкий/печенье<о");
            AddSynonym(53, "шкаф", "кладовка");

            PatchSaid(56, 2, "загляни/лестница");

            PatchSaid(57, 1, "заблокируй/*");
            PatchSaid(57, 2, "опусти,задвинь/засов");
            PatchSaid(57, 6, "загляни,почитай/пластина,табличка");
            PatchSaid(57, 11, "/*<(ячейка,(дверца<ячейка))>"); // сломай ячейку тростью
            PatchSaid(57, 16, "(разбей,взломай,открой)/трость"); // сломай ячейку тростью
            PatchSaid(57, 17, "(разбей,взломай,открой)/<трость");
            PatchSaid(57, 19, "(разбей,взломай,открой)/кочерга"); // сломай ячейку кочергой
            PatchSaid(57, 20, "(разбей,взломай,открой)/<кочерга");
            PatchSaid(57, 22, "(разбей,взломай,открой)/лом"); // сломай ячейку ломом
            PatchSaid(57, 23, "(разбей,взломай,открой)/<лом"); // открой ломом ячейку
            PatchSaid(57, 33, "отопри,вытащи,двигай,подними");
            PatchSaid(57, 52, "закрой>");
            PatchSaid(57, 56, "погаси,выключи");

            PatchSaid(58, 1, "/*<доска>");
            PatchSaid(58, 3, "(разбей,взломай,открой,выдерни)/лом"); // сломай доску ломом
            PatchSaid(58, 4, "(разбей,взломай)<используй<лом");
            PatchSaid(58, 7, "возьми,открой,двигай,дерни,разбей,взломай");
            PatchSaid(58, 8, "(разбей,взломай,открой,выдерни)/кочерга"); // сломай доску кочергой
            PatchSaid(58, 9, "(разбей,взломай)<используй<кочерга");
            PatchSaid(58, 11, "(разбей,взломай,открой,выдерни)/трость"); // сломай доску тростью
            PatchSaid(58, 12, "(разбей,взломай)<используй<трость");
            PatchSaid(58, 34, "загляни/дверца<задний");
            PatchSaid(58, 42, "загляни<(в,сквозь)/окно");
            PatchSaid(58, 43, "садись[/банкетка]");

            PatchSaid(59, 13, "садись");
            PatchSaid(59, 14, "встань");
            PatchSaid(59, 17, "ложись");
            PatchSaid(59, 18, "ложись/кровать<на,в");
            PatchSaid(59, 34, "спроси[/кухарка]/морковка<о");
            PatchSaid(59, 37, "возьми/кастрюля,суп");
            PatchSaid(59, 38, "возьми/кастрюля");
            PatchSaid(59, 39, "загляни/кастрюля,суп");
            PatchSaid(59, 42, "загляни<(в,сквозь)/окно");

            PatchSaid(61, 7, "<(в,сквозь)/окно");
            PatchSaid(61, 27, "открой/коробка");
            PatchSaid(61, 28, "возьми,двигай/коробка");
            PatchSaid(61, 35, "загляни[<на]/карета");
            PatchSaid(61, 38, "загляни[<на]/стол");

            PatchScr63();
            PatchSaid(63, 2, "[<вокруг,на][/комната]");
            PatchSaid(63, 4, "/дом[<играй]");
            PatchSaid(63, 9, "напиши,нарисуй,очисть/доска");
            RemoveSynonyms(63, "blackboard");

            PatchSaid(65, 4, "/изгородь");
            PatchSaid(65, 8, "вставь/рукоятка/вал");
            PatchSaid(65, 9, "вставь/вентиль/вал");
            PatchSaid(65, 10, "вставь/рукоятка/вал");
            PatchSaid(65, 11, "выкрути/вентиль");
            PatchSaid(65, 12, "включи/фонтан");
            PatchSaid(65, 13, "включи/фонтан");
            PatchSaid(65, 14, "выключи/фонтан");
            PatchSaid(65, 15, "выключи/фонтан");
            PatchSaid(65, 16, "потрогай,опусти/фонтан,вода,рука[/фонтан,вода]");
            PatchSaid(65, 20, "войди,иди,заберись/вода,фонтан");
            PatchSaid(65, 24, "выкрути/вал");
            PatchSaid(65, 25, "возьми,вытащи/вентиль");
            PatchSaid(65, 27, "возьми/напиток");
            PatchSaid(65, 28, "смаж/вал");
            PatchSaid(65, 29, "взломай,сломай/вал");
            PatchSaid(65, 32, "загляни<в/люк");
            PatchSaid(65, 35, "возьми/монумент");
            PatchSaid(65, 36, "двигай,нажми,выдерни,выкрути/монумент");
            PatchSaid(65, 37, "загляни/монумент");
            AddSynonym(65, "ваза", "урна");

            PatchSaid(69, 3, "/стойло");
            PatchSaid(69, 4, "/земля");
            PatchSaid(69, 9, "возьми/трость<фонарь");
            PatchSaid(69, 10, "заберись/ворота,стойло");
            PatchSaid(69, 17, "загляни[<на]/корыто,вода");
            PatchSaid(69, 20, "войди,(иди<в)/стойло,ворота");
            PatchSaid(69, 21, "открой/стойло,ворота");
            PatchSaid(69, 22, "закрой/ворота,стойло");
            PatchSaid(69, 23, "загляни/ворота");
            PatchSaid(69, 24, "загляни/фонарь,лампа");
            PatchSaid(69, 25, "возьми/фонарь,лампа");
            PatchSaid(69, 27, "запряги/блейз");
            PatchSaid(69, 28, "/поводья/блейз");
            PatchSaid(69, 38, "оседлай,заберись");
            PatchSaid(69, 39, "веди,двигай,нажми");
            PatchSaid(69, 44, "погладь");
            PatchSaid(69, 54, "загляни<(в,сквозь)/окно");

            PatchSaid(73, 1, "используй/монокль/банка,этикетка");
            PatchSaid(73, 7, "почитай,загляни/этикетка,надпись");
            PatchSaid(73, 11, "/занавеска<душевой");
            PatchSaid(73, 17, "прими/душ");
            PatchSaid(73, 20, "включи/вода");
            PatchSaid(73, 21, "мой/рука");
            PatchSaid(73, 25, "садись,иди,используй/туалет");
            PatchSaid(73, 32, "открой,(загляни<в)/кресло,банкетка");
            PatchSaid(73, 34, "возьми/корзина[<мусор][/мусор]");
            PatchSaid(73, 35, "загляни<в/корзина[<мусор][/мусор]");
            PatchSaid(73, 36, "загляни/корзина[<мусор][/мусор]");
            PatchSaid(73, 37, "возьми/полотенце");
            PatchSaid(73, 38, "загляни/полотенце");
            PatchSaid(73, 42, "открой,(загляни<в)/шкаф[<бельевой]");
            PatchSaid(73, 43, "загляни<за/шкаф[<бельевой]");
            PatchSaid(73, 44, "загляни/шкаф[<бельевой]");
            AddSynonym(73, "ведро", "корзина");

            PatchScr74();
            PatchSaid(74, 2, "/(комната<туалетный),гардеробная");
            PatchSaid(74, 22, "открой,(загляни<в)/ящик,(стол<туалетный)");
            PatchSaid(74, 23, "загляни/стол<туалетный");
            PatchSaid(74, 25, "возьми/стакан");
            PatchSaid(74, 26, "загляни<в/стакан");
            PatchSaid(74, 27, "загляни/стакан");
            PatchSaid(74, 28, "загляни,используй/монокль/графин"); // используй монокль на графине
            PatchSaid(74, 29, "загляни/графин/монокль"); // осмотри графин через монокль
            PatchSaid(74, 30, "загляни/отпечаток");
            PatchSaid(74, 36, "выключи/пластинка,виктрола,(проигрыватель[<пластинка])");
            PatchSaid(74, 37, "играй,включи/виктрола,музыка,пластинка,(проигрыватель[<пластинка])");
            PatchSaid(74, 38, "возьми/виктрола,пластинка,(проигрыватель[<пластинка])");
            PatchSaid(74, 40, "загляни/виктрола,(проигрыватель[<пластинка])");
            PatchSaid(74, 44, "возьми/костюм");
            PatchSaid(74, 54, "ищи,войди,(иди[<в,на])/чердак");
            PatchSaid(74, 56, "загляни/дверка/чердак");

            PatchSaid(76, 20, "надень>");
            PatchSaid(76, 27, "загляни/сундук");

            PatchSaid(201, 0, "*/лифт");
            PatchSaid(201, 1, "лифт[<открой][/*]");
            PatchSaid(201, 3, "/ворота,(дверь[<!*])");
            PatchSaid(201, 4, "<в/лифт");
            PatchSaid(201, 5, "/лифт");
            PatchSaid(201, 8, "/шахта");
            PatchSaid(201, 9, "открой/ворота,лифт,(дверь[<!*])");
            PatchSaid(201, 10, "войди/лифт");
            PatchSaid(201, 11, "выйди/лифт");
            PatchSaid(201, 12, "закрой/ворота,лифт,(дверь[<!*])");
            PatchSaid(201, 14, "(нажми,выдерни,двигай)<вверх/рукоятка");
            PatchSaid(201, 16, "(нажми,выдерни,двигай)<вверх[/!*]");
            PatchSaid(201, 17, "(нажми,выдерни,двигай)<вниз/рукоятка");
            PatchSaid(201, 19, "(нажми,выдерни,двигай)<вниз[/!*]");
            PatchSaid(201, 20, "(используй,нажми,выдерни,двигай)/рукоятка");
            PatchSaid(201, 21, "заблокируй/лифт,рукоятка");
            PatchSaid(201, 22, "отопри/лифт,рукоятка");
            PatchSaid(201, 24, "вставь/ключ[/!*]"); // не срабатывало "вставь ключ в пушку" в 42

            PatchSaid(202, 1, "/напиток,стакан>");

            PatchSaid(204, 1, "пофлиртуй<с/дворецкий,мужчина");
            PatchSaid(204, 2, "спроси,возьми/напиток,стакан");
            PatchSaid(204, 6, "загляни/человек");
            PatchSaid(204, 7, "загляни/мужчина");

            PatchSaid(205, 3, "[<вокруг,на][/комната,болото]");
            PatchSaid(205, 10, "искупайся,нырни");
            PatchSaid(205, 11, "войди,иди,перепрыгни,нырни,(возьми<в)/болото,поток");
            PatchSaid(205, 12, "выпей[/болото]");

            PatchSaid(206, 0, "загляни[<в]/кабинет");
            PatchSaid(206, 1, "загляни[<в,на]/кухня");
            PatchSaid(206, 2, "загляни[<в]/бильярдная");
            PatchSaid(206, 3, "загляни[<в]/коридор");
            PatchSaid(206, 17, "включи,зажги/лампа");

            PatchSaid(207, 3, "/дорога");

            PatchSaid(208, 36, "/тростник");
            PatchSaid(208, 39, "/ветер");

            PatchSaid(210, 9, "<в/комод");
            PatchSaid(210, 18, "<в/окно");
            PatchSaid(210, 25, "/лампа");
            PatchSaid(210, 36, "/комод");

            PatchSaid(213, 4, "/полка[<каминный]");

            PatchSaid(217, 2, "/напиток,стакан");
            PatchSaid(217, 5, "возьми/напиток,стакан");

            PatchSaid(229, 2, "покорми,дай/(кость<бьюргард),(бьюргард<кость)");
            PatchSaid(229, 11, "толкни,двигай");
            PatchSaid(229, 17, "чисть,мой/миска,посуда");

            PatchSaid(230, 2, "поговори/человек");
            PatchSaid(230, 8, "загляни/напиток,стакан");
            PatchSaid(230, 9, "загляни/рука");
            PatchSaid(230, 12, "/напиток,стакан");

            PatchSaid(231, 5, "возьми,двигай,нажми/кресло<инвалидный");
            PatchSaid(231, 12, "дай,покажи/*<горничная"); // дай горничной блокнот
            PatchSaid(231, 13, "дай,покажи/горничная<*"); // дай блокнот горничной

            PatchSaid(232, 4, "возьми,кури>");
            PatchSaid(232, 11, "загляни,поговори/человек");

            PatchSaid(233, 1, "выключи,включи/виктрола,проигрыватель");
            PatchSaid(233, 6, "кури");
            PatchSaid(233, 8, "расскажи[/актриса]/(смерть<герти),герти<о");

            PatchSaid(236, 0, "возьми/скалка");
            PatchSaid(236, 1, "загляни/скалка");

            PatchSaid(237, 2, "загляни/кларенс");
            PatchSaid(237, 5, "поговори/кларенс");
            PatchSaid(237, 9, "загляни/люди");
            PatchSaid(237, 10, "загляни,поговори/человек,мужчина");
            PatchSaid(237, 11, "поговори/люди");
            PatchSaid(237, 12, "загляни/доктор,человек,мужчина,люди");
            RemoveSynonyms(237, "men");

            PatchSaid(238, 0, "/напиток,стакан>");

            PatchSaid(239, 1, "покорми,дай,брось/кость");
            PatchSaid(239, 2, "покорми,дай,брось/кость[/бьюргард,будка]");
            PatchSaid(239, 3, "дай/бьюргард<*");
            PatchSaid(239, 5, "возьми/кость");
            PatchSaid(239, 15, "зови");

            PatchSaid(240, 1, "возьми,двигай,нажми/кресло<инвалидный");

            PatchSaid(241, 2, "загляни/напиток,стакан");
            PatchSaid(241, 4, "возьми/напиток,стакан");
            PatchSaid(241, 5, "выпей/напиток");
            RemoveSynonyms(241, "выпей");
            AddSynonym(241, "алкоголь", "напиток");

            PatchSaid(242, 9, "погаси,выключи");

            PatchSaid(243, 29, "/вентиль");
            PatchSaid(243, 70, "//вентиль");

            PatchSaid(245, 5, "загляни,поговори/человек");

            PatchScr246();
            PatchSaid(246, 9, "пофлиртуй<с/дворецкий");
            PatchSaid(246, 18, "(загляни<в),открой/шкафчик");
            PatchSaid(246, 19, "загляни/шкафчик,зеркало");
            AddSynonym(246, "аптечка", "шкафчик");

            PatchScr259();
            PatchSaid(259, 4, "загляни/люди");
            PatchSaid(259, 5, "поговори/люди");
            PatchSaid(259, 15, "загляни/напиток,стакан,алкоголь");
            PatchSaid(259, 16, "возьми/напиток,стакан,алкоголь");

            PatchSaid(261, 3, "дай,покажи/горничная<*");

            RemoveSynonym(266, "кресло", "сели");

            PatchSaid(270, 0, "послушай/лил,анри");
            PatchSaid(270, 3, "покажи/*");

            PatchSaid(272, 0, "загляни/напиток,стакан");
            PatchSaid(272, 1, "возьми/напиток,стакан,алкоголь");

            PatchSaid(273, 4, "загляни/напиток,стакан");
            PatchSaid(273, 5, "возьми/напиток,стакан");

            PatchScr277();
            PatchSaid(277, 0, "корми,дай/бьюргард<*>"); // дай кость псу
            PatchSaid(277, 2, "/кость,(*<кость)"); // дай кость псу, дай псу кость
            PatchSaid(277, 3, "дай,покажи/бьюргард<*"); // покажи кость псу
            PatchSaid(277, 5, "брось/кость");
            PatchSaid(277, 17, "дай/руди<*");
            PatchSaid(277, 19, "покажи/руди<*");
            PatchSaid(277, 25, "пофлиртуй/руди");

            PatchSaid(282, 1, "загляни/доска");
            PatchSaid(282, 2, "возьми/доска");

            PatchSaid(378, 3, "/напиток,стакан");
            PatchSaid(378, 7, "возьми/напиток,стакан");

            PatchSaid(379, 1, "загляни/напиток,стакан");
            PatchSaid(379, 3, "возьми/напиток,стакан");
            PatchSaid(379, 4, "выпей/напиток");
            RemoveSynonyms(379, "выпей");
            AddSynonym(379, "алкоголь", "напиток");

            PatchSaid(381, 1, "пофлиртуй<с/дворецкий");

            PatchScr382();
            PatchSaid(382, 11, "пофлиртуй");

            PatchScr385();
            PatchSaid(385, 8, "пофлиртуй/руди");

            PatchSaid(407, 3, "высморкай[/нос]");
            PatchSaid(407, 26, "спроси/*<о");
            PatchSaid(407, 27, "спроси//*<о");
            PatchSaid(407, 38, "программировал<кто/игра"); // кто программировал игру

            PatchSaid(410, 0, "вылей<из/масло/масленка");
            PatchSaid(410, 1, "(загляни<в),открой/масленка");
            PatchSaid(410, 2, "смаж[/*]");
            PatchSaid(410, 3, "/монокль>"); //  смотри в монокль
            PatchSaid(410, 4, "/*<монокль>"); // осмотри моноклем кочергу
            PatchSaid(410, 5, "загляни/*/монокль>"); // осмотри * через монокль
            PatchSaid(410, 6, "загляни[<на]/монокль[<!*][/!*]>"); // осмотри монокль
            PatchSaid(410, 7, "надень,вставь"); // надень,вставь монокль
            PatchSaid(410, 9, "<в/монокль[<!*]"); // чтобы срабатывало "осмотри * в монокль", но срабатывало "посмотри в монокль"
            PatchSaid(410, 13, "вытащи,разряди/пуля[/пистолет<из]");
            PatchSaid(410, 19, "кури/окурок");
            PatchSaid(410, 21, "спроси/*<о");
            PatchSaid(410, 22, "спроси//*<о");
            PatchSaid(410, 38, "зажги,включи");
            PatchSaid(410, 39, "погаси,выключи");
            PatchSaid(410, 41, "вращай/*<страница");

            PatchScr413();
            PatchSaid(413, 46, "//вентиль");
            PatchSaid(413, 70, "/<вентиль");
            PatchSaid(413, 85, "ищи[/!*]");

            RemoveSynDubl();

            CBSkipButtonReplace(37, 'e');
            CBSkipButtonReplace(301);
            CBSkipButtonReplace(302);
            CBSkipButtonReplace(303);
            CBSkipButtonReplace(305);
            CBSkipButtonReplace(321);
            CBSkipButtonReplace(323);
            CBSkipButtonReplace(330);
            CBSkipButtonReplace(333);
            CBSkipButtonReplace(350);
            CBSkipButtonReplace(353);
            CBSkipButtonReplace(354);

            PatchScr781();

            Save();
        }

        private void PatchScr781()
        {
            var res = _translate.GetResource<ResScript>(781);
            var scr = res.GetScript() as Script;

            var op = scr.GetOperator(0x82b);
            if (op == null || op.Name != "pushi") throw new Exception("PatchScr781 failed");
            
            if ((byte)op.Arguments[0] != 56)
            {
                op.Arguments[0] = (byte)56;
                Changed(res);
            }
        }

        private void PatchScr14()
        {
            // Исправление действия "осмотри веревку"
            var res = _translate.GetResource<ResScript>(14);
            var scr = res.GetScript() as Script;

            var op = scr.GetOperator(0x4ad);
            if (op == null || op.Name != "bnt") throw new Exception("PatchScr14 failed");

            var r = op.Arguments[0] as CodeRef;
            if (r.TargetOffset == 0x4ce)
            {
                r.TargetOffset = 0x4bb;
                r.SetupByOffset();
                Changed(res);
            }
        }

        private void PatchScr49()
        {
            // Исправление действия "осмотри бтльярдную" и пр.
            var res = _translate.GetResource<ResScript>(49);
            var scr = res.GetScript() as Script;

            foreach (var code in scr.Get<CodeSection>())
            {
                foreach (var op in code.Operators)
                {
                    if (op.Type == 0x67 && op.Arguments[0] is byte arg && arg == 0x20)
                    {
                        Console.WriteLine($"Patch room 49 at {op.Address:x04}");
                        op.Arguments[0] = (byte)0x1c;
                        Changed(res);
                    }
                }
            }
        }

        private void PatchScr63()
        {
            // Исправление действия "осмотри игрушки" в 3 акте
            var res = _translate.GetResource<ResScript>(63);
            var scr = res.GetScript() as Script;

            var op = scr.GetOperator(0x3fb) ?? throw new Exception("PatchRoom63 failed");
            if (op.Name == "push0") return;
            if (op.Name != "push1") throw new Exception("PatchRoom63 failed");

            op.Type = 0x76;
            Changed(res);
        }

        private void PatchScr74()
        {
            // Исправление действия "выключи виктролу"
            var res = _translate.GetResource<ResScript>(74);
            var scr = res.GetScript() as Script;

            var op = scr.GetOperator(0x178a);
            if (op == null || op.Name != "ldi") throw new Exception("PatchRoom74 failed");
            var val = (byte)op.Arguments[0];
            if (val != 40)
            {
                op.Arguments[0] = (byte)40;
                Changed(res);
            }
        }

        private void PatchScr246()
        {
            // Меняем ветвление условий, чтобы срабатывал запрос "пофлиртуй с дворецким"
            var res = _translate.GetResource<ResScript>(246);
            var scr = res.GetScript() as Script;

            var op = scr.GetOperator(0x463);
            if (op == null || op.Name != "bnt") throw new Exception("PatchScr246 failed");

            var r = op.Arguments[0] as CodeRef;
            if (r.TargetOffset != 0x4de) return;

            r.TargetOffset = 0x4f6;
            r.SetupByOffset();

            op = scr.GetOperator(0x4ce);
            if (op == null || op.Name != "bnt") throw new Exception("PatchScr246 failed");

            r = op.Arguments[0] as CodeRef;
            if (r.TargetOffset != 0x529) return;

            r.TargetOffset = 0x4de;
            r.SetupByOffset();

            Changed(res);
        }

        private void PatchScr277()
        {
            // Меняем ветвление условий, чтобы срабатывал запрос "пофлиртуй с руди"
            var res = _translate.GetResource<ResScript>(277);
            var scr = res.GetScript() as Script;

            var op = scr.GetOperator(0x4e0);
            if (op == null || op.Name != "bnt") throw new Exception("PatchScr277 failed");

            var r = op.Arguments[0] as CodeRef;
            if (r.TargetOffset != 0x5d0) return;

            r.TargetOffset = 0x5e5;
            r.SetupByOffset();

            op = scr.GetOperator(0x563);
            if (op == null || op.Name != "bnt") throw new Exception("PatchScr277 failed");

            r = op.Arguments[0] as CodeRef;
            if (r.TargetOffset != 0x5e5) return;

            r.TargetOffset = 0x5d0;
            r.SetupByOffset();

            Changed(res);
        }

        private void PatchScr259()
        {
            // Исправление действия "слушай руди"
            var res = _translate.GetResource<ResScript>(259);
            var scr = res.GetScript() as Script;

            var op = scr.GetOperator(0xd30) ?? throw new Exception("PatchScr259 failed");
            if (op.Name == "pushi") return;
            if (op.Name != "push2") throw new Exception("PatchScr259 failed");

            // заменяем оператор на pushi 3 
            op.Type = 0x39;
            op.Arguments.Clear();
            op.Arguments.Add((byte)3);

            // меняем на callb 1 6
            var op2 = scr.GetOperator(0xd36) ?? throw new Exception("PatchScr259 failed");
            op2.Arguments[1] = (byte)6;

            // Вставляем оператор push1
            op.InjectNext(0x78);

            Changed(res);
        }

        private void PatchScr382()
        {
            // Меняем ветвление условий, чтобы срабатывал запрос "пофлиртуй с дживсом"
            var res = _translate.GetResource<ResScript>(382);
            var scr = res.GetScript() as Script;

            var op = scr.GetOperator(0x5d9);
            if (op == null || op.Name != "bnt") throw new Exception("PatchScr382 failed");

            var r = op.Arguments[0] as CodeRef;
            if (r.TargetOffset != 0x6b4) return;

            r.TargetOffset = 0x6c9;
            r.SetupByOffset();

            op = scr.GetOperator(0x6a4);
            if (op == null || op.Name != "bnt") throw new Exception("PatchScr382 failed");

            r = op.Arguments[0] as CodeRef;
            if (r.TargetOffset != 0x6c9) return;

            r.TargetOffset = 0x6b4;
            r.SetupByOffset();

            Changed(res);
        }

        private void PatchScr385()
        {
            // Меняем ветвление условий, чтобы срабатывал запрос "пофлиртуй с руди"
            var res = _translate.GetResource<ResScript>(385);
            var scr = res.GetScript() as Script;

            var op = scr.GetOperator(0xe4);
            if (op == null || op.Name != "bnt") throw new Exception("PatchScr385 failed");

            var r = op.Arguments[0] as CodeRef;
            if (r.TargetOffset != 0x17a) return;

            r.TargetOffset = 0x18f;
            r.SetupByOffset();

            op = scr.GetOperator(0x16a);
            if (op == null || op.Name != "bnt") throw new Exception("PatchScr385 failed");

            r = op.Arguments[0] as CodeRef;
            if (r.TargetOffset != 0x18f) throw new Exception("PatchScr385 failed");

            r.TargetOffset = 0x17a;
            r.SetupByOffset();

            Changed(res);
        }

        private void PatchScr413()
        {
            // дай <предмет> <кому-то>
            var res = _translate.GetResource<ResScript>(413);
            var scr = res.GetScript() as Script;
            var saidSection = scr.Get<SaidSection>()[0];
            for (int i = 57; i <= 80; i++)
            {
                var said = saidSection.Saids[i].ToString();
                if (said.StartsWith('<'))
                {
                    if (saidSection.Saids[i].Set("/" + said))
                        Changed(res);
                }
            }
        }

        private void PatchScr777()
        {
            // Смещаем капли крови
            var res = _translate.GetResource<ResScript>(777);
            var scr = res.GetScript();
            var drip1 = scr.Get<ClassSection>().Find(c => c.Name == "Drip1");
            if (((ShortElement)drip1.Selectors[4]).Value == 165) return;

            ((ShortElement)drip1.Selectors[4]).Value = 165; // Y
            ((ShortElement)drip1.Selectors[5]).Value = 41;  // X

            var drip3 = scr.Get<ClassSection>().Find(c => c.Name == "Drip3");
            ((ShortElement)drip3.Selectors[4]).Value = 164; // Y
            ((ShortElement)drip3.Selectors[5]).Value = 295; // X

            Changed(res);
        }

        private void CBSkipButtonReplace(ushort scriptNum, char sym = 's')
        {
            var srcKey = (byte)sym;
            // script.301   036a:35 73              ldi 73
            var res = _translate.GetResource<ResScript>(scriptNum);
            var scr = res.GetScript() as Script;
            bool found = false;
            foreach (var codeSec in scr.Get<CodeSection>())
            {
                var op = codeSec.Operators.Find(o => o.Name == "ldi" && (byte)o.Arguments[0] == srcKey);
                if (op != null)
                {
                    op.Arguments[0] = (byte)32;
                    found = true;
                }
            }
            if (found)
                Changed(res);
        }

    }
}
