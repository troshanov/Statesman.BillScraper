using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Statesman.BillParsers.Expressions;

namespace Statesman.BillParsers;

public static class Parser
{
    public static void ParseBill(string bill)
    {
        var matches = BillExpressions.ParagraphExpression.Matches(bill);
        var bills = new List<string>();
        foreach (Match match in matches)
        {
            bills.Add(match.Value);
        }
    }
}
