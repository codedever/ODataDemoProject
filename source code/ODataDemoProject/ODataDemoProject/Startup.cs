using Microsoft.AspNet.OData.Builder;
using Microsoft.AspNet.OData.Extensions;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using ODataDemoProject.Models;
using System;

namespace ODataDemoProject
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
            //读取配置文件
            AppSettings.KeyGenerator = Configuration.GetConfig<KeyGeneratorOptions>(nameof(AppSettings.KeyGenerator));
            AppSettings.AppSetting = Configuration.GetConfig<AppSetting>(nameof(AppSettings.AppSetting));
            services.AddDistributedMemoryCache();
            services.AddHttpContextAccessor();
            services.AddSession(options =>
            {
                //空闲多少时间后, session自动清空
                options.IdleTimeout = TimeSpan.FromHours(1);
                //options.Cookie.Expiration = TimeSpan.FromHours(1);
                //options.Cookie.HttpOnly = true;
                options.Cookie.SameSite = SameSiteMode.None;
                options.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest;
            });

            services.AddMvc(setup => setup.EnableEndpointRouting = false)
                .SetCompatibilityVersion(CompatibilityVersion.Version_2_2)
                .AddJsonOptions(setup => AppSettings.SetupJsonSerializerSetting(setup.SerializerSettings));

            //services.AddMvcCore().AddApiExplorer().AddAuthorization();

            //添加Cors服务
            Console.WriteLine($"allowed origins are: {AppSettings.AppSetting.CorsOrigins}");
            services.AddCors(options =>
            {
                options.AddPolicy("AllowSpecificOrigins", builder =>
                {
                    builder
                        .WithOrigins(AppSettings.AppSetting.CorsOrigins.Split(AppSettings.DEFAULT_SPLITER, StringSplitOptions.RemoveEmptyEntries))
                        .AllowAnyMethod()
                        .AllowAnyHeader()
                        .AllowCredentials()
                        .SetIsOriginAllowed(x => true)
                        .SetIsOriginAllowedToAllowWildcardSubdomains();
                });
            });

            services.AddOData();
            services.AddDbContext<ApplicationDbContext>(options => options.UseInMemoryDatabase(nameof(ApplicationDbContext)));
            //services.AddDbContext<ApplicationDbContext>(options => options.UseSqlServer(Configuration.GetConnectionString("ApplicationDbContext")));
            services.AddKeyGenerator(options =>
            {
                options.MachineCode = 0;
                options.BenchmarkMachineCode = 256;
                options.BenchmarkDateTime = new DateTime(2000, 1, 1);
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            //添加身份验证中间件
            app.UseSession();
            app.UseCookiePolicy(new CookiePolicyOptions()
            {
                MinimumSameSitePolicy = SameSiteMode.None
            });
            app.UseCors("AllowSpecificOrigins");

            app.UseMvc(config =>
            {
                config.EnableDependencyInjection();
                // and this line to enable OData query option, for example $filter
                config.Select().Expand().Filter().OrderBy().MaxTop(100).Count();
                var builder = new ODataConventionModelBuilder(app.ApplicationServices).BuildEdmModel<ApplicationDbContext>();
                config.MapODataServiceRoute("ODataRoute", "odata", builder.GetEdmModel());
                // uncomment the following line to Work-around for #1175 in beta1

                config.MapRoute(name: "default", template: "api/{controller}/{id?}");
            });

            app.UseFileServer();
            app.UseStaticFiles();
            app.UseMvcWithDefaultRoute();
        }
    }
}
