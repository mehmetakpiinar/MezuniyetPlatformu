using MezuniyetPlatformu.Entities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MezuniyetPlatformu.DataAccess
{
    public class MezuniyetPlatformuDbContext:DbContext
    {
        public MezuniyetPlatformuDbContext(DbContextOptions<MezuniyetPlatformuDbContext> options) : base(options)
        {

        }

        public DbSet<User> Users { get; set; }
        public DbSet<UserType> UserTypes { get; set; }
        public DbSet<AlumniProfile> AlumniProfiles { get; set; }
        public DbSet<Company> Companies { get; set; }
        public DbSet<EmployerProfile> EmployerProfiles { get; set; }
        public DbSet<JobPost> JobPosts { get; set; }
        public DbSet<JobApplication> JobApplications { get; set; }
        public DbSet<ExperiencePost> ExperiencePosts { get; set; }
        public DbSet<Message> Messages { get; set; }


        // ---- ÇOK ÖNEMLİ DÜZELTME ----
        // EF Core'un "multiple cascade paths" hatasını engellemek için
        // OnModelCreating metodunu eklememiz/güncellememiz gerekiyor.

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder); // Var olan ayarlar için bu gerekli

            // User -> Message (Sender) ilişkisi için cascade delete'i kapat
            modelBuilder.Entity<Message>()
                .HasOne(m => m.SenderUser)
                .WithMany() // User sınıfında bir 'GönderilenMesajlar' listesi tutmuyorsak .WithMany() boş kalır
                .HasForeignKey(m => m.SenderUserId)
                .OnDelete(DeleteBehavior.Restrict); // Bir kullanıcı silinirse, gönderdiği mesajların ne olacağı (Restrict: Engelle/Hata Ver)

            // User -> Message (Recipient) ilişkisi için cascade delete'i kapat
            modelBuilder.Entity<Message>()
                .HasOne(m => m.RecipientUser)
                .WithMany() // User sınıfında bir 'AlınanMesajlar' listesi tutmuyorsak .WithMany() boş kalır
                .HasForeignKey(m => m.RecipientUserId)
                .OnDelete(DeleteBehavior.Restrict); // Bir kullanıcı silinirse, aldığı mesajların ne olacağı
        }
    }
}
