﻿using MediaBrowser.Controller.Entities;
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
        Task DeleteUser(User user, CancellationToken cancellationToken);

        /// <summary>
        /// Saves the user.
        /// </summary>
        /// <param name="user">The user.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>Task.</returns>
        Task UpdateUser(User user, CancellationToken cancellationToken);

        /// <summary>
        /// Saves the user.
        /// </summary>
        /// <param name="user">The user.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>Task.</returns>
        Task CreateUser(User user, CancellationToken cancellationToken);

        /// <summary>
        /// Retrieves all users.
        /// </summary>
        /// <returns>IEnumerable{User}.</returns>
        IEnumerable<User> RetrieveAllUsers();

        Task UpdateUserPassword(Guid id, string password, CancellationToken cancellationToken);

    }
}
