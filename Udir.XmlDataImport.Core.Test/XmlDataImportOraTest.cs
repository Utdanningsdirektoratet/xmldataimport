using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using UDir.XmlDataImport;
using Xunit;

namespace Udir.XmlDataImport.Core.Test
{
    public class XmlDataImportOraTest : IDisposable
    {
        private static XmlInsert _xmlInsert;
        private static int _maxSalary = 9000;
        private static string _lastName = "Nordmann";


        public XmlDataImportOraTest()
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
        public void Dispose()
        {
            _xmlInsert.Dispose();
        }

        [Fact]
        public void must_insert_expected_sql_variable_values()
        {
            var variableValueCount = _xmlInsert.DataContext.GetCount(new List<string>
            {
                "SELECT COUNT(*) FROM Employees WHERE FIRST_NAME = 'Odd'"
            });

            Assert.Equal(1, variableValueCount);
        }

        [Fact]
        public void must_insert_expected_string_code_variable_value()
        {
            var lastNameValue = _xmlInsert.DataContext.ExecuteScalar(
                string.Format("SELECT LAST_NAME FROM Employees WHERE LAST_NAME = '{0}'", _lastName)
            );

            Assert.Equal(_lastName, lastNameValue.ToString());
        }

        [Fact]
        public void must_insert_expected_int_code_variable_value()
        {
            var ageValue = _xmlInsert.DataContext.ExecuteScalar(
                string.Format("SELECT MAX_SALARY FROM Jobs WHERE MAX_SALARY = {0}", _maxSalary)
            );

            Assert.Equal(_maxSalary, int.Parse(ageValue.ToString()));
        }
    }
}