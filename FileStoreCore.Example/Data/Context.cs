using FileStoreCore.Example.Data.Entities;
using FileStoreCore.Extensions;
using Microsoft.EntityFrameworkCore;

namespace FileStoreCore.Example.Data
{
    public class Context : DbContext
    {
        public DbSet<ContentEntry> ContentEntries { get; set; }
        public DbSet<Content> Contents { get; set; }
        public DbSet<GenericTest<int>> Generics { get; set; }
        public DbSet<Messurement> Messurements { get; set; }
        public DbSet<Setting> Settings { get; set; }
        public DbSet<SimpleEntity> SimpleEntities { get; set; }
        public DbSet<User> Users { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseFileStoreDatabase("my_local_db");
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            //modelBuilder.Entity<SimpleEntity>().Property(x => x.Id);
            //modelBuilder.Entity<SimpleEntity>().Property(x => x.Name);

            //modelBuilder.Entity<SimpleEntity>().ToTable("custom_SimpleEntity_table");

            //modelBuilder.Entity<Messurement>().Property(x => x.Id);
            //modelBuilder.Entity<Messurement>().Property(x => x.EntryCount);
            //modelBuilder.Entity<Messurement>().Property(x => x.CreatedOn);
            //modelBuilder.Entity<Messurement>().Property(x => x.UpdatedOn);
            //modelBuilder.Entity<Messurement>().Property(x => x.TimeRead);
            //modelBuilder.Entity<Messurement>().Property(x => x.TimeWrite);

            //modelBuilder.Entity<User>().HasMany<Content>();
            //modelBuilder.Entity<Content>().HasMany<ContentEntry>();
        }
    }
}