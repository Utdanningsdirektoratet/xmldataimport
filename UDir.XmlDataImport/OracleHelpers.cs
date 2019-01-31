using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace UDir.XmlDataImport
{
    public static class OracleHelpers
    {
        public static List<string> Blockify(List<string> sqlPackInserts, Settings settings)
        {
            if (settings.DbVendor.ToLower() == "oracle")
            {
                var blocks = new List<string>();
                sqlPackInserts.ForEach(x =>
                {
                    blocks.Add(FormatAnonBlock(x));
                });

                return blocks;
            }

            return sqlPackInserts;
        }

        static string FormatAnonBlock(string userData)
        {
            string[] blocks = userData.Split('|');            
            
            var sb = new StringBuilder();

            if (blocks.Length == 2)
            {
                sb.Append(blocks[0]);
            }
            
            sb.Append("BEGIN ");
            string[] statements = blocks.Length == 1 ? blocks[0].Split(';') : blocks[1].Split(';');
            foreach (string s in statements.Where(x => IsNotEmptyText(x) && !x.ToLower().StartsWith("declare")))
            {
                if (s.Length > 0)
                {
                    sb.AppendFormat(" {0}; ", s);
                }
            }
            sb.Append(" END ; ");
            return sb.ToString();
        }

        public static string DelimitVariableBlock(string variableDeclarations, string joinedQuery, Settings settings)
        {
            if (IsNotEmptyText(variableDeclarations) && settings.DbVendor.ToLower() == "oracle" && !joinedQuery.ToLower().StartsWith("declare"))
            {
                return variableDeclarations.Trim() + "|" + joinedQuery.Trim();
            }

            return variableDeclarations.Trim() + "\n" + joinedQuery.Trim();
        }

        static bool IsNotEmptyText(string statement)
        {
            var expr = new Regex("[A-Za-z]");
            return expr.IsMatch(statement);
        }
    }
}
