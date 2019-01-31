using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Xml;

namespace UDir.XmlDataImport
{
    public class XmlInsert : IDisposable
    {
        const string Setup = "setup";
        const string VariableNode = "variables";
        const string Startup = "startup";
        const string Teardown = "teardown";

        //Matches a _function1Name_2( or just a '('
        const string SqlFunctionSignature = @"[a-zA-Z_]*[a-zA-Z0-9_]*\(";

        const string VariableFunctionSignature = "variable\\s?\\(\\'[a-zA-Z_0-9]*\\'\\)";

        const string SqlInsertTemplate = "INSERT INTO {0} ({1}) VALUES({2});";

        private readonly string[] _paths;
        private readonly IFileProvider _fileProvider;        
        public readonly IImportDataContext DataContext;
        private readonly bool _ignoreNoChange;
        private readonly SqlPack _sqlPack;

        private Dictionary<string, object> Variables { get; }

        /// <summary>
        /// Use this constructor to just run some SQL.
        /// </summary>
        // ReSharper disable once UnusedParameter.Local
        public XmlInsert()
        {
            DataContext = new ImportDataContext();
        }

        /// <summary>
        /// Use this constructor to insert a .xmld file or one or more directories of .xmld file 
        /// contents into the database.
        /// </summary>
        /// <param name="path">Directory, individual file path, array of individual files 
        /// or array of directories</param>
        public XmlInsert(string[] path) : this(new FileProvider(), new ImportDataContext(), false, null, path)
        {
        }

        public XmlInsert(string path) : this(new FileProvider(), new ImportDataContext(), false, null, path)
        {
        }

        /// <summary>
        /// Use this constructor to insert a .xmld file or one or more directories of .xmld file 
        /// contents into the database.
        /// </summary>
        /// <param name="ignoreNoChange">Do not throw exception when no rows affected</param>
        /// <param name="path">Directory, individual file path, array of individual files 
        /// or array of directories</param>
        public XmlInsert(bool ignoreNoChange, params string[] path) : this(new FileProvider(), new ImportDataContext(), ignoreNoChange, null, path)
        {
        }

        /// <summary>
        /// Use this constructor to insert a .xmld file or one or more directories of .xmld file 
        /// contents into the database.
        /// </summary>
        /// <param name="ignoreNoChange">Do not throw exception when no rows affected</param>
        /// <param name="variables">Collection of variables to use in script</param>
        /// <param name="path">Directory, individual file path, array of individual files 
        /// or array of directories</param>
        public XmlInsert(bool ignoreNoChange, Dictionary<string, object> variables, params string[] path) : this(new FileProvider(), new ImportDataContext(), ignoreNoChange, variables, path)
        {
        }

        public XmlInsert(bool ignoreNoChange, Dictionary<string, object> variables, string connectionStringId, string dbVendor, params string[] path): this(new FileProvider(), new ImportDataContext(new Settings(connectionStringId, dbVendor)), ignoreNoChange, variables, path)
        {
            
        }

        /// <summary>
        /// Use this constructor to insert a .xmld file or one or more directories of .xmld file 
        /// contents into the database.
        /// </summary>        
        /// <param name="variables">Collection of variables to use in script</param>
        /// <param name="path">Directory, individual file path, array of individual files 
        /// or array of directories</param>
        public XmlInsert(Dictionary<string, object> variables, params string[] path) : this(new FileProvider(), new ImportDataContext(), false, variables, path)
        {
        }

        XmlInsert(IFileProvider fileProvider, IImportDataContext dataContext, bool ignoreNoChange, Dictionary<string, object> variables = null, params string[] path)
        {            
            _paths = path;
            _fileProvider = fileProvider;
            DataContext = dataContext;
            _ignoreNoChange = ignoreNoChange;
            _sqlPack = new SqlPack();
            Variables = variables ?? new Dictionary<string, object>();

            ParseXmlSql(_sqlPack);

            ExecPack(_sqlPack);
        }
        public void Dispose()
        {
            if (_sqlPack != null)
            {
                if (_sqlPack.SetupScripts != null)
                {
                    RunScriptPack(OracleHelpers.Blockify(_sqlPack.SetupScripts, DataContext.Settings));
                }

                if (_sqlPack.TearDownScripts != null)
                {
                    RunScriptPack(OracleHelpers.Blockify(_sqlPack.TearDownScripts, DataContext.Settings));
                }
            }
        }
        
        private void ParseXmlSql(SqlPack sqlPack)
        {
            var xmlDocument = new XmlDocument();

            if (_paths.Length == 1 && _paths[0].ToLower().StartsWith("<?xml"))
            {
                xmlDocument.LoadXml(_paths[0]);
                ParseAllScripts(sqlPack, xmlDocument);
            }
            else
            {
                foreach (var file in _fileProvider.GetFiles(_paths))
                {
                    xmlDocument.Load(file);
                    ParseAllScripts(sqlPack, xmlDocument);
                }
            }
        }

        private void ParseAllScripts(SqlPack sqlPack, XmlDocument xmlDocument)
        {
            var xmlNodes = GetChildNodes(xmlDocument.DocumentElement);

            AddScripts(sqlPack.Variables, RetrieveNodeContent(xmlNodes, VariableNode));

            AddScripts(sqlPack.StartupScripts, RetrieveNodeContent(xmlNodes, Startup), sqlPack.Variables);

            AddScripts(sqlPack.SetupScripts, RetrieveNodeContent(xmlNodes, Setup), sqlPack.Variables);

            AddScripts(sqlPack.TearDownScripts, RetrieveNodeContent(xmlNodes, Teardown), sqlPack.Variables);

            AddInsertScripts(sqlPack, xmlNodes, sqlPack.Variables);
        }
        
