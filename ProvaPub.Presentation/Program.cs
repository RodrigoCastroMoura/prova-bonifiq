using Microsoft.EntityFrameworkCore;

using ProvaPub.App.PaymentStrategies;
using ProvaPub.App.Services;
using ProvaPub.Domain.Interfaces.Unit;
using ProvaPub.Domain.Services;
using ProvaPub.Infrastructure.Context;
using ProvaPub.Infrastructure.Unit;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Database Context - usando o namespace correto
builder.Services.AddDbContext<TestDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("ctx")));

// Unit of Work Pattern
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();

// Services
builder.Services.AddScoped<IRandomService, RandomService>();
builder.Services.AddScoped<IProductService, ProductService>();
builder.Services.AddScoped<ICustomerService, CustomerService>();
builder.Services.AddScoped<IOrderService, OrderService>();

// Utilities
builder.Services.AddScoped<IDateTimeProvider, DateTimeProvider>();

// Payment Strategy Pattern
builder.Services.AddScoped<IPaymentStrategyFactory, PaymentStrategyFactory>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

app.Run();