
using System.Text.Json;

public static class KommentarMigrering
{
    public static void KontrollerKommentarer(string filsti)
    {
        // Kontrollerer at filen finnes
        if (!File.Exists(filsti))
        {
            Console.WriteLine("Kommentarfilen finnes ikke.");
            return;
        }

        // Leser eksisterende kommentarer fra JSON
        var json = File.ReadAllText(filsti);

        using var dokument = JsonDocument.Parse(json);

        // Teller kommentarer uten å endre data
        var antall = dokument.RootElement.GetArrayLength();

        Console.WriteLine(
            $"Fant {antall} kommentarer som kan migreres."
        );
    }

    public static async Task MigrerKommentarerAsync(
        string filsti,
        string connectionString)
    {
        // Leser kommentarer fra JSON-filen
        if (!File.Exists(filsti))
        {
            Console.WriteLine("Kommentarfilen finnes ikke.");
            return;
        }

        var kommentarer = JsonSerializer.Deserialize<
            List<Dictionary<string, string>>>(
            File.ReadAllText(filsti)
        ) ?? new();

        await using var connection =
            new Npgsql.NpgsqlConnection(connectionString);

        await connection.OpenAsync();

        var antall = 0;

        foreach (var kommentar in kommentarer)
        {
            // Beholder eksisterende ID-er og datoer
            if (!kommentar.TryGetValue("id", out var id) ||
                !Guid.TryParse(id, out var kommentarId))
            {
                throw new InvalidOperationException(
                    "En kommentar mangler gyldig ID.");
            }


            if (!kommentar.TryGetValue("navn", out var navn) ||
                !kommentar.TryGetValue("tekst", out var tekst) ||
                string.IsNullOrWhiteSpace(navn) ||
                string.IsNullOrWhiteSpace(tekst))
            {
                throw new InvalidOperationException(
                    "En kommentar mangler navn eller tekst.");
            }

            // Beholder datoen hvis den finnes og er gyldig
            DateTime? datoVerdi = null;

            if (kommentar.TryGetValue("dato", out var dato) &&
                !string.IsNullOrWhiteSpace(dato))
            {
                if (!DateTime.TryParseExact(
                    dato,
                    "yyyy-MM-dd HH:mm",
                    System.Globalization.CultureInfo.InvariantCulture,
                    System.Globalization.DateTimeStyles.None,
                    out var parsedDato))
                {
                    throw new InvalidOperationException(
                        "En kommentar har ugyldig dato.");
                }

                datoVerdi = parsedDato;
            }


            await using var command = new Npgsql.NpgsqlCommand(
                """
            INSERT INTO kommentarer (id, navn, tekst, dato)
            VALUES (@id, @navn, @tekst, @dato)
            ON CONFLICT (id) DO NOTHING
            """,
                connection);

            command.Parameters.AddWithValue("id", kommentarId);
            command.Parameters.AddWithValue("navn", navn);
            command.Parameters.AddWithValue("tekst", tekst);
            command.Parameters.Add(
    "dato",
    NpgsqlTypes.NpgsqlDbType.Timestamp
).Value = datoVerdi.HasValue
    ? datoVerdi.Value
    : DBNull.Value;

            antall += await command.ExecuteNonQueryAsync();
        }

        Console.WriteLine(
            $"Migrering fullført. {antall} nye kommentarer lagret.");
    }
}