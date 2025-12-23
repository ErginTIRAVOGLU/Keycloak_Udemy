using Keycloak.WebAPI.Extensions;
using Keycloak.WebAPI.Options;
using Keycloak.WebAPI.Services;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddCors();

builder.Services.Configure<KeycloakConfiguration>(
    builder.Configuration.GetSection("KeycloakConfiguration")); 

builder.Services.AddAuthentication()
    .AddJwtBearer();

builder.Services.AddAuthorization();
builder.Services.AddOpenApi("v1", options => { 
    options.AddDocumentTransformer<BearerSecuritySchemeTransformer>(); 
});

builder.Services.AddHttpClient();
builder.Services.AddScoped<IKeycloakServices, KeycloakServices>();

var app = builder.Build();

app.UseCors(policy =>
    policy.AllowAnyOrigin()
        .AllowAnyMethod()
        .AllowAnyHeader());

app.UseAuthentication();
app.UseAuthorization();

app.MapOpenApi("/openapi/{documentName}.json");

app.MapScalarApiReference(options =>
{
    options.OpenApiRoutePattern = "/openapi/{documentName}.json";
});

app.MapGet("/", () => "Hello World!");
app.MapGet("/getToken", async (IKeycloakServices keycloakServices) =>
{
    var token = await keycloakServices.GetAccessTokenAsync();
    return Results.Ok( new { AccessToken = token });
});


app.Run();
