namespace Clubs.Types
{
    using Newtonsoft.Json;

    public class AddPlayerRequest
    {
        [JsonProperty("playerId")]
        public long PlayerId { get; set; }
    }
}
