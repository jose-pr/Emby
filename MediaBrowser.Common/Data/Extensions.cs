using MediaBrowser.Model.Logging;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections;
using System.IO;
using MediaBrowser.Model.Serialization;

namespace MediaBrowser.Common.Data
{
    public static class DBExtensions
    {
        public static IDbRecord[] Execute(this IDbQuery query,IDbConnection connection, ILogger logger)
        {
            return connection.ExecuteQuery(query, logger);
        }
        public static DbType GetDbType(this Object obj)
        {
            DbType result;
            if (!DbTypeMaps.DbTypeMap.TryGetValue(obj.GetType(), out result)) result = DbType.String;
            return result;
        }

        public static DbType GetDbType(this Type type)
        {
            DbType result;
            if (!DbTypeMaps.DbTypeMap.TryGetValue(type, out result)) result = DbType.String;
            return result;
        }
    
        public static Type GetValueType<T>(this IEnumerable<T> list)
        {
            return typeof(T);
        }

        public static string GetPrefix(this IDbConnection connection)
        {
            return "@";
        }

        public static IDbDataParameter AddParameter(this IDbCommand cmd, string name, DbType type, string prefix = "")
        {
            var param = cmd.CreateParameter();
            param.ParameterName = prefix + name;
            param.DbType = type;
            cmd.Parameters.Add(param);
            return param;
        }

        public static DbRecord GetRecord(this IDataReader reader)
        {
            return (DbRecord) Enumerable.Range(0, reader.FieldCount).ToDictionary(reader.GetName,i => {
                if (reader.IsDBNull(i)) return null;
                return reader.GetValue(i);
           });
        }

        public static IDbRecord[] ExecuteQuery(this IDbConnection connection, IDbQuery query, ILogger logger)
        {
            return connection.ExecuteQueries(new Query[] { query }, logger).First();
        }

        public static IEnumerable<T> ExecuteQuery<T>(this IDbConnection connection, IDbQuery query, ILogger logger)
        {
            return connection.ExecuteQueries(new Query[] { query }, logger).First().Cast<T>();
        }

        public static IEnumerable<IEnumerable<Object>> ExecuteQueries(this IDbConnection connection, IEnumerable<Query> queries, ILogger logger)
        {
            var prefix = connection.GetPrefix().ToString();
            logger.Info(prefix);
            using (var transaction = connection.BeginTransaction())
            {
                var error = "";
                var results = new List<List<Object>>();
                try
                {
                    foreach (var query in queries)
                    {
                        logger.Info("Executing query " + query.Cmd);
                        error = query.ErrorMsg;
                        int total = 1, count = 0;
                        var _params = new List<Tuple<IDbDataParameter, QueryParams>>();
                        results.Add(new List<Object>());

                        using (var cmd = connection.CreateCommand())
                        {
                            cmd.CommandText = query.Cmd;
                            cmd.Transaction = transaction;

                            query.Parameters.ForEach(p =>
                            {
                                logger.Info("Adding {0} with type {1} inside", p.Id, p.Type.ToString());
                                _params.Add(Tuple.Create(cmd.AddParameter(p.Id, p.Type, prefix), p));
                                total = p.Count();
                            });
                            while (count++ < total)
                            {
                                logger.Info(count.ToString());
                                _params.ForEach(p => {
                                    p.Item1.Value = p.Item2[count - 1];
                                    logger.Info("Adding {0} with type {1} inside and value {2} and internal type {3}",
                                        p.Item1.ParameterName,
                                        p.Item1.DbType,
                                        p.Item1.Value.ToString(),
                                        p.Item1.Value.GetType().ToString()
                                        );

                                });

                                logger.Info("CMD TYPE");
                                logger.Info(query.CmdType.ToString());
                                switch (query.CmdType)
                                {
                                    case QueryCmd.NonQuery:
                                        results.Last().Add(cmd.ExecuteNonQuery());
                                        break;
                                    case QueryCmd.Reader:
                                        using (var reader = cmd.ExecuteReader(query.CmdBehavior))
                                        {
                                            while (reader.Read())
                                            {
                                                results.Last().Add(reader.GetRecord() ?? new DbRecord());
                                            }
                                        }
                                        break;
                                    case QueryCmd.Scalar:
                                        results.Last().Add(cmd.ExecuteScalar());
                                        break;
                                    default:
                                        throw new ArgumentException("No valid cmd type for query");
                                }
                            }

                        }
                    }
                    transaction.Commit();
                }
                catch (OperationCanceledException e)
                {
                    logger.Info("FAILED COMMAND"+e.Message);
                    logger.Error(error ?? "Query Execution Failed", e);
                    return null;
                }
                catch (Exception e)
                {
                    logger.Error(error ?? "Query Execution Failed", e);
                    throw;
                }
                finally
                {
                    queries.ToList().ForEach(query => query.Parameters.Clear());
                    if (transaction != null)
                    {
                        transaction.Dispose();
                    }
                }
                return results;
            }
        }

        public static DataTable GetTableSchema(this IDbConnection connection, string table)
        {
            using (var cmd = connection.CreateCommand())
            {
                cmd.CommandText = "SELECT * FROM " + table;

                using (var reader = cmd.ExecuteReader(CommandBehavior.SchemaOnly))
                {
                    return reader.GetSchemaTable();
                }
            }
        }
    }
}
