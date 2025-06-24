using Ludix.Data;
using Ludix.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddDbContext<LudixContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("LudixContext") ?? throw new InvalidOperationException("Connection string 'LudixContext' not found.")));

// Add services to the container.
builder.Services.AddControllersWithViews();

builder.Services.AddDefaultIdentity<IdentityUser>(options => options.SignIn.RequireConfirmedAccount = true)
   .AddRoles<IdentityRole>()
   .AddEntityFrameworkStores<LudixContext>();
builder.Services.AddScoped<IUserClaimsPrincipalFactory<IdentityUser>, UserClaimsService>();
builder.Services.AddControllersWithViews();



builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("DeveloperOrAdmin", policy =>
        policy.RequireAssertion(context =>
            context.User.HasClaim("IsAdmin", "True") ||
            context.User.HasClaim("IsDeveloper", "True")));
});


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

app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.MapRazorPages();

app.Run();
