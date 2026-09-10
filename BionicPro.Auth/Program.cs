using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.DataProtection; 
using Microsoft.IdentityModel.Protocols.OpenIdConnect;

namespace BionicPro.Auth
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            builder.Services.AddHttpClient();
            builder.Services.AddScoped<CookieRefresher>();

            builder.Services.AddDataProtection()
                .PersistKeysToFileSystem(new DirectoryInfo(@"/app/shared-auth-keys/"))
                .SetApplicationName("bionicpro_shared_auth");

            builder.Services.AddCors(options =>
            {
                options.AddPolicy("AllowAll", policy =>
                {
                    policy.SetIsOriginAllowed(origin => true)
                          .AllowAnyMethod()
                          .AllowAnyHeader()
                          .AllowCredentials();
                });
            });

            builder.Services.AddControllers();

            builder.Services.AddAuthentication(options =>
            {
                options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = OpenIdConnectDefaults.AuthenticationScheme;
            })
            .AddCookie(CookieAuthenticationDefaults.AuthenticationScheme, options =>
            {
                options.Cookie.Name = "reports_session";
                options.Cookie.HttpOnly = true;
                options.ExpireTimeSpan = TimeSpan.FromMinutes(30);
                options.SlidingExpiration = true;
                options.Cookie.SameSite = SameSiteMode.Lax;
                options.CookieManager = new ChunkingCookieManager();
                options.EventsType = typeof(CookieRefresher);
            })
            .AddOpenIdConnect(OpenIdConnectDefaults.AuthenticationScheme, options =>
            {
                options.MetadataAddress = builder.Configuration["Keycloak:MetadataAddress"];
                options.Authority = builder.Configuration["Keycloak:Authority"];
                options.ClientId = "reports-frontend";
                options.ClientSecret = "";
                options.ResponseType = OpenIdConnectResponseType.Code;
                options.PushedAuthorizationBehavior = PushedAuthorizationBehavior.Disable;
                options.UsePkce = true;
                options.RefreshOnIssuerKeyNotFound = true;
                options.RequireHttpsMetadata = false;
                options.SaveTokens = false;
                options.Scope.Clear();
                options.Scope.Add("openid");
                options.Scope.Add("profile");
                options.Scope.Add("email");

                options.Events = new OpenIdConnectEvents
                {
                    OnRedirectToIdentityProviderForSignOut = context =>
                    {
                        var logoutUri = $"{context.Options.Authority}/protocol/openid-connect/logout";
                        context.ProtocolMessage.IssuerAddress = logoutUri;
                        return Task.CompletedTask;
                    }
                };
            });

            var app = builder.Build();

            app.UseCors("AllowAll");
            app.UseRouting();

            app.UseAuthentication();
            app.UseAuthorization();

            app.MapControllers();

            app.Run();
        }
    }
}
