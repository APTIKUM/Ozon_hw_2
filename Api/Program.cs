using Application.Abstractions;
using Application.Services;
using Infrastructure.Repositories;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSingleton<IProductRepository, InMemoryProductRepository>();
builder.Services.AddScoped<IProductService, ProductService>();

builder.Services.AddControllers();
builder.Services.AddOpenApi();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.MapControllers();

app.Run();
