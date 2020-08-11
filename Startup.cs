﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using MyCourse.Customizations.ModelBinders;
using MyCourse.Models.Enums;
using MyCourse.Models.Options;
using MyCourse.Models.Services.Application;
using MyCourse.Models.Services.Infrastructure;

namespace MyCourse
{
    public class Startup
    {
        public IConfiguration Configuration { get; }

        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }
        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddResponseCaching();

            services.AddMvc(options =>
            {
                var homeProfile = new CacheProfile();
                //homeProfile.Duration = Configuration.GetValue<int>("ResponseCache:Home:Duration");
                //homeProfile.Location = Configuration.GetValue<ResponseCacheLocation>("ResponseCache:Home:Location");
                //homeProfile.VaryByQueryKeys = new string[] { "page" };
                Configuration.Bind("ResponseCache:Home", homeProfile);
                options.CacheProfiles.Add("Home", homeProfile);

                options.ModelBinderProviders.Insert(0, new DecimalModelBinderProvider());

            }).SetCompatibilityVersion(CompatibilityVersion.Version_3_0)
#if DEBUG
            .AddRazorRuntimeCompilation()
#endif
            ;

            //Usiamo ADO.NET o Entity Framework Core per l'accesso ai dati?
            var persistence = Persistence.EfCore;
            switch (persistence)
            {
                case Persistence.AdoNet:
                    services.AddTransient<ICourseService, AdoNetCourseService>();
                    services.AddTransient<IDatabaseAccessor, SqliteDatabaseAccessor>();
                    break;

                case Persistence.EfCore:
                    services.AddTransient<ICourseService, EfCoreCourseService>();
                    services.AddDbContextPool<MyCourseDbContext>(optionsBuilder => {
                        string connectionString = Configuration.GetSection("ConnectionStrings").GetValue<string>("Default");
                        optionsBuilder.UseSqlite(connectionString);
                    });
                    break;
            }

            services.AddTransient<ICachedCourseService, MemoryCacheCourseService>();
            services.AddSingleton<IImagePersister, MagickNetImagePersister>();

            //Options
            services.Configure<ConnectionStringsOptions>(Configuration.GetSection("ConnectionStrings"));
            services.Configure<CoursesOptions>(Configuration.GetSection("Courses"));
            services.Configure<MemoryCacheOptions>(Configuration.GetSection("MemoryCache"));
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            //if (env.IsDevelopment())
            if (env.IsEnvironment("Development"))
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Error");
            }

            app.UseStaticFiles();

            //Nel caso volessi impostare una Culture specifica...
            /*var appCulture = CultureInfo.InvariantCulture;
            app.UseRequestLocalization(new RequestLocalizationOptions
            {
                DefaultRequestCulture = new RequestCulture(appCulture),
                SupportedCultures = new[] { appCulture }
            });*/

            //Endpoint Routing Middleware
            app.UseRouting();

            app.UseResponseCaching();

            app.UseEndpoints(routeBuilder => {
                routeBuilder.MapControllerRoute("default", "{controller=Home}/{action=Index}/{id?}");
            });

            //app.UseMvcWithDefaultRoute();
            //app.UseMvc(routeBuilder =>
            //{
            //    routeBuilder.MapRoute("default", "{controller=Home}/{action=Index}/{id?}");
            //});
            
            //app.Run(async (context) =>
            //{
            //    string nome = context.Request.Query["nome"];
            //    await context.Response.WriteAsync($"Hello {nome}!");
            //});
        }
    }
}
