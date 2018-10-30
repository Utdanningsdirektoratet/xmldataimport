using System.Collections.Generic;
using System.IO;
using System.Reflection;
using UDir.XmlDataImport;
using Xunit;

namespace Udir.XmlDataImport.Core.Test
{
    public class XmlDataImportTest
    {
        private XmlInsert _xmlInsert;
        private static int _age = 13;
        private static string _otherLastName = "Ingebretsen";

        public XmlDataImportTest()
        {
            string path = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            _xmlInsert = new XmlInsert(true,
                new Dictionary<string, object>
                {
                    {"age", _age},
                    {"otherLastName", _otherLastName }
                },
                path + "\\examplexmld" //Currently just contains Persons.xmld. Add \\ora\\Persons for Oracle
            );

        }

        ~XmlDataImportTest()
        {
            _xmlInsert.Dispose();
        }

        [Fact]
        public void must_insert_expected_sql_variable_values()
        {
            var variableValueCount = _xmlInsert.DataContext.GetCount(new List<string>
            {
                "SELECT COUNT(*) FROM Persons WHERE LastName = 'Smith'"
            });

            Assert.Equal(2, variableValueCount);
        }

        [Fact]
        public void must_insert_expected_string_code_variable_value()
        {
            var lastNameValue = _xmlInsert.DataContext.ExecuteScalar(
                string.Format("SELECT LastName FROM Persons WHERE LastName = '{0}'", _otherLastName)
            );

            Assert.Equal(_otherLastName, lastNameValue.ToString());
        }

        [Fact]
        public void must_insert_expected_int_code_variable_value()
        {
            var ageValue = _xmlInsert.DataContext.ExecuteScalar(
                string.Format("SELECT age FROM Persons WHERE age = {0}", _age)
            );

            Assert.Equal(_age, int.Parse(ageValue.ToString()));
        }

    }
}
