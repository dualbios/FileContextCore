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

            //SimpleEntity firstOrDefault = db.SimpleEntities.FirstOrDefault();

            //List<User> users2 = db.Users.Include("Contents.Entries").Include("Contents").Include("Contents").ToList();

            //db.Users.Add(new User() { Name = "nnmnmn" });


            db.SimpleEntities.Add(new SimpleEntity(){Id = 1, Name = "Name1"});
            db.SaveChanges();
        }
    }
}