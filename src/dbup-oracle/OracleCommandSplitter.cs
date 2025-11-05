using System;
using System.Collections.Generic;
using DbUp.Support;

namespace DbUp.Oracle
{
    /// <summary>
    /// Splits Oracle SQL scripts into individual commands using configurable delimiters.
    /// </summary>
    public class OracleCommandSplitter
    {
        private readonly Func<string, SqlCommandReader> commandReaderFactory;

        /// <summary>
        /// Initializes a new instance of the <see cref="OracleCommandSplitter"/> class using the default semicolon delimiter.
        /// </summary>
        [Obsolete]
        public OracleCommandSplitter()
        {
            this.commandReaderFactory = scriptContents => new OracleCommandReader(scriptContents);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="OracleCommandSplitter"/> class using a custom delimiter.
        /// </summary>
        /// <param name="delimiter">The delimiter character to use for splitting commands.</param>
        public OracleCommandSplitter(char delimiter)
        {
            this.commandReaderFactory = scriptContents => new OracleCustomDelimiterCommandReader(scriptContents, delimiter);
        }

        /// <summary>
        /// Splits a script with multiple delimited commands into commands
        /// </summary>
        /// <param name="scriptContents"></param>
        /// <returns></returns>
        public IEnumerable<string> SplitScriptIntoCommands(string scriptContents)
        {
            using (var reader = commandReaderFactory(scriptContents))
            {
                var commands = new List<string>();
                reader.ReadAllCommands(c => commands.Add(c));
                return commands;
            }
        }
    }
}
