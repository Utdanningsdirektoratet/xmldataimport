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
                path + "\\examplexmld" //Currently just contains Persons.xmld. Add \\ora for Oracle
                );
        }

        [ClassCleanup]
        public static void Cleanup()
        {
            _xmlInsert.Dispose();
        }

        [TestMethod]
        public void must_insert_expected_sql_variable_values()
        {
            var variableValueCount = _xmlInsert.DataContext.GetCount(new List<string>
            {
                "SELECT COUNT(*) FROM Persons WHERE LastName = 'Smith'"
            });

            Assert.AreEqual(2, variableValueCount, "Failed to insert the two expected variable values when" +
                " insert of Persons.xmld ran.");
        }

        [TestMethod]
        public void must_insert_expected_string_code_variable_value()
        {
            var lastNameValue = _xmlInsert.DataContext.ExecuteScalar(
                string.Format("SELECT LastName FROM Persons WHERE LastName = '{0}'", _otherLastName)
            );

            Assert.AreEqual(_otherLastName, lastNameValue.ToString(), "Failed to insert string variable value from code when" +
                " insert of Persons.xmld ran.");
        }

        [TestMethod]
        public void must_insert_expected_int_code_variable_value()
        {
            var ageValue = _xmlInsert.DataContext.ExecuteScalar(
                string.Format("SELECT age FROM Persons WHERE age = {0}", _age)
            );

            Assert.AreEqual(_age, int.Parse(ageValue.ToString()), "Failed to insert int variable value from code when" +
                " insert of Persons.xmld ran.");
        }
    }
}
