using Microsoft.EntityFrameworkCore;
using ImpostoLula.Models;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

// Configurar DbContext com MySQL
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") 
    ?? "server=localhost;port=3307;database=calculo_imposto_produtos;user=root;password=;";

builder.Services.AddDbContext<CalculoImpostoProdutosContext>(options =>
    options.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString))
);

// Adicionar controllers
builder.Services.AddControllers();

// Configurar CORS (opcional - remover se não precisar)
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", builder =>
    {
        builder.AllowAnyOrigin()
               .AllowAnyMethod()
               .AllowAnyHeader();
    });
});

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
    {
        Title = "API Cálculo Imposto Produtos",
        Version = "v1",
        Description = "API para gerenciar produtos e cálculo de impostos",
        Contact = new Microsoft.OpenApi.Models.OpenApiContact
        {
            Name = "Nicolly Santos",
            Email = "nicolly.r.santos11@aluno.senai.br"
        }
    });
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "API v1");
        options.RoutePrefix = string.Empty; // Abre Swagger na raiz
    });
}

// Usar CORS (opcional)
app.UseCors("AllowAll");

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
