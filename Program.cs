using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.Identity.Client;
using Microsoft.IdentityModel.Tokens;
using MinimalsApi.Dominio.DTOs;
using MinimalsApi.Dominio.Entidades;
using MinimalsApi.Dominio.Enuns;
using MinimalsApi.Dominio.Interface;
using MinimalsApi.Dominio.ModelViews;
using MinimalsApi.Dominio.Servicos;
using MinimalsApi.Infraestrutura;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace MinimalsApi
{
    public class Program
    {
        public static void Main(string[] args)
        {

            var builder = WebApplication.CreateBuilder(args);

            // Configura a string de conexão
            string connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

            builder.Services.AddScoped<IAdministradorServico, AdministradorServico>();



            // Adiciona o DbContext com o SQL Server
            builder.Services.AddDbContext<DbContexto>(options =>
            {
                options.UseSqlServer(connectionString);
            });
            // Add services to the container.


            builder.Services.AddControllers();
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();
       

            var app = builder.Build();

            //// Cria o escopo para usar o DbContext e garantir que o banco de dados seja criado
            //using (var scope = app.Services.CreateScope())
            //{
            //    var db = scope.ServiceProvider.GetRequiredService<DbContexto>();
            //    db.Database.EnsureDeleted();  // Opcional: Use apenas para deletar o banco de dados existente
            //    db.Database.EnsureCreated();  // Cria o banco de dados se ele não existir
            //}



            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseAuthorization();

            app.MapControllers();

            //app.MapGet("/", () => "Olá pessoal");

            //app.MapPost("/login", (LoginDTO loginDTO) =>
            //{
            //    if (loginDTO.Email == "amauri@amauri" && loginDTO.Senha == "123")
            //    {
            //        return Results.Ok("Login com sucesso");
            //    }
            //    else
            //    {
            //        return Results.Unauthorized();
            //    }
            //});

            app.UseSwagger();
            app.UseSwaggerUI();

            app.UseRouting();

            app.UseAuthentication();
            app.UseAuthorization();

            app.UseCors();
            app.UseEndpoints(endpoints =>
            {

                #region Home
                endpoints.MapGet("/", () => Results.Json(new Home())).AllowAnonymous().WithTags("Home");
                #endregion

                #region Administradores
                string GerarTokenJwt(Administrador administrador)
                {


                    var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes("amauriTeste"));
                    var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

                    var claims = new List<Claim>()
                {
                    new Claim("Email", administrador.Email),
                    new Claim("Perfil", administrador.Perfil),
                    new Claim(ClaimTypes.Role, administrador.Perfil),
                };

                    var token = new JwtSecurityToken(
                        claims: claims,
                        expires: DateTime.Now.AddDays(1),
                        signingCredentials: credentials
                    );

                    return new JwtSecurityTokenHandler().WriteToken(token);
                }

                endpoints.MapPost("/administradores/login", ([FromBody] LoginDTO loginDTO, IAdministradorServico administradorServico) =>
                {
                    var adm = administradorServico.Login(loginDTO);
                    if (adm != null)
                    {
                        //string token = GerarTokenJwt(adm);
                        return Results.Ok(new AdministradorLogado
                        {
                            Email = adm.Email,
                            Perfil = adm.Perfil,
                            //Token = token
                        });
                    }
                    else
                        return Results.Unauthorized();
                }).AllowAnonymous().WithTags("Administradores");

                endpoints.MapGet("/administradores", ([FromQuery] int? pagina, IAdministradorServico administradorServico) =>
                {
                    var adms = new List<AdministradorModelView>();
                    var administradores = administradorServico.Todos(pagina);
                    foreach (var adm in administradores)
                    {
                        adms.Add(new AdministradorModelView
                        {
                            Id = adm.Id,
                            Email = adm.Email,
                            Perfil = adm.Perfil
                        });
                    }
                    return Results.Ok(adms);
                })
                .RequireAuthorization()
                .RequireAuthorization(new AuthorizeAttribute { Roles = "Adm" })
                .WithTags("Administradores");

                endpoints.MapGet("/Administradores/{id}", ([FromRoute] int id, IAdministradorServico administradorServico) =>
                {
                    var administrador = administradorServico.BuscaPorId(id);
                    if (administrador == null) return Results.NotFound();
                    return Results.Ok(new AdministradorModelView
                    {
                        Id = administrador.Id,
                        Email = administrador.Email,
                        Perfil = administrador.Perfil
                    });
                })
                .RequireAuthorization()
                .RequireAuthorization(new AuthorizeAttribute { Roles = "Adm" })
                .WithTags("Administradores");

                endpoints.MapPost("/administradores", ([FromBody] AdministradorDTO administradorDTO, IAdministradorServico administradorServico) =>
                {
                    var validacao = new ErrosDeValidacao
                    {
                        Mensagens = new List<string>()
                    };

                    if (string.IsNullOrEmpty(administradorDTO.Email))
                        validacao.Mensagens.Add("Email não pode ser vazio");
                    if (string.IsNullOrEmpty(administradorDTO.Senha))
                        validacao.Mensagens.Add("Senha não pode ser vazia");
                    if (administradorDTO.Perfil == null)
                        validacao.Mensagens.Add("Perfil não pode ser vazio");

                    if (validacao.Mensagens.Count > 0)
                        return Results.BadRequest(validacao);

                    var administrador = new Administrador
                    {
                        Email = administradorDTO.Email,
                        Senha = administradorDTO.Senha,
                        Perfil = administradorDTO.Perfil.ToString() ?? Perfil.Editor.ToString()
                    };

                    administradorServico.Incluir(administrador);

                    return Results.Created($"/administrador/{administrador.Id}", new AdministradorModelView
                    {
                        Id = administrador.Id,
                        Email = administrador.Email,
                        Perfil = administrador.Perfil
                    });

                })
                    .RequireAuthorization()
                    .RequireAuthorization(new AuthorizeAttribute { Roles = "Adm" })
                    .WithTags("Administradores");
                #endregion

                //#region Veiculos
                //ErrosDeValidacao validaDTO(VeiculoDTO veiculoDTO)
                //{
                //    var validacao = new ErrosDeValidacao
                //    {
                //        Mensagens = new List<string>()
                //    };

                //    if (string.IsNullOrEmpty(veiculoDTO.Nome))
                //        validacao.Mensagens.Add("O nome não pode ser vazio");

                //    if (string.IsNullOrEmpty(veiculoDTO.Marca))
                //        validacao.Mensagens.Add("A Marca não pode ficar em branco");

                //    if (veiculoDTO.Ano < 1950)
                //        validacao.Mensagens.Add("Veículo muito antigo, aceito somete anos superiores a 1950");

                //    return validacao;
                //}

                //endpoints.MapPost("/veiculos", ([FromBody] VeiculoDTO veiculoDTO, IVeiculoServico veiculoServico) => {
                //    var validacao = validaDTO(veiculoDTO);
                //    if (validacao.Mensagens.Count > 0)
                //        return Results.BadRequest(validacao);

                //    var veiculo = new Veiculo
                //    {
                //        Nome = veiculoDTO.Nome,
                //        Marca = veiculoDTO.Marca,
                //        Ano = veiculoDTO.Ano
                //    };
                //    veiculoServico.Incluir(veiculo);

                //    return Results.Created($"/veiculo/{veiculo.Id}", veiculo);
                //})
                //.RequireAuthorization()
                //.RequireAuthorization(new AuthorizeAttribute { Roles = "Adm,Editor" })
                //.WithTags("Veiculos");

                //endpoints.MapGet("/veiculos", ([FromQuery] int? pagina, IVeiculoServico veiculoServico) => {
                //    var veiculos = veiculoServico.Todos(pagina);

                //    return Results.Ok(veiculos);
                //}).RequireAuthorization().WithTags("Veiculos");

                //endpoints.MapGet("/veiculos/{id}", ([FromRoute] int id, IVeiculoServico veiculoServico) => {
                //    var veiculo = veiculoServico.BuscaPorId(id);
                //    if (veiculo == null) return Results.NotFound();
                //    return Results.Ok(veiculo);
                //})
                //.RequireAuthorization()
                //.RequireAuthorization(new AuthorizeAttribute { Roles = "Adm,Editor" })
                //.WithTags("Veiculos");

                //endpoints.MapPut("/veiculos/{id}", ([FromRoute] int id, VeiculoDTO veiculoDTO, IVeiculoServico veiculoServico) => {
                //    var veiculo = veiculoServico.BuscaPorId(id);
                //    if (veiculo == null) return Results.NotFound();

                //    var validacao = validaDTO(veiculoDTO);
                //    if (validacao.Mensagens.Count > 0)
                //        return Results.BadRequest(validacao);

                //    veiculo.Nome = veiculoDTO.Nome;
                //    veiculo.Marca = veiculoDTO.Marca;
                //    veiculo.Ano = veiculoDTO.Ano;

                //    veiculoServico.Atualizar(veiculo);

                //    return Results.Ok(veiculo);
                //})
                //.RequireAuthorization()
                //.RequireAuthorization(new AuthorizeAttribute { Roles = "Adm" })
                //.WithTags("Veiculos");

                //endpoints.MapDelete("/veiculos/{id}", ([FromRoute] int id, IVeiculoServico veiculoServico) => {
                //    var veiculo = veiculoServico.BuscaPorId(id);
                //    if (veiculo == null) return Results.NotFound();

                //    veiculoServico.Apagar(veiculo);

                //    return Results.NoContent();
                //})
                //.RequireAuthorization()
                //.RequireAuthorization(new AuthorizeAttribute { Roles = "Adm" })
                //.WithTags("Veiculos");
                //#endregion
            });

            app.Run();

        }
    }
}