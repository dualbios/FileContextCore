﻿using FileStoreCore.Example.Data;
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

            //List<User> users = db.Users.Include(x=>x.Contents).ThenInclude(x=>x.Entries).ToList();

            ContentEntry entry = db.ContentEntries.FirstOrDefault();
            db.Entry(entry).Reference(x=>x.Content).Load();
            db.Users.Load();
            User user = db.Users.FirstOrDefault();
            //string userName = entry.Content.User.Name;


            //InitDb(db);
        }

        private static void InitDb(Context db)
        {
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

            if (!db.Messurements.Any())
            {
                var messurement = new Messurement()
                {
                    Id = 1,
                    EntryCount = 12,
                    CreatedOn = DateTime.Parse("01/01/2001"),
                    UpdatedOn = DateTime.Parse("02/02/2002"),
                    TimeRead = TimeSpan.Parse("0:10"),
                    TimeWrite = TimeSpan.Parse("0:15")
                };
                db.Messurements.Add(messurement);
            }

            var user = new User()
            {
                Name = "User11",
                Username = "Username222",
                Id = 2,
                Type = User.UserType.User,
                CreatedOn = DateTime.Now,
                UpdatedOn = DateTime.MinValue,
                Contents = new List<Content>()
                {
                    new()
                    {
                        Id = 55,
                        Text = "Content Text",
                        Entries = new List<ContentEntry>()
                        {
                            new()
                            {
                                Id = 777,
                                Text = "uyiuyuiyiuyui",
                            }
                        }
                    }
                },
                Ignored = "false",
                Settings = new List<Setting>()
                {
                    new()
                    {
                        Id = 2,
                        CreatedOn = DateTime.Now,
                        UpdatedOn = DateTime.MinValue,
                        Key = "key",
                        Value = "setting value"
                    }
                }
            };

            db.Users.Add(user);

            db.SaveChanges();
        }
    }
}