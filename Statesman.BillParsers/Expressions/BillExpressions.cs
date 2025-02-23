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

    public static readonly Regex LawExpression = new Regex(
               @"^ЗАКОН\s+ЗА\s+[\s\S]+?(?=\n+§|\n+\s*(?:заключителна|вносители|допълнителна))",
                      DefaultOptions);

    public static readonly Regex ParagraphExpression = new(
        @"^§[\s\S]+?(?=\n+§|\n+\s*(?:заключителна|вносители|допълнителна))",
        DefaultOptions);

    public static readonly Regex PointExpression = new(
        @"^\d+\.\s*[\s\S]+?(?=\n\d+\.|\Z)",
        DefaultOptions);
}
