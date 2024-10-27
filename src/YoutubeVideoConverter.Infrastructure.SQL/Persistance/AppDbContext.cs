using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YoutubeVideoConverter.Infrastructure.SQL.Models;

namespace YoutubeVideoConverter.Infrastructure.SQL.Persistance
{
    public class AppDbContext : DbContext
    {

        public DbSet<ConvertLog> ConversionLogs { get; set; }
        public DbSet<UserRequestResponse> UserRequestResponses { get; set; }
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        { }

        //protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        //{
        //    if (!optionsBuilder.IsConfigured)
        //    {
        //        optionsBuilder.UseSqlServer("Server=localhost\\SQLEXPRESS01;Database=YoutubeConverter;TrustServerCertificate=True;Trusted_Connection=True;MultipleActiveResultSets=true");
        //    }
        //}

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<ConvertLog>()
               .Property(b => b.Url)
               .IsRequired(true);

            modelBuilder.Entity<ConvertLog>()
               .Property(b => b.VideoName)
               .IsRequired(false);

            modelBuilder.Entity<ConvertLog>()
               .Property(b => b.ConversionTime)
               .HasDefaultValue(TimeSpan.Zero);

            modelBuilder.Entity<ConvertLog>()
                .Property(b => b.ConversionDestination)
                .HasConversion<string>();

            modelBuilder.Entity<ConvertLog>()
               .Property(b => b.ConvertTo)
               .HasConversion<string>();


            modelBuilder.Entity<UserRequestResponse>()
               .Property(b => b.Message)
               .HasMaxLength(1000);

            modelBuilder.Entity<UserRequestResponse>()
               .Property(b => b.Username)
               .HasMaxLength(50);
        }
    }
}