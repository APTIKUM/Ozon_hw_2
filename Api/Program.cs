using Application.Abstractions;
using Application.Services;
using Infrastructure.Repositories;
using Api.Middleware;
using Api.Validators;
using Application.DTOs;
using FluentValidation;
using Swashbuckle.AspNetCore.SwaggerGen;
using ProductsGrpcService = Api.Grpc.ProductsGrpcService;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSingleton<IProductRepository, InMemoryProductRepository>();
builder.Services.AddScoped<IProductService, ProductService>();

builder.Services.AddControllers();
builder.Services.AddGrpc(options => options.Interceptors.Add<Api.Grpc.LoggingInterceptor>())
    .AddJsonTranscoding();

builder.Services.AddScoped<IValidator<CreateProductRequest>, CreateProductRequestValidator>();
builder.Services.AddScoped<IValidator<GetProductsQuery>, GetProductsQueryValidator>();

builder.Services.AddGrpcSwagger();
builder.Services.AddSwaggerGen(options =>
{
    // HTTP DTOs and generated gRPC messages can have the same class name.
    var defaultSchemaId = new SchemaGeneratorOptions().SchemaIdSelector;
    options.CustomSchemaIds(type => $"{type.Namespace}.{defaultSchemaId(type)}");
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseMiddleware<ExceptionHandlingMiddleware>();

app.MapControllers();
app.MapGrpcService<ProductsGrpcService>();

app.Run();
