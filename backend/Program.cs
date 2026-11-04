
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Npgsql;
using NpgsqlTypes;

var builder = WebApplication.CreateBuilder(args);

// Leser tilkoblingsinformasjon for PostgreSQL
var databaseUrl = builder.Configuration["Database:Url"];

if (string.IsNullOrWhiteSpace(databaseUrl))
{
    throw new InvalidOperationException(
        "Databasetilkoblingen er ikke konfigurert.");
}

var databaseUri = new Uri(databaseUrl);
var userInfo = databaseUri.UserInfo.Split(':', 2);

if (userInfo.Length != 2)
{
    throw new InvalidOperationException(
        "Databasens brukernavn eller passord mangler.");
}

var connectionBuilder = new NpgsqlConnectionStringBuilder
{
    Host = databaseUri.Host,
    Port = databaseUri.IsDefaultPort ? 5432 : databaseUri.Port,
    Database = databaseUri.AbsolutePath.TrimStart('/'),
    Username = Uri.UnescapeDataString(userInfo[0]),
    Password = Uri.UnescapeDataString(userInfo[1]),
    SslMode = SslMode.Require
};

var connectionString = connectionBuilder.ConnectionString;

// Kontrollerer tilkoblingen til PostgreSQL
try
{
    await using var connection =
        new NpgsqlConnection(connectionString);

    await connection.OpenAsync();

    Console.WriteLine(
        "Tilkobling til PostgreSQL er vellykket!");
}
catch (Exception ex)
{
    Console.WriteLine(
        $"Kunne ikke koble til PostgreSQL: {ex.GetType().Name}");

    throw;
}

// Konfigurerer JWT-autentisering
var jwtKey = builder.Configuration["Jwt:Key"]
    ?? throw new InvalidOperationException(
        "JWT-nøkkelen er ikke konfigurert.");

builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters =
            new TokenValidationParameters
            {
                ValidateIssuer = false,
                ValidateAudience = false,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(
                    Encoding.UTF8.GetBytes(jwtKey))
            };
    });

builder.Services.AddAuthorization();

