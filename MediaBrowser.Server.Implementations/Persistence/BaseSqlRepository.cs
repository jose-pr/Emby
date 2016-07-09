using MediaBrowser.Common.Data;
using MediaBrowser.Common.Data.Sql;
using MediaBrowser.Model.Logging;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace MediaBrowser.Server.Implementations.Persistence
{
    public abstract class BaseSqlRepository : IDisposable
    {
        protected SemaphoreSlim WriteLock = new SemaphoreSlim(1, 1);
        protected readonly IDbConnector DbConnector;
        protected ILogger Logger;
        protected List<SqlTable> Tables;

        protected string DbFilePath { get; set; }

        protected BaseSqlRepository(ILogManager logManager, IDbConnector dbConnector)
        {
            DbConnector = dbConnector;
            Logger = logManager.GetLogger(GetType().Name);
            Tables = new List<SqlTable>();
        }

        protected virtual bool EnableConnectionPooling
        {
            get { return true; }
        }

        protected virtual async Task<IDbConnection> CreateConnection(bool isReadOnly = false)
        {
            var connection = await DbConnector.Connect(DbFilePath, false, true).ConfigureAwait(false);

            connection.RunQueries(new[]
            {
                "pragma temp_store = memory"

            }, Logger);

            return connection;
        }

        private bool _disposed;
        protected void CheckDisposed()
        {
            if (_disposed)
            {
                throw new ObjectDisposedException(GetType().Name + " has been disposed and cannot be accessed.");
            }
        }

        public void Dispose()
        {
            _disposed = true;
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected async Task Vacuum(IDbConnection connection)
        {
            CheckDisposed();

            await WriteLock.WaitAsync().ConfigureAwait(false);

            try
            {
                using (var cmd = connection.CreateCommand())
                {
                    cmd.CommandText = "vacuum";
                    cmd.ExecuteNonQuery();
                }
            }
            catch (Exception e)
            {
                Logger.ErrorException("Failed to vacuum:", e);

                throw;
            }
            finally
            {
                WriteLock.Release();
            }
        }

        private readonly object _disposeLock = new object();

        /// <summary>
        /// Releases unmanaged and - optionally - managed resources.
        /// </summary>
        /// <param name="dispose"><c>true</c> to release both managed and unmanaged resources; <c>false</c> to release only unmanaged resources.</param>
        protected virtual void Dispose(bool dispose)
        {
            if (dispose)
            {
                try
                {
                    lock (_disposeLock)
                    {
                        WriteLock.Wait();

                        CloseConnection();
                    }
                }
                catch (Exception ex)
                {
                    Logger.ErrorException("Error disposing database", ex);
                }
            }
        }

        protected virtual void CloseConnection()
        {

        }

        protected async Task BuildDb()
        {
            using(var connection = await CreateConnection().ConfigureAwait(false))
            {
                Tables.ToList().ForEach(t => t.Create(connection, Logger));
            }
        }
        protected async Task Insert(string tableName, Dictionary<string,Object> record)
        {
            var table = Tables.FirstOrDefault(t=> t.Name == tableName);
            using(var connnection = await CreateConnection().ConfigureAwait(false))
            {
                Logger.Info("ALL is good"); 
                table.Insert(connnection, Logger, record);
            }
        }
        protected async Task Update(string tableName, Dictionary<string, Object> record, string where)
        {
            var table = Tables.FirstOrDefault(t => t.Name == tableName);
            using (var connnection = await CreateConnection().ConfigureAwait(false))
            {
                table.Update(connnection, Logger, record, where);
            }
        }
        protected async Task<IEnumerable<T>> Select<T>(string tableName, IEnumerable<string> select, string where, Func<DbRecord,T> get)
        {
            var table = Tables.FirstOrDefault(t => t.Name == tableName);
            using (var connnection = await CreateConnection().ConfigureAwait(false))
            {
                return table.Select(connnection, Logger, select, where).Select(get);
            }
        }

        protected async Task<IEnumerable<Object>> ExecuteQuery(Query query)
        {
            using (var connection = await CreateConnection().ConfigureAwait(false))
            {
                return connection.ExecuteQuery<object>(query, Logger);
            }
        }
    }

    public class SqlTable
    {

        {
            get
            {
                var tableQuery = new List<string>();
                Columns.ToList().ForEach(c => tableQuery.Add(c.BuildString + (PrimaryKey == c.Name ? " PRIMARY KEY" : "")));
            }
        }


                



        public IEnumerable<string> IndexBuildStrings
        {
            get
            {
                var strings = new List<string>();
                Indexes.ToList().ForEach(i => strings.Add(i.BuildString.Replace("tableName", Name)));
                return strings;
            }
        }

        public void Update(IDbConnection conn, ILogger logger, Dictionary<string, object> newValues, string where)
        {
            var updateString = new List<string>();
            Columns.Where(c => newValues.ContainsKey(c.Name)).ToList().ForEach(c => {
                updateString.Add(c.Name+" = @"+c.Name);
                updateQ.AddParameter(c.Name, newValues[c.Name], c.ColumnType);
                logger.Info("Adding {0} with type {1}", c.Name, c.ColumnType.ToString());
            });
            updateQ.SetCmd( String.Format(updateQ.Cmd, Name, String.Join(", ", updateString), where ));
            conn.ExecuteQuery(updateQ, logger);
        }
        public IEnumerable<DbRecord> Select(IDbConnection conn, ILogger logger, IEnumerable<string> columns, string where)
        {
            var select = new List<string>();
            var selectQ = SqlQuery.Select;
            Columns.Where(c => columns.Contains(c.Name)).ToList().ForEach(c => {
                select.Add(c.Name);
            });
            selectQ.FormatCmdTxt(Name, String.Join(", ", select), string.IsNullOrWhiteSpace(where) ? "" : (" WHERE " + where));
            return conn.ExecuteQuery(selectQ, logger).Cast<DbRecord>();
        }
        public void Create(IDbConnection conn, ILogger logger)
        {
            var query = new SqlQuery(BuildString);
            conn.ExecuteQuery(query, logger); //Build table if it doesnt exist

            DataTable schema = conn.GetTableSchema(Name); //Get Table Schema

            //Check For Missing columns

            Dictionary<string, SqlColumn> columns = Columns.ToDictionary(c => c.Name, c => c);
            foreach (DataRow myField in schema.Rows)
            {
                columns.Remove(myField["ColumnName"].ToString());
            }
            var addColumnsQ = new List<Query>();
            columns.Values.ToList().ForEach(c => addColumnsQ.Add(new SqlQuery(AddColumnString(c.Name))));
            conn.ExecuteQueries(addColumnsQ, logger);

            //Now that we have all columns create indexes
            var addIndexesQ = new List<Query>();
            IndexBuildStrings.ToList().ForEach(cmd => addColumnsQ.Add(new SqlQuery(cmd)));
            conn.ExecuteQueries(addColumnsQ, logger);
        }
    }

    }
    public class SqlColumn
    {
        public string WhereString { get { return string.Format("{0} = {1}", Name, "@" + Name); } }
    }

}