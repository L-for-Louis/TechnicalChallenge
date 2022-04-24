namespace Clubs.Types
{
    using Newtonsoft.Json;

    public class CreateClubRequest
    {
        [JsonProperty("name")]
        public string ClubName { get; set; }
    }
}
