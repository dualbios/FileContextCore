using FileStoreCore.Example.Data.Entities;
using FileStoreCore.Extensions;
using Microsoft.EntityFrameworkCore;

namespace FileStoreCore.Example.Data
{
    public class Context : DbContext
    {
        //public DbSet<User> Users { get; set; }

        //public DbSet<Content> Contents { get; set; }

        //public DbSet<ContentEntry> ContentEntries { get; set; }

        //public DbSet<Setting> Settings { get; set; }

        //public DbSet<Messurement> Messurements { get; set; }

        //public DbSet<GenericTest<int>> Generics { get; set; }
        public DbSet<SimpleEntity> SimpleEntities { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseFileStoreDatabase();
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<SimpleEntity>().Property(x => x.Id);
            modelBuilder.Entity<SimpleEntity>().Property(x => x.Name);

            //modelBuilder.Entity<User>()
            //    .ToTable("custom_user_table");
        }
    }
}
