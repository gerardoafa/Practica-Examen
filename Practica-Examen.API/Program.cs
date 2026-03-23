using Practica_Examen.API.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSingleton<FirebaseService>();

builder.Services.AddScoped<IUsuariosService, UsuariosService>();
builder.Services.AddScoped<ILibrosService, LibrosService>();
builder.Services.AddScoped<IReservasService, ReservasService>();
builder.Services.AddScoped<IPrestamosService, PrestamosService>();
builder.Services.AddScoped<IReportesService, ReportesService>();

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAngular", policy =>
    {
        policy.WithOrigins("http://localhost:4200", "https://localhost:4200")
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials();
    });
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors("AllowAngular");
app.UseAuthorization();
app.MapControllers();

app.Run();