using System;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using Microsoft.Data.Entity;
using Microsoft.Data.Sqlite;

namespace Emby.Data.Entity
{
    public class EmbyPersistence : DbContext
    {

        public virtual DbSet<UserData> usersdata { get; set; }
        public virtual DbSet<BaseEntity> entities { get; set; }

        public BaseEntity this[Guid uid] { get { return null; } }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            var connectionStringBuilder = new SqliteConnectionStringBuilder { DataSource = "test.db" };
            var connectionString = connectionStringBuilder.ToString();
            var connection = new SqliteConnection(connectionString);
            
            optionsBuilder.UseSqlite(connection);
        }
    }
}
