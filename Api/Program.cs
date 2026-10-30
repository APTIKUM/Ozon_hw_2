using Application.Abstractions;
using Application.Services;
using Infrastructure.Repositories;
using Api.Middleware;
using Api.Validators;
using Application.DTOs;
using FluentValidation;
using Microsoft.AspNetCore.HttpLogging;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSingleton<IProductRepository, InMemoryProductRepository>();
builder.Services.AddScoped<IProductService, ProductService>();

builder.Services.AddControllers();

builder.Services.AddScoped<IValidator<CreateProductRequest>, CreateProductRequestValidator>();
builder.Services.AddScoped<IValidator<GetProductsQuery>, GetProductsQueryValidator>();

builder.Services.AddSwaggerGen();

builder.Services.AddHttpLogging(options =>
{
    options.LoggingFields = HttpLoggingFields.RequestMethod
                            | HttpLoggingFields.RequestPath
                            | HttpLoggingFields.RequestQuery
                            | HttpLoggingFields.RequestBody
                            | HttpLoggingFields.ResponseStatusCode
                            | HttpLoggingFields.ResponseBody;
});

builder.Logging.AddFilter("Microsoft.AspNetCore.HttpLogging", LogLevel.Information);

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpLogging();

app.UseMiddleware<ExceptionHandlingMiddleware>();

app.MapControllers();

app.Run();
