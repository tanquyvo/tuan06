using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using _31241023037_VoTanQuy.Models;

namespace _31241023037_VoTanQuy.Data;

/// <summary>
/// Keeps the cinema catalogue aligned with the five movie posters supplied for this project.
/// Any other existing movie rows are removed once, on the first successful seed run.
/// </summary>
public static class CinemaCatalogSeeder
{
    private sealed record MovieSeed(
        string Title,
        string GenreName,
        string LegacyGenre,
        string Summary,
        DateTime ReleaseDate,
        decimal Price,
        double Rated,
        string PicturePath);

    private const string PruneMarker = "SuppliedPosterCatalogV1";

    private static readonly string[] GenreNames =
    {
        "Action", "Adventure", "Animation", "Comedy", "Crime", "Drama",
        "Fantasy", "Horror", "Romance", "Science Fiction", "Thriller"
    };

    // The five titles and poster paths match the five images supplied by the user, in order.
    private static readonly MovieSeed[] MovieSeeds =
    {
        new(
            "Love Destiny: The Movie",
            "Romance",
            "Romance",
            "A historical romantic comedy about two people whose connection is shaped by fate, memory and a journey across time.",
            new DateTime(2022, 7, 28),
            89000m,
            4.6d,
            "/images/catalog/love-destiny-the-movie.jpg"),
        new(
            "Addicted",
            "Drama",
            "Drama",
            "A youth romance drama about two students whose unexpected connection grows into a complicated relationship.",
            new DateTime(2016, 1, 29),
            79000m,
            4.4d,
            "/images/catalog/addicted.jpg"),
        new(
            "La La Land",
            "Romance",
            "Romance",
            "An aspiring actress and a jazz musician pursue their dreams while navigating love and ambition in Los Angeles.",
            new DateTime(2016, 12, 9),
            99000m,
            4.8d,
            "/images/catalog/la-la-land.jpg"),
        new(
            "The Dark Knight",
            "Action",
            "Action",
            "Batman faces the Joker, a criminal mastermind who pushes Gotham City into chaos and tests the limits of its hero.",
            new DateTime(2008, 7, 18),
            99000m,
            4.9d,
            "/images/catalog/the-dark-knight.jpg"),
        new(
            "Interstellar",
            "Science Fiction",
            "Sci-Fi",
            "A team of explorers travels beyond Earth in search of a future for humanity as time and distance reshape their lives.",
            new DateTime(2014, 11, 7),
            109000m,
            4.9d,
            "/images/catalog/interstellar.jpg")
    };

