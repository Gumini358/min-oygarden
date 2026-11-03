using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
var builder = WebApplication.CreateBuilder(args);
var jwtKey = builder.Configuration["Jwt:Key"]
    ?? throw new InvalidOperationException("Brak klucza JWT.");

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
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
builder.Services.AddCors(options =>
{
    options.AddPolicy(name: "MyAllowSpecificOrigins",
                      policy =>
                      {
                          policy.WithOrigins("http://127.0.0.1:5500")
       .AllowAnyHeader()
       .AllowAnyMethod();
                      });
});
var app = builder.Build();
app.UseAuthentication();
app.UseAuthorization();
app.UseCors("MyAllowSpecificOrigins");

var filsti = Path.Combine(
    app.Environment.ContentRootPath,
    "vurderinger.json"
);

var vurderinger = System.Text.Json.JsonSerializer.Deserialize<Dictionary<int, int>>(
    File.ReadAllText(filsti)
) ?? new Dictionary<int, int>();

var kommentarFil = Path.Combine(
    app.Environment.ContentRootPath,
    "kommentarer.json"
);

var kommentarer = System.Text.Json.JsonSerializer.Deserialize<
    List<Dictionary<string, string>>
>(
    File.ReadAllText(kommentarFil)
) ?? new List<Dictionary<string, string>>();
foreach (var kommentar in kommentarer)
{
    if (!kommentar.ContainsKey("id"))
    {
        kommentar["id"] = Guid.NewGuid().ToString();
    }
}

File.WriteAllText(
    kommentarFil,
    System.Text.Json.JsonSerializer.Serialize(kommentarer)
);
app.MapGet("/api/tjenester", () =>
{
    var tjenester = new[]
    {
        new
{
    id = 1,
    navn = "Barnehage",
    beskrivelse = "Informasjon om barnehager i Øygarden.",
    vurdering = vurderinger[1],
    kategori = "Barn og familie",
    bilde = "https://encrypted-tbn0.gstatic.com/images?q=tbn:ANd9GcSTQ8wIxfZlfsexsYDSkB3OtTqY5-YfYo4UydI8_w9t8ibwaoOYtTxY19fd&s=10",
    lenke = "https://www.oygarden.kommune.no/tjenester/barnehage/"
},

        new
{
    id = 2,
    navn = "Skole",
    beskrivelse = "Informasjon om skoler i Øygarden.",
    vurdering = vurderinger[2],
    kategori = "Utdanning",
    bilde = "https://holon.no/wp-content/uploads/2019/08/skogsvaag_skole_4-2000x1333.jpg",
    lenke = "https://www.oygarden.kommune.no/tjenester/skule-og-sfo/skule/skuleoversikt/"
},

        new
{
    id = 3,
    navn = "Kultur",
    beskrivelse = "Informasjon om kulturtilbud i Øygarden.",
    vurdering = vurderinger[3],
    kategori = "Kultur",
    bilde = "https://cdn.prod.website-files.com/6710ebdd64c5072a7aa45c03/674834a3e3deae494da66a68_arrangement_07.jpg",
    lenke = "https://www.oygarden.kommune.no/tjenester/kultur-idrett-og-fritid/kultur/historie-og-kulturminne/kulturminne/"
},
        new
{
    id = 4,
    navn = "Transport",
    beskrivelse = "Informasjon om transportmuligheter i Øygarden.",
    vurdering = vurderinger[4],
    kategori = "Transport",
    bilde = "https://g.acdn.no/api/lumina/backend/ami-medium/ece5/fana/2026/5/26/13/02F49900-DA35-4782-8BE3-F56697790EA5.JPG",
    lenke = "https://www.oygarden.kommune.no/tjenester/okonomi-og-planlegging/samfunnsutvikling/sotrasambandet/god-trafikkflyt/reis-smart/"
},
        new
{
    id = 5,
    navn = "Byggesaker",
    beskrivelse = "Informasjon om byggesaker i Øygarden.",
    vurdering = vurderinger[5],
    kategori = "Bygg",
    bilde = "https://vcdn.polarismedia.no/e7d771e2-3aa4-40d5-a12b-ff4c24a81d8c?fit=clip&q=80&tight=false&w=2000",
    lenke = "https://www.oygarden.kommune.no/tjenester/kart-bygg-veg-og-eigedom/kontakt-oss/"
},
        new
{
    id = 6,
    navn = "NAV",
    beskrivelse = "Informasjon om NAV-tjenester i Øygarden.",
    vurdering = vurderinger[6],
    kategori = "Offentlig",
    bilde = "https://g.acdn.no/obscura/API/dynamic/r1/ece5/tr_2000_2000_s_f/1786958922000/nett/2026/8/17/11/NTB_8cnfHL4p5QY.jpg?chk=2459BF",
    lenke = "http://www.nav.no/"
},
        new
{
    id = 7,
    navn = "Frivillig arbeid",
    beskrivelse = "Informasjon om frivillig arbeid i Øygarden.",
    vurdering = vurderinger[7],
    kategori = "Frivillig",
    bilde = "https://encrypted-tbn0.gstatic.com/images?q=tbn:ANd9GcT91-2X_DvgS1raYGDOaESje6ih7aEEkqpBMpNEAaO0Ji6JQxvGqG9-kKI&s=10",
    lenke = "https://www.oygarden.kommune.no/tjenester/helse-omsorg-og-velferd/fyll-dagane/frivillig/"
}
    };

    return tjenester;
});

