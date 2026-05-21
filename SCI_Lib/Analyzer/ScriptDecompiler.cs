using SCI_Lib.Resources.Scripts;
using SCI_Lib.Resources.Scripts1;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace SCI_Lib.Analyzer;

/// <summary>
/// Класс для декомпиляции скриптов SCI в псевдокод, подобный C++
/// </summary>
public class ScriptDecompiler
{
    private readonly ScriptAnalyzer _analyzer;

    public List<ProcedureTree> Procedures => _analyzer.Procedures;
    public Dictionary<string, ProcedureTree> Exports => _analyzer.Exports;

    /// <summary>
    /// Конструктор для скриптов SCI0
    /// </summary>
    public ScriptDecompiler(Script script, string classFilter = null, string methodFilter = null)
    {
        _analyzer = new ScriptAnalyzer(script, classFilter, methodFilter);
    }

    /// <summary>
    /// Конструктор для скриптов SCI1
    /// </summary>
    public ScriptDecompiler(Script1 script, string classFilter = null, string methodFilter = null)
    {
        _analyzer = new ScriptAnalyzer(script, classFilter, methodFilter);
    }

    /// <summary>
    /// Оптимизировать дерево процедур
    /// </summary>
    public void Optimize()
    {
        _analyzer.Optimize();
    }

    /// <summary>
    /// Декомпилировать все процедуры и вернуть результат в виде строки
    /// </summary>
    public string Decompile()
    {
        var sb = new StringBuilder();

        // Декомпилируем экспортированные функции
        foreach (var export in Exports.OrderBy(e => e.Key))
        {
            DecompileProcedure(export.Value, sb);
            sb.AppendLine();
        }

        // Декомпилируем процедуры
        foreach (var proc in Procedures.OrderBy(p => p.Name))
        {
            DecompileProcedure(proc, sb);
            sb.AppendLine();
        }

        return sb.ToString();
    }

    /// <summary>
    /// Декомпилировать конкретную процедуру
    /// </summary>
    public string DecompileProcedure(ProcedureTree proc)
    {
        var sb = new StringBuilder();
        DecompileProcedure(proc, sb);
        return sb.ToString();
    }

    /// <summary>
    /// Декомпилировать процедуру в StringBuilder
    /// </summary>
    private void DecompileProcedure(ProcedureTree proc, StringBuilder sb)
    {
        // Оптимизируем процедуру перед декомпиляцией
        var opt = proc.Optimize();

        // Заголовок функции
        sb.AppendLine($"// Procedure: {proc}");
        sb.AppendLine($"// Address: {proc.Begin.Address:x4}");

        // Объявление функции
        sb.AppendLine($"short {proc.Define}");
        sb.AppendLine("{");

        // Тело функции
        if (opt.Node != null)
        {
            DecompileNode(opt.Node, sb, 1);
        }

        sb.AppendLine("}");
    }

    /// <summary>
    /// Рекурсивная декомпиляция узла дерева
    /// </summary>
    private void DecompileNode(CodeNode node, StringBuilder sb, int indentLevel)
    {
        if (node == null) return;

        string indent = new string(' ', indentLevel * 4);

        // Выводим выражения
        if (node.Expressions != null)
        {
            foreach (var expr in node.Expressions)
            {
                if (expr != null)
                    sb.AppendLine($"{indent}{FormatExpression(expr)};");
            }
        }

        // Если у этого узла есть ReturnValue — выводим return здесь
        if (node.ReturnValue != null)
        {
            Debug.Assert(node.Condition == null);
            sb.AppendLine($"{indent}return {FormatExpression(node.ReturnValue)};");
            return;
        }

        // Обработка условия
        if (node.Condition != null)
        {
            DecompileCondition(node, sb, indentLevel, indent);
        }
        else
        {
            if (node.NextA != null)
                DecompileNode(node.NextA, sb, indentLevel);
        }
    }

