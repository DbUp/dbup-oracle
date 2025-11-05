using System;
using System.Data;
using System.Globalization;
using DbUp.Engine;
using DbUp.Engine.Output;
using DbUp.Engine.Transactions;
using DbUp.Support;

namespace DbUp.Oracle
{
    /// <summary>
    /// An implementation of the <see cref="IJournal"/> interface which tracks version numbers for an
    /// Oracle database using a table called SchemaVersions.
    /// </summary>
    public class OracleTableJournal : TableJournal
    {
        bool journalExists;
        /// <summary>
        /// Creates a new Oracle table journal.
        /// </summary>
        /// <param name="connectionManager">The Oracle connection manager.</param>
        /// <param name="logger">The upgrade logger.</param>
        /// <param name="schema">The name of the schema the journal is stored in.</param>
        /// <param name="table">The name of the journal table.</param>
        public OracleTableJournal(Func<IConnectionManager> connectionManager, Func<IUpgradeLog> logger, string schema, string table)
            : base(connectionManager, logger, new OracleObjectParser(), schema, table)
        {
        }

        /// <summary>
        /// English culture info used for formatting.
        /// </summary>
        public static CultureInfo English = new CultureInfo("en-US", false);

        /// <inheritdoc/>
        protected override string CreateSchemaTableSql(string quotedPrimaryKeyName)
        {
            var fqSchemaTableName = UnquotedSchemaTableName;
            return
                $@" CREATE TABLE {fqSchemaTableName} 
                (
                    schemaversionid NUMBER(10),
                    scriptname VARCHAR2(255) NOT NULL,
                    applied TIMESTAMP NOT NULL,
                    CONSTRAINT PK_{ fqSchemaTableName } PRIMARY KEY (schemaversionid) 
                )";
        }

        /// <summary>
        /// Creates the SQL for the sequence used by the schema table.
        /// </summary>
        /// <returns>The SQL statement to create the sequence.</returns>
        protected string CreateSchemaTableSequenceSql()
        {
            var fqSchemaTableName = UnquotedSchemaTableName;
            return $@" CREATE SEQUENCE {fqSchemaTableName}_sequence";
        }

        /// <summary>
        /// Creates the SQL for the trigger used by the schema table.
        /// </summary>
        /// <returns>The SQL statement to create the trigger.</returns>
        protected string CreateSchemaTableTriggerSql()
        {
            var fqSchemaTableName = UnquotedSchemaTableName;
            return $@" CREATE OR REPLACE TRIGGER {fqSchemaTableName}_on_insert
                    BEFORE INSERT ON {fqSchemaTableName}
                    FOR EACH ROW
                    BEGIN
                        SELECT {fqSchemaTableName}_sequence.nextval
                        INTO :new.schemaversionid
                        FROM dual;
                    END;
                ";
        }

        /// <inheritdoc/>
        protected override string GetInsertJournalEntrySql(string scriptName, string applied)
        {
            var unquotedSchemaTableName = UnquotedSchemaTableName.ToUpper(English);
            return $"insert into {unquotedSchemaTableName} (ScriptName, Applied) values (:" + scriptName.Replace("@", "") + ",:" + applied.Replace("@", "") + ")";
        }

        /// <inheritdoc/>
        protected override string GetJournalEntriesSql()
        {
            var unquotedSchemaTableName = UnquotedSchemaTableName.ToUpper(English);
            return $"select scriptname from {unquotedSchemaTableName} order by scriptname";
        }

        /// <inheritdoc/>
        protected override string DoesTableExistSql()
        {
            var unquotedSchemaTableName = UnquotedSchemaTableName.ToUpper(English);
            return $"select 1 from user_tables where table_name = '{unquotedSchemaTableName}'";
        }

        /// <summary>
        /// Gets the command to create the sequence for the schema table.
        /// </summary>
        /// <param name="dbCommandFactory">Factory to create database commands.</param>
        /// <returns>A command to create the sequence.</returns>
        protected IDbCommand GetCreateTableSequence(Func<IDbCommand> dbCommandFactory)
        {
            var command = dbCommandFactory();
            command.CommandText = CreateSchemaTableSequenceSql();
            command.CommandType = CommandType.Text;
            return command;
        }

        /// <summary>
        /// Gets the command to create the trigger for the schema table.
        /// </summary>
        /// <param name="dbCommandFactory">Factory to create database commands.</param>
        /// <returns>A command to create the trigger.</returns>
        protected IDbCommand GetCreateTableTrigger(Func<IDbCommand> dbCommandFactory)
        {
            var command = dbCommandFactory();
            command.CommandText = CreateSchemaTableTriggerSql();
            command.CommandType = CommandType.Text;
            return command;
        }

        /// <inheritdoc/>
        public override void EnsureTableExistsAndIsLatestVersion(Func<IDbCommand> dbCommandFactory)
        {
            if (!journalExists && !DoesTableExist(dbCommandFactory))
            {
                Log().LogInformation("Creating the {0} table", FqSchemaTableName);

                // We will never change the schema of the initial table create.
                using (var command = GetCreateTableSequence(dbCommandFactory))
                {
                    command.ExecuteNonQuery();
                }

                // We will never change the schema of the initial table create.
                using (var command = GetCreateTableCommand(dbCommandFactory))
                {
                    command.ExecuteNonQuery();
                }

                // We will never change the schema of the initial table create.
                using (var command = GetCreateTableTrigger(dbCommandFactory))
                {
                    command.ExecuteNonQuery();
                }

                Log().LogInformation("The {0} table has been created", FqSchemaTableName);

                OnTableCreated(dbCommandFactory);
            }

            journalExists = true;
        }
    }
}
