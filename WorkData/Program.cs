using Microsoft.EntityFrameworkCore;
using WorkData.Data;
using WorkData.Repository;
using WorkData.Repository.Interface;

var builder = WebApplication.CreateBuilder(args);
var culture = new System.Globalization.CultureInfo("it-IT");
// Add services to the container.
builder.Services.AddControllersWithViews();
builder.Services.AddSqlite<AppDbContext>(builder.Configuration.GetConnectionString("DefaultConnection"));
builder.Services.AddScoped<IDeclarationValidator, DeclarationValidator>();

var app = builder.Build();

//Apply migrations at startup
using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    dbContext.Database.Migrate();
}

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

app.Run();
