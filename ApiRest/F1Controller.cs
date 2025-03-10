using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;
using System.Linq;

[ApiController]
[Route("api/f1")]
public class F1Controller : ControllerBase
{
    private static readonly string jsonPath = "f1_data.json";
    private static readonly JsonSerializerOptions jsonOptions = new JsonSerializerOptions
    {
        PropertyNameCaseInsensitive = true
    };

    private async Task<F1Data> LoadData()
    {
        if (!System.IO.File.Exists(jsonPath))
            return new F1Data();

        var jsonData = await System.IO.File.ReadAllTextAsync(jsonPath);
        return JsonSerializer.Deserialize<F1Data>(jsonData, jsonOptions) ?? new F1Data();
    }

    [HttpGet("pilotos")]
    public async Task<IActionResult> GetDrivers()
    {
        var data = await LoadData();
        return Ok(data.Drivers);
    }

    [HttpGet("pilotos/{nombre}")]
    public async Task<IActionResult> GetDriver(string nombre)
    {
        var data = await LoadData();
        var piloto = data.Drivers.FirstOrDefault(p => 
            p.Name.Equals(nombre, StringComparison.OrdinalIgnoreCase));

        if (piloto == null)
            return NotFound($"No se encontró el piloto: {nombre}");

        return Ok(piloto);
    }

    [HttpGet("equipos")]
    public async Task<IActionResult> GetTeams()
    {
        var data = await LoadData();
        return Ok(data.Teams);
    }

    [HttpGet("equipos/{nombre}")]
    public async Task<IActionResult> GetTeam(string nombre)
    {
        var data = await LoadData();
        var equipo = data.Teams.FirstOrDefault(t => 
            t.Name.Equals(nombre, StringComparison.OrdinalIgnoreCase));

        if (equipo == null)
            return NotFound($"No se encontró el equipo: {nombre}");

        return Ok(equipo);
    }

    [HttpGet("circuitos")]
    public async Task<IActionResult> GetCircuits()
    {
        var data = await LoadData();
        return Ok(data.Circuits);
    }

    [HttpGet("circuitos/pais/{pais}")]
    public async Task<IActionResult> GetCircuitsByCountry(string pais)
    {
        var data = await LoadData();
        var circuitos = data.Circuits.Where(c => 
            c.Location.Equals(pais, StringComparison.OrdinalIgnoreCase)).ToList();

        if (!circuitos.Any())
            return NotFound($"No se encontraron circuitos en: {pais}");

        return Ok(circuitos);
    }

    [HttpGet("records")]
    public async Task<IActionResult> GetRecords()
    {
        var data = await LoadData();
        return Ok(data.Records);
    }

    [HttpGet("estadisticas")]
    public async Task<IActionResult> GetStatistics()
    {
        var data = await LoadData();
        var stats = new
        {
            TotalDrivers = data.Drivers.Count,
            TotalTeams = data.Teams.Count,
            TotalCircuits = data.Circuits.Count,
            MostSuccessfulDriver = data.Drivers.OrderByDescending(d => d.Wins).FirstOrDefault()?.Name,
            MostSuccessfulTeam = data.Teams.OrderByDescending(t => t.Championships).FirstOrDefault()?.Name,
            OldestCircuit = data.Circuits.OrderBy(c => c.FirstRace).FirstOrDefault()?.Name
        };

        return Ok(stats);
    }
}
