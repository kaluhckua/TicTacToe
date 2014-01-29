using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using TicTacToe.Models;

namespace TicTacToe.DataLayer
{
    public class DataContext : DbContext
    {

        public DbSet<Game> Games { get; set; }
        public DbSet<Guess> Guesses { get; set; }
       
        public DbSet<User> Users { get; set; }
        public DataContext()
            : base("DefaultConnection")
        {

        }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            modelBuilder.Entity<User>()
                        .Property(u => u.Username)
                        .IsRequired()
                        .HasMaxLength(40);
            modelBuilder.Entity<User>()
                        .Property(u => u.Nickname)
                        .IsRequired()
                        .HasMaxLength(30);
            modelBuilder.Entity<User>()
                       .Property(u => u.SessionKey)                     
                       .HasMaxLength(50)
                      .IsFixedLength();
            modelBuilder.Entity<User>()
                      .Property(u => u.AuthCode)
                      .IsRequired()
                      .HasMaxLength(40)
                      .IsFixedLength();
            modelBuilder.Entity<User>()
                     .Property(u => u.ConnectionId)
                     .IsRequired();

            modelBuilder.Entity<Guess>()
                 .HasRequired(g => g.Game)
                 .WithMany()
                 .HasForeignKey(g => g.GameId)
                 .WillCascadeOnDelete(true);

            //modelBuilder.Entity<Comment>()
            //            .Property(c => c.CommentText)
            //            .IsRequired();
            //modelBuilder.Entity<Comment>()
            //            .Property(c => c.Username)
            //            .IsRequired()
            //            .HasMaxLength(30);

            //modelBuilder.Entity<Place>()
            //            .Property(p => p.Name)
            //            .IsRequired()
            //            .HasMaxLength(40);
            //modelBuilder.Entity<Place>()
            //            .Property(p => p.Latitude)
            //            .IsRequired();
            //modelBuilder.Entity<Place>()
            //            .Property(p => p.Longitude)
            //            .IsRequired();

            //modelBuilder.Entity<Vote>()
            //            .Property(v => v.Value)
            //            .IsRequired();

            base.OnModelCreating(modelBuilder);
        }

    }
}
