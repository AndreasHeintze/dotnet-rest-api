using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace RestApi;

public class JokeDb : DbContext
{
    public JokeDb(DbContextOptions<JokeDb> options) : base(options) { }
    public DbSet<SavedJoke> Jokes => Set<SavedJoke>();   // ← RÄTT STAVNING
}

public class SavedJoke
{
    [Key]
    public int Id { get; set; }

    public string Value { get; set; } = null!;

    public DateTime SavedAt { get; set; }
}