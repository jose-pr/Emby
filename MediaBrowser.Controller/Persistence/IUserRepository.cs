using MediaBrowser.Controller.Entities;
using MediaBrowser.Model.Configuration;
using MediaBrowser.Model.Entities;
using MediaBrowser.Model.Users;
using MediaBrowser.Providers.Authentication;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace MediaBrowser.Controller.Persistence
{
    /// <summary>
    /// Provides an interface to implement a User repository
    /// </summary>
    public interface IUserRepository : IRepository, IDirectoriesProvider
    {
        /// <summary>
        /// Deletes the user.
        /// </summary>
        /// <param name="user">The user.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>Task.</returns>
        Task DeleteUser(User user, CancellationToken cancellationToken = default(CancellationToken));

        /// <summary>
        /// Saves the user.
        /// </summary>
        /// <param name="user">The user.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>Task.</returns>
        Task UpdateEntry(User user, CancellationToken cancellationToken = default(CancellationToken));

        /// <summary>
        /// Retrieves all users.
        /// </summary>
        /// <returns>IEnumerable{User}.</returns>
        IEnumerable<User> RetrieveAllUsers();

        Task<User> RetrieveUser(Guid guid, CancellationToken cancellationToken = default(CancellationToken));

        Task UpdateUserConfig(User user, UserConfiguration config = null, CancellationToken cancellationToken = default(CancellationToken));

        Task UpdateUserPolicy(User user, UserPolicy policy = null, CancellationToken cancellationToken = default(CancellationToken));

        Task<User> CreateUser(string rdn, string cn, IDictionary<string,string> externalDn,IEnumerable<string> memberOf = null,
            CancellationToken cancellationToken = default(CancellationToken));

        Task UpdateCommonName(Guid guid, string newName, CancellationToken cancellationToken = default(CancellationToken));

        Task UpdateUserName(Guid guid, string newName, CancellationToken cancellationToken = default(CancellationToken));

        Task UpdatePassword(Guid guid, string password, CancellationToken cancellationToken = default(CancellationToken));

    }
}
