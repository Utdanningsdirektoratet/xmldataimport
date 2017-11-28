using System;
using System.Configuration;

namespace UDir.XmlDataImport.Console
{
    class Program
    {
        static int Main(string[] args)
        {
            try
            {
                var ignoreNoChangeSetting = ConfigurationManager.AppSettings["ignoreNoChange"];
                bool ignoreNoChange;
                bool.TryParse(ignoreNoChangeSetting, out ignoreNoChange);

                // ReSharper disable once ObjectCreationAsStatement
                new XmlInsert(ignoreNoChange, args);
            }
            catch (Exception ex)
            {
                System.Console.Error.Write(ex.Message);
                return 1;
            }

            return 0;
        }
    }
}
