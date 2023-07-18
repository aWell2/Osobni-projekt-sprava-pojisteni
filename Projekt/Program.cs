using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Projekt.Data;
using Projekt.Resources;

internal class Program
{
	private static async Task Main(string[] args)
	{
		var builder = WebApplication.CreateBuilder(args);

		// Add services to the container.
		var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
		builder.Services.AddDbContext<ApplicationDbContext>(options =>
			options.UseSqlServer(connectionString));
		builder.Services.AddDatabaseDeveloperPageExceptionFilter();


		builder.Services.AddDefaultIdentity<IdentityUser>(options => options.SignIn.RequireConfirmedAccount = false)
			.AddRoles<IdentityRole>()
			.AddEntityFrameworkStores<ApplicationDbContext>()
			.AddErrorDescriber<CustomIdentityErrorDescriber>();



		builder.Services.AddControllersWithViews()
			.AddMvcOptions(m =>
			{
				m.ModelBindingMessageProvider.SetValueMustBeANumberAccessor(
					fieldName => string.Format("{0} mus� b�t platn� ��slo.", fieldName));
			});


		var app = builder.Build();

		// Configure the HTTP request pipeline.
		if (app.Environment.IsDevelopment())
		{
			app.UseMigrationsEndPoint();
		}
		else
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
		name: "home",
		pattern: "{controller=Home}/{action=Index}/{id?}");

		app.MapRazorPages();
		// vytvo�� u�ivatelesk� role p�i prvn�m spu�t�n�m
		using (var scope = app.Services.CreateScope())
		{
			var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
			var roles = new[] { "admin", "User" };
			foreach (var role in roles)
			{
				if (!await roleManager.RoleExistsAsync(role))
					await roleManager.CreateAsync(new IdentityRole(role));
			}
		}
		// vytvo�� u�ivatele s u�ivatelskou roli admin p�i prvn�m spu�t�n�
		using (var scope = app.Services.CreateScope())
		{
			var UserManager = scope.ServiceProvider.GetRequiredService<UserManager<IdentityUser>>();

			string email = "admin@admin.com";
			string password = "123456aA*";

			if (await UserManager.FindByEmailAsync(email) == null)
			{
				var user = new IdentityUser();
				user.UserName = email;
				user.Email = email;
				await UserManager.CreateAsync(user, password);

				await UserManager.AddToRoleAsync(user, "admin");
			}

		}

		app.Run();
	}
}