app.MapPost("/api/tjenester/{id}/vurdering/{verdi}", (int id, int verdi) =>
{
    if (id < 1 || id > 7)
    {
        return Results.NotFound("Tjenesten finnes ikke.");
    }

    if (verdi < 1 || verdi > 5)
    {
        return Results.BadRequest("Vurdering må være mellom 1 og 5.");
    }

    vurderinger[id] = verdi;
    File.WriteAllText(filsti, System.Text.Json.JsonSerializer.Serialize(vurderinger));

    return Results.Ok(new
    {
        id = id,
        vurdering = verdi
    });
});

app.MapGet("/api/kommentarer", () =>
{
    return Results.Ok(kommentarer);
});
app.MapPost("/api/kommentarer", (Dictionary<string, string> nyKommentar) =>
{


    if (!nyKommentar.TryGetValue("navn", out var navn) ||
        string.IsNullOrWhiteSpace(navn) ||
        navn.Trim().Length < 2 ||
        navn.Trim().Length > 50)
    {
        return Results.BadRequest(
            "Navn må inneholde mellom 2 og 50 tegn."
        );
    }


    if (!nyKommentar.TryGetValue("tekst", out var tekst) ||
        string.IsNullOrWhiteSpace(tekst) ||
        tekst.Trim().Length < 3 ||
        tekst.Trim().Length > 500)
    {
        return Results.BadRequest(
            "Kommentar må inneholde mellom 3 og 500 tegn."
        );
    }

    nyKommentar["navn"] = navn.Trim();
    nyKommentar["tekst"] = tekst.Trim();

    nyKommentar["dato"] = DateTime.Now.ToString("yyyy-MM-dd HH:mm");
    nyKommentar["id"] = Guid.NewGuid().ToString();
    kommentarer.Add(nyKommentar);
    File.WriteAllText(kommentarFil, System.Text.Json.JsonSerializer.Serialize(kommentarer));
    return Results.Ok(nyKommentar);
});

app.MapDelete("/api/kommentarer/{id}", (string id) =>
{
    var kommentar = kommentarer.FirstOrDefault(k =>
        k.ContainsKey("id") && k["id"] == id);

    if (kommentar == null)
    {
        return Results.NotFound("Kommentar ikke funnet.");
    }

    kommentarer.Remove(kommentar);

    File.WriteAllText(
        kommentarFil,
        System.Text.Json.JsonSerializer.Serialize(kommentarer)
    );

    return Results.Ok("Kommentar slettet.");

}).RequireAuthorization(policy =>
    policy.RequireRole("Admin"));
app.MapPost("/api/admin/login", (AdminLogin login) =>
{
    var adminPassword = app.Configuration["Admin:Password"];

    if (string.IsNullOrEmpty(adminPassword))
    {
        return Results.Problem("Manglende administrator-konfigurasjon.");
    }

    var expected = System.Security.Cryptography.SHA256.HashData(
        Encoding.UTF8.GetBytes(adminPassword));

    var actual = System.Security.Cryptography.SHA256.HashData(
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
        key, SecurityAlgorithms.HmacSha256);

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