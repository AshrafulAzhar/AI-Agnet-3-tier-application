using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using MongoDB.Driver;
using UserManagement.Domain.DTOs;
using UserManagement.Domain.Interfaces;
using UserManagement.Repository;
using UserManagement.Repository.Settings;
using UserManagement.Service;
using UserManagement.Service.Settings;
using UserManagement.Service.Validators;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.Configure<MongoDbSettings>(
    builder.Configuration.GetSection("MongoDbSettings"));

builder.Services.Configure<EmailSettings>(
    builder.Configuration.GetSection("EmailSettings"));

builder.Services.AddSingleton<IMongoClient>(sp =>
{
    var settings = sp.GetRequiredService<IOptions<MongoDbSettings>>().Value;
    return new MongoClient(settings.ConnectionString);
});

builder.Services.AddScoped(sp =>
{
    var settings = sp.GetRequiredService<IOptions<MongoDbSettings>>().Value;
    var client = sp.GetRequiredService<IMongoClient>();
    return client.GetDatabase(settings.DatabaseName);
});

// Layer Registrations
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IEmailService, EmailService>();
builder.Services.AddScoped<IUserService, UserService>();

// Validator Registrations for Registration
builder.Services.AddScoped<IValidator<UserRegistrationRequest>, EmailUniquenessValidator>();
builder.Services.AddScoped<IValidator<UserRegistrationRequest>, PhoneUniquenessValidator<UserRegistrationRequest>>();
builder.Services.AddScoped<IValidator<UserRegistrationRequest>, SoftDeleteValidator>();
builder.Services.AddScoped<IValidator<UserRegistrationRequest>, PasswordPolicyValidator>();
builder.Services.AddScoped<IValidator<UserRegistrationRequest>, AgePolicyValidator<UserRegistrationRequest>>();

// Validator Registrations for Update
builder.Services.AddScoped<IValidator<UserUpdateRequest>, PhoneUniquenessValidator<UserUpdateRequest>>();
builder.Services.AddScoped<IValidator<UserUpdateRequest>, AgePolicyValidator<UserUpdateRequest>>();

builder.Services.AddControllers();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    // Enable any dev tools here if needed
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

app.Run();
