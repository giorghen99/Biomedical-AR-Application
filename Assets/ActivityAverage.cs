using System;
using System.Globalization; // Necessario per CultureInfo.InvariantCulture

public class ActivityAverage
{
    public string type { get; set; }
    public string date { get; set; }
    public int hour { get; set; }
    public float value { get; set; }

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