    /// <summary>
    /// Декомпиляция условного блока
    /// </summary>
    private void DecompileCondition(CodeNode node, StringBuilder sb, int indentLevel, string indent)
    {
        string conditionStr = FormatExpression(node.Condition);

        // Проверяем, является ли это if-else конструкцией
        bool hasTrueBranch = node.NextA != null && !IsEmptyBlock(node.NextA);
        bool hasFalseBranch = node.NextB != null && !IsEmptyBlock(node.NextB);

        if (hasTrueBranch || hasFalseBranch)
        {
            sb.AppendLine($"{indent}if ({conditionStr})");
            sb.AppendLine($"{indent}{{");

            if (hasTrueBranch)
            {
                DecompileNode(node.NextA, sb, indentLevel + 1);
            }

            sb.AppendLine($"{indent}}}");

            if (hasFalseBranch)
            {
                sb.AppendLine($"{indent}else");
                sb.AppendLine($"{indent}{{");
                DecompileNode(node.NextB, sb, indentLevel + 1);
                sb.AppendLine($"{indent}}}");
            }
        }
        else
        {
            // Простое условие без тела (может быть оптимизировано)
            sb.AppendLine($"{indent}// Condition: {conditionStr}");

            if (node.NextA != null)
                DecompileNode(node.NextA, sb, indentLevel);
            else if (node.NextB != null)
                DecompileNode(node.NextB, sb, indentLevel);
        }
    }

    /// <summary>
    /// Проверка, является ли блок пустым
    /// </summary>
    private bool IsEmptyBlock(CodeNode node)
    {
        if (node == null) return true;
        if (node.Expressions != null && node.Expressions.Count > 0) return false;
        if (node.Condition != null) return false;
        return true;
    }

    /// <summary>
    /// Форматирование выражения в строку
    /// </summary>
    private string FormatExpression(Expr expr)
    {
        if (expr == null) return "null";

        return expr switch
        {
            SetExpr set => $"{FormatExpression(set.A)} = {FormatExpression(set.B)}",
            Math1Expr m1 => $"{m1.Op}{FormatExpression(m1.Expression)}",
            Math2Expr m2 => $"{FormatExpression(m2.A)} {m2.Op} {FormatExpression(m2.B)}",
            CallExpr call => FormatCallExpr(call),
            ParamExpr p => p.Name,
            ConstExpr c => c.Value.ToString(),
            ClassExpr ce => ce.Name,
            ArrayExpr arr => $"{FormatExpression(arr.Array)}[{FormatExpression(arr.Index)}]",
            RefExpr r => r.Label,
            LinkExpr l => l.Label,
            _ => expr.Label
        };
    }

    /// <summary>
    /// Форматирование вызова функции
    /// </summary>
    private string FormatCallExpr(CallExpr call)
    {
        string target = call.Target != null
            ? (call.Target is ClassExpr ce ? $"{ce.Name}::" : $"{FormatExpression(call.Target)}.")
            : "";

        string args = call.Args != null && call.Args.Count > 0
            ? string.Join(", ", call.Args.Select(a => FormatExpression(a)))
            : "";

        return $"{target}{call.Method}({args})";
    }

    /// <summary>
    /// Получить список используемых скриптов
    /// </summary>
    public IEnumerable<ushort> GetUsedScripts()
    {
        var usedScripts = new HashSet<ushort>();
        foreach (var proc in Procedures)
        {
            // Получаем использованные скрипты из процедуры
            // Это можно расширить для более детального анализа
        }
        return usedScripts;
    }

    /// <summary>
    /// Декомпилировать конкретный класс (для SCI0)
    /// </summary>
    public string DecompileClass(string className)
    {
        var sb = new StringBuilder();
        var classProcs = Procedures.Where(p => p.ClassName == className).ToList();

        sb.AppendLine($"// Class: {className}");
        sb.AppendLine();

        foreach (var proc in classProcs)
        {
            DecompileProcedure(proc, sb);
            sb.AppendLine();
        }

        return sb.ToString();
    }

    /// <summary>
    /// Декомпилировать конкретный метод класса
    /// </summary>
    public string DecompileMethod(string className, string methodName)
    {
        var proc = Procedures.FirstOrDefault(p => p.ClassName == className && p.Name == methodName);
        if (proc != null)
            return DecompileProcedure(proc);
        return $"// Method {className}::{methodName} not found";
    }
}