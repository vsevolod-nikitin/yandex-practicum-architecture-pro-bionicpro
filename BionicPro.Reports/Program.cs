using Amazon.S3;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.DataProtection;

namespace BionicPro.Reports
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            var s3Config = new AmazonS3Config
            {
                ServiceURL = "http://minio:9000",
                ForcePathStyle = true
            };
            builder.Services.AddSingleton<IAmazonS3>(new AmazonS3Client("minio_admin", "minio_password", s3Config));

            builder.Services.AddDataProtection()
                .PersistKeysToFileSystem(new DirectoryInfo(@"/app/shared-auth-keys/"))
                .SetApplicationName("bionicpro_shared_auth");

            builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
                .AddCookie(CookieAuthenticationDefaults.AuthenticationScheme, options =>
                {
                    options.Cookie.Name = "reports_session";
                    options.Cookie.HttpOnly = true;
                    options.Cookie.SameSite = SameSiteMode.Lax;
                    options.CookieManager = new ChunkingCookieManager();
                });

            builder.Services.AddAuthorization();
            builder.Services.AddControllers();

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

            var app = builder.Build();

            app.UseCors("AllowAll");

            app.UseAuthentication();
            app.UseAuthorization();

            app.MapControllers();
            app.Run();
        }
    }
}
