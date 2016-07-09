using MediaBrowser.Controller;
using MediaBrowser.Model.Entities;
using MediaBrowser.Model.Logging;
using MediaBrowser.Model.Serialization;
using MediaBrowser.Providers.Authentication;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using MediaBrowser.Common.Security;
/*

namespace MediaBrowser.Server.Implementations.Persistence
{
    class LocalDirectoryProvider : BaseSqliteRepository, IDirectoriesProvider
    {
        private IDbConnection _connection;
        private readonly IServerApplicationPaths _appPaths;
        private readonly ILogger _logger;
        private readonly IJsonSerializer _jsonSerializer;
        

        public LocalDirectoryProvider(ILogger logger, ILogManager logManager, IServerApplicationPaths appPaths, IDbConnector dbConnector, IJsonSerializer jsonSerializer) : base(logManager, dbConnector)
        {
            _appPaths = appPaths;
            _logger = logger;
            _jsonSerializer = jsonSerializer;
            DbFilePath = Path.Combine(appPaths.DataPath, "users.db");
            try
            {
                Initialize(dbConnector).Wait();
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
                return "LocalDirectory";
            }
        }


        
  

        public async Task<IEnumerable<DirectoryEntry>> RetrieveAll(string fqdn)
        {
            var query = new Query() { Cmd = select };
            return await Read(query, CreateDirectoryEntry);
        }






        public async Task<bool> Authenticate(string rdn, string fqdn =  "Local", string password = "", CancellationToken cancellationToken = default(CancellationToken))
        {
            var user = await RetrieveEntryByRdn(rdn, fqdn, cancellationToken).ConfigureAwait(false);
            if (user != null)
            {
                password = String.IsNullOrEmpty(password) ? "DA39A3EE5E6B4B0D3255BFEF95601890AFD80709" : Crypto.GetSha1(password);
                var pass = (user.GetAttribute("password") ?? "DA39A3EE5E6B4B0D3255BFEF95601890AFD80709");
                return string.Equals(pass, password, StringComparison.OrdinalIgnoreCase);
            }
            return false;
        }

        public async Task<DirectoryEntry> RetrieveEntry(string uid, string fqdn, CancellationToken cancellationToken = default(CancellationToken))
        {
            ValidEntry(uid, fqdn);
            var query = new Query() { Cmd = select+"WHERE uid=@uid" };
            query.AddValue("@uid", uid);
            var result = await Read(query, CreateDirectoryEntry);
            return result.FirstOrDefault();
        }
        public async Task<DirectoryEntry> RetrieveEntryByRdn(string rdn, string fqdn, CancellationToken cancellationToken = default(CancellationToken))
        {
            ValidEntry(rdn, fqdn);
            var query = new Query() { Cmd = select + "WHERE rdn=@rdn" };
            query.AddValue("@rdn", rdn);
            var result = await Read(query, CreateDirectoryEntry);
            return result.FirstOrDefault();
        }
        private DirectoryEntry CreateDirectoryEntry(IDataReader reader)
        {
            var entry = new DirectoryEntry()
            {
                UniqueId = reader["uid"] as string,
                RDN = reader["rdn"] as string,
                CommonName = reader["cn"] as string,
                FQDN = "Local",
                Type = (EntryType)(reader.GetInt32(3)),
                Attributes = new Dictionary<string, string>()
                {
                    { "password", reader["pwd"] as string }
                }
            };
            using (var stream = reader.GetMemoryStream(1))
            {
                var memberOf = _jsonSerializer.DeserializeFromStream<List<string>>(stream);
                entry.MemberOf = memberOf;
            }
            return entry;
        }
        private void ValidEntry(string uid, string fqdn)
        {
            if (fqdn != "Local")
            {
                throw new ArgumentNullException("Dont manage the domain");
            }
            if (string.IsNullOrWhiteSpace(uid))
            {
                throw new ArgumentNullException("invalid entry uid");
            }
        }

        public async Task UpdateEntry(string uid, DirectoryEntry entry, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (entry == null)
            {
                throw new ArgumentNullException("domain entry");
            }

            cancellationToken.ThrowIfCancellationRequested();

            var serialized = _jsonSerializer.SerializeToBytes(entry.MemberOf);

            cancellationToken.ThrowIfCancellationRequested();

            var commit = new Query()
            {
                Cmd = "Update local_domain SET cn = @cn, memberOf = @memberOf where cn=@uid",
                ErrorMsg = "Failed to Update directory entry"
            };

            commit.AddValue("@uid", uid);
            commit.AddValue("@memberOf", serialized);
            commit.AddValue("@cn", entry.CN);

            await Commit(commit).ConfigureAwait(false);
    
        }

        public async Task UpdateRdn(string uid, string rdn, CancellationToken cancellationToken = default(CancellationToken))
        {
           cancellationToken.ThrowIfCancellationRequested();

            var commit = new Query()
            {
                Cmd = "Update local_domain SET rdn = @rdn where uid=@uid",
                ErrorMsg = "Failed to Update directory entry"
            };

            commit.AddValue("@uid", uid);
            commit.AddValue("@rdn", rdn);

            await Commit(commit).ConfigureAwait(false);

        }

        public async Task UpdateCn(string uid, string cn, CancellationToken cancellationToken = default(CancellationToken))
        {
            cancellationToken.ThrowIfCancellationRequested();

            var commit = new Query()
            {
                Cmd = "Update local_domain SET cn = @cn where uid=@uid",
                ErrorMsg = "Failed to Update directory entry"
            };

            commit.AddValue("@uid", uid);
            commit.AddValue("@cn", cn);

            await Commit(commit).ConfigureAwait(false);

        }

        public async Task UpdatePassword(string uid, string fqdn, string password, CancellationToken cancellationToken = default(CancellationToken))
        {
            ValidEntry(uid, fqdn);
            var commit = new Query()
            {
                Cmd = "update local_domain SET pwd=@pwd where uid=@uid",
                ErrorMsg = "failed to update password"
            };
            commit.AddValue("@uid", uid);
            commit.AddValue("@pwd", Crypto.GetSha1(password));

            await Commit(commit).ConfigureAwait(false);
   
        }



    }
}
*/