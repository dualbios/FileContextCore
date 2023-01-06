using FileStoreCore.Example.Data;
using FileStoreCore.Example.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace FileStoreCore.Example
{
    internal class Program
    {
        static void Main(string[] args)
        {
            Context db = new Context();
            Console.WriteLine(db.Database.CanConnect());

            List<SimpleEntity> simpleEntities = db.SimpleEntities.ToList();

            //SimpleEntity firstOrDefault = db.SimpleEntities.FirstOrDefault();

            //List<User> users2 = db.Users.Include("Contents.Entries").Include("Contents").Include("Contents").ToList();

            //db.Users.Add(new User() { Name = "nnmnmn" });


            // db.SimpleEntities.Load();

            //db.SimpleEntities.Add(new SimpleEntity(){Id = 1, Name = "Name1"});
            //db.SimpleEntities.Add(new SimpleEntity(){Id = 2, Name = "Name2"});
            //db.SaveChanges();

            SimpleEntity? entity = db.SimpleEntities.Local.FirstOrDefault(x => x.Name.Contains("2"));
            if (entity != null)
            {
                db.SimpleEntities.Remove(entity);
            }
            else
            {
                db.SimpleEntities.Add(new SimpleEntity() { Id = 2, Name = "Name2" });
            }

            


            db.SaveChanges();
        }
    }
}