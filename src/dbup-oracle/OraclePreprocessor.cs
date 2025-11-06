using DbUp.Engine;

namespace DbUp.Oracle
{
    /// <summary>
    /// This preprocessor makes adjustments to your sql to make it compatible with Oracle.
    /// </summary>
    public class OraclePreprocessor : IScriptPreprocessor
    {
        /// <inheritdoc/>
        public string Process(string contents) => contents;
    }
}
