using MediaBrowser.Model.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace MediaBrowser.Providers.Authentication
{
    public interface IDirectoriesProvider
    {
        Task<bool> Authenticate(string dn, string password = "", CancellationToken cancellationToken = default(CancellationToken));

        Task<DirectoryEntry> RetrieveEntryByName(string dn, CancellationToken cancellationToken = default(CancellationToken));

        Task<DirectoryEntry> RetrieveEntryById(string uid, string fqdn="", CancellationToken cancellationToken = default(CancellationToken));

        Task<IEnumerable<DirectoryEntry>> RetrieveAll(string fqdn=null);

        IEnumerable<string> GetDomains();
    }
}
