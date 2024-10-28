using BillSplitter.DBContext;
using BillSplitter.Operations;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace BillSplitter;

public static class MauiProgram
{
	public static MauiApp CreateMauiApp()
	{
		var builder = MauiApp.CreateBuilder();
		builder
			.UseMauiApp<App>()
			.ConfigureFonts(fonts =>
			{
				fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
				fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
			});
 var connectionString = "Server=tcp:st10263888.database.windows.net,1433;Initial Catalog=HAckathon;Persist Security Info=False;User ID=nandi;Password=N@ndi123;MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;";
            builder.Services.AddDbContext<ApplicationDbContext>(options =>
                options.UseSqlServer(connectionString));
				builder.Services.AddScoped<UserService>();
builder.Services.AddScoped<BillService>();
builder.Services.AddScoped<ParticipantService>();
builder.Services.AddScoped<IOUService>();
#if DEBUG
		builder.Logging.AddDebug();
#endif

		return builder.Build();
	}
}
