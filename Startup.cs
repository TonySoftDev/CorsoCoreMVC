using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using MyCourse.Customizations.Identity;
using MyCourse.Customizations.ModelBinders;
using MyCourse.Models.Entities;
using MyCourse.Models.Enums;
using MyCourse.Models.Options;
using MyCourse.Models.Services.Application.Courses;
using MyCourse.Models.Services.Application.Lessons;
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
            services.AddRazorPages();

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

            var identityBuilder = services.AddDefaultIdentity<ApplicationUser>(options => {
                options.Password.RequireDigit = true;
                options.Password.RequiredLength = 8;
                options.Password.RequireUppercase = true;
                options.Password.RequireLowercase = true;
                options.Password.RequireNonAlphanumeric = true;
                options.Password.RequiredUniqueChars = 4;

                //Conferma dell'account
                options.SignIn.RequireConfirmedAccount = true;

                //Blocco dell'account
                options.Lockout.AllowedForNewUsers = true;
                options.Lockout.MaxFailedAccessAttempts = 5;
                options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(5);

            })
                .AddClaimsPrincipalFactory<CustomClaimPrincipalFactory>()
                .AddPasswordValidator<CommonPasswordValidator<ApplicationUser>>();

            //Usiamo ADO.NET o Entity Framework Core per l'accesso ai dati?
            var persistence = Persistence.EfCore;
            switch (persistence)
            {
                case Persistence.AdoNet:
                    services.AddTransient<ICourseService, AdoNetCourseService>();
                    services.AddTransient<ILessonService, AdoNetLessonService>();
                    services.AddTransient<IDatabaseAccessor, SqliteDatabaseAccessor>();

                    identityBuilder.AddUserStore<AdoNetUserStore>();
                    break;

                case Persistence.EfCore:
                    identityBuilder.AddEntityFrameworkStores<MyCourseDbContext>();

                    services.AddTransient<ICourseService, EfCoreCourseService>();
                    services.AddTransient<ILessonService, EfCoreLessonService>();
                    services.AddDbContextPool<MyCourseDbContext>(optionsBuilder => {
                        string connectionString = Configuration.GetSection("ConnectionStrings").GetValue<string>("Default");
                        optionsBuilder.UseSqlite(connectionString);
                    });
                    break;
            }

            services.AddTransient<ICachedCourseService, MemoryCacheCourseService>();
            services.AddTransient<ICachedLessonService, MemoryCacheLessonService>();
            services.AddSingleton<IImagePersister, MagickNetImagePersister>();
            services.AddSingleton<IEmailSender, MailKitEmailSender>();

            //Options
            services.Configure<ConnectionStringsOptions>(Configuration.GetSection("ConnectionStrings"));
            services.Configure<CoursesOptions>(Configuration.GetSection("Courses"));
            services.Configure<MemoryCacheOptions>(Configuration.GetSection("MemoryCache"));
            services.Configure<KestrelServerOptions>(Configuration.GetSection("Kestrel"));
            services.Configure<SmtpOptions>(Configuration.GetSection("Smtp"));
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

            app.UseAuthentication();
            app.UseAuthorization();

            app.UseResponseCaching();
            
            //EndpointMiddleware
            app.UseEndpoints(routeBuilder => {
                routeBuilder.MapControllerRoute("default", "{controller=Home}/{action=Index}/{id?}");
                routeBuilder.MapRazorPages();
            });

        }
    }
}
