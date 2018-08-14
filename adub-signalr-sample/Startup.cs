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
        private string corsPolicyName = "CorsPolicy";

        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }
        
        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddCors(options => options.AddPolicy(corsPolicyName, 
            builder => 
             {
                builder.WithOrigins("https://adub-signalr-client.apps.pcf.sandbox.cudirect.com")
                        .AllowAnyMethod()
                        .AllowAnyHeader()
                        .AllowCredentials();
                
            }));

            services.AddDistributedMemoryCache();
            services.AddSession(options =>
            {
                // Set a short timeout for easy testing.
                options.IdleTimeout = TimeSpan.FromSeconds(30);
                options.Cookie.HttpOnly = true;
                options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
                options.Cookie.Name = "JSESSIONID";
            });

            services.ConfigureApplicationCookie(options => {
                options.Cookie.Name = "JSESSIONID";
                options.Cookie.HttpOnly = true;
                options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
            });

            services.AddSignalR().AddRedis(GetRedisConnectionString());
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            Console.WriteLine("Adding cors in configure...");
            app.UseCors(builder => 
            {
                builder.AllowAnyHeader()
                        .AllowAnyMethod()
                        .WithOrigins("https://adub-signalr-client.apps.pcf.sandbox.cudirect.com")
                        .AllowCredentials();
            });

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Error");
            }

            var so = new SessionOptions();
            so.Cookie.HttpOnly = true;
            so.Cookie.SecurePolicy = CookieSecurePolicy.Always;
            so.Cookie.Name = "JSESSIONID";
            so.IdleTimeout = TimeSpan.FromSeconds(30);
            app.UseSession(options: so);

           app.Use(async (context, next) =>
           {
                 var sessionGuid = Guid.NewGuid().ToString();
                Console.WriteLine("Setting sessionid " + sessionGuid);
                context.Session.SetString("JSESSIONID", sessionGuid);
                await next();
            });
    
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
