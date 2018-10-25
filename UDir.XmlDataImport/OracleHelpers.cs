using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace UDir.XmlDataImport
{
    public static class OracleHelpers
    {
        public static List<string> Blockify(List<string> sqlPackInserts)
        {
            if (Settings.DbVendor.ToLower() == "oracle")
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
            
            StringBuilder sb = new StringBuilder();

            if (blocks.Length == 2)
            {
                sb.Append(blocks[0]);
            }
            
            sb.Append("BEGIN ");
            string[] statements = blocks.Length == 1 ? blocks[0].Split(';') : blocks[1].Split(';');
            foreach (string s in statements.Where(x => !x.ToLower().StartsWith("declare")))
            {
                if (s.Length > 0)
                {
                    sb.AppendFormat(" {0}; ", s);
                }
            }
            sb.Append(" END ; ");
            return sb.ToString();
        }

        public static string DelimitVariableBlock(string variableDeclarations, string joinedQuery)
        {
            if (Settings.DbVendor.ToLower() == "oracle" && !joinedQuery.ToLower().StartsWith("declare"))
            {
                return variableDeclarations + "|" + joinedQuery;
            }

            return variableDeclarations + "\n" + joinedQuery;
        }
      
    }
}
