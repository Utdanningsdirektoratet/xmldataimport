﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Xml;

namespace UDir.XmlDataImport
{
    public class XmlInsert : IDisposable
    {
        const string Setup = "setup";
        const string Startup = "startup";
        const string Teardown = "teardown";

        //Matches a _function1Name_2( or just a '('
        const string SqlFunctionSignature = @"[a-zA-Z_]*[a-zA-Z0-9_]*\(";

        const string VariableFunctionSignature = "variable\\s?\\(\\'[a-zA-Z_0-9]*\\'\\)";

        const string SqlInsertTemplate = "INSERT INTO {0} ({1}) VALUES({2})";

        private readonly string[] _paths;
        private readonly IFileProvider _fileProvider;        
        public readonly IImportDataContext DataContext;
        private readonly bool _ignoreNoChange;
        private readonly SqlPack _sqlPack;

        private Dictionary<string, string> Variables { get; }

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
        public XmlInsert(bool ignoreNoChange, Dictionary<string, string> variables, params string[] path) : this(new FileProvider(), new ImportDataContext(), ignoreNoChange, variables, path)
        {
        }

        XmlInsert(IFileProvider fileProvider, IImportDataContext dataContext, bool ignoreNoChange, Dictionary<string, string> variables = null, params string[] path)
        {            
            _paths = path;
            _fileProvider = fileProvider;
            DataContext = dataContext;
            _ignoreNoChange = ignoreNoChange;
            _sqlPack = new SqlPack();
            Variables = variables ?? new Dictionary<string, string>();

            ParseXmlSql(_sqlPack);

            ExecPack(_sqlPack);
        }
        public void Dispose()
        {
            if (_sqlPack != null)
            {
                if (_sqlPack.SetupScripts != null)
                {
                    RunScriptPack(_sqlPack.SetupScripts);
                }

                if (_sqlPack.TearDownScripts != null)
                {
                    RunScriptPack(_sqlPack.TearDownScripts);
                }
            }
        }
        
        private void ParseXmlSql(SqlPack sqlPack)
        {
            var xmlDocument = new XmlDocument();

            foreach (var file in _fileProvider.GetFiles(_paths))
            {
                xmlDocument.Load(file);

                var xmlNodes = GetChildNodes(xmlDocument.DocumentElement);

                AddScripts(sqlPack.StartupScripts, RetrieveNodeContent(xmlNodes, Startup));

                AddScripts(sqlPack.SetupScripts, RetrieveNodeContent(xmlNodes, Setup));

                AddScripts(sqlPack.TearDownScripts, RetrieveNodeContent(xmlNodes, Teardown));

                AddInsertScripts(sqlPack, xmlNodes);
            }
        }

        private void AddInsertScripts(SqlPack sqlPack, List<XmlElement> xmlNodes)
        {
            var columnList = new List<string>();
            var valueList = new List<string>();
            var excluded = new List<string> { Startup, Setup, Teardown };
            foreach (var xmlNode in xmlNodes.Where(x => !excluded.Contains( x.Name.ToLower())))
            {
                foreach (var childNode in xmlNode.ChildNodes)
                {
                    //Escape column names with [ColName] to avoid keyword problems
                    columnList.Add(string.Format("[{0}]",((XmlElement) childNode).Name));
                    valueList.Add(GetFormattedValue(childNode));
                }

                sqlPack.Inserts.Add(string.Format(SqlInsertTemplate, xmlNode.Name,
                    string.Join(",", columnList), string.Join(",", valueList)));

                columnList.Clear();
                valueList.Clear();
            }
        }

        private void ExecPack(SqlPack sqlPack)
        {
            var changedCount = 0;

            changedCount += RunScriptPack(sqlPack.StartupScripts);

            changedCount += RunScriptPack(sqlPack.SetupScripts);

            changedCount += DataContext.ExecBatch(sqlPack.Inserts);

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

        private static void AddScripts(List<string> collection, string currentSetupSql)
        {
            if (!string.IsNullOrEmpty(currentSetupSql))
            {
                collection.Add(currentSetupSql);
            }
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
