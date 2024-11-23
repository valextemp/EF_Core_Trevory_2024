using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EntityFrameworkNet5.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddingSPCoachName : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"CREATE PROCEDURE sp_GetTeamCoach
	                                @teamId int 
                                AS
                                BEGIN
	                                SELECT * from Coaches where TeamId = @teamid
                                END");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"DROP PROCEDURE [dbo].[sp_CoachName]");
        }
    }
}
