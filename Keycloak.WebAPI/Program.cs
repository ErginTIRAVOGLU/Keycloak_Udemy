using Keycloak.AuthServices.Authentication;
using Keycloak.AuthServices.Authorization;
using Keycloak.WebAPI.Extensions;
using Keycloak.WebAPI.Options;
using Keycloak.WebAPI.Services;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddCors();

builder.Services.Configure<KeycloakConfiguration>(
    builder.Configuration.GetSection("KeycloakConfiguration")); 

 

 
builder.Services.AddOpenApi("v1", options => { 
    options.AddDocumentTransformer<BearerSecuritySchemeTransformer>(); 
});

builder.Services.AddHttpClient();
builder.Services.AddScoped<IKeycloakServices, KeycloakServices>();

builder.Services.AddControllers();

builder.Services.AddKeycloakWebApiAuthentication(builder.Configuration);
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("users", policy =>
    {
        policy.RequireResourceRoles("UserGetAll","UserCreate","UserUpdate","UserDelete");
    });

     options.AddPolicy("RolesGetAll", policy =>
    {
        policy.RequireResourceRoles("RolesGetAll");
    });
    options.AddPolicy("RolesCreate", policy =>
    {
        policy.RequireResourceRoles("RolesCreate");
    });
}).AddKeycloakAuthorization(builder.Configuration);

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

app.MapControllers();

app.Run();
