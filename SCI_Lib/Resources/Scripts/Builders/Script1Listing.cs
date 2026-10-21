using SCI_Lib.Resources.Scripts.Elements;
using SCI_Lib.Resources.Scripts1;
using System.Linq;

namespace SCI_Lib.Resources.Scripts.Builders
{
    internal static class Script1Listing
    {
        public static bool IsHeapExport(Script1 script, GlobalRef export) =>
            export != null && script.HeapPointers.OfType<BaseRef>()
                .Any(pointer => pointer.TargetOffset == export.Address);

        public static string ExportName(Script1 script, GlobalRef export)
        {
            if (export == null) return "$0000";
            if (IsHeapExport(script, export))
            {
                var obj = script.Objects.FirstOrDefault(o => o.Address == export.TargetOffset);
                return obj == null ? $"heap_{export.TargetOffset:x4}" : ObjectName(obj);
            }
            return $"localproc_{export.TargetOffset:x4}";
        }

        public static string ObjectName(Object1 obj) => obj.Name ?? obj.Label;

        public static string PropertyName(Object1 obj, Object1 super, int index)
        {
            var name = obj.Properties[index].Name;
            if (name == null && obj.IsInstance && super != null && index < super.Properties.Length)
                name = super.Properties[index].Name;
            return name ?? $"property_{index}";
        }

        public static string Value(ushort value, StringConst str) =>
            str == null ? $"${value:x4}" : $"string_{str.Address:x4} // \"{str.ValueSlashEsc}\"";
    }
}
