﻿using MediaBrowser.Controller;
using MediaBrowser.Controller.Entities;
using MediaBrowser.Controller.Persistence;
using MediaBrowser.Model.Logging;
using MediaBrowser.Model.Serialization;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using MediaBrowser.Model.Entities;
using MediaBrowser.Common.Security;

namespace MediaBrowser.Server.Implementations.Persistence
{
    /// <summary>
    /// Class SQLiteUserRepository
    /// </summary>
    public class SqliteUserRepository : BaseSqliteRepository, IUserRepository
    {
        private static bool init = false;
        private readonly IJsonSerializer _jsonSerializer;

        public SqliteUserRepository(ILogManager logManager, IServerApplicationPaths appPaths, IJsonSerializer jsonSerializer, IDbConnector dbConnector) : base(logManager, dbConnector)
        {
            _jsonSerializer = jsonSerializer;
            DbFilePath = Path.Combine(appPaths.DataPath, "users.db");

            try
            {
                Initialize().Wait();
            }
            catch (Exception ex)
            {
                Logger.ErrorException("Error opening user db", ex);
                throw;
            }
            
        }

        /// <summary>
        /// Gets the name of the repository
        /// </summary>
        /// <value>The name.</value>
        public string Name
        {
            get
            {
                return "SQLite";
            }
        }

        /// <summary>
        /// Opens the connection to the database
        /// </summary>
        /// <returns>Task.</returns>
        public async Task Initialize()
        {
            init = true;
            using (var connection = await CreateConnection().ConfigureAwait(false))
            {
                string[] queries = {

                                "create table if not exists users (guid GUID primary key, data BLOB, password CHAR(40), login_name varchar(255))",
                                "create index if not exists idx_users on users(guid)",
                                "create table if not exists schema_version (table_name primary key, version)",

                                "pragma shrink_memory"
                               };

                connection.RunQueries(queries, Logger);
            }
        }

        /// <summary>
        /// Save a user in the repo
        /// </summary>
        /// <param name="user">The user.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>Task.</returns>
        /// <exception cref="System.ArgumentNullException">user</exception>
        public async Task SaveUser(User user, CancellationToken cancellationToken)
        {
            if (user == null)
            {
                throw new ArgumentNullException("user");
            }

            cancellationToken.ThrowIfCancellationRequested();

            var serialized = _jsonSerializer.SerializeToBytes(user);

            cancellationToken.ThrowIfCancellationRequested();

            using (var connection = await CreateConnection().ConfigureAwait(false))
            {
                IDbTransaction transaction = null;

                try
                {
                    transaction = connection.BeginTransaction();

                    using (var cmd = connection.CreateCommand())
                    {
                        cmd.CommandText = "replace into users (guid, data, login_name, password) values (@1, @2, @3, @4)";
                        cmd.Parameters.Add(cmd, "@1", DbType.Guid).Value = user.Id;
                        cmd.Parameters.Add(cmd, "@2", DbType.Binary).Value = serialized;
                        cmd.Parameters.Add(cmd, "@3", DbType.String).Value = user.Name;
                        cmd.Parameters.Add(cmd, "@4", DbType.String).Value = user.Password;

                        cmd.Transaction = transaction;

                        cmd.ExecuteNonQuery();
                    }

                    transaction.Commit();
                }
                catch (OperationCanceledException)
                {
                    if (transaction != null)
                    {
                        transaction.Rollback();
                    }

                    throw;
                }
                catch (Exception e)
                {
                    Logger.ErrorException("Failed to save user:", e);

                    if (transaction != null)
                    {
                        transaction.Rollback();
                    }

                    throw;
                }
                finally
                {
                    if (transaction != null)
                    {
                        transaction.Dispose();
                    }
                }
            }
        }

        /// <summary>
        /// Retrieve all users from the database
        /// </summary>
        /// <returns>IEnumerable{User}.</returns>
        public IEnumerable<User> RetrieveAllUsers()
        {
            var list = new List<User>();

            using (var connection = CreateConnection(true).Result)
            {
                using (var cmd = connection.CreateCommand())
                {
                    cmd.CommandText = "select guid,data,password from users";

                    using (var reader = cmd.ExecuteReader(CommandBehavior.SequentialAccess | CommandBehavior.SingleResult))
                    {
                        while (reader.Read())
                        {
                            var id = reader.GetGuid(0);

                            using (var stream = reader.GetMemoryStream(1))
                            {
                                var user = _jsonSerializer.DeserializeFromStream<User>(stream);
                                user.Id = id;
                                user.Password = reader.GetString(2);
                                list.Add(user);
                            }
                        }
                    }
                }
            }

            return list;
        }

