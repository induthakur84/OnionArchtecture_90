using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Onion.Data;
using OnionArchitecture.Data.IServices;
using OnionArchitecture.Data.Services;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.



builder.Services.AddDbContext<ApplicationDbContext>(options =>
options.UseSqlServer(builder.Configuration.GetConnectionString("DbContext")));

builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IProductService, ProductService>();


// here we can reads the jwt section from our appsettins.json


var jwtSettings = builder.Configuration.GetSection("Jwt");


// we are use jwt bearer authentication schecme

//Header:  Authrozation: Bearer asdgdfsgdsgsdfsgdsgdsgddfgsdfgd

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;

    // a this challenge schecme it will verify when user tries to access a protected api but is not authenication
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})

    // when ever i will get make the these rules to validate token
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        // Example settings, adjust as needed:


        // who created/issued the token?

        ValidateIssuer = true,

        // who is this token take?

        ValidateAudience = true,
        // it will verify like token will not expired


        ValidateLifetime = true,

        // here our will uses a scret key to create/validation the signture


        ValidateIssuerSigningKey = true,


        ValidIssuer = jwtSettings["Issuer"],
        ValidAudience = jwtSettings["Audience"],
        // why is it called symmenrict


        // Symmetric authentication Scheme measn

        // it means same scecret key is used to sign and validate the token.

          // Same Scret key
          //1 gernerate tokne
          //2/ validation token


        IssuerSigningKey = new SymmetricSecurityKey(
            System.Text.Encoding.UTF8.GetBytes(jwtSettings["Key"]))
    };
});


























builder.Services.AddControllers();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{

    app.MapOpenApi();
    app.MapScalarApiReference();
    app.MapGet("/", () => Results.Redirect("/scalar/v1"));
    app.MapGet("/scaler", () => Results.Redirect("/scalar/v1"));
}

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
