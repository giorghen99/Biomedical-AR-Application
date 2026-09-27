using System;
using System.Globalization; // Necessario per CultureInfo.InvariantCulture

public class HeartRateAverage
{
    public string date { get; set; }
    public int hour { get; set; }
    public float average { get; set; }
    public float MinBpm { get; set; } // Campo per il minimo orario
    public float MaxBpm { get; set; } // Campo per il massimo orario

    // Proprietà per ottenere la data come DateTime
    public DateTime Date
    {
        get
        {
            if (DateTime.TryParseExact(date, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime parsedDate))
            {
                return parsedDate;
            }
            return DateTime.MinValue; // Data non valida
        }
    }
}