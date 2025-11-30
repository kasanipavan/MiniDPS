using System;
using Microsoft.EntityFrameworkCore;
using Npgsql.EntityFrameworkCore.PostgreSQL;
using DocumentProcessor.Web.Models;

namespace DocumentProcessor.Web.Data;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    static AppDbContext()
    {
        AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);
    }

    public DbSet<Document> Documents { get; set; }

    protected override void OnModelCreating(ModelBuilder mb)
    {
        mb.Entity<Document>(e =>
        {
            e.HasKey(d => d.Id);
            
            // Apply table mapping with schema
            e.ToTable("documents", "dps_dbo");
            
            // Apply column mappings for all properties
            e.Property(d => d.Id).HasColumnName("id");
            e.Property(d => d.FileName).HasColumnName("filename").IsRequired().HasMaxLength(500);
            e.Property(d => d.OriginalFileName).HasColumnName("originalfilename").IsRequired().HasMaxLength(500);
            e.Property(d => d.FileExtension).HasColumnName("fileextension").HasMaxLength(50);
            e.Property(d => d.FileSize).HasColumnName("filesize");
            e.Property(d => d.ContentType).HasColumnName("contenttype").HasMaxLength(100);
            e.Property(d => d.StoragePath).HasColumnName("storagepath").HasMaxLength(1000);
            e.Property(d => d.Source).HasColumnName("source");
            e.Property(d => d.Status).HasColumnName("status");
            e.Property(d => d.DocumentTypeName).HasColumnName("documenttypename").HasMaxLength(255);
            e.Property(d => d.DocumentTypeCategory).HasColumnName("documenttypecategory").HasMaxLength(100);
            e.Property(d => d.ProcessingStatus).HasColumnName("processingstatus").HasMaxLength(50);
            e.Property(d => d.ProcessingRetryCount).HasColumnName("processingretrycount");
            e.Property(d => d.ProcessingErrorMessage).HasColumnName("processingerrormessage").HasMaxLength(1000);
            e.Property(d => d.ProcessingStartedAt).HasColumnName("processingstartedat");
            e.Property(d => d.ProcessingCompletedAt).HasColumnName("processingcompletedat");
            e.Property(d => d.ExtractedText).HasColumnName("extractedtext");
            e.Property(d => d.Summary).HasColumnName("summary");
            e.Property(d => d.UploadedAt).HasColumnName("uploadedat");
            e.Property(d => d.ProcessedAt).HasColumnName("processedat");
            e.Property(d => d.UploadedBy).HasColumnName("uploadedby").IsRequired().HasMaxLength(255);
            e.Property(d => d.CreatedAt).HasColumnName("createdat");
            e.Property(d => d.UpdatedAt).HasColumnName("updatedat");
            e.Property(d => d.IsDeleted).HasColumnName("isdeleted").HasConversion<int>();
            e.Property(d => d.DeletedAt).HasColumnName("deletedat");
            
            // Preserve existing indexes and query filter
            e.HasIndex(d => d.Status);
            e.HasIndex(d => d.UploadedAt);
            e.HasIndex(d => d.IsDeleted);
            e.HasQueryFilter(d => !d.IsDeleted);
        });
    }

    public override async Task<int> SaveChangesAsync(CancellationToken ct = default)
    {
        foreach (var e in ChangeTracker.Entries<Document>().Where(e => e.State is EntityState.Added or EntityState.Modified))
        {
            if (e.State == EntityState.Added) e.Entity.CreatedAt = DateTime.UtcNow;
            e.Entity.UpdatedAt = DateTime.UtcNow;
        }
        return await base.SaveChangesAsync(ct);
    }
}
