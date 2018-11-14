using System.Collections.Generic;
using System.IO;
using System.Reflection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using UDir.XmlDataImport;

namespace Udir.XmlDataImport.Test
{
    //Uncomment if running Oracle
    //[TestClass]
    public class XmlDataImportOraTest
    {
        private static XmlInsert _xmlInsert;
        private static int _maxSalary = 9000;
        private static string _lastName = "Nordmann";

        [ClassInitialize]
        public static void Startup(TestContext context)
        {
            string path = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            _xmlInsert = new XmlInsert(true,
                new Dictionary<string, object>
                {
                    {"maxSalary", _maxSalary},
                    {"lastName", _lastName }
                },
                path + "\\examplexmld\\ora"
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
                "SELECT COUNT(*) FROM Employees WHERE FIRST_NAME = 'Odd'"
            });

            Assert.AreEqual(1, variableValueCount, "Failed to insert the expected variable values when" +
                " insert of Employees.xmld ran.");
        }

        [TestMethod]
        public void must_insert_expected_string_code_variable_value()
        {
            var lastNameValue = _xmlInsert.DataContext.ExecuteScalar(
                string.Format("SELECT LAST_NAME FROM Employees WHERE LAST_NAME = '{0}'", _lastName)
            );

            Assert.AreEqual(_lastName, lastNameValue.ToString(), "Failed to insert string variable value from code when" +
                " insert of Employees.xmld ran.");
        }

        [TestMethod]
        public void must_insert_expected_int_code_variable_value()
        {
            var ageValue = _xmlInsert.DataContext.ExecuteScalar(
                string.Format("SELECT MAX_SALARY FROM Jobs WHERE MAX_SALARY = {0}", _maxSalary)
            );

            Assert.AreEqual(_maxSalary, int.Parse(ageValue.ToString()), "Failed to insert int variable value from code when" +
                " insert of Employees.xmld ran.");
        }
    }
}