        /// <summary>
        /// Deletes the user.
        /// </summary>
        /// <param name="user">The user.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>Task.</returns>
        /// <exception cref="System.ArgumentNullException">user</exception>
        public async Task DeleteUser(User user, CancellationToken cancellationToken)
        {
            if (user == null)
            {
                throw new ArgumentNullException("user");
            }

            cancellationToken.ThrowIfCancellationRequested();

            using (var connection = await CreateConnection().ConfigureAwait(false))
            {
                IDbTransaction transaction = null;

                try
                {
                    transaction = connection.BeginTransaction();

                    using (var cmd = connection.CreateCommand())
                    {
                        cmd.CommandText = "delete from users where guid=@guid";

                        cmd.Parameters.Add(cmd, "@guid", DbType.Guid).Value = user.Id;

                        cmd.Transaction = transaction;

                        cmd.ExecuteNonQuery();
                    }

                    transaction.Commit();
                }
                catch (OperationCanceledException)
                {
                    if (transaction != null)
                    {
                        transaction.Rollback();
                    }

                    throw;
                }
                catch (Exception e)
                {
                    Logger.ErrorException("Failed to delete user:", e);

                    if (transaction != null)
                    {
                        transaction.Rollback();
                    }

                    throw;
                }
                finally
                {
                    if (transaction != null)
                    {
                        transaction.Dispose();
                    }
                }
            }
        }

        public async Task UpdateUserPassword(Guid id, string password, CancellationToken cancellationToken)
        {
            if (id == null)
            {
                throw new ArgumentNullException("user");
            }

            using (var connection = await CreateConnection().ConfigureAwait(false))
            {
                IDbTransaction transaction = null;

                try
                {
                    transaction = connection.BeginTransaction();

                    using (var cmd = connection.CreateCommand())
                    {
                        cmd.CommandText = "update users set password=@2 where guid=@1";
                        cmd.Parameters.Add(cmd, "@1", DbType.Guid).Value = id;
                        cmd.Parameters.Add(cmd, "@2", DbType.String).Value = Crypto.GetSha1(password);

                        cmd.Transaction = transaction;

                        cmd.ExecuteNonQuery();
                    }

                    transaction.Commit();
                }
                catch (OperationCanceledException)
                {
                    if (transaction != null)
                    {
                        transaction.Rollback();
                    }

                    throw;
                }
                catch (Exception e)
                {
                    Logger.ErrorException("Failed to save user:", e);

                    if (transaction != null)
                    {
                        transaction.Rollback();
                    }

                    throw;
                }
                finally
                {
                    if (transaction != null)
                    {
                        transaction.Dispose();
                    }
                }
            }
        }

        public async Task<bool> AuthenticateUser(string login_name, string fqdn, string password)
        {
            password = password ?? String.Empty; 
            using (var connection = await CreateConnection(true))
            {
                using (var cmd = connection.CreateCommand())
                {
                    cmd.CommandText = "select password from users where login_name=@1";
                    cmd.Parameters.Add(cmd, "@1", DbType.String).Value = login_name;

                    using (var reader = cmd.ExecuteReader(CommandBehavior.SequentialAccess | CommandBehavior.SingleResult))
                    {
                        while (reader.Read())
                        {
                            var p = reader.GetString(0);
                            return String.Equals(Crypto.GetSha1(password), p, StringComparison.InvariantCultureIgnoreCase);
                        }
                    }
                }
            }
            return false;
        }

        public async Task<DirectoryEntry> RetrieveEntry(string uid, string fqdn, CancellationToken cancellationToken)
        {
            var entry = new DirectoryEntry();
            using (var connection = await CreateConnection(true))
            {
                using (var cmd = connection.CreateCommand())
                {
                    cmd.CommandText = "select guid,login_name,password from users where guid=@1";
                    cmd.Parameters.Add(cmd, "@1", DbType.Guid).Value = Guid.Parse(uid);

                    using (var reader = cmd.ExecuteReader(CommandBehavior.SequentialAccess | CommandBehavior.SingleResult))
                    {
                        while (reader.Read())
                        {
                            var name = reader.GetString(1);
                            var e = new DirectoryEntry()
                            {
                                Id = reader.GetGuid(0).ToString(),
                                Name = name,
                                FQDN = "Local",
                                Type = EntryType.User,
                                LoginName = name,
                            };
                            e.SetAttribute("pwd", reader.GetString(2));
                            return e;
                        }
                    }
                }
            }
            return entry;
        }

        public async Task<IEnumerable<DirectoryEntry>> RetrieveAll(string fqdn)
        {
            var list = new List<DirectoryEntry>();

            using (var connection = await CreateConnection(true))
            {
                using (var cmd = connection.CreateCommand())
                {
                    cmd.CommandText = "select guid,login_name from users";

                    using (var reader = cmd.ExecuteReader(CommandBehavior.SequentialAccess | CommandBehavior.SingleResult))
                    {
                        while (reader.Read())
                        {
                            var name = reader.GetString(1).ToString();
                            var entry = new DirectoryEntry()
                            {
                                Id = reader.GetGuid(0).ToString(),
                                Name = name,
                                FQDN = "Local",
                                Type = EntryType.User,
                                LoginName = name
                            };
                        }
                    }
                }
            }

            return list;
        }

        public IEnumerable<string> GetDirectories()
        {
            return new string[] { "Local" };
        }
    }
}