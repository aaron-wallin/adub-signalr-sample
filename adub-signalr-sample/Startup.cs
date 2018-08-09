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
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.Configure<CookiePolicyOptions>(options =>
            {
                // This lambda determines whether user consent for non-essential cookies is needed for a given request.
                options.CheckConsentNeeded = context => true;
                options.MinimumSameSitePolicy = SameSiteMode.None;
            });

            services.AddSignalR().AddRedis(GetRedisConnectionString());

            services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_1);

            //services.AddCors(options => options.AddPolicy("CorsPolicy", 
            //builder => 
            // {
            //    builder.AllowAnyMethod().AllowAnyHeader()
            //           .WithOrigins("https://adub-signalr-sample.apps.pcf.sandbox.cudirect.com")
            //          .AllowCredentials();
            //}));
        }

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

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Error");
            }

            app.UseStaticFiles();
            app.UseCookiePolicy();
            //app.UseCors("CorsPolicy");
            app.UseSignalR(routes =>
            {
                routes.MapHub<ChatHub>("/chatHub");
            });

            app.UseMvc();
        }
    }
}