// Tillater tilkobling fra lokal frontend
builder.Services.AddCors(options =>
{
    options.AddPolicy("MyAllowSpecificOrigins", policy =>
    {
        policy.WithOrigins("http://127.0.0.1:5500")
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

var app = builder.Build();

app.UseCors("MyAllowSpecificOrigins");
app.UseAuthentication();
app.UseAuthorization();

// Henter alle vurderinger fra PostgreSQL
async Task<Dictionary<int, int>> HentVurderingerAsync()
{
    var vurderinger = new Dictionary<int, int>();

    await using var connection =
        new NpgsqlConnection(connectionString);

    await connection.OpenAsync();

    await using var command = new NpgsqlCommand(
        """
        SELECT tjeneste_id, verdi
        FROM vurderinger
        """,
        connection);

    await using var reader =
        await command.ExecuteReaderAsync();

    while (await reader.ReadAsync())
    {
        vurderinger[reader.GetInt32(0)] =
            reader.GetInt32(1);
    }

    return vurderinger;
}

// Henter alle tilgjengelige tjenester
app.MapGet("/api/tjenester", async () =>
{
    var vurderinger = await HentVurderingerAsync();

    var tjenester = new[]
    {
        new
        {
            id = 1,
            navn = "Barnehage",
            beskrivelse = "Informasjon om barnehager i Øygarden.",
            vurdering = vurderinger.GetValueOrDefault(1),
            kategori = "Barn og familie",
            bilde = "https://encrypted-tbn0.gstatic.com/images?q=tbn:ANd9GcSTQ8wIxfZlfsexsYDSkB3OtTqY5-YfYo4UydI8_w9t8ibwaoOYtTxY19fd&s=10",
            lenke = "https://www.oygarden.kommune.no/tjenester/barnehage/"
        },
        new
        {
            id = 2,
            navn = "Skole",
            beskrivelse = "Informasjon om skoler i Øygarden.",
            vurdering = vurderinger.GetValueOrDefault(2),
            kategori = "Utdanning",
            bilde = "https://holon.no/wp-content/uploads/2019/08/skogsvaag_skole_4-2000x1333.jpg",
            lenke = "https://www.oygarden.kommune.no/tjenester/skule-og-sfo/skule/skuleoversikt/"
        },
        new
        {
            id = 3,
            navn = "Kultur",
            beskrivelse = "Informasjon om kulturtilbud i Øygarden.",
            vurdering = vurderinger.GetValueOrDefault(3),
            kategori = "Kultur",
            bilde = "https://cdn.prod.website-files.com/6710ebdd64c5072a7aa45c03/674834a3e3deae494da66a68_arrangement_07.jpg",
            lenke = "https://www.oygarden.kommune.no/tjenester/kultur-idrett-og-fritid/kultur/historie-og-kulturminne/kulturminne/"
        },
        new
        {
            id = 4,
            navn = "Transport",
            beskrivelse = "Informasjon om transportmuligheter i Øygarden.",
            vurdering = vurderinger.GetValueOrDefault(4),
            kategori = "Transport",
            bilde = "https://g.acdn.no/api/lumina/backend/ami-medium/ece5/fana/2026/5/26/13/02F49900-DA35-4782-8BE3-F56697790EA5.JPG",
            lenke = "https://www.oygarden.kommune.no/tjenester/okonomi-og-planlegging/samfunnsutvikling/sotrasambandet/god-trafikkflyt/reis-smart/"
        },
        new
        {
            id = 5,
            navn = "Byggesaker",
            beskrivelse = "Informasjon om byggesaker i Øygarden.",
            vurdering = vurderinger.GetValueOrDefault(5),
            kategori = "Bygg",
            bilde = "https://vcdn.polarismedia.no/e7d771e2-3aa4-40d5-a12b-ff4c24a81d8c?fit=clip&q=80&tight=false&w=2000",
            lenke = "https://www.oygarden.kommune.no/tjenester/kart-bygg-veg-og-eigedom/kontakt-oss/"
        },
        new
        {
            id = 6,
            navn = "NAV",
            beskrivelse = "Informasjon om NAV-tjenester i Øygarden.",
            vurdering = vurderinger.GetValueOrDefault(6),
            kategori = "Offentlig",
            bilde = "https://g.acdn.no/obscura/API/dynamic/r1/ece5/tr_2000_2000_s_f/1786958922000/nett/2026/8/17/11/NTB_8cnfHL4p5QY.jpg?chk=2459BF",
            lenke = "http://www.nav.no/"
        },
        new
        {
            id = 7,
            navn = "Frivillig arbeid",
            beskrivelse = "Informasjon om frivillig arbeid i Øygarden.",
            vurdering = vurderinger.GetValueOrDefault(7),
            kategori = "Frivillig",
            bilde = "https://encrypted-tbn0.gstatic.com/images?q=tbn:ANd9GcT91-2X_DvgS1raYGDOaESje6ih7aEEkqpBMpNEAaO0Ji6JQxvGqG9-kKI&s=10",
            lenke = "https://www.oygarden.kommune.no/tjenester/helse-omsorg-og-velferd/fyll-dagane/frivillig/"
        }
    };

    return Results.Ok(tjenester);
});

// Lagrer eller oppdaterer en vurdering i PostgreSQL
app.MapPost(
    "/api/tjenester/{id}/vurdering/{verdi}",
    async (int id, int verdi) =>
{
    if (id < 1 || id > 7)
    {
        return Results.NotFound(
            "Tjenesten finnes ikke.");
    }

    if (verdi < 1 || verdi > 5)
    {
        return Results.BadRequest(
            "Vurdering må være mellom 1 og 5.");
    }

    await using var connection =
        new NpgsqlConnection(connectionString);

    await connection.OpenAsync();

    await using var command = new NpgsqlCommand(
        """
        INSERT INTO vurderinger (tjeneste_id, verdi)
        VALUES (@id, @verdi)
        ON CONFLICT (tjeneste_id)
        DO UPDATE SET verdi = EXCLUDED.verdi
        """,
        connection);

    command.Parameters.AddWithValue("id", id);
    command.Parameters.AddWithValue("verdi", verdi);

    await command.ExecuteNonQueryAsync();

    return Results.Ok(new
    {
        id,
        vurdering = verdi
    });
});

// Henter alle kommentarer fra PostgreSQL
app.MapGet("/api/kommentarer", async () =>
{
    var kommentarer =
        new List<Dictionary<string, string?>>();

    await using var connection =
        new NpgsqlConnection(connectionString);

    await connection.OpenAsync();

    await using var command = new NpgsqlCommand(
        """
        SELECT id, navn, tekst, dato
        FROM kommentarer
        ORDER BY dato ASC NULLS FIRST, id ASC
        """,
        connection);

    await using var reader =
        await command.ExecuteReaderAsync();

    while (await reader.ReadAsync())
    {
        var kommentar = new Dictionary<string, string?>
        {
            ["id"] = reader.GetGuid(0).ToString(),
            ["navn"] = reader.GetString(1),
            ["tekst"] = reader.GetString(2),
            ["dato"] = reader.IsDBNull(3)
                ? null
                : reader.GetDateTime(3)
                    .ToString("yyyy-MM-dd HH:mm")
        };

        kommentarer.Add(kommentar);
    }

    return Results.Ok(kommentarer);
});

// Lagrer en ny kommentar i PostgreSQL
app.MapPost(
    "/api/kommentarer",
    async (Dictionary<string, string> nyKommentar) =>
{
    // Validerer navn
    if (!nyKommentar.TryGetValue("navn", out var navn) ||
        string.IsNullOrWhiteSpace(navn) ||
        navn.Trim().Length < 2 ||
        navn.Trim().Length > 50)
    {
        return Results.BadRequest(
            "Navn må inneholde mellom 2 og 50 tegn.");
    }

    // Validerer kommentartekst
    if (!nyKommentar.TryGetValue("tekst", out var tekst) ||
        string.IsNullOrWhiteSpace(tekst) ||
        tekst.Trim().Length < 3 ||
        tekst.Trim().Length > 500)
    {
        return Results.BadRequest(
            "Kommentar må inneholde mellom 3 og 500 tegn.");
    }

    var kommentarId = Guid.NewGuid();
    var datoVerdi = DateTime.Now;

    nyKommentar["navn"] = navn.Trim();
    nyKommentar["tekst"] = tekst.Trim();
    nyKommentar["id"] = kommentarId.ToString();
    nyKommentar["dato"] =
        datoVerdi.ToString("yyyy-MM-dd HH:mm");

    await using var connection =
        new NpgsqlConnection(connectionString);

    await connection.OpenAsync();

    await using var command = new NpgsqlCommand(
        """
        INSERT INTO kommentarer (id, navn, tekst, dato)
        VALUES (@id, @navn, @tekst, @dato)
        """,
        connection);

    command.Parameters.AddWithValue(
        "id", kommentarId);

    command.Parameters.AddWithValue(
        "navn", nyKommentar["navn"]);

    command.Parameters.AddWithValue(
        "tekst", nyKommentar["tekst"]);

    command.Parameters.AddWithValue(
        "dato",
        NpgsqlDbType.Timestamp,
        DateTime.SpecifyKind(
            datoVerdi,
            DateTimeKind.Unspecified));

    await command.ExecuteNonQueryAsync();

    return Results.Ok(nyKommentar);
});

// Sletter en kommentar fra PostgreSQL
// Bare administratoren kan slette kommentarer
app.MapDelete(
    "/api/kommentarer/{id}",
    async (string id) =>
{
    if (!Guid.TryParse(id, out var kommentarId))
    {
        return Results.BadRequest(
            "Ugyldig kommentar-ID.");
    }

    await using var connection =
        new NpgsqlConnection(connectionString);

    await connection.OpenAsync();

    await using var command = new NpgsqlCommand(
        "DELETE FROM kommentarer WHERE id = @id",
        connection);

    command.Parameters.AddWithValue(
        "id", kommentarId);

    var antallSlettet =
        await command.ExecuteNonQueryAsync();

    if (antallSlettet == 0)
    {
        return Results.NotFound(
            "Kommentar ikke funnet.");
    }

    return Results.Ok("Kommentar slettet.");
})
.RequireAuthorization(policy =>
    policy.RequireRole("Admin"));

// Logger inn administratoren
app.MapPost("/api/admin/login", (AdminLogin login) =>
{
    var adminPassword =
        app.Configuration["Admin:Password"];

    if (string.IsNullOrEmpty(adminPassword))
    {
        return Results.Problem(
            "Manglende administrator-konfigurasjon.");
    }

    // Sammenligner passordene på en sikker måte
    var expected =
        System.Security.Cryptography.SHA256.HashData(
            Encoding.UTF8.GetBytes(adminPassword));

    var actual =
        System.Security.Cryptography.SHA256.HashData(
            Encoding.UTF8.GetBytes(login.Password ?? ""));

    if (!System.Security.Cryptography.CryptographicOperations
        .FixedTimeEquals(expected, actual))
    {
        return Results.Unauthorized();
    }

    var claims = new[]
    {
        new Claim(ClaimTypes.Name, "Administrator"),
        new Claim(ClaimTypes.Role, "Admin")
    };

    var key = new SymmetricSecurityKey(
        Encoding.UTF8.GetBytes(jwtKey));

    var credentials = new SigningCredentials(
        key,
        SecurityAlgorithms.HmacSha256);

    var token = new JwtSecurityToken(
        claims: claims,
        expires: DateTime.UtcNow.AddHours(1),
        signingCredentials: credentials);

    var tokenString = new JwtSecurityTokenHandler()
        .WriteToken(token);

    return Results.Ok(new { token = tokenString });
});

app.Run();

record AdminLogin(string Password);
