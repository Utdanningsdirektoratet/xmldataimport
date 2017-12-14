using System.Collections.Generic;
using System.IO;
using System.Reflection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using UDir.XmlDataImport;

namespace Udir.XmlDataImport.Test
{
    [TestClass]
    public class XmlDataImportTest
    {
        private static XmlInsert _xmlInsert;
        private static int _age = 13;
        private static string _otherLastName = "Ingebretsen";

        [ClassInitialize]
        public static void Startup(TestContext context)
        {
            string path = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            _xmlInsert = new XmlInsert(true,
                new Dictionary<string, object>
                {
                    {"age", _age},
                    {"otherLastName", _otherLastName }
                },
                path + "\\examplexmld" //Currently just contains Persons.xmld
                );
        }

        [TestMethod]
        public void must_insert_expected_variable_values()
        {
            var variableValueCount = _xmlInsert.DataContext.GetCount(new List<string>
            {
                "SELECT COUNT(*) FROM Persons WHERE LastName = 'SMITH'"
            });

            Assert.AreEqual(2, variableValueCount, "Failed to insert the two expected variable values when" +
                " insert of Persons.xmld ran.");
        }
    }
}
