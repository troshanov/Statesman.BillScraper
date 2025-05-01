using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace Statesman.BillParsers.Expressions;

public static class BillExpressions
{
    private static readonly RegexOptions DefaultOptions =
        RegexOptions.Compiled |
        RegexOptions.IgnoreCase |
        RegexOptions.CultureInvariant;

    private static readonly RegexOptions SingleLineOptions =
        DefaultOptions | RegexOptions.Singleline;

    private static readonly RegexOptions MultilineOptions =
        DefaultOptions | RegexOptions.Multiline;

    public static readonly Regex ChapterTitleExpression = new(
        @"Глава\s*[а-я]+\.\s*([А-Я][А-Я\s]+?)(?=\n\s*Чл\.|\n\s*$)",
        SingleLineOptions);

    public static readonly Regex ArticleExpression = new(
        @"Чл\.\s*(\d+)([а-яa-z]?)\.[\s\S]*?(?=\s*Чл\.\s*\d+[а-яa-z]?\.|\s*Глава\s*[а-я]+\.|\s*Преходни\s+и\s+Заключителни\s+разпоредби|\s*$)",
        SingleLineOptions);

    public static readonly Regex ParagraphExpression = new(
        @"\((\d+)\)(?:\s+\(([^)]+)\))?\s+(.*?)(?:(?=\n\d+\.)|(?=\n\s*\(\d+\)|\s*$))((?:\n\d+\.[\s\S]*?(?=\n\s*\(\d+\)|\s*$))?)",
        SingleLineOptions);

    public static readonly Regex ParagraphContentExpression = new(
        @"\((\d+)\)\s+(.*?)(?=\n\d+\.|$)",
        SingleLineOptions);

    public static readonly Regex PointExpression = new(
        @"^(\d+)\.\s+([\s\S]*?)(?=\n\d+\.\s+|\s*$)",
        MultilineOptions);  // Multiline for ^ to match line beginnings

    public static readonly Regex TransitionalProvisionExpression = new(
        @"§\s*(\d+)\.[\s\S]*?(?=\s*§\s*\d+\.|\s*$)",
        SingleLineOptions);
}