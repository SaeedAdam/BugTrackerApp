using BugTracker.Data;
using BugTracker.Models;
using BugTracker.Services;
using BugTracker.Services.Factories;
using BugTracker.Services.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.EntityFrameworkCore;

namespace BugTracker;

public class Program
{
    public static async Task Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        // Add services to the container.
        builder.Services.AddDbContext<ApplicationDbContext>(options =>
            options.UseNpgsql(DataUtility.GetConnectionString(builder.Configuration),
                o => o.UseQuerySplittingBehavior(QuerySplittingBehavior.SplitQuery)));

        builder.Services.AddDatabaseDeveloperPageExceptionFilter();

        builder.Services.AddIdentity<BTUser, IdentityRole>(options => options.SignIn.RequireConfirmedAccount = true)
            .AddEntityFrameworkStores<ApplicationDbContext>()
            .AddClaimsPrincipalFactory<BTUserClaimsPrincipalFactory>()
            .AddDefaultUI()
            .AddDefaultTokenProviders();

        // Custom Services
        builder.Services.AddScoped<IBTRolesService, BTRolesService>();
        builder.Services.AddScoped<IBTCompanyInfoService, BTCompanyInfoService>();
        builder.Services.AddScoped<IBTProjectService, BTProjectService>();
        builder.Services.AddScoped<IBTTicketService, BTTicketService>();
        builder.Services.AddScoped<IBTTicketHistory, BTTicketHistoryService>();
        builder.Services.AddScoped<IBTNotificationService, BTNotificationService>();
        builder.Services.AddScoped<IBTInviteService, BTInviteService>();
        builder.Services.AddScoped<IBTFileService, BTFileService>();
        builder.Services.AddScoped<IBTLookupService, BTLookupService>();
        builder.Services.AddScoped<IBTImageService, BTBasicImageService>();

        // Email Service
        builder.Services.AddScoped<IEmailSender, BTEmailService>();
        builder.Services.Configure<MailSettings>(builder.Configuration.GetSection("MailSettings"));

        builder.Services.AddControllersWithViews().AddRazorRuntimeCompilation();


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

        app.UseAuthentication();
        app.UseAuthorization();

        app.MapControllerRoute(
            "default",
            "{controller=Home}/{action=Index}/{id?}");
        app.MapRazorPages();

        // Create instance of our DataUtility and call initial migration
        //var dataService = app.Services.CreateScope().ServiceProvider.GetRequiredService<DataUtility>();
        await DataUtility.ManageDataAsync(app);


        await app.RunAsync();
    }
}