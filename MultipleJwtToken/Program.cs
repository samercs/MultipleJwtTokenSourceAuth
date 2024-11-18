using System.Security.Claims;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Http.Json;
using Microsoft.IdentityModel.Tokens;
using MultipleJwtToken;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(CustomAuthSchemes.Keycloak, i =>
    {
        i.RequireHttpsMetadata = false;
        i.Audience = builder.Configuration["Auth:Keycloak:Audience"]!;
        i.MetadataAddress = builder.Configuration["Auth:Keycloak:MetadataUrl"]!;
        i.TokenValidationParameters = new TokenValidationParameters()
        {
            ValidIssuer = builder.Configuration["Auth:Keycloak:ValidIssuer"]!,
        };
    })
    .AddJwtBearer(CustomAuthSchemes.Supabase, options =>
    {
        byte[] bytes = Encoding.UTF8.GetBytes(builder.Configuration["Auth:Supabase:JwtSecret"]!);
        options.TokenValidationParameters = new TokenValidationParameters()
        {
            IssuerSigningKey = new SymmetricSecurityKey(bytes),
            ValidAudience = builder.Configuration["Auth:Supabase:ValidAudience"],
            ValidIssuer = builder.Configuration["Auth:Supabase:ValidIssuer"]
        };
    });

builder.Services.AddAuthorization(option =>
{
    var defaultSchema = new AuthorizationPolicyBuilder(CustomAuthSchemes.Keycloak, CustomAuthSchemes.Supabase)
        .RequireAuthenticatedUser()
        .Build();
    option.DefaultPolicy = defaultSchema;
});
builder.Services.AddMvc();
builder.Services.Configure<JsonOptions>(i =>
{
    i.SerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
    i.SerializerOptions.MaxDepth = 0;
});
builder.Services.AddTransient<IClaimsTransformation, CustomClaimTransform>();
var app = builder.Build();


app.UseAuthentication();
app.UseAuthorization();

app.MapGet("/", () => "Hello World!");
app.MapGet("/me", async (ClaimsPrincipal cp) =>
{
    return Results.Ok(cp.Claims.ToDictionary(i=> i.Type, j=>j.Value));
}).RequireAuthorization();

app.Run();
