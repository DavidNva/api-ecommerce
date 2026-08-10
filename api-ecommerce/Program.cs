using api_ecommerce.Data;
using api_ecommerce.Repository;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

//Configuracion de conexión SQL Server
var dbConnectionString = builder.Configuration.GetConnectionString("ConexionSql");
builder.Services.AddDbContext<ApplicationDbContext>(options => options.UseSqlServer(dbConnectionString));

//Configuracion Inyeccion de dependencias
builder.Services.AddScoped<ICategoryRepository, CategoryRepository>();
//builder.Services.AddAutoMapper(typeof(Program).Assembly);
builder.Services.AddAutoMapper(cfg =>
{
    cfg.AddMaps(typeof(Program).Assembly);
});

builder.Services.AddControllers();

builder.Services.AddEndpointsApiExplorer();//Activa el "explorador" que examina tus endpoints, controllers y atributos para generar la metadata que Swagger necesita.
builder.Services.AddSwaggerGen();//Registra el generador de documentación Swagger (Swashbuckle) en el proyecto.

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())//Solo aplica en ambiente de desarrollo
{
    //app.MapOpenApi();
    app.UseSwagger();//Este se usa para generar los archivos json para la documentacion de swagger
    app.UseSwaggerUI();//Este se usa, para la UI de swagger

}

// Configure the HTTP request pipeline.
app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
