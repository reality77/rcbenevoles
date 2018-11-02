using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.Cookies;
using System.Globalization;
using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.Mvc;
using web.Filters;
using App.Metrics;
using App.Metrics.Extensions.Reporting.InfluxDB;
using App.Metrics.Extensions.Reporting.InfluxDB.Client;
using App.Metrics.Reporting.Abstractions;
using App.Metrics.Reporting.Interfaces;
using App.Metrics.Configuration;

namespace web
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            string connectionString = dal.RCBenevoleContextFactory.GetConnectionString();

            services.AddDbContext<dal.RCBenevoleContext>(options =>
                options.UseNpgsql(connectionString));


            var influxdb_database = "appmetricsdemo" ;
            var influxdb_uri = new Uri("http://influxdb:8086");

            services.AddMetrics(options =>
                {
                    options.WithGlobalTags((globalTags,info) =>
                    {
                        globalTags.Add("app", info.EntryAssemblyName);
                    });
                })
            .AddHealthChecks()
            .AddReporting(
                factory =>
                {
                    factory.AddInfluxDb(
                        new  InfluxDBReporterSettings
                        {
                            InfluxDbSettings = new InfluxDBSettings(influxdb_database, influxdb_uri),
                            ReportInterval = TimeSpan.FromSeconds(5)
                        });
                })
            .AddMetricsMiddleware(/*options => options.IgnoredHttpStatusCodes = new[] { 404 }*/);

            services.AddMvc(options =>
            {
                options.Filters.Add(new RequestLogFilter());
                options.Filters.Add(new MaintenanceModeFilter());
                options.AddMetricsResourceFilter();
            });

            services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
                .AddCookie(opt => {
                    opt.Cookie.Name = "rcbene_auth";
                    opt.Cookie.Expiration = TimeSpan.FromHours(2);
                    opt.Cookie.SameSite = Microsoft.AspNetCore.Http.SameSiteMode.Strict;
                    opt.LoginPath = "/Home";
                    opt.LogoutPath = "/Account/Logout";
                });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, IApplicationLifetime lifetime)
        {
			// Pour utilisation d'un sous-répertoire via nginx (rendre configurable pour docker par variable d'environnement)
			var pathBase = Environment.GetEnvironmentVariable("APP_PATH_BASE");
			if(!string.IsNullOrEmpty(pathBase))
				app.UsePathBase(pathBase);

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
            }

            app.UseStaticFiles();

            // Pour que l'authentification soit bien gérée depuis un reverse-proxy. A appeler avant app.UseAuthentication():
			app.UseForwardedHeaders(new ForwardedHeadersOptions
			{
				   ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto
			});

            app.UseRequestLocalization(BuildLocalizationOptions());
            
            app.UseAuthentication();

            app.UseMetrics();
            app.UseMetricsReporting(lifetime);

            app.UseMvc(routes =>
            {
                routes.MapRoute(
                    name: "default",
                    template: "{controller=Home}/{action=Index}/{id?}");
            });


            // Astuce pour appeler une méthode de SeedData
            var serviceScopeFactory = app.ApplicationServices.GetRequiredService<IServiceScopeFactory>();
            serviceScopeFactory.SeedData();
        }

        private RequestLocalizationOptions BuildLocalizationOptions()
        {
            var supportedCultures = new List<CultureInfo>
            {
                new CultureInfo("fr-FR"),
                new CultureInfo("en-US"),
                new CultureInfo("es-ES"),
                new CultureInfo("de-DE"),
            };
 
            var options = new RequestLocalizationOptions {
                DefaultRequestCulture = new RequestCulture("fr-FR"),
                SupportedCultures = supportedCultures,
                SupportedUICultures = supportedCultures
            };
 
            return options;
        }
    }
}
