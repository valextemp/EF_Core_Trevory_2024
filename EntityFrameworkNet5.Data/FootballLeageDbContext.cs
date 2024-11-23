using EntityFrameworkNet5.Data.Configurations.Entities;
using EntityFrameworkNet5.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace EntityFrameworkNet5.Data
{
    public class FootballLeageDbContext : DbContext
    {
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlServer("Data Source=(localdb)\\MSSQLLocalDB; Initial Catalog=FootballLeage_EfCore")
                .LogTo(Console.WriteLine, new[] { DbLoggerCategory.Database.Command.Name }, LogLevel.Information)
                .EnableSensitiveDataLogging();
        }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // !!!!! Кнфигурация сущности перехала в TeamConfiguration
            //modelBuilder.Entity<Team>()
            //    .HasMany(m => m.HomeMatches)
            //    .WithOne(m => m.HomeTeam)
            //    .HasForeignKey(m => m.HomeTeamId)
            //    .IsRequired()
            //    .OnDelete(DeleteBehavior.Restrict);

            //modelBuilder.Entity<Team>()
            //    .HasMany(m => m.AwayMatches)
            //    .WithOne(m => m.AwayTeam)
            //    .HasForeignKey(m => m.AwayTeamId)
            //    .IsRequired()
            //    .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<TeamsCoachesLeaguesView>().HasNoKey().ToView("TeamsCoachesLeagues");

            /* Валидация */
            //!!!!!Кнфигурация сущности перехала в TeamConfiguration
            //modelBuilder.Entity<Team>().Property(p => p.Name).HasMaxLength(50);
            //modelBuilder.Entity<Team>().HasIndex(h => h.Name).IsUnique();

            modelBuilder.Entity<League>().Property(p => p.Name).HasMaxLength(50);
            modelBuilder.Entity<League>().HasIndex(h => h.Name);

            modelBuilder.Entity<Coach>().Property(p => p.Name).HasMaxLength(50);
            modelBuilder.Entity<Coach>().HasIndex(h => new { h.Name, h.TeamId }).IsUnique(); // Уникальный индекс по 2 полям

            modelBuilder.ApplyConfiguration(new TeamConfiguration());

            /*Seeding data*/
            modelBuilder.ApplyConfiguration(new LeagueConfiguration());

            modelBuilder.Entity<Team>().HasData(
                new Team { Id = 22, Name = "Valex sample team", LeagueId=20 });

            // HasData переехал в класс CoachSeedConfiguration
            //modelBuilder.Entity<Coach>().HasData(
            //    new Coach                { Id = 20, Name = "Valex", TeamId=22 });

            //теперь так заполняем тренеров
            modelBuilder.ApplyConfiguration(new CoachConfiguration());

        }

        public DbSet<Team> Teams { get; set; }
        public DbSet<League> Leagues { get; set; }
        public DbSet<Match> Matches { get; set; }
        public DbSet<Coach> Coaches { get; set; }
        public DbSet<TeamsCoachesLeaguesView> TeamsCoachesLeagues { get; set; }
    }
}
