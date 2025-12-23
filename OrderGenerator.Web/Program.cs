using FluentValidation;
using OrderGenerator.Application.Interfaces;
using OrderGenerator.Application.Service;
using OrderGenerator.Application.Validator;
using OrderGenerator.Fix;
using Serilog;

Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Debug()
    .MinimumLevel.Override("Microsoft", Serilog.Events.LogEventLevel.Information)
    .MinimumLevel.Override("Microsoft.AspNetCore", Serilog.Events.LogEventLevel.Warning)
    .WriteTo.Console(outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj}{NewLine}{Exception}")
    .WriteTo.File("logs/ordergenerator-.log", rollingInterval: RollingInterval.Day)
    .CreateLogger();
try
{
    Log.Information("Iniciando OrderGenerator...");

    var builder = WebApplication.CreateBuilder(args);

    builder.Host.UseSerilog();

    // Add services to the container.
    builder.Services.AddControllersWithViews();

    builder.Services.AddValidatorsFromAssemblyContaining<OrderDtoValidator>(ServiceLifetime.Singleton);

    //Singleton
    builder.Services.AddSingleton<FixApplication>();
    builder.Services.AddSingleton<IFixOrderSender, FixOrderSender>();
    builder.Services.AddSingleton<FixStart>();
    builder.Services.AddHostedService(provider =>
        provider.GetRequiredService<FixStart>());

    //Scoped
    builder.Services.AddScoped<ISendOrder, SendOrder>();

    var app = builder.Build();

    // Configure the HTTP request pipeline.
    if (!app.Environment.IsDevelopment())
    {
        app.UseExceptionHandler("/Home/Error");
        // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
        app.UseHsts();
    }

    app.UseSerilogRequestLogging();

    app.UseHttpsRedirection();
    app.UseStaticFiles();

    app.UseRouting();

    app.UseAuthorization();

    app.MapControllerRoute(
        name: "default",
        pattern: "{controller=Home}/{action=Index}/{id?}");

    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Aplicação falhou ao iniciar");
    throw;
}
finally 
{
    Log.CloseAndFlush();
}
