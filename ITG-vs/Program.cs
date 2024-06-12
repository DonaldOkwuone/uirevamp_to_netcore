using Services;
using Services.interfaces;
using Services.utils;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorPages();
builder.Services.AddControllersWithViews();
builder.Services.AddScoped<ITimelineService, TimelineService>();
builder.Services.AddScoped<IEmailService, EmailService>();
var settings = builder.Configuration.GetSection("Configs").GetChildren().ToDictionary(x => x.Key, x => x.Value);
Settings.Initiate(settings);

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

app.MapRazorPages();
app.MapControllers();
// Optionally, map default route for MVC
app.MapDefaultControllerRoute();

app.Run();
