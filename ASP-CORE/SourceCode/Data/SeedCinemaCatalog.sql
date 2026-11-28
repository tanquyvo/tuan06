-- TanQuy Cinema: keep exactly the five titles represented by the five supplied posters.
-- DESTRUCTIVE: all other movie rows are deleted. The FK to ChiTietHoaDons uses ON DELETE CASCADE,
-- so related invoice detail rows for deleted movies are also removed. Back up the database first.
SET NOCOUNT ON;
SET XACT_ABORT ON;

BEGIN TRY
    BEGIN TRANSACTION;

    DECLARE @GenreNames TABLE ([Name] nvarchar(200) NOT NULL PRIMARY KEY);
    INSERT INTO @GenreNames ([Name]) VALUES
        (N'Action'), (N'Adventure'), (N'Animation'), (N'Comedy'), (N'Crime'),
        (N'Drama'), (N'Fantasy'), (N'Horror'), (N'Romance'), (N'Science Fiction'), (N'Thriller');

    INSERT INTO dbo.Genres ([Name])
    SELECT g.[Name]
    FROM @GenreNames AS g
    WHERE NOT EXISTS (SELECT 1 FROM dbo.Genres AS existing WHERE existing.[Name] = g.[Name]);

    -- Keep one row for each approved title (lowest ID wins) and delete every other movie.
    ;WITH RankedApprovedMovies AS
    (
        SELECT ID,
               ROW_NUMBER() OVER (PARTITION BY LTRIM(RTRIM(Title)) ORDER BY ID) AS RowNumber
        FROM dbo.Movies
        WHERE LTRIM(RTRIM(Title)) IN
        (
            N'Love Destiny: The Movie', N'Addicted', N'La La Land',
            N'The Dark Knight', N'Interstellar'
        )
    )
    DELETE FROM dbo.Movies
    WHERE ID NOT IN (SELECT ID FROM RankedApprovedMovies WHERE RowNumber = 1);

    -- Insert missing approved titles. All poster paths point to the supplied JPG files.
    IF NOT EXISTS (SELECT 1 FROM dbo.Movies WHERE LTRIM(RTRIM(Title)) = N'Love Destiny: The Movie')
        INSERT INTO dbo.Movies (Title, Summary, ReleaseDate, Genre, GenreID, Price, Rated, PicturePath)
        SELECT N'Love Destiny: The Movie',
               N'A historical romantic comedy about two people whose connection is shaped by fate, memory and a journey across time.',
               CONVERT(datetime2, '2022-07-28', 23), N'Romance', ID, 89000, 4.6,
               N'/images/catalog/love-destiny-the-movie.jpg'
        FROM (SELECT TOP (1) ID FROM dbo.Genres WHERE Name = N'Romance' ORDER BY ID) AS chosenGenre;

    IF NOT EXISTS (SELECT 1 FROM dbo.Movies WHERE LTRIM(RTRIM(Title)) = N'Addicted')
        INSERT INTO dbo.Movies (Title, Summary, ReleaseDate, Genre, GenreID, Price, Rated, PicturePath)
        SELECT N'Addicted',
               N'A youth romance drama about two students whose unexpected connection grows into a complicated relationship.',
               CONVERT(datetime2, '2016-01-29', 23), N'Drama', ID, 79000, 4.4,
               N'/images/catalog/addicted.jpg'
        FROM (SELECT TOP (1) ID FROM dbo.Genres WHERE Name = N'Drama' ORDER BY ID) AS chosenGenre;

    IF NOT EXISTS (SELECT 1 FROM dbo.Movies WHERE LTRIM(RTRIM(Title)) = N'La La Land')
        INSERT INTO dbo.Movies (Title, Summary, ReleaseDate, Genre, GenreID, Price, Rated, PicturePath)
        SELECT N'La La Land',
               N'An aspiring actress and a jazz musician pursue their dreams while navigating love and ambition in Los Angeles.',
               CONVERT(datetime2, '2016-12-09', 23), N'Romance', ID, 99000, 4.8,
               N'/images/catalog/la-la-land.jpg'
        FROM (SELECT TOP (1) ID FROM dbo.Genres WHERE Name = N'Romance' ORDER BY ID) AS chosenGenre;

    IF NOT EXISTS (SELECT 1 FROM dbo.Movies WHERE LTRIM(RTRIM(Title)) = N'The Dark Knight')
        INSERT INTO dbo.Movies (Title, Summary, ReleaseDate, Genre, GenreID, Price, Rated, PicturePath)
        SELECT N'The Dark Knight',
               N'Batman faces the Joker, a criminal mastermind who pushes Gotham City into chaos and tests the limits of its hero.',
               CONVERT(datetime2, '2008-07-18', 23), N'Action', ID, 99000, 4.9,
               N'/images/catalog/the-dark-knight.jpg'
        FROM (SELECT TOP (1) ID FROM dbo.Genres WHERE Name = N'Action' ORDER BY ID) AS chosenGenre;

    IF NOT EXISTS (SELECT 1 FROM dbo.Movies WHERE LTRIM(RTRIM(Title)) = N'Interstellar')
        INSERT INTO dbo.Movies (Title, Summary, ReleaseDate, Genre, GenreID, Price, Rated, PicturePath)
        SELECT N'Interstellar',
               N'A team of explorers travels beyond Earth in search of a future for humanity as time and distance reshape their lives.',
               CONVERT(datetime2, '2014-11-07', 23), N'Sci-Fi', ID, 109000, 4.9,
               N'/images/catalog/interstellar.jpg'
        FROM (SELECT TOP (1) ID FROM dbo.Genres WHERE Name = N'Science Fiction' ORDER BY ID) AS chosenGenre;

    -- Refresh fields for rows that already existed under one of the approved titles.
    UPDATE m
    SET m.Summary = s.Summary,
        m.ReleaseDate = s.ReleaseDate,
        m.Genre = s.LegacyGenre,
        m.GenreID = g.ID,
        m.Price = s.Price,
        m.Rated = s.Rated,
        m.PicturePath = s.PicturePath
    FROM dbo.Movies AS m
    INNER JOIN
    (
        VALUES
        (N'Love Destiny: The Movie', N'A historical romantic comedy about two people whose connection is shaped by fate, memory and a journey across time.', CONVERT(datetime2, '2022-07-28', 23), N'Romance', CAST(89000 AS decimal(18,2)), CAST(4.6 AS float), N'/images/catalog/love-destiny-the-movie.jpg'),
        (N'Addicted', N'A youth romance drama about two students whose unexpected connection grows into a complicated relationship.', CONVERT(datetime2, '2016-01-29', 23), N'Drama', CAST(79000 AS decimal(18,2)), CAST(4.4 AS float), N'/images/catalog/addicted.jpg'),
        (N'La La Land', N'An aspiring actress and a jazz musician pursue their dreams while navigating love and ambition in Los Angeles.', CONVERT(datetime2, '2016-12-09', 23), N'Romance', CAST(99000 AS decimal(18,2)), CAST(4.8 AS float), N'/images/catalog/la-la-land.jpg'),
        (N'The Dark Knight', N'Batman faces the Joker, a criminal mastermind who pushes Gotham City into chaos and tests the limits of its hero.', CONVERT(datetime2, '2008-07-18', 23), N'Action', CAST(99000 AS decimal(18,2)), CAST(4.9 AS float), N'/images/catalog/the-dark-knight.jpg'),
        (N'Interstellar', N'A team of explorers travels beyond Earth in search of a future for humanity as time and distance reshape their lives.', CONVERT(datetime2, '2014-11-07', 23), N'Sci-Fi', CAST(109000 AS decimal(18,2)), CAST(4.9 AS float), N'/images/catalog/interstellar.jpg')
    ) AS s(Title, Summary, ReleaseDate, LegacyGenre, Price, Rated, PicturePath)
        ON LTRIM(RTRIM(m.Title)) = s.Title
    INNER JOIN dbo.Genres AS g ON g.Name = CASE s.LegacyGenre WHEN N'Sci-Fi' THEN N'Science Fiction' ELSE s.LegacyGenre END;

    COMMIT TRANSACTION;

    SELECT ID, Title, Genre, GenreID, PicturePath
    FROM dbo.Movies
    ORDER BY CASE Title
        WHEN N'Love Destiny: The Movie' THEN 1
        WHEN N'Addicted' THEN 2
        WHEN N'La La Land' THEN 3
        WHEN N'The Dark Knight' THEN 4
        WHEN N'Interstellar' THEN 5
        ELSE 99
    END;
    SELECT COUNT(*) AS RemainingMovies FROM dbo.Movies;
END TRY
BEGIN CATCH
    IF @@TRANCOUNT > 0 ROLLBACK TRANSACTION;
    THROW;
END CATCH;
