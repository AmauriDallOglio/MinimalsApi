
using System.Security.Cryptography.X509Certificates;

namespace MinimalsApi
{
    public class Program
    {
        public static void Main(string[] args)
        {

            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.
            builder.Services.AddControllers();
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseAuthorization();

            app.MapControllers();

            app.MapGet("/", () => "Olá pessoal");

            app.MapPost("/login", (LoginDto loginDTO) =>
            {
                if (loginDTO.Email == "amauri@amauri" && loginDTO.Senha == "123")
                {
                    return Results.Ok("Login com sucesso");
                }
                else
                {
                    return Results.Unauthorized();
                }
            });

            app.Run();

        }
       
    }
}
