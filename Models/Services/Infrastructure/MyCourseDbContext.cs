using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using MyCourse.Models.Entities;

namespace MyCourse.Models.Services.Infrastructure
{
    public partial class MyCourseDbContext : DbContext
    {
        public MyCourseDbContext(DbContextOptions<MyCourseDbContext> options)
            : base(options)
        {
        }

        public virtual DbSet<Course> Courses { get; set; }
        public virtual DbSet<Lesson> Lessons { get; set; }

        //protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        //{
        //    if (!optionsBuilder.IsConfigured)
        //    {
        //        //#warning To protect potentially sensitive information in your connection string, you should move it out of source code. See http://go.microsoft.com/fwlink/?LinkId=723263 for guidance on storing connection strings.
        //        optionsBuilder.UseSqlite("Data Source=Data/MyCourse.db");
        //    }
        //}

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.HasAnnotation("ProductVersion", "2.2.6-servicing-10079");

            modelBuilder.Entity<Course>(entity =>
            {
                entity.ToTable("Courses");
                entity.HasKey(course => course.Id);

                entity.Property(course => course.RowVersion).IsRowVersion();

                entity.OwnsOne(course => course.CurrentPrice, builder => {
                    builder.Property(money => money.Currency)
                        .HasConversion<string>()
                        .HasColumnName("CurrentPrice_Currency"); //Suprelfuo perché i nomi di colonne sono già convenzionali
                    builder.Property(money => money.Amount)
                        .HasConversion<float>()
                        .HasColumnName("CurrentPrice_Amount"); //Suprelfuo perché i nomi di colonne sono già convenzionali
                });

                entity.OwnsOne(course => course.FullPrice, builder => {
                    builder.Property(money => money.Currency).HasConversion<string>();
                });

                entity.HasMany(course => course.Lessons)
                    .WithOne(lesson => lesson.Course)
                    .HasForeignKey(lesson => lesson.CourseId); 


                #region MAPPING generato automaticamente
                //entity.Property(e => e.Id).ValueGeneratedNever();

                //entity.Property(e => e.Author)
                //    .IsRequired()
                //    .HasColumnType("TEXT (100)");

                //entity.Property(e => e.CurrentPriceAmount)
                //    .IsRequired()
                //    .HasColumnName("CurrentPrice_Amount")
                //    .HasColumnType("NUMERIC")
                //    .HasDefaultValueSql("0");

                //entity.Property(e => e.CurrentPriceCurrency)
                //    .IsRequired()
                //    .HasColumnName("CurrentPrice_Currency")
                //    .HasColumnType("TEXT (3)")
                //    .HasDefaultValueSql("'EUR'");

                //entity.Property(e => e.Description).HasColumnType("TEXT (10000)");

                //entity.Property(e => e.Email).HasColumnType("TEXT (100)");

                //entity.Property(e => e.FullPriceAmount)
                //    .IsRequired()
                //    .HasColumnName("FullPrice_Amount")
                //    .HasColumnType("NUMERIC")
                //    .HasDefaultValueSql("0");

                //entity.Property(e => e.FullPriceCurrency)
                //    .IsRequired()
                //    .HasColumnName("FullPrice_Currency")
                //    .HasColumnType("TEXT (3)")
                //    .HasDefaultValueSql("'EUR'");

                //entity.Property(e => e.ImagePath).HasColumnType("TEXT (100)");

                //entity.Property(e => e.Title)
                //    .IsRequired()
                //    .HasColumnType("TEXT (100)");
                #endregion MAPPING generato automaticamente
            });

            modelBuilder.Entity<Lesson>(entity =>
            {
                entity.HasOne(lesson => lesson.Course)
                    .WithMany(course => course.Lessons);


                #region MAPPING generato automaticamente
                //entity.Property(e => e.Id).ValueGeneratedNever();

                //entity.Property(e => e.Description).HasColumnType("TEXT (10000)");

                //entity.Property(e => e.Duration)
                //    .IsRequired()
                //    .HasColumnType("TEXT (8)")
                //    .HasDefaultValueSql("'00:00:00'");

                //entity.Property(e => e.Title)
                //    .IsRequired()
                //    .HasColumnType("TEXT (100)");

                //entity.HasOne(d => d.Course)
                //    .WithMany(p => p.Lessons)
                //    .HasForeignKey(d => d.CourseId);
                #endregion MAPPING generato automaticamente
            });
        }
    }
}
