using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi;
using Onion.Data;
using OnionArchitecture.Data.IServices;
using OnionArchitecture.Data.Services;
using Scalar.AspNetCore;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.


// 1 Database context & 2 dependency injection for our services


//here we can register ef core dbcontext with our sql server connection string
builder.Services.AddDbContext<ApplicationDbContext>(options =>
options.UseSqlServer(builder.Configuration.GetConnectionString("DbContext")));



//userservice -> it handles login, register, and user related operations

//productservice -> it handles product related operations

builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IProductService, ProductService>();


// here we can reads the jwt section from our appsettins.json

//jwt authentication Configuration



var jwtSettings = builder.Configuration.GetSection("Jwt");


// we are use jwt bearer authentication schecme

//Header:  Authrozation: Bearer asdgdfsgdsgsdfsgdsgdsgddfgsdfgd

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;

    // a this challenge schecme it will verify when user tries to access a protected api but is not authenication
   // (it will return here 401 unauthorized error)
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;

    // default forbid scheme it will verify when user tries to access a protected api
    // but is not authorized(it will return here 403 forbidden error)
    options.DefaultForbidScheme=JwtBearerDefaults.AuthenticationScheme;
})

    // when ever i will get make the these rules to validate token
.AddJwtBearer(x =>
{
    x.TokenValidationParameters = new TokenValidationParameters
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
    Encoding.UTF8.GetBytes(jwtSettings["SecretKey"])),


        //because by default jwt token has a clock skew of 5 minutes,
        //so if you want to set it to zero,
        //you can do that by setting the clock skew to zero.
        //zero means token will expire immediately after the expiration time.
        ClockSkew = TimeSpan.Zero // Optional: Set clock skew to zero for immediate expiration
    };

    // Custom events so Scalar (and any client) displays descriptive JSON error
    // messages when an unauthenticated or unauthorized user hits the API.
    x.Events = new JwtBearerEvents
    {
        // Triggered when 401 Unauthorized occurs (token is missing, expired, or invalid)
        OnChallenge = async context =>
        {
            // Skip the default empty 401 response
            context.HandleResponse();

            context.Response.StatusCode = StatusCodes.Status401Unauthorized;
            context.Response.ContentType = "application/json";

            string message = "You are not authenticated. Please provide a valid Bearer token in the Authorization header.";

            if (context.AuthenticateFailure is SecurityTokenExpiredException expiredEx)
            {
                message = $"Token has expired at {expiredEx.Expires:u}. Please login again to obtain a new token.";
            }
            else if (context.AuthenticateFailure is SecurityTokenInvalidSignatureException)
            {
                message = "Token signature is invalid. The token may have been tampered with or signed with an unknown key.";
            }
            else if (context.AuthenticateFailure != null)
            {
                message = $"Authentication failed: {context.AuthenticateFailure.Message}";
            }
            else if (!string.IsNullOrEmpty(context.ErrorDescription))
            {
                message = context.ErrorDescription;
            }

            var response = new
            {
                statusCode = StatusCodes.Status401Unauthorized,
                error = "Unauthorized",
                message = message,
                timestamp = DateTime.UtcNow
            };

            await context.Response.WriteAsJsonAsync(response);
        },

        // Triggered when 403 Forbidden occurs (user is authenticated, but their role/policy is not allowed)
        OnForbidden = async context =>
        {
            context.Response.StatusCode = StatusCodes.Status403Forbidden;
            context.Response.ContentType = "application/json";

            var response = new
            {
                statusCode = StatusCodes.Status403Forbidden,
                error = "Forbidden",
                message = "Access denied. You do not have permission to access this resource. Required role is missing.",
                timestamp = DateTime.UtcNow
            };

            await context.Response.WriteAsJsonAsync(response);
        }
    };
});



//Role based authorization policies
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("AdminOnly", policy => policy.RequireRole("Admin"));
    options.AddPolicy("MainUser", policy => policy.RequireRole("MainUser"));
    // Add more policies as needed
});





















builder.Services.AddControllers();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi

// open api configuration for scalar



builder.Services.AddOpenApi(options =>
{
    options.AddDocumentTransformer((document, context, cancellationToken) =>
    {
        document.Info = new OpenApiInfo
        {
            Title = "Demo API",
            Version = "v1"
        };

        // here we  can define the jwt bearer authentication scheme so that scaler ui show an
        // authrorizae option where you can paste or enter your jwt token
        // and it will be sent in the header of every request to the api
        // then scalar automatically send the token in the header of every request to the api

        //Authorization: Bearer <token>

        var securityScheme = new OpenApiSecurityScheme
        {
            Name = "Authorization",
            Description = "Enter JWT Bearer token: Bearer {token}",
            In = ParameterLocation.Header,// it means the token will be sent in the header
                                          // of every request to the api
            Type = SecuritySchemeType.Http,
            Scheme = "Bearer",
            BearerFormat = "JWT"
        };

        document.Components ??= new OpenApiComponents();
        document.Components.SecuritySchemes ??= new Dictionary<string, IOpenApiSecurityScheme>();
        document.Components.SecuritySchemes["Bearer"] = securityScheme;


        var requirement = new OpenApiSecurityRequirement
        {
            [new OpenApiSecuritySchemeReference("Bearer", document)] = new List<string>()
        };

        document.Security ??= new List<OpenApiSecurityRequirement>();
        document.Security.Add(requirement);

        return Task.CompletedTask;
    });


});


//Cors(For angular frontend)
// here we can define the cors policy to allow our angular frontend to access our api

//beacuse angular frontend will run on a different port than our api,
//so we need to allow it to access our api
//allow any origin, any method, and any header to access our api

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAngularApp", policy =>
    {
        policy.WithOrigins("https://localhost:4200") // Replace with your Angular app's URL
              .AllowAnyMethod()
              .AllowAnyHeader()
              .AllowCredentials();
    });
});

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

app.UseCors("AllowAngularApp"); // Use the defined CORS policy

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
