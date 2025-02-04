using System.Text.Json;
using System.Text.Json.Serialization;
using MHClub.Domain;
using MHClub.Utils;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;

namespace MHClub;

public class Startup
{
    private IConfiguration Configuration { get; }

    public Startup(IConfiguration configuration)
    {
        Configuration = configuration;

    }

    public void ConfigureServices(IServiceCollection services)
    {
        services.AddCors(options =>
        {
            options.AddPolicy(name: "_MyPolicy", policy => policy
                    .AllowAnyHeader()
                    .AllowAnyMethod()
                    .SetIsOriginAllowed((host) => true)
                    .AllowCredentials()
                    .WithExposedHeaders(
                        "X-Pagination") // if you want to add any additional headers - place them here, parameter is string[]
            );
        });

        var connection = Configuration.GetConnectionString("DefaultConnection")!;
        services.AddMvc();
        services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme).AddCookie(options => options.LoginPath = "/Auth/Login");
        services.AddDbContext<ApplicationDbContext>(options => options.UseNpgsql(connection));
        AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);
        services.AddControllers(options => options.AllowEmptyInputInBodyModelBinding = true);
        services.AddControllers().AddJsonOptions(opt =>
        {
            opt.JsonSerializerOptions.Converters.Add(new JsonDateTimeConverter());
            opt.JsonSerializerOptions.DefaultIgnoreCondition =
                JsonIgnoreCondition.WhenWritingNull;
            opt.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
            opt.JsonSerializerOptions.PropertyNameCaseInsensitive = true;
            opt.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
        });
        services.AddControllersWithViews();
    }

    public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
    {
        app.UseCors("_MyPolicy");
        app.UseStaticFiles();
        
        if (env.IsDevelopment())
        {
            app.UseDeveloperExceptionPage();
        }
        else
        {
            app.UseHttpsRedirection();
        }

        app.UseAuthentication();
        app.UseRouting();
        app.UseAuthorization();
        app.UseEndpoints(endpoints =>
        {
            endpoints.MapControllers();
        });
    }
}