using Microsoft.EntityFrameworkCore;
using DocumentManagementSystem.Model;

namespace DocumentManagementSystem.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        // DbSets para las tablas
        public DbSet<DocumentModel> Documents { get; set; }
        public DbSet<Office> Offices { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configurar la tabla Documents
            modelBuilder.Entity<DocumentModel>(entity =>
            {
                entity.ToTable("documents");
                entity.HasKey(e => e.DocumentID);
                entity.Property(e => e.DocumentID).HasColumnName("documentid");
                entity.Property(e => e.FileName).HasColumnName("filename");
                entity.Property(e => e.FilePath).HasColumnName("filepath");
                entity.Property(e => e.FileType).HasColumnName("filetype");
                entity.Property(e => e.UploadDate).HasColumnName("uploaddate");
                entity.Property(e => e.OfficeID).HasColumnName("officeid");

                // Relación con Offices
                entity.HasOne(d => d.Office)
                    .WithMany()
                    .HasForeignKey(d => d.OfficeID)
                    .OnDelete(DeleteBehavior.SetNull);
            });

            // Configurar la tabla Offices
            modelBuilder.Entity<Office>(entity =>
            {
                entity.ToTable("offices");
                entity.HasKey(e => e.OfficeID);
                entity.Property(e => e.OfficeID).HasColumnName("officeid");
                entity.Property(e => e.OfficeName).HasColumnName("officename");
            });
        }
    }
}

