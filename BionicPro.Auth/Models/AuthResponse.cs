using System.Text.Json.Serialization;

namespace BionicPro.Auth.Models
{
    public class AuthResponse
    {
        [JsonPropertyName("access_token")]
        public string AccessToken { get; set; } = string.Empty;

        [JsonPropertyName("refresh_token")]
        public string? RefreshToken { get; set; }
    }
}