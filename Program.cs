using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Text.Json.Serialization;

namespace RestApi;

public class Program
{
    public static async Task Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        // Swagger
        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen();
        
        builder.Services.AddHttpClient();

        // SQLite
        builder.Services.AddDbContext<JokeDb>(opt =>
            opt.UseSqlite("Data Source=jokes.db"));

        // .NET 9 Source Generator (valfritt men proffsigt)
        builder.Services.AddSingleton<JsonSerializerContext, ApiJsonContext>();

        var app = builder.Build();

        app.UseSwagger();
        app.UseSwaggerUI();

        // GET – nytt Chuck-skämt
        // app.MapGet("/chuck", async (HttpClient client) =>
        app.MapGet("/chuck", async ([FromServices] HttpClient client) =>
            await client.GetFromJsonAsync<Joke>("https://api.chucknorris.io/jokes/random"))
           .WithName("GetChuck")
           .WithOpenApi();

        // POST – spara skämt
        app.MapPost("/chuck", async (JokeDb db, Joke joke) =>
        {
          var saved = new SavedJoke
          {
            Value = joke.Value,
            SavedAt = DateTime.UtcNow
          };
          db.Jokes.Add(saved);
          await db.SaveChangesAsync();
          return Results.Created($"/saved/{saved.Id}", saved);
        }).WithOpenApi();

        // GET – alla sparade
        app.MapGet("/saved", async (JokeDb db) =>
          await db.Jokes.OrderByDescending(j => j.SavedAt).ToListAsync())
         .WithOpenApi();
           
        // GET – hämta ett specifikt skämt
        app.MapGet("/saved/{id:int}", async (JokeDb db, int id) =>
            await db.Jokes.FindAsync(id) is SavedJoke joke
                ? Results.Ok(joke)
                : Results.NotFound())
            .WithName("GetSavedJoke")
            .WithOpenApi();

        await app.RunAsync();
    }
}

public record Joke(string Value);

// .NET 9 Source Generator – supersnabb JSON
[JsonSerializable(typeof(Joke))]
[JsonSerializable(typeof(SavedJoke))]
[JsonSerializable(typeof(List<SavedJoke>))]
public partial class ApiJsonContext : JsonSerializerContext { }