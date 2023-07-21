using Api.FurnitureStore.Api.Configuration;
using Api.FurnitureStore.Api.services;
using Api.FurnitureStore.Data;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Furniture_Store_Api",
        Version = "v1"
    });

    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme()
    {
        Name = "Authorization",
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header ,
        Description = "Jwt Authorization header using the Bearer scheme. \n \t Enter Prefix (Bearer) , space , and the your token \n Example : 'Bearer mkkjndsjkvdsbdskjb'  " 
     
    });
    c.AddSecurityRequirement(new OpenApiSecurityRequirement{
        {
                new OpenApiSecurityScheme{
                    Reference = new OpenApiReference{
                        Type = ReferenceType.SecurityScheme,
                        Id = "Bearer"
                    }
                },
            new string [] {}


        }
    });
}
);
builder.Services.AddDbContext<APIFurnitureStoreContext>(options => options.UseSqlite(builder.Configuration.GetConnectionString("ApiFurnitureStoreContext")));
builder.Services.Configure<JwtConfig>(builder.Configuration.GetSection("JwtConfig"));

// Email
builder.Services.Configure<SmtpSettings>(builder.Configuration.GetSection("SmtpSettings"));
builder.Services.AddSingleton<IEmailSender, EmailServices>();
    
    //agregar esquema 
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;

}).AddJwtBearer(jwt =>
    {
        var key = Encoding.ASCII.GetBytes(builder.Configuration.GetSection("JwtConfig:secret").Value);
        jwt.SaveToken = true;//Esta l�nea indica que se debe guardar el token JWT en el contexto de autenticaci�n. Esto permite acceder al token en futuras solicitudes, si es necesario.
        jwt.TokenValidationParameters = new Microsoft.IdentityModel.Tokens.TokenValidationParameters()// Aqu� se configuran los par�metros de validaci�n del token JWT. Estos par�metros se utilizan para validar la firma y las propiedades del token.
        {
            ValidateIssuerSigningKey = true,//Habilita la validaci�n de la clave de firma del token JWT. 
            IssuerSigningKey = new SymmetricSecurityKey(key),//  validamos que la firma que resivimos sea la igual a key 
            ValidateIssuer = false,// valida quien emitio el token  para evitar que el token sea manejado por terceros 
            ValidateAudience = false,// valida que el destinatario sea el mismo con quien deberia recibirlo 
            RequireExpirationTime = false,// Indica que el token no requiere un tiempo de expiraci�n expl�cito.
            ValidateLifetime = true//Habilita la validaci�n de la vigencia del token.

        };
    });

builder.Services.AddDefaultIdentity<IdentityUser>(options =>
                    options.SignIn.RequireConfirmedAccount = false)
                    .AddEntityFrameworkStores<APIFurnitureStoreContext>();

var app = builder.Build(); 

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthentication();

app.UseAuthorization();

app.MapControllers();

app.Run();