    public static async Task SeedAsync(ApplicationDbContext db, ILogger logger)
    {
        if (!await db.Database.CanConnectAsync())
        {
            logger.LogWarning("Cinema catalogue cleanup/seed skipped because the database is not reachable.");
            return;
        }

        // Preserve the project's complete 11-genre list, adding only missing entries.
        var existingGenres = await db.Genres.ToListAsync();
        var genreLookup = new Dictionary<string, Genre>(StringComparer.OrdinalIgnoreCase);
        foreach (var genre in existingGenres)
            genreLookup.TryAdd(Normalize(genre.Name), genre);

        var missingGenres = GenreNames
            .Where(name => !genreLookup.ContainsKey(Normalize(name)))
            .Select(name => new Genre { Name = name })
            .ToList();

        if (missingGenres.Count > 0)
        {
            db.Genres.AddRange(missingGenres);
            await db.SaveChangesAsync();
            foreach (var genre in missingGenres)
                genreLookup[Normalize(genre.Name)] = genre;
        }

        var keepTitles = MovieSeeds
            .Select(movie => Normalize(movie.Title))
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        // Prune the old catalogue only once. This keeps the requested five initial entries,
        // while allowing later CRUD-created movies to survive subsequent app restarts.
        var pruneAlreadyApplied = await HasPruneAlreadyRunAsync(db);
        var existingMovies = await db.Movies.ToListAsync();

        if (!pruneAlreadyApplied)
        {
            var keepRows = existingMovies
                .Where(movie => keepTitles.Contains(Normalize(movie.Title)))
                .GroupBy(movie => Normalize(movie.Title), StringComparer.OrdinalIgnoreCase)
                .Select(group => group.OrderBy(movie => movie.ID).First())
                .ToHashSet();

            // Remove every non-approved movie and duplicate approved rows.
            // The existing FK from ChiTietHoaDons to Movies is configured with cascade delete.
            var moviesToDelete = existingMovies.Where(movie => !keepRows.Contains(movie)).ToList();
            if (moviesToDelete.Count > 0)
            {
                db.Movies.RemoveRange(moviesToDelete);
                await db.SaveChangesAsync();
                logger.LogWarning(
                    "Removed {DeletedMovieCount} movie rows outside the five approved titles (including duplicate rows).",
                    moviesToDelete.Count);
            }
        }

        var keptMovies = await db.Movies.ToListAsync();
        foreach (var movieSeed in MovieSeeds)
        {
            var genre = genreLookup[Normalize(movieSeed.GenreName)];
            var existing = keptMovies.FirstOrDefault(movie =>
                string.Equals(Normalize(movie.Title), Normalize(movieSeed.Title), StringComparison.OrdinalIgnoreCase));

            if (existing is not null)
            {
                existing.Summary = movieSeed.Summary;
                existing.ReleaseDate = movieSeed.ReleaseDate;
                existing.Genre = movieSeed.LegacyGenre;
                existing.GenreID = genre.ID;
                existing.Price = movieSeed.Price;
                existing.Rated = movieSeed.Rated;
                existing.PicturePath = movieSeed.PicturePath;
                continue;
            }

            var newMovie = new Movie
            {
                Title = movieSeed.Title,
                Summary = movieSeed.Summary,
                ReleaseDate = movieSeed.ReleaseDate,
                Genre = movieSeed.LegacyGenre,
                GenreID = genre.ID,
                Price = movieSeed.Price,
                Rated = movieSeed.Rated,
                PicturePath = movieSeed.PicturePath
            };
            db.Movies.Add(newMovie);
            keptMovies.Add(newMovie);
        }

        await db.SaveChangesAsync();

        if (!pruneAlreadyApplied)
        {
            await MarkPruneAsAppliedAsync(db);
            logger.LogInformation("Initial catalogue cleanup completed and recorded.");
        }

        logger.LogInformation(
            "Cinema catalogue seed ready: {GenreCount} genres and {SeedMovieCount} approved seed movies.",
            genreLookup.Count,
            MovieSeeds.Length);
    }

    private static async Task<bool> HasPruneAlreadyRunAsync(ApplicationDbContext db)
    {
        // A small state table makes the destructive initial cleanup a one-time operation.
        // It is created on demand, so no extra EF migration is required.
        await db.Database.ExecuteSqlRawAsync("""
            IF OBJECT_ID(N'dbo.CinemaCatalogSeedState', N'U') IS NULL
            BEGIN
                CREATE TABLE dbo.CinemaCatalogSeedState
                (
                    SeedKey nvarchar(100) NOT NULL CONSTRAINT PK_CinemaCatalogSeedState PRIMARY KEY,
                    AppliedAtUtc datetime2(7) NOT NULL CONSTRAINT DF_CinemaCatalogSeedState_AppliedAtUtc DEFAULT SYSUTCDATETIME()
                );
            END;
            """);

        var appliedCount = await db.Database
            .SqlQueryRaw<int>("SELECT COUNT(*) AS [Value] FROM dbo.CinemaCatalogSeedState WHERE SeedKey = N'SuppliedPosterCatalogV1'")
            .SingleAsync();

        return appliedCount > 0;
    }

    private static async Task MarkPruneAsAppliedAsync(ApplicationDbContext db)
    {
        await db.Database.ExecuteSqlInterpolatedAsync($"""
            IF NOT EXISTS (SELECT 1 FROM dbo.CinemaCatalogSeedState WHERE SeedKey = {PruneMarker})
                INSERT INTO dbo.CinemaCatalogSeedState (SeedKey) VALUES ({PruneMarker});
            """);
    }

    private static string Normalize(string? value) => (value ?? string.Empty).Trim().ToLowerInvariant();
}
