using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// CORS politikasını ekliyoruz
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowReactApp",
        builder =>
        {
            builder.WithOrigins("http://localhost:5173") // React'ın default portu
                   .AllowAnyHeader()
                   .AllowAnyMethod();
        });
});

// Add services to the container.
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Entity Framework için SQLite bağlantısı ekliyoruz
builder.Services.AddDbContext<TaskDbContext>(options =>
    options.UseSqlite("Data Source=tasks.db"));

// Controller'ları ekliyoruz
builder.Services.AddControllers();

var app = builder.Build();

// Basit health check endpoint'i
app.MapGet("/", () => Results.Ok());

// Migration'ları otomatik uygula
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<TaskDbContext>();
    db.Database.Migrate();
}

if (app.Environment.IsDevelopment())
{
  app.UseSwagger();
  app.UseSwaggerUI();
}

// CORS middleware'ini ekliyoruz
app.UseCors("AllowReactApp");

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

app.Run();
