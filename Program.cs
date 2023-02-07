using Microsoft.EntityFrameworkCore;
using MarketAnalytics.Data;
using Microsoft.AspNetCore.Identity;

var builder = WebApplication.CreateBuilder(args);
//var connectionString = builder.Configuration.GetConnectionString("DBContextConnection") ?? throw new InvalidOperationException("Connection string 'DBContextConnection' not found.");

// Add services to the container.
builder.Services.AddRazorPages();
builder.Services.AddDbContext<DBContext>(options => options.UseSqlite("Name=MarketDatabase"));

builder.Services.AddDefaultIdentity<IdentityUser>
    (options =>
    {
        options.SignIn.RequireConfirmedAccount = true;
        options.SignIn.RequireConfirmedEmail = true;
        options.User.RequireUniqueEmail = true;
        options.Password.RequireDigit = false;
        options.Password.RequiredLength = 6;
        options.Password.RequireNonAlphanumeric = false;
        options.Password.RequireUppercase = false;
        options.Password.RequireLowercase = false;
    })
.AddEntityFrameworkStores<DBContext>();

builder.Services.AddDatabaseDeveloperPageExceptionFilter();
var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}
else
{
    app.UseDeveloperExceptionPage();
    app.UseMigrationsEndPoint();
}

//string fetchedData = await DbInitializer.FetchMasterData();

using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;

    var context = services.GetRequiredService<DBContext>();
    context.Database.EnsureCreated();
    //DbInitializer.Initialize(context, fetchedData);
}
app.UseHttpsRedirection();
app.UseStaticFiles();


app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();

app.MapRazorPages();

//app.MapControllerRoute(
//            name: "PortfolioPages",
//            pattern: "{controller=portfoliopages}/{action=portfoliomasterindex}/{masterid?}")
//    .RequireAuthorization();

app.Run();
