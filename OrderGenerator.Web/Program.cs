using FluentValidation;
using OrderGenerator.Application.Interfaces;
using OrderGenerator.Application.Service;
using OrderGenerator.Application.Validator;
using OrderGenerator.Fix;
using QuickFix;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();


// Validators
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

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
