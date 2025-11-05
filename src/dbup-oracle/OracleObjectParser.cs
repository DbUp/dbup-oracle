using DbUp.Support;

namespace DbUp.Oracle
{
    /// <summary>
    /// Parses Sql Objects and performs quoting functions.
    /// </summary>
    public class OracleObjectParser : SqlObjectParser
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="OracleObjectParser"/> class.
        /// </summary>
        public OracleObjectParser() : base("\"", "\"")
        {
        }
    }
}
