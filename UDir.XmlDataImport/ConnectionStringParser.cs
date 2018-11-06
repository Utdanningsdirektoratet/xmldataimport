using System;
using System.Text.RegularExpressions;

namespace UDir.XmlDataImport
{
    public class ConnectionStringParser
    {
        public static string ParseConnectionString(string connectionString)
        {
            var regex = new Regex(@"%[A-Za-z0-9\(\)]*%", RegexOptions.Compiled | RegexOptions.IgnoreCase);
            var matches = regex.Matches(connectionString);

            for(var i = 0; i < matches.Count; i++)
            {
                var key = matches[i].Value.Replace("%", string.Empty);
                var variableValue = Environment.GetEnvironmentVariable(key);
                if (variableValue != null)
                {
                    connectionString = connectionString.Replace(matches[i].Value, variableValue);
                }
            }

            return connectionString;
        }
    }
}