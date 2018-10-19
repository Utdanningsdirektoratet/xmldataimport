using System.Collections.Generic;
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
            StringBuilder sb = new StringBuilder();
            sb.Append("Begin ");
            string[] statements = userData.Split(';');
            foreach (string s in statements)
            {
                if (s.Length > 0)
                {
                    sb.AppendFormat(" EXECUTE IMMEDIATE '{0}';", s.Replace("'", "''"));
                }
            }
            sb.Append(" END ; ");
            return sb.ToString();
        }
    }
}