        private void AddInsertScripts(SqlPack sqlPack, List<XmlElement> xmlNodes, List<string> variables = null)
        {
            var columnList = new List<string>();
            var valueList = new List<string>();
            var excluded = new List<string> { Startup, Setup, Teardown, VariableNode };
            var tempQueries = new List<string>();

            foreach (var xmlNode in xmlNodes.Where(x => !excluded.Contains( x.Name.ToLower())))
            {
                foreach (var childNode in xmlNode.ChildNodes)
                {
                    //Escape column names with [ColName] to avoid keyword problems
                    //columnList.Add(string.Format("[{0}]",((XmlElement) childNode).Name));
                    columnList.Add(((XmlElement)childNode).Name);
                    valueList.Add(GetFormattedValue(childNode));
                }
                
                tempQueries.Add(string.Format(SqlInsertTemplate, xmlNode.Name,
                    string.Join(",", columnList), string.Join(",", valueList)));

                columnList.Clear();
                valueList.Clear();
            }

            if (tempQueries.Any())
            {
                var variableDeclarations = GetVariableDeclarations(variables);
                var joinedQuery = string.Join("\n", tempQueries);
                sqlPack.Inserts.Add(OracleHelpers.DelimitVariableBlock(variableDeclarations, joinedQuery, DataContext.Settings));
            }
        }

        private void ExecPack(SqlPack sqlPack)
        {
            var changedCount = 0;

            changedCount += RunScriptPack(OracleHelpers.Blockify(sqlPack.StartupScripts, DataContext.Settings));

            changedCount += RunScriptPack(OracleHelpers.Blockify(sqlPack.SetupScripts, DataContext.Settings));

            changedCount += DataContext.ExecBatch(OracleHelpers.Blockify(sqlPack.Inserts, DataContext.Settings));

            if (!_ignoreNoChange && changedCount == 0)
            {
                throw new ApplicationException(
                    string.Format("No records inserted. There were {0} statements issued against the DB. Paths used: {1}", 
                    sqlPack.SetupScripts.Count + sqlPack.Inserts.Count, string.Join(";",_paths)));
            }
        }

        private string GetFormattedValue(object childNode)
        {
            var value = ((XmlElement)childNode).InnerText;

            var functionSegment = value.Length > 100 ? value.Substring(0, 100) : value;

            if (functionSegment.Trim().StartsWith("@")) //Variable
            {
                return value;
            }

            if (!Regex.IsMatch(functionSegment, SqlFunctionSignature))
            {
                value = string.Format("'{0}'", value);
            }
            
            if (Regex.IsMatch(functionSegment, VariableFunctionSignature))
            {
                value = GetVariableValue(value);
            }                
            
            return value;
        }

        private string GetVariableValue(string variableExpression)
        {
            var retval = string.Empty;
            var matchingExpression = new Regex(VariableFunctionSignature).Match(variableExpression);
            if (matchingExpression.Value != string.Empty)
            {
                var key = matchingExpression.Value.Replace("variable('", string.Empty).Replace("')", string.Empty);
                try
                {
                    var value = "'" + Variables[key] + "'";
                    retval = variableExpression.Replace(matchingExpression.Value, value);
                }
                catch (Exception)
                {
                    ThrowArgException(variableExpression);
                }                
            }

            if (string.IsNullOrWhiteSpace(retval))
            {
                ThrowArgException(variableExpression);
            }

            return retval;
        }
        private static void ThrowArgException(string variableExpression)
        {
            throw new ArgumentException(
                string.Format("You must define a value for expression: {0}", variableExpression));
        }

        public int RunScriptPack(List<string> pack)
        {
            return DataContext.ExecBatch(pack);
        }

        private void AddScripts(List<string> collection, string currentSetupSql, List<string> variables = null)
        {

            if (!string.IsNullOrEmpty(currentSetupSql))
            {
                var variableDeclaration = GetVariableDeclarations(variables);
                collection.Add(OracleHelpers.DelimitVariableBlock(variableDeclaration, currentSetupSql, DataContext.Settings));
            }
        }

        private static string GetVariableDeclarations(List<string> variables)
        {
            var variableDeclaration = string.Empty;

            if (variables != null && variables.Any())
            {
                variableDeclaration = string.Join(";", variables);

                if (!variableDeclaration.TrimEnd().EndsWith(";"))
                {
                    variableDeclaration += ";";
                }
            }
           
            return variableDeclaration;
        }

        private string RetrieveNodeContent(List<XmlElement> xmlNodes, string nodeName)
        {
            var matchingNode = xmlNodes.FirstOrDefault(x => x.Name.ToLower() == nodeName);

            var nodeValue = matchingNode != null ? matchingNode.InnerText : string.Empty;
            
            var variableFunctionRegex = new Regex(VariableFunctionSignature);
            var matches = variableFunctionRegex.Matches(nodeValue);

            foreach (var match in matches)
            {
                var matchValue = match.ToString();
                nodeValue = nodeValue.Replace(matchValue, GetVariableValue(matchValue));
            }

            return nodeValue;
        }                

        private static List<XmlElement> GetChildNodes(XmlElement rootNode)
        {
            return
                (from XmlNode childNode in rootNode.ChildNodes
                    where childNode.Name != "XmlNode"
                    select childNode as XmlElement).Where(n => n != null).ToList();
        }        
    }
}
