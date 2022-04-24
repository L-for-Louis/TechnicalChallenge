namespace Clubs.Types
{
    using Newtonsoft.Json;
    using System.Collections.Generic;

    public class RetrieveClubResponse
    {
        [JsonProperty("id")]
        public string ClubId { get; set; }

        [JsonProperty("members")]
        public List<long> PlayerIds { get; set; }
    }
}
