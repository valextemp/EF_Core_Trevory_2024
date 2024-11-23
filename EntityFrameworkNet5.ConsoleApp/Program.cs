using EntityFrameworkNet5.Data;
using EntityFrameworkNet5.Data.Migrations;
using EntityFrameworkNet5.Domain;
using EntityFrameworkNet5.Domain.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Query.Internal;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace EntityFrameworkNet5.ConsoleApp
{
    internal class Program
    {
        private static readonly FootballLeageDbContext context =new FootballLeageDbContext();
        static async Task Main(string[] args)
        {
            var league = new League() { Name = "Seria A" };

            //Console.WriteLine($"engLeague Id before SaveChanges = {engLeague.Id}");
            //Console.WriteLine("-----------------------------------------------------");
            //context.Leagues.Add(engLeague);
            //context.SaveChanges();

            //Console.WriteLine($"engLeague Id after SaveChanges = {engLeague.Id}");
            //Console.WriteLine("-----------------------------------------------------");

            //await context.Leagues.AddAsync(league);
            //await context.SaveChangesAsync();

            //await AddTeamsWithLeague(league);
            //await context.SaveChangesAsync();

            //await AddTeamsWithLeagueSimultaneously();

            //await SimpleSelectQuery();

            //await QueryFilters();

            //await AdditionalExecutionMethods();

            //await AlternativeLinqSyntax();

            //await SimpleUpdateLeagueRecord();

            //await SimpleUpdateTeamRecord();

            //await SimpleDelete();
            //await DeleteWithRelationship();

            //await TrackingVsNoTracking();

            /* Adding Records with relationship*/
            //// Adding OneToMany Ralated Records
            //await AddNewTeamWithLeagueId();
            //await AddNewLeagueWithTeams();

            //// Adding ManyToMany Records
            //await AddNewMatches();

            //// Adding OneToOne Records
            //await AddNewCoach();

            /* Including Realated Data - Eager loading*/
            //await QueryRalatedRecords();

            /* Projections to Other Data Types or Ananymos Types */
            //await SelectOneProperty();
            //await AnonymousProjection();
            //await StronglyTypedProjection();

            /* Filter Based on Related Data */
            //await FilteringWithRelatedData();

            /* Querying view */
            //await QueryView();

            /* Query With Raw SQL */
            //await RawSQLQuery();

            /* Query Stored Procedure */
            //await ExecStoredProcedure();

            /* RAW SQL Non-Query Commands */
            await ExecuteNonQueryCommand();

            Console.WriteLine("Press any key To End....");
            Console.ReadKey();
        }

        private static async Task ExecuteNonQueryCommand()
        {
            var teamId1 = 22;
            var affectedRows1 = await context.Database.ExecuteSqlRawAsync("exec sp_DeleteTeamById {0}", teamId1);
        //info: 21.11.2024 23:49:39.752 RelationalEventId.CommandExecuted[20101](Microsoft.EntityFrameworkCore.Database.Command)
        // Executed DbCommand(46ms) [Parameters= [@p0 = '22'], CommandType = 'Text', CommandTimeout = '30']
        //   exec sp_DeleteTeamById @p0

            var teamId2 = 5;
            var team2 = await context.Database.ExecuteSqlInterpolatedAsync($"exec sp_DeleteTeamById {teamId2}");
        //fail: 21.11.2024 23:49:51.974 RelationalEventId.CommandError[20102](Microsoft.EntityFrameworkCore.Database.Command)
        //  Failed executing DbCommand(22ms)[Parameters = [@p0 = '5'], CommandType = 'Text', CommandTimeout = '30']
        //  exec sp_DeleteTeamById @p0
        }

        private static async Task ExecStoredProcedure()
        {
            var teamId = 3;
            var result = await context.Coaches.FromSqlRaw("Exec dbo.sp_GetTeamCoach {0}", teamId).ToListAsync();
            //Executed DbCommand(46ms) [Parameters= [p0 = '3'], CommandType = 'Text', CommandTimeout = '30']
            //  Exec dbo.sp_GetTeamCoach @p0
        }

        private static async Task RawSQLQuery()
        {
            var name = "AS Rome";
            //var teams = await context.Teams.FromSqlRaw("Select * from Teams").ToListAsync();
            //var teams = await context.Teams.FromSqlRaw($"Select * from Teams where name = '{name}'").Include(q=>q.Coach).ToListAsync();
            //SELECT[e].[Id], [e].[LeagueId], [e].[Name], [c].[Id], [c].[Name], [c].[TeamId]
            //    FROM(
            //        Select * from Teams where name = 'AS Rome'
            //          ) AS[e]
            //          LEFT JOIN[Coaches] AS[c] ON[e].[Id] = [c].[TeamId]

            var teams = await context.Teams.FromSqlInterpolated($"Select * from Teams where name = {name}").ToListAsync();
            //Executed DbCommand(48ms) [Parameters= [p0 = 'AS Rome'(Size = 4000)], CommandType = 'Text', CommandTimeout = '30']
            //Select* from Teams where name = @p0
        }

        private static async Task QueryView()
        {
            var details = await context.TeamsCoachesLeagues.ToListAsync();
        }

        private static async Task FilteringWithRelatedData()
        {
            var leagues = await context.Leagues.Where(q => q.Teams.Any(x => x.Name.Contains("Bay"))).ToListAsync();
            //var leagues = await context.Leagues.Where(q => q.Teams.Any(x => x.Name.Contains("Bay"))).Include(q => q.Teams).ToListAsync();
            //SELECT[l].[Id], [l].[Name]
            //  FROM[Leagues] AS[l]
            //  WHERE EXISTS(
            //      SELECT 1
            //      FROM[Teams] AS[t]
            //      WHERE[l].[Id] = [t].[LeagueId] AND[t].[Name] LIKE N'%Bay%')
        }

        private static async Task SelectOneProperty()
        {
            var teams = await context.Teams.Select(q => q.Name).ToListAsync();
        }

        private static async Task AnonymousProjection()
        {
            var teams = await context.Teams.Include(q => q.Coach).Select(
                q => 
                new
                {
                    TeamName= q.Name,
                    CoachName=q.Coach.Name,
                }
                ).ToListAsync();
            //SELECT[t].[Name] AS[TeamName], [c].[Name] AS[CoachName]
            //  FROM[Teams] AS[t]
            //  LEFT JOIN[Coaches] AS[c] ON[t].[Id] = [c].[TeamId]
        }

        private static async Task StronglyTypedProjection()
        {
            var teams = await context.Teams.Include(q => q.Coach).Include(q => q.League).Select(
             q =>
             new TeamDetail
             {
                 Name = q.Name,
                 CoachName = q.Coach.Name,
                 LeagueName = q.League.Name,
             }).ToListAsync();
        }

        private static async Task QueryRalatedRecords()
        {
            //// Get Many Raleted Records - Leagues -> Teams
            //var leagues = await context.Leagues.Include(q => q.Teams).ToListAsync();
            //Executed DbCommand(24ms) [Parameters= [], CommandType = 'Text', CommandTimeout = '30']
            //  SELECT[l].[Id], [l].[Name], [t].[Id], [t].[LeagueId], [t].[Name]
            //  FROM[Leagues] AS[l]
            //     LEFT JOIN[Teams] AS[t] ON[l].[Id] = [t].[LeagueId]
            //  ORDER BY[l].[Id]

            //// Get One Raleted Records - Team -> Coach
            //var team = await context.Teams
            //    .Include(q => q.Coach)
            //    .FirstOrDefaultAsync(q => q.Id == 3);
            //Executed DbCommand(28ms) [Parameters= [], CommandType = 'Text', CommandTimeout = '30']
            //  SELECT TOP(1) [t].[Id], [t].[LeagueId], [t].[Name], [c].[Id], [c].[Name], [c].[TeamId]
            //  FROM[Teams] AS[t]
            //  LEFT JOIN[Coaches] AS[c] ON[t].[Id] = [c].[TeamId]
            //  WHERE[t].[Id] = 3


            //// Get 'Grand Children' Related Record - Team -> Matches -> Home/Away Team
            //var teamsWithMatchesAndOpponents = await context.Teams
            //    .Include(q => q.AwayMatches).ThenInclude(q => q.HomeTeam)//.ThenInclude(q => q.Coach) // если нужен тренер
            //    .Include(q => q.HomeMatches).ThenInclude(q => q.AwayTeam)//.ThenInclude(q => q.Coach) // если нужен тренер
            //.FirstOrDefaultAsync(q => q.Id == 2);
            //SELECT[t0].[Id], [t0].[LeagueId], [t0].[Name], [t1].[Id], [t1].[AwayTeamId], [t1].[Date], [t1].[HomeTeamId], [t1].[Id0], [t1].[LeagueId], [t1].[Name], [t3].[Id], [t3].[AwayTeamId], [t3].[Date], [t3].[HomeTeamId], [t3].[Id0], [t3].[LeagueId], [t3].[Name]
            //  FROM(
            //      SELECT TOP(1)[t].[Id], [t].[LeagueId], [t].[Name]
            //      FROM[Teams] AS[t]
            //      WHERE[t].[Id] = 2
            //        ) AS[t0]
            //  LEFT JOIN(
            //        SELECT[m].[Id], [m].[AwayTeamId], [m].[Date], [m].[HomeTeamId], [t2].[Id] AS[Id0], [t2].[LeagueId], [t2].[Name]
            //        FROM [Matches] AS [m]
            //        INNER JOIN [Teams] AS[t2] ON [m].[HomeTeamId] = [t2].[Id]
            //        ) AS[t1] ON[t0].[Id] = [t1].[AwayTeamId]
            //  LEFT JOIN(
            //        SELECT[m0].[Id], [m0].[AwayTeamId], [m0].[Date], [m0].[HomeTeamId], [t4].[Id] AS[Id0], [t4].[LeagueId], [t4].[Name]
            //        FROM [Matches] AS [m0]
            //        INNER JOIN [Teams] AS[t4] ON [m0].[AwayTeamId] = [t4].[Id]
            //        ) AS[t3] ON[t0].[Id] = [t3].[HomeTeamId]
            //  ORDER BY[t0].[Id], [t1].[Id], [t1].[Id0], [t3].[Id]


            //// Get One Record with Related Record(s) - Team -> Matches -> Home/Away Team

            //// Get Includes with Filters
            var teams = await context.Teams
                .Where(q => q.HomeMatches.Count > 0)
                .Include(q => q.Coach)
                .ToListAsync();
            //SELECT[t].[Id], [t].[LeagueId], [t].[Name], [c].[Id], [c].[Name], [c].[TeamId]
            //  FROM[Teams] AS[t]
            //  LEFT JOIN[Coaches] AS[c] ON[t].[Id] = [c].[TeamId]
            //  WHERE(
            //  SELECT COUNT(*)
            //  FROM[Matches] AS[m]
            //  WHERE[t].[Id] = [m].[HomeTeamId]) > 0
        }
        private static async Task AddNewCoach()
        {
            var coach1 = new Coach() { Name = "Jose Mourinho", TeamId = 3 };
            await context.AddAsync(coach1);

            var coach2 = new Coach() { Name = "Antonio Conte" };
            await context.AddAsync(coach2);
            await context.SaveChangesAsync();
        }

        private static async Task AddNewMatches()
        {
            var matches = new List<Domain.Match>()
            {
                new Domain.Match
                {
                    AwayTeamId = 1,
                    HomeTeamId = 2,
                    Date = new DateTime(2021, 10, 28)
                },
                new Domain.Match
                {
                    AwayTeamId = 5,
                    HomeTeamId = 4,
                    Date = DateTime.Now
                },
                new Domain.Match
                {
                    AwayTeamId = 5,
                    HomeTeamId = 4,
                    Date = DateTime.Now
                }
            };
            await context.AddRangeAsync(matches);
            await context.SaveChangesAsync();

      //      Executed DbCommand(58ms) [Parameters= [@p0 = '1', @p1 = '2021-10-28T00:00:00.0000000', @p2 = '2', @p3 = '5', @p4 = '2024-11-19T22:20:28.5375495+03:00', @p5 = '4', @p6 = '5', @p7 = '2024-11-19T22:20:28.5399571+03:00', @p8 = '4'], CommandType = 'Text', CommandTimeout = '30']
      //SET IMPLICIT_TRANSACTIONS OFF;
      //      SET NOCOUNT ON;
      //      MERGE[Matches] USING(
      //      VALUES(@p0, @p1, @p2, 0),
      //      (@p3, @p4, @p5, 1),
      //      (@p6, @p7, @p8, 2)) AS i([AwayTeamId], [Date], [HomeTeamId], _Position) ON 1 = 0
      //WHEN NOT MATCHED THEN
      //INSERT([AwayTeamId], [Date], [HomeTeamId])
      //VALUES(i.[AwayTeamId], i.[Date], i.[HomeTeamId])
      //OUTPUT INSERTED.[Id], i._Position;
        }

            private static async Task AddNewLeagueWithTeams()
        {
            var teams = new List<Team>() {
                new Team
                {
                    Name = "Rivoli United"
                },
                new Team
                {
                    Name = "Waterhouse FC"
                },
            };
            var league = new League() { Name = "CIFA", Teams = teams };
            await context.AddAsync(league);
            await context.SaveChangesAsync();

         //   Executed DbCommand(43ms) [Parameters= [@p0 = 'CIFA'(Nullable = false)(Size = 4000)], CommandType = 'Text', CommandTimeout = '30']
         //     SET IMPLICIT_TRANSACTIONS OFF;
         //   SET NOCOUNT ON;
         //   INSERT INTO[Leagues] ([Name])
         //     OUTPUT INSERTED.[Id]
         //   VALUES(@p0);

         //Executed DbCommand(9ms) [Parameters= [@p1 = '9', @p2 = 'Rivoli United'(Nullable = false)(Size = 4000), @p3 = '9', @p4 = 'Waterhouse FC'(Nullable = false)(Size = 4000)], CommandType = 'Text', CommandTimeout = '30']
         //SET IMPLICIT_TRANSACTIONS OFF;
         //   SET NOCOUNT ON;
         //   MERGE[Teams] USING(
         //   VALUES(@p1, @p2, 0),
         //   (@p3, @p4, 1)) AS i([LeagueId], [Name], _Position) ON 1 = 0
         // WHEN NOT MATCHED THEN
         // INSERT([LeagueId], [Name])
         // VALUES(i.[LeagueId], i.[Name])
         // OUTPUT INSERTED.[Id], i._Position;

        }

        private static async Task AddNewTeamWithLeagueId()
        {
            var team = new Team() { Name = "Bayern Munich", LeagueId = 5 };
            await context.AddAsync(team);
            await context.SaveChangesAsync();
        }

        private static async Task TrackingVsNoTracking()
        {
            var withTracking = await context.Teams.FirstOrDefaultAsync(q => q.Id == 2);
            var withNoTracking = await context.Teams.AsNoTracking().FirstOrDefaultAsync(q => q.Id == 4);

            withTracking.Name = "Inter Milan";
            withNoTracking.Name = "Rivoli Unated";

            var entriesBeforeSave = context.ChangeTracker.Entries();

            await context.SaveChangesAsync();

            var entriesAfterSave= context.ChangeTracker.Entries();
        }

        private static async Task DeleteWithRelationship()
        {
            // Каскадное удаление
            var league = await context.Leagues.FindAsync(8);
            context.Leagues.Remove(league);
            await context.SaveChangesAsync();
        }

        private static async Task SimpleDelete()
        {
            var league = await context.Leagues.FindAsync(7);
            context.Leagues.Remove(league);
            await context.SaveChangesAsync();
        }

        private static async Task SimpleUpdateTeamRecord()
        {
            //// Если team определено с Id, то будел update записи при вызове метода context.Teams.Update(team);
            //var team = new Team()
            //{
            //    Id = 5,
            //    Name = "Valex Team",
            //    LeagueId = 5
            //};

            // Если team определено без Id, то будел insert !!! записи при вызове метода context.Teams.Update(team);
            var team = new Team()
            {
                Name = "Valex Team",
                LeagueId = 5
            };

            context.Teams.Update(team);
            await context.SaveChangesAsync();

        }

        private static async Task SimpleUpdateLeagueRecord()
        {
            //// Retrieve Record
            var league = await context.Leagues.FindAsync(3);

            /// Make Record Changes
            league.Name = "Scottish Primiership";

            /// Save Changes
            await context.SaveChangesAsync();

            await GetRecord();
        }

        private static async Task GetRecord()
        {
            var league = await context.Leagues.FindAsync(3);

            Console.WriteLine($"{league.Id} -- {league.Name}");
        }

        private static async Task AlternativeLinqSyntax()
        {
            var teamName = "Juventas";
            //var teams = from team in context.Teams select team;
            var teams = await (from team in context.Teams 
                               where  EF.Functions.Like(team.Name, $"%{teamName}%")
                               select team).ToListAsync();
            foreach (var team in teams)
            {
                Console.WriteLine($"{team.Id} -- {team.Name}");
            }
        }

        private static async Task AdditionalExecutionMethods()
        {
            //var l = await context.Leagues.Where(q => q.Name.Contains("A")).FirstOrDefaultAsync();
            //var l = await context.Leagues.FirstOrDefaultAsync(q => q.Name.Contains("A"));

            var legues = context.Leagues;
            var list = await legues.ToListAsync();
            var first = await legues.FirstAsync();
            var firstOrDefault = await legues.FirstOrDefaultAsync();
            //var single = await legues.SingleAsync();
            //var singleOrDefaultAsync = await legues.SingleOrDefaultAsync();

            var count=await legues.CountAsync();
            var longCount=await legues.LongCountAsync();
            var min=await legues.MinAsync();
            var max=await legues.MaxAsync();

            ////DbSet method that willexecute
            var league = await legues.FindAsync(1);
        }

        private static async Task QueryFilters()
        {
            //var lesgues = await context.Leagues.Where(league => league.Name == "Seria A").ToListAsync();

            var leagueName = "Seria A";
            //var lesgues=await context.Leagues.Where(league => league.Name == leagueName).ToListAsync();
            //var lesgues = await context.Leagues.Where(league => league.Name.Equals(leagueName)).ToListAsync();
            //var lesgues = await context.Leagues.Where(league => league.Name.Contains(leagueName)).ToListAsync();
            //var lesgues = await context.Leagues.Where(league => EF.Functions.Like(league.Name,"%Premiere%")).ToListAsync();
            var lesgues = await context.Leagues.Where(league => EF.Functions.Like(league.Name, $"%{leagueName}%")).ToListAsync();

            foreach (var league in lesgues)
            {
                Console.WriteLine($"{league.Id} -- {league.Name}");
            }
        }

        static async Task SimpleSelectQuery()
        {
            var leagues = await context.Leagues.ToListAsync();
            foreach (var league1 in leagues)
            {
                Console.WriteLine($"{league1.Id} -- {league1.Name}");
            }
        }

        private static async Task AddTeamsWithLeague(League league)
        {
            var teams = new List<Team>()
            {
                new Team()
                {
                    Name="Juventas",
                    LeagueId=league.Id
                },
                new Team()
                {
                    Name="AC Milan",
                    LeagueId=league.Id
                },
                new Team()
                {
                    Name="AS Rome",
                    League=league
                }
            };
            await context.AddRangeAsync(teams);
        }

        private static async Task AddTeamsWithLeagueSimultaneously()
        {
            var league = new League { Name = "Bundesliga" };
            var team = new Team() { Name="Bayern Munich", League=league };
            await context.AddAsync(team);
            await context.SaveChangesAsync();
        }
    }
}
