﻿using SCI_Lib.Resources;
using SCI_Lib.Resources.Scripts;
using SCI_Lib.Resources.Scripts.Sections;
using System;
using System.Collections.Generic;

namespace SCI_Lib.Utils
{
    // Colonel's Bequest
    public class SaidExtract
    {
        static readonly string[] themes = new string[] { "Сели", "Кларенс", "полковник", "Этель", "Фифи", "Герти", "Глория", "Дживс", "Лилиан", "Рудольф", "Уилбур", "Бьюргард", "Блейз", "саквояж", "Библия", "кость", "трость", "окурок", "дневник", "маховичок", "ожерелье", "магия", "сокровище", "рычаг", "попугай", "платок", "Сара", "призрак", "плантация", "Крутон" };
        static readonly int[] themeFlags = new int[] { 2, 64, 512, 8, 16, 1, 4, 1024, 32, 256, 128, 8256, 8320, 8200, 10240, 8704, 8196, 9216, 8224, 8208, 12288, 16384, 16385, 8194, 16388, 16392, 16400, 16416, 16448, 16512 };
        private readonly SCIPackage _package;

        public SaidExtract(SCIPackage package)
        {
            _package = package;
        }

        public string[] Process(ushort resNum)
        {
            var resTxt = _package.GetResource<ResText>(resNum);
            var strings = resTxt.GetStrings();
            var result = new string[strings.Length];

            var res = _package.GetResource<ResScript>(resNum);
            var scr = res.GetScript() as Script;
            var vars = scr.Get<LocalVariablesSection>()[0];

            var txtBegin = 1;
            var txtCount = 0;
            while (true)
            {
                txtCount++;
                var i = txtBegin + txtCount * 2;
                if (vars[i] != resNum || vars[i + 1] >= strings.Length)
                    break;
            }

            var refBegin = txtBegin + txtCount * 2;
            var refsCount = 0;
            for (int i = refBegin; i < vars.Count; i++)
            {
                if (vars[i] == ushort.MaxValue)
                {
                    refsCount = i - refBegin + 1;
                    break;
                }
            }
            if (refsCount == 0) throw new Exception();

            var persionId = vars[txtCount * 2 + refsCount - 1];
            string person = GetThemes(persionId)[0];

            var actions = new string[] { $"спроси {person} о", $"расскажи {person} о", $"покажи {person}", $"дай {person}", "осмотри", "возьми", "убей", "поцелуй", "обними", "флиртуй с" };
            var actionInd = new ushort[actions.Length];

            var actionsBegin = refBegin + refsCount;
            for (int i = 0; i < actions.Length; i++)
                actionInd[i] = vars[actionsBegin + i];

            for (int txtInd = 0; txtInd < strings.Length; txtInd++)
            {
                Console.WriteLine($"{txtInd}: {strings[txtInd]}");
                var ind = -1;
                for (int i = 0; i < txtCount; i++)
                {
                    var val = vars.Vars[txtBegin + i * 2 + 1];
                    if (val is ushort s)
                    {
                        if (s == txtInd)
                        {
                            ind = i;
                            //Console.WriteLine($"ind = {ind}");
                            break;
                        }
                    }
                    else
                        Console.WriteLine(val.GetType());
                }

                if (ind == -1)
                {
                    Console.WriteLine($"Text {txtInd} not found");
                    continue;
                }

                var act = 0;
                for (int i = 0; i < actions.Length; i++)
                {
                    if (actionInd[i] > ind)
                        break;
                    act = i;
                }

                var v = vars[refBegin + ind];
                if (v == ushort.MaxValue)
                    break;
                Console.WriteLine($"{v} ({v:x3})");
                var themes = GetThemes(v);
                if (themes.Count == 0)
                    result[txtInd] = actions[act] + " *";
                else
                    result[txtInd] = actions[act] + " " + string.Join(" и ", themes);
                Console.WriteLine(result[txtInd]);
            }

            return result;
        }

        static List<string> GetThemes(ushort val)
        {
            for (int i = 0; i < themes.Length; i++)
            {
                if (val == themeFlags[i])
                {
                    return new List<string>() { themes[i] };
                }
            }

            List<string> list = new();
            for (int i = 0; i < themes.Length; i++)
            {
                var mask = themeFlags[i];
                if ((val & mask) == mask)
                {
                    list.Add(themes[i]);
                }
            }
            return list;
        }
    }
}