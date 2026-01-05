using CustomAutoComplet.Components;
using CustomAutoComplet.Hubs;
using CustomAutoComplet.Repository.Contracts;
using CustomAutoComplet.Repository.Implementations;
using CustomAutoComplet.Services.Contracts;
using CustomAutoComplet.Services.Implementations;

using MudBlazor.Services;

namespace CustomAutoComplet
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Debug : afficher toutes les clés disponibles
            var config = builder.Configuration;
            Console.WriteLine("=== ConnectionStrings disponibles ===");
            foreach (var cs in config.GetSection("ConnectionStrings").GetChildren())
            {
                Console.WriteLine($"{cs.Key} = {cs.Value}");
            }
            var connectionString = config.GetConnectionString("DefaultConnection")
     ?? throw new InvalidOperationException("Chaîne de connexion 'DBConnection' non trouvée dans appsettings.json");

   
            builder.Services.AddSingleton<ISqlConnectionFactory>(
                _ => new SqlConnectionFactory(connectionString));
            // Add MudBlazor services
            builder.Services.AddMudServices();

            // Add services to the container.
            builder.Services.AddRazorComponents()
                .AddInteractiveServerComponents();
            builder.Services.AddScoped<IUserService, UserService>();
            builder.Services.AddScoped<IUserRepo, UserRepo>();


            builder.Services.AddSignalR();

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseHttpsRedirection();

            app.MapHub<UserHub>("/userHub");
            app.UseAntiforgery();
            app.MapStaticAssets();
            app.MapRazorComponents<App>()
                .AddInteractiveServerRenderMode();

            app.Run();
        }
    }
}
