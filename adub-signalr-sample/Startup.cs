using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json.Linq;
using StackExchange.Redis;
using System;

namespace adub_signalr_sample
{
    public class Startup
    {
        public IConfiguration Configuration { get; }
        
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }
        
        public void ConfigureServices(IServiceCollection services)
        {
            // Not sure why, but seem to need to configure corspolicybuilder here as well as
            // in Configure method.  Adding same builder configuration.
            services.AddCors(options => options.AddPolicy("CorsPolicy", builder =>
            {
                builder.AllowAnyHeader()
                        .AllowAnyMethod()
                        .WithOrigins("https://adub-signalr-client.apps.pcf.sandbox.cudirect.com")
                        .AllowCredentials();
            }));

            // Add distributed cache which is required for session state
            services.AddDistributedMemoryCache();

            // Add session service and specify JSESSIONID for cookie name
            // This is used during negotiation phase for sticky sessions on PCF
            // Not ideal, but seems to be required by SignalR for negotiation
            services.AddSession(options =>
            {
                options.IdleTimeout = TimeSpan.FromSeconds(30);
                options.Cookie.HttpOnly = true;
                options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
                options.Cookie.Name = "JSESSIONID";
            });

            // Add SignalR service with redis backplane
            services.AddSignalR().AddRedis(GetRedisConnectionString());
        }

        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            // Not sure why, but seem to need to configure corspolicybuilder here as well as
            // in ConfigureServices method. Adding same builder configuration.
            app.UseCors(builder => 
            {
                builder.AllowAnyHeader()
                        .AllowAnyMethod()
                        .WithOrigins("https://adub-signalr-client.apps.pcf.sandbox.cudirect.com")
                        .AllowCredentials();
            });

            if (env.IsDevelopment())
                app.UseDeveloperExceptionPage();
            else
                app.UseExceptionHandler("/Error");

            // Use session service
            app.UseSession();

            // Custom use method to add a value to the session cookie
            app.Use(async (context, next) =>
            {
                var sessionGuid = Guid.NewGuid().ToString();
                Console.WriteLine("Setting sessionid " + sessionGuid);
                context.Session.SetString("JSESSIONID", sessionGuid);
                await next();
            });
    
            // Map SignalR routes
            app.UseSignalR(routes =>
            {
                routes.MapHub<ChatHub>("/chatHub");
            });
        }

        // Retreive connection string to PCF managed Redis cache
        private string GetRedisConnectionString()
        {
            var credentials = "$..[?(@.name=='signalr-sync-cache')].credentials";
            var jObj = JObject.Parse(Environment.GetEnvironmentVariable("VCAP_SERVICES"));

            if (jObj.SelectToken($"{credentials}") == null)
                throw new Exception("Expects a PCF managed redis cache service binding named 'signalr-sync-cache'");

            var host = (string)jObj.SelectToken($"{credentials}.host");
            var pwd = (string)jObj.SelectToken($"{credentials}.password");
            var port = (string)jObj.SelectToken($"{credentials}.port");

            Console.Out.WriteLine($"{host}:{port},password={pwd},allowAdmin=true");

            return $"{host}:{port},password={pwd},allowAdmin=true";
        }
    }
}
