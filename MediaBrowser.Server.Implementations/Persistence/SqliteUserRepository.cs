using MediaBrowser.Controller;
using MediaBrowser.Controller.Entities;
using MediaBrowser.Controller.Persistence;
using MediaBrowser.Model.Configuration;
using MediaBrowser.Common.Data;
using MediaBrowser.Common.Data.Sql;
using MediaBrowser.Model.Logging;
using MediaBrowser.Model.Serialization;
using MediaBrowser.Model.Users;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Text;
using MediaBrowser.Model.Entities;
using MediaBrowser.Common.Security;
using MediaBrowser.Server.Implementations.Persistence;

namespace MediaBrowser.Server.Implementations.Persistence
{
    /// <summary>
    /// Class SQLiteUserRepository
    /// </summary>
    public class SqliteUserRepository : BaseSqlRepository, IUserRepository
    {
        private readonly IJsonSerializer _jsonSerializer;
        private ILogger _logger;
        private readonly string select = "select guid, select, cn, memberOf, pwd, type from local_domain";

        public SqliteUserRepository(ILogManager logManager, IServerApplicationPaths appPaths, IJsonSerializer jsonSerializer, IDbConnector dbConnector) : base(logManager, dbConnector)
        {
            _jsonSerializer = jsonSerializer;
            DbFilePath = Path.Combine(appPaths.DataPath, "users.db");
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

        public IEnumerable<string> GetDomains()
        {
            return new List<string>() { "Local" };
        }

        /// <summary>
        /// Opens the connection to the database
        /// </summary>
        /// <returns>Task.</returns>
        public async Task Initialize()
        {
            var guidCol = new SqlColumn() { Name = "guid", ColumnType = DbType.Guid };
            var userCol = new SqlColumn() { Name = "username", ColumnType = DbType.String,Length=255, Nullable = false };
            Tables.Add(new SqlTable() {
                Name = "users",
                PrimaryKey = "guid",
                Columns = new List<SqlColumn>()
                {
                    guidCol, userCol,
                    new SqlColumn(){   Name = "config", ColumnType = DbType.Binary,
                        DefaultValue = _jsonSerializer.SerializeToBytes(new UserConfiguration())
                    },
                    new SqlColumn(){   Name = "policy", ColumnType = DbType.Binary,
                        DefaultValue = _jsonSerializer.SerializeToBytes(new UserPolicy())
                    },
                    new SqlColumn(){   Name = "data", ColumnType = DbType.Binary },
                    new SqlColumn(){   Name = "name", ColumnType = DbType.String, Length = 255},
                    new SqlColumn(){   Name = "memberOf", ColumnType = DbType.Binary,
                        DefaultValue = _jsonSerializer.SerializeToBytes(new List<string>())},
                    new SqlColumn(){   Name = "type", ColumnType = DbType.Int32, DefaultValue = EntryType.User},
                    new SqlColumn(){   Name = "pwd", ColumnType = DbType.StringFixedLength, Length=40, DefaultValue = Crypto.GetSha1(String.Empty) },
                    new SqlColumn(){   Name = "localPwd", ColumnType = DbType.StringFixedLength, Length=40, DefaultValue = Crypto.GetSha1(String.Empty) }
                },
                Indexes = new List<SqlIndex>()
                {
                    new SqlIndex() { Name = "idx_users", Columns=new List<SqlColumn>() { guidCol} },
                    new SqlIndex() {Name = "idx_dn", Columns=new List<SqlColumn> { userCol } }
                }
            });
        //    await BuildDb();

            var entries = RetrieveAll("Local").Result;

            // Create root user.
            if (entries.FirstOrDefault(e => e.Type == EntryType.User) == null)
            {
                  var name = MakeValidUsername(Environment.UserName);
                    CreateUser("root",name).Wait();
            }
        }

        public async Task<IEnumerable<DirectoryEntry>> RetrieveAll(string fqdn)
        {
            return await Select("users", new string[]{"username","guid","name","memberOf" },"type = 0", GetDirectoryEntry);
        }
        public async Task<User> CreateUser(string rdn, string cn, IDictionary<string, string> externalDn = null, IEnumerable<string> memberOf = null,
            CancellationToken cancellationToken = default(CancellationToken))
        {
           
            var user = new User()
            {
                Id = Guid.NewGuid(),
                Name = cn,
                LocalUserName = rdn,
                ExternalDn = externalDn != null ? externalDn.ToDictionary(e => e.Key, e => e.Value) : new Dictionary<string, string>(),
                DateCreated = DateTime.UtcNow,
                DateModified = DateTime.UtcNow,
                DateLastSaved = DateTime.UtcNow
            };
            var record = new DbRecord()
            {
                { "guid",user.Id }, {"username",rdn }, { "name",cn},
                {"data", _jsonSerializer.SerializeToBytes(user) }
            };
            Logger.Info("RECORD DONE");

            await Insert("users", record).ConfigureAwait(false);

            return user;//await RetrieveUser(user.Id, cancellationToken).ConfigureAwait(false);
        }
        /// <summary>
        /// Save a user in the repo
        /// </summary>
        /// <param name="user">The user.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>Task.</returns>
        /// <exception cref="System.ArgumentNullException">user</exception>
        public async Task UpdateEntry(User user, CancellationToken cancellationToken)
        {
            if (user == null)
            {
                throw new ArgumentNullException("user");
            }

            cancellationToken.ThrowIfCancellationRequested();

            user.DateModified = DateTime.UtcNow;
            user.DateLastSaved = DateTime.UtcNow;

            await Update("users", new Dictionary<string, object>() {
                {"data",_jsonSerializer.SerializeToBytes(user) },
                {"config",_jsonSerializer.SerializeToBytes(user.Configuration) },
                {"policy",_jsonSerializer.SerializeToBytes(user.Policy) },
                {"gui",user.Id },
            }, "guid=@guid");
        }

        private User GetUser(DbRecord record)
        {
            Logger.Info("CREATING USER FROM RECORD");
            if (record == null) return new User();
            var user = new User();
            user = _jsonSerializer.DeserializeFromBytes<User>((byte[])record["data"]);
            user.Id = (Guid) record["guid"];
            user.LocalUserName = record["username"] as string;
            user.Name = record["name"] as string;
            user.Policy = _jsonSerializer.DeserializeFromBytes<UserPolicy>((byte[])record["policy"]);
            user.Configuration  = _jsonSerializer.DeserializeFromBytes<UserConfiguration>((byte[])record["configuration"]);
            return user;
        }

        /// <summary>
        /// Retrieve all users from the database
        /// </summary>
        /// <returns>IEnumerable{User}.</returns>
        public IEnumerable<User> RetrieveAllUsers()
        {
            return Select("users", new string[] { "username", "guid", "name", "data","policy","config" }, "type = 0", GetUser).Result;

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

            var commit = new SqlQuery("delete from users where guid=@guid")
            {
                ErrorMsg = "Failed to Delete User"
            };
            commit.AddParameter("@guid", user.Id, DbType.Guid);

            await ExecuteQuery(commit).ConfigureAwait(false);
        }

        public async Task UpdateUserPolicy(User user, UserPolicy policy =  null, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (user == null)
            {
                throw new ArgumentNullException("user");
            }

            cancellationToken.ThrowIfCancellationRequested();

            var serialized = _jsonSerializer.SerializeToBytes(policy ?? user.Policy);

            cancellationToken.ThrowIfCancellationRequested();

            var commit = new SqlQuery("UPDATE users SET policy=@policy WHERE guid=@guid")
            {
                   ErrorMsg = "Failed to Update User Policy"
            };
            commit.AddParameter("@guid", user.Id, DbType.Guid);
            commit.AddParameter("@policy", serialized, DbType.Binary);

            await ExecuteQuery(commit).ConfigureAwait(false);

        }

        public async Task UpdateUserConfig(User user, UserConfiguration config = null, CancellationToken cancellationToken=default(CancellationToken))
        {
            if (user == null)
            {
                throw new ArgumentNullException("user");
            }

            cancellationToken.ThrowIfCancellationRequested();

            var serialized = _jsonSerializer.SerializeToBytes(config ?? user.Configuration);

            cancellationToken.ThrowIfCancellationRequested();

            var commit = new SqlQuery("Update users SET config=@config where guid=@guid")
            {
                ErrorMsg = "Failed to update Configuration"
            };
            commit.AddParameter("@guid", user.Id, DbType.Guid);
            commit.AddParameter("@config", serialized, DbType.Binary);

            await ExecuteQuery(commit).ConfigureAwait(false);

        }

        public async Task<User> RetrieveUser(Guid guid, CancellationToken cancellationToken)
        {
            var query = new SqlQuery("select guid,data,policy,config,name,usernamen from users where guid=@guid");
            query.AddParameter("@guid", guid, DbType.Guid);
            var result = await ExecuteQuery(query);

            return GetUser((DbRecord) result.FirstOrDefault());
        }

        private DirectoryEntry GetDirectoryEntry(DbRecord record)
        {
            var entry = new DirectoryEntry()
            {
                UniqueId = record["guid"].ToString(),
                RDN = record["username"] as string,
                CommonName = record["name"] as string,
                FQDN = "Local",
                Type = (EntryType)(Int32.Parse(record["type"].ToString())),
                Attributes = new Dictionary<string, string>()
                {
                    { "password", record["pwd"] as string }
                }
            };
            return entry;
        }

        public static bool IsValidUsername(string username)
        {
            // Usernames can contain letters (a-z), numbers (0-9), dashes (-), underscores (_), apostrophes ('), and periods (.)
            return username.All(IsValidUsernameCharacter);
        }

        private static bool IsValidUsernameCharacter(char i)
        {
            return char.IsLetterOrDigit(i) || char.Equals(i, '-') || char.Equals(i, '_') || char.Equals(i, '\'') ||
                   char.Equals(i, '.');
        }

        public static string MakeValidUsername(string username)
        {
            if (IsValidUsername(username))
            {
                return username;
            }

            // Usernames can contain letters (a-z), numbers (0-9), dashes (-), underscores (_), apostrophes ('), and periods (.)
            var builder = new StringBuilder();

            foreach (var c in username)
            {
                if (IsValidUsernameCharacter(c))
                {
                    builder.Append(c);
                }
            }
            return builder.ToString();
        }

        public Task UpdateCommonName(Guid guid, string newName, CancellationToken cancellationToken = default(CancellationToken))
        {
            throw new NotImplementedException();
        }

        public Task UpdateUserName(Guid guid, string newName, CancellationToken cancellationToken = default(CancellationToken))
        {
            throw new NotImplementedException();
        }

        public Task UpdatePassword(Guid guid, string password, CancellationToken cancellationToken = default(CancellationToken))
        {
            throw new NotImplementedException();
        }

        public Task<bool> Authenticate(string dn, string password = "", CancellationToken cancellationToken = default(CancellationToken))
        {
            throw new NotImplementedException();
        }

        public Task<DirectoryEntry> RetrieveEntryByName(string dn, CancellationToken cancellationToken = default(CancellationToken))
        {
            throw new NotImplementedException();
        }

        public Task<DirectoryEntry> RetrieveEntryById(string uid, string fqdn = "", CancellationToken cancellationToken = default(CancellationToken))
        {
            throw new NotImplementedException();
        }
    }
}