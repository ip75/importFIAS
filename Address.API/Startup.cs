using Address.API.Import.Xml;
using Address.API.Model;
using CommonExtensions;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;

namespace Address.API
{
    public class Startup
    {
        readonly string _corsPolicy = "CorsPolicy";
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddCors(options =>
            {
                options.AddPolicy(_corsPolicy,
                    builder => builder.AllowAnyOrigin()
                        .AllowAnyMethod()
                        .AllowAnyHeader());
            });
            Localization(services);
            services.Configure<Configuration>(Configuration.GetSection("config"));
            var postgresqlConnectionString = Configuration.GetConnectionString("Default") ?? Environment.GetEnvironmentVariable("DB_CONNECTION_STRING") ?? Configuration.GetConnectionString("fias");

            services.AddDbContext<fiasContext>(options => options.UseNpgsql(postgresqlConnectionString));

            services.AddRouting(routeOptions =>
            {
                routeOptions.AppendTrailingSlash = false;
                routeOptions.SuppressCheckForUnhandledSecurityMetadata = true;
            });

            // https://docs.microsoft.com/en-us/aspnet/core/migration/22-to-30?view=aspnetcore-3.1&tabs=visual-studio#use-mvc-without-endpoint-routing
            services.AddControllers(options => options.EnableEndpointRouting = true); // If the app requires legacy IRouter support, disable EnableEndpointRouting set it to false

            services.AddHttpContextAccessor();

            services.AddSingleton<FiasDatabase>();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.Localization();

            //app.UseHttpsRedirection();
            app.UseRouting();
            app.UseCors(_corsPolicy);
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
            // return error when request path is wrong 
            app.Run(async (handler ) => 
            {
                var response = handler.Response;
                response.StatusCode = StatusCodes.Status405MethodNotAllowed;
                await response.WriteAsync($"{handler.Request.Method} request with action {handler.Request.Path} not found. Please check request parameters.");
            });
        }
        private void Localization(IServiceCollection services)
        {
            services.AddLocalization(options => options.ResourcesPath = "Resources");
            services.AddControllersWithViews()
                .AddDataAnnotationsLocalization()
                .AddViewLocalization();
        }
    }
}
