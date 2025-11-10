using MyApp.Web.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services
builder.Services.AddControllersWithViews();
builder.Services.AddSession();
builder.Logging.AddConsole(); // show logs in VS Code terminal
builder.Logging.AddDebug(); 
// Register AuthService with HttpClient
builder.Services.AddHttpClient<AuthService>();
builder.Services.AddHttpClient();
var app = builder.Build();

app.UseStaticFiles();
app.UseRouting();
app.UseSession();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Account}/{action=Login}/{id?}");

app.Run();
