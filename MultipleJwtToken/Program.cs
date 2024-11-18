using System.Security.Claims;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Http.Json;
using Microsoft.IdentityModel.Tokens;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        byte[] bytes = Encoding.UTF8.GetBytes(builder.Configuration["Auth:Supabase:JwtSecret"]!);
        options.TokenValidationParameters = new TokenValidationParameters()
        {
            IssuerSigningKey = new SymmetricSecurityKey(bytes),
            ValidAudience = builder.Configuration["Auth:Supabase:ValidAudience"],
            ValidIssuer = builder.Configuration["Auth:Supabase:ValidIssuer"]
        };
    });
builder.Services.AddMvc();
builder.Services.Configure<JsonOptions>(i =>
{
    i.SerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
    i.SerializerOptions.MaxDepth = 0;
});
var app = builder.Build();


app.UseAuthentication();
app.UseAuthorization();

app.MapGet("/", () => "Hello World!");
app.MapGet("/me", async (ClaimsPrincipal cp) =>
{
    return Results.Ok(cp);
}).RequireAuthorization();

app.Run();
