using Hangfire;
using Hangfire.SqlServer;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;

namespace HangfireDashboardIdentityServer4
{
    public class Startup
    {
        private readonly IConfiguration configuration;

        public Startup(IConfiguration configuration)
        {
            this.configuration = configuration;
        }

        public void ConfigureServices(IServiceCollection services)
        {
            services
                .AddAuthorization(cfg =>
                {
                    cfg.AddPolicy("Hangfire", cfgPolicy =>
                    {
                        cfgPolicy.AddRequirements().RequireAuthenticatedUser();
                        cfgPolicy.AddAuthenticationSchemes(OpenIdConnectDefaults.AuthenticationScheme);
                    });
                })
                .AddAuthentication(cfg =>
                {
                    cfg.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
                    cfg.DefaultChallengeScheme = OpenIdConnectDefaults.AuthenticationScheme;
                })
                .AddCookie()
                .AddOpenIdConnect(cfg =>
                {
                    cfg.Authority = "https://difc-virtual-hub-dev.eu.auth0.com";
                    cfg.ClientId = "cSg9nflRCzY9aGRSrwYwDCUo1xIKkVFn";
                    cfg.ClientSecret = "lMgV_5BnWKlEG2sB_CqF1xphtPthQmgb3MrGrNThLhDZlwXE81XDYlBn1EtWxdwy";
                    cfg.ResponseType = "code";
                    cfg.UsePkce = true;

                    cfg.Scope.Clear();
                    cfg.Scope.Add("openid");
                    cfg.Scope.Add("profile");

                    cfg.SaveTokens = true;
                });

            services.AddHangfire(cfg =>
            {
                cfg.UseSqlServerStorage("Server=vh-dev.database.windows.net;Database=vh-dev;User ID=vh-admin-dev;Password=LITOT[}0&Xi03FKe;TrustServerCertificate=True;MultipleActiveResultSets=True;", new SqlServerStorageOptions
                {
                    CommandBatchMaxTimeout = TimeSpan.FromMinutes(5),
                    SlidingInvisibilityTimeout = TimeSpan.FromMinutes(5),
                    QueuePollInterval = TimeSpan.Zero,
                    UseRecommendedIsolationLevel = true,
                    DisableGlobalLocks = true
                });
            });

            services.AddHangfireServer();

            services.AddMvc();
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            app.UseStaticFiles();

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseAuthentication();
            app.UseRouting();
            app.UseAuthorization();
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapHangfireDashboard().RequireAuthorization("Hangfire");
            });
        }
    }
}
