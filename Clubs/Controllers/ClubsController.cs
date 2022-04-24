namespace Clubs.Controllers
{
    using Clubs.Tables;
    using Clubs.Types;
    using Dapper;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.Logging;
    using System;
    using System.Collections.Generic;
    using System.Data.SqlClient;
    using System.Linq;
    using System.Threading.Tasks;

    [ApiController]
    [Route("api/clubs")]
    public class ClubsController : ControllerBase
    {
        private readonly IConfiguration configuration;

        private readonly ILogger<ClubsController> logger;

        private readonly string connectionString;

        public ClubsController(IConfiguration configuration, ILogger<ClubsController> logger)
        {
            this.configuration = configuration;
            this.logger = logger;
            this.connectionString = this.configuration["ConnectionStrings:DB"];
        }

        [HttpPost]
        public async Task<ActionResult<RetrieveClubResponse>> CreateClub([FromHeader(Name = "Player-ID")] long playerId, [FromBody] CreateClubRequest body)
        {
            // Validation
            if (playerId <= 0 || string.IsNullOrEmpty(body.ClubName))
            {
                return this.BadRequest();
            }

            // Generate a new club ID
            var clubId = Guid.NewGuid().ToString().ToLower();

            try
            {
                using var db = new SqlConnection(this.connectionString);
                await db.OpenAsync();
                using var trans = await db.BeginTransactionAsync();

                const string query = @"
                    INSERT INTO [clubsdb].[dbo].[Clubs]
                                ([ClubId], [ClubName])
                    VALUES      (@ClubId, @ClubName);

                    INSERT INTO [clubsdb].[dbo].[PlayersInClub]
                                ([PlayerId] ,[ClubId])
                    VALUES      (@PlayerId, @ClubId)";

                try
                {
                    await db.ExecuteAsync(query, new { ClubId = clubId, ClubName = body.ClubName, PlayerId = playerId }, trans);
                    await trans.CommitAsync();
                }
                catch (SqlException)
                {
                    // Duplicated club name or player is joining more than 1 club
                    await trans.RollbackAsync();
                    return this.Conflict();
                }

                return this.Created(
                    $"/{ControllerContext.ActionDescriptor.AttributeRouteInfo.Template}/{clubId}",
                    new RetrieveClubResponse
                    {
                        ClubId = clubId,
                        PlayerIds = new List<long> { playerId },
                    });
            }
            catch (Exception)
            {
                return this.InternalServerError();
            }
        }

        [HttpGet("{clubId}")]
        public async Task<ActionResult<RetrieveClubResponse>> RetrieveClub(string clubId)
        {
            // Validation
            if (string.IsNullOrEmpty(clubId))
            {
                return this.BadRequest();
            }

            try
            {
                using var db = new SqlConnection(this.connectionString);

                const string query1 = @"
                    SELECT  COUNT(1)
                    FROM    [clubsdb].[dbo].[Clubs]
                    WHERE   [ClubId] = @ClubId";
                const string query2 = @"
                    SELECT  [Id], [PlayerId], [ClubId]
                    FROM    [clubsdb].[dbo].[PlayersInClub]
                    WHERE   [ClubId] = @ClubId";

                try
                {
                    // Club can be created but no one is inside it, in this case do not consider as 404, but return empty members
                    if (await db.ExecuteScalarAsync<int>(query1, new { ClubId = clubId }) == 0)
                    {
                        return this.NotFound();
                    }

                    var ret = (await db.QueryAsync<PlayerInClub>(query2, new { ClubId = clubId })).ToList();

                    return this.Ok(
                        new RetrieveClubResponse
                        {
                            ClubId = clubId,
                            PlayerIds = ret.Select(t => t.PlayerId).ToList()
                        });
                }
                catch (SqlException)
                {
                    return this.InternalServerError();
                }
            }
            catch (Exception)
            {
                return this.InternalServerError();
            }
        }

        [HttpPost("{clubId}/members")]
        public async Task<ActionResult> AddPlayer(string clubId, [FromBody] AddPlayerRequest body)
        {
            // Validation
            if (string.IsNullOrEmpty(clubId) || body.PlayerId <= 0)
            {
                return this.BadRequest();
            }

            try
            {
                using var db = new SqlConnection(this.connectionString);

                const string query = @"
                    INSERT INTO [clubsdb].[dbo].[PlayersInClub]
                                ([PlayerId] ,[ClubId])
                    SELECT      @PlayerId, [ClubId]
                    FROM        [clubsdb].[dbo].[Clubs]
                    WHERE       [ClubId] = @ClubId";

                try
                {
                    // Club id maybe not exist, so insert 0 row
                    if (await db.ExecuteAsync(query, new { body.PlayerId, ClubId = clubId }) == 0)
                    {
                        return this.NotFound();
                    }

                    return this.NoContent();
                }
                catch (SqlException)
                {
                    // Unique key violation, player joins more than 1 club
                    return this.Conflict();
                }
            }
            catch (Exception)
            {
                return this.InternalServerError();
            }
        }

        private ActionResult InternalServerError()
        {
            return this.StatusCode(StatusCodes.Status500InternalServerError);
        }
    }
}
