using HumanResources.Data;
using HumanResources.Data.Seeder;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using HumanResources.Areas.Identity.Data;
using System.Threading.Tasks;

namespace HumanResources
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.
            builder.Services.AddControllersWithViews();

            var connectionString = builder.Configuration.GetConnectionString("HumanResourcesContextConnection");

            builder.Services.AddDbContext<HumanResourcesContext>(options=> options.UseSqlServer(connectionString));

            builder.Services.AddDefaultIdentity<HumanResourcesUser>(options => options.SignIn.RequireConfirmedAccount = false)
                .AddRoles<IdentityRole>()
                .AddEntityFrameworkStores<HumanResourcesContext>();

            builder.Services.AddControllersWithViews();
            builder.Services.AddRazorPages();

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Home/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();

            app.UseAuthorization();

            app.MapControllerRoute(
                name: "default",
                pattern: "{controller=Home}/{action=Index}/{id?}");

            app.MapRazorPages();


            using (var scope = app.Services.CreateScope())
            {
                var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();

                var roles = new[] { "Admin", "Employee" , "Cliente", "Project Manager" };

                foreach (var role in roles)
                {
                    if (!await roleManager.RoleExistsAsync(role))
                    {
                        await roleManager.CreateAsync(new IdentityRole(role));
                    }
                }

            }
            using (var scope = app.Services.CreateScope())
            {
                var userManager = scope.ServiceProvider.GetRequiredService<UserManager<HumanResourcesUser>>();

                string email = "email@email.com";
                string password = "Teste123!";

                if (await userManager.FindByEmailAsync(email) == null)
                {
                    var user = new HumanResourcesUser();
                    user.UserName= email ;
                    user.Email = email;


                    await userManager.CreateAsync(user, password);

                    await userManager.AddToRoleAsync(user, "Admin");

                }
            }

            using (var scope = app.Services.CreateScope())
            {
                var services = scope.ServiceProvider;
                // Chama o método estático da sua classe seeder
                await HumanResourcesSeeder.Initialize(services);
            }


            app.Run();
        }
    }
}
