namespace Emby.Data.Entity
{
    using System;
    using System.Data.Entity;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Linq;

    public partial class ttt : DbContext
    {
        public ttt()
            : base("name=ttt")
        {
        }

        public virtual DbSet<user> users { get; set; }
        public virtual DbSet<entity> entities { get; set; }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            modelBuilder.Entity<entity>()
                .Property(e => e.name)
                .IsUnicode(false);
        }
    }
}
