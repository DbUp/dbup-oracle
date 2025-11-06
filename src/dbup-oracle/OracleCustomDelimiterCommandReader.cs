using System;
using System.Text;
using DbUp.Support;

namespace DbUp.Oracle
{
    /// <summary>
    /// Reads Oracle commands from an underlying text stream using a custom delimiter. Supports DELIMITER statements.
    /// </summary>
    public class OracleCustomDelimiterCommandReader : SqlCommandReader
    {
        const string DelimiterKeyword = "DELIMITER";

        /// <summary>
        /// Creates an instance of OracleCommandReader
        /// </summary>
        public OracleCustomDelimiterCommandReader(string sqlText, char delimiter) : base(sqlText, delimiter.ToString(), delimiterRequiresWhitespace: false)
        {
        }

        /// <summary>
        /// Hook to support custom statements
        /// </summary>
        /// <inheritdoc/>
        protected override bool IsCustomStatement
            => TryPeek(DelimiterKeyword.Length - 1, out var statement) &&
               string.Equals(DelimiterKeyword, CurrentChar + statement, StringComparison.OrdinalIgnoreCase) &&
               string.IsNullOrEmpty(GetCurrentCommandTextFromBuffer());

        /// <summary>
        /// Read a custom statement
        /// </summary>
        /// <inheritdoc/>
        protected override void ReadCustomStatement()
        {
            // Move past Delimiter keyword
            var count = DelimiterKeyword.Length + 1;
            Read(new char[count], 0, count);

            SkipWhitespace();
            // Read until we hit the end of line.
            var delimiter = new StringBuilder();
            do
            {
                delimiter.Append(CurrentChar);
                if (Read() == FailedRead)
                {
                    break;
                }
            } while (!IsEndOfLine && !IsWhiteSpace);

            Delimiter = delimiter.ToString();
        }

        void SkipWhitespace()
        {
            while (char.IsWhiteSpace(CurrentChar))
            {
                Read();
            }
        }
    }
}
