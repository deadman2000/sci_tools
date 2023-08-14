using McMaster.Extensions.CommandLineUtils;
using SCI_Lib.Resources;
using SCI_Lib.Resources.Scripts;
using SCI_Lib.Resources.Vocab;
using System;

namespace SCI_Tools
{
    // Русификация Conquests of Camelot
    [Command("patch_camelot", Description = "Export font to images")]
    class PatchCamelot : PatchCommand
    {
        protected override void Patch()
        {
            CreateWord("пути", WordClass.Noun);
            CreateWord("бронза,бронзовые", WordClass.Noun);

            CreateWord("попроси", WordClass.ImperativeVerb);
            CreateWord("переоденься", WordClass.ImperativeVerb);

            CreateWord("у", WordClass.Proposition);

            CreateWord("письменный", WordClass.QualifyingAdjective);
            CreateWord("книжный", WordClass.QualifyingAdjective);
            CreateWord("драгоценный", WordClass.QualifyingAdjective);

            RemoveSaidDubl();
            PatchWhereIs();
            ReplaceWord("plate", "блюдо");

            Commit();

            PatchSaid(2, 1, "попроси/*<мерлин"); // попроси у мерлина *
            PatchSaid(2, 28, "/митра<герб");
            PatchSaid(2, 29, "//митра<герб");
            PatchSaid(2, 32, "/венера<герб");
            PatchSaid(2, 33, "//венера<герб");
            PatchSaid(2, 40, "/ковер/[!*]");
            PatchSaid(2, 48, "посмотри[<вокруг][/!*][/!*]");
            PatchSaid(2, 56, "/стол[<!*]");
            PatchSaid(2, 57, "//стол[<!*]");
            PatchSaid(2, 58, "/стол<письменный");
            PatchSaid(2, 59, "//стол<письменный");
            PatchSaid(2, 65, "/(полка,шкаф)<книжный,книга");
            PatchSaid(2, 66, "//(полка,шкаф)<книжный,книга");
            PatchSaid(2, 129, "//лес<жуткий");
            PatchSaid(2, 130, "//лес,бат");
            PatchSaid(2, 136, "/пути<счастливого");
            PatchSaid(2, 141, "возьми/магнетит");
            PatchSaid(2, 142, "возьми,потрогай,двигай>");
            PatchSaid(2, 145, "убей,режь,бей/маг");

            PatchScript3();

            PatchSaid(4, 3, "/щит,пендрагон");
            PatchSaid(4, 4, "//щит,пендрагон");
            PatchSaid(4, 61, "(спи,ложись[<на])[/кровать]");
            PatchSaid(4, 62, "иди<в/кровать");
            PatchSaid(4, 70, "возьми,надень,смени/наряд<придворный");
            PatchSaid(4, 71, "возьми,надень/броня,туника,щит,меч");
            PatchSaid(4, 73, "разденься,оденься,переоденься");
            PatchSaid(4, 77, "/стойка");

            PatchSaid(5, 24, "/сверток,мешок");
            PatchSaid(5, 25, "//сверток,мешок");
            PatchSaid(5, 28, "/ларец,богатство,самоцвет,чаша,блюдо,камень<драгоценный");
            PatchSaid(5, 29, "//ларец,богатство,самоцвет,чаша,блюдо,камень<драгоценный");
            PatchSaid(5, 30, "посмотри/казначей[<!*]");
            PatchSaid(5, 31, "спроси//казначей[<!*]");
            PatchSaid(5, 37, "/меч,(казначей<меч)");
            PatchSaid(5, 38, "//меч,(казначей<меч)");
            PatchSaid(5, 42, "гаси/факел,огонь");
            PatchSaid(5, 62, "[возьми]/олово,(монета<олово)");
            PatchSaid(5, 63, "[возьми]/бронза,(монета<бронза)");
            PatchSaid(5, 64, "[возьми]/золото,(монета<золото),серебро,(монета<серебро),медь,(монета<медь)>");

            PatchSaid(7, 64, "преклони,встань/колено");

            Save();
        }

        private void PatchScript3()
        {
            var res = _translate.GetResource<ResScript>(3);
            var scr = res.GetScript() as Script;
            var op = scr.GetOperator(0x1b2);
            if (op.Name != "pushi") throw new Exception();

            if ((ushort)op.Arguments[0] == 165) return;
            op.Arguments[0] = (ushort)165;
            Changed(res);
        }

        private void PatchWhereIs()
        {
            // (is<where) -> is
            foreach (var res in _translate.GetResources<ResScript>())
            {
                var scr = res.GetScript() as Script;
                if (scr.SaidSection == null) continue;
                foreach (var said in scr.SaidSection.Saids)
                {
                    if (said.Label.Contains("(где<where)"))
                    {
                        var newSaid = said.Label.Replace("(где<where)", "где");
                        Console.WriteLine($"{res.FileName}: {said} -> {newSaid}");
                        said.Set(newSaid);
                        Changed(res);
                    }
                }
            }
        }
    }
}
