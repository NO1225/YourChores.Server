using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;
using YourChores.Data.Models;

namespace YourChores.Data.DataAccess
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser,IdentityRole,string>
    {
        public ApplicationDbContext(DbContextOptions options):base(options)
        {

        }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder.Entity<Room>().HasIndex(room => room.RoomName).IsUnique();
        }

        public DbSet<Room> Rooms { get; set; }
        public DbSet<ToDoItem> ToDoItems { get; set; }
        public DbSet<RoomJoinRequest> RoomJoinRequests { get; set; }
        public DbSet<RoomUser> RoomUsers { get; set; }

    }
}
