using System.Globalization;
using BionicPro.Auth.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;

namespace BionicPro.Auth
{
    public class CookieRefresher(
        IHttpClientFactory httpClientFactory,
        IConfiguration configuration) : CookieAuthenticationEvents
    {
        public override async Task ValidatePrincipal(CookieValidatePrincipalContext context)
        {
            var tokens = context.Properties.GetTokens();

            var expiresAtToken = tokens.FirstOrDefault(t => t.Name == "expires_at");
            if (expiresAtToken is null || !DateTimeOffset.TryParse(expiresAtToken.Value, CultureInfo.InvariantCulture, out var expiresAt))
            {
                return;
            }

            if (expiresAt > DateTimeOffset.UtcNow.AddSeconds(10))
            {
                return;
            }

            var refreshToken = tokens.FirstOrDefault(t => t.Name == OpenIdConnectParameterNames.RefreshToken)?.Value;
            if (string.IsNullOrEmpty(refreshToken))
            {
                context.RejectPrincipal();
                return;
            }

            try
            {
                var client = httpClientFactory.CreateClient();
                var tokenResponse = await client.PostAsync(
                    configuration["Keycloak:TokenAddress"],
                    new FormUrlEncodedContent(new Dictionary<string, string>
                    {
                        { "grant_type", "refresh_token" },
                        { "refresh_token", refreshToken },
                        { "client_id", "reports-frontend" }
                    })
                );

                if (!tokenResponse.IsSuccessStatusCode)
                {
                    context.RejectPrincipal();
                    return;
                }

                var payload = await tokenResponse.Content.ReadFromJsonAsync<KeycloakTokenResponse>();
                if (payload is null) return;

                var newExpiresAt = DateTimeOffset.UtcNow.AddSeconds(payload.ExpiresIn);

                context.Properties.UpdateTokenValue("access_token", payload.AccessToken);
                context.Properties.UpdateTokenValue("refresh_token", payload.RefreshToken ?? refreshToken);
                context.Properties.UpdateTokenValue("expires_at", newExpiresAt.ToString("o", CultureInfo.InvariantCulture));

                context.ShouldRenew = true;
            }
            catch
            {
                context.RejectPrincipal();
            }
        }
    }
}
