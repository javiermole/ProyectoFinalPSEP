public class F1Driver
{
    public string Name { get; set; } = string.Empty;
    public string Nationality { get; set; } = string.Empty;
    public int Titles { get; set; }
    public int Wins { get; set; }
    public int Podiums { get; set; }
    public int FastestLaps { get; set; }
    public int Poles { get; set; }
    public List<string> Teams { get; set; } = new List<string>();
}

public class F1Team
{
    public string Name { get; set; } = string.Empty;
    public string Nationality { get; set; } = string.Empty;
    public int Championships { get; set; }
    public int RaceWins { get; set; }
    public int Podiums { get; set; }
    public List<string> Drivers { get; set; } = new List<string>();
}

public class F1Circuit
{
    public string Name { get; set; } = string.Empty;
    public string Location { get; set; } = string.Empty;
    public int FirstRace { get; set; }
    public double LengthKm { get; set; }
    public string LapRecord { get; set; } = string.Empty;
    public string RecordHolder { get; set; } = string.Empty;
}

public class F1Records
{
    public string MostWins { get; set; } = string.Empty;
    public string MostPoles { get; set; } = string.Empty;
    public string MostTitles { get; set; } = string.Empty;
    public string FastestLapEver { get; set; } = string.Empty;
}

public class F1Data
{
    public List<F1Driver> Drivers { get; set; } = new List<F1Driver>();
    public List<F1Team> Teams { get; set; } = new List<F1Team>();
    public List<F1Circuit> Circuits { get; set; } = new List<F1Circuit>();
    public F1Records Records { get; set; } = new F1Records();
}