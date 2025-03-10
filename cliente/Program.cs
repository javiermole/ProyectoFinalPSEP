using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Text.Json;
using System.Collections.Generic;

namespace cliente
{
    class Piloto
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

    class Equipo
    {
        public string Name { get; set; } = string.Empty;
        public string Nationality { get; set; } = string.Empty;
        public int Championships { get; set; }
        public int RaceWins { get; set; }
        public int Podiums { get; set; }
        public List<string> Drivers { get; set; } = new List<string>();
    }

    class Circuito
    {
        public string Name { get; set; } = string.Empty;
        public string Location { get; set; } = string.Empty;
        public int FirstRace { get; set; }
        public double LengthKm { get; set; }
        public string LapRecord { get; set; } = string.Empty;
        public string RecordHolder { get; set; } = string.Empty;
    }

    class Records
    {
        public string MostWins { get; set; } = string.Empty;
        public string MostPoles { get; set; } = string.Empty;
        public string MostTitles { get; set; } = string.Empty;
        public string FastestLapEver { get; set; } = string.Empty;
    }

    class ApiResponse<T>
    {
        public T Value { get; set; }
    }

    class Program
    {
        private static readonly Socket clientSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        private const int PORT = 11000;
        private static readonly JsonSerializerOptions jsonOptions = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        };

        static async Task Main(string[] args)
        {
            try
            {
                await ConnectToServer();
                while (true)
                {
                    Console.WriteLine("\n=== MENÚ PRINCIPAL F1 ===");
                    Console.WriteLine("1. Ver pilotos");
                    Console.WriteLine("2. Ver equipos");
                    Console.WriteLine("3. Ver circuitos");
                    Console.WriteLine("4. Ver récords históricos");
                    Console.WriteLine("5. Buscar piloto por nombre");
                    Console.WriteLine("6. Buscar equipo por nombre");
                    Console.WriteLine("7. Buscar circuito por país");
                    Console.WriteLine("8. Salir");
                    Console.Write("\nOpción: ");

                    var opcion = Console.ReadLine();

                    switch (opcion)
                    {
                        case "1":
                            await ObtenerPilotos();
                            break;
                        case "2":
                            await ObtenerEquipos();
                            break;
                        case "3":
                            await ObtenerCircuitos();
                            break;
                        case "4":
                            await ObtenerRecords();
                            break;
                        case "5":
                            await BuscarPiloto();
                            break;
                        case "6":
                            await BuscarEquipo();
                            break;
                        case "7":
                            await BuscarCircuitoPorPais();
                            break;
                        case "8":
                            clientSocket.Close();
                            return;
                        default:
                            Console.WriteLine("Opción no válida");
                            break;
                    }

                    Console.WriteLine("\nPresione cualquier tecla para continuar...");
                    Console.ReadKey();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
                clientSocket.Close();
            }
        }

        private static async Task ConnectToServer()
        {
            try
            {
                await clientSocket.ConnectAsync(IPAddress.Parse("127.0.0.1"), PORT);
                Console.WriteLine("Conectado al servidor.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error al conectar al servidor: {ex.Message}");
                throw;
            }
        }

        private static async Task<string> SendRequest(string endpoint)
        {
            try
            {
                // Encriptar la solicitud
                string encryptedRequest = EncryptionHelper.Encrypt(endpoint);
                
                // Enviar la solicitud encriptada
                byte[] requestData = Encoding.ASCII.GetBytes(encryptedRequest);
                await clientSocket.SendAsync(requestData, SocketFlags.None);

                // Recibir la respuesta
                byte[] buffer = new byte[4096];
                int bytesRead = await clientSocket.ReceiveAsync(buffer, SocketFlags.None);
                string encryptedResponse = Encoding.ASCII.GetString(buffer, 0, bytesRead);

                // Desencriptar la respuesta
                return EncryptionHelper.Decrypt(encryptedResponse);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error en la comunicación: {ex.Message}");
                throw;
            }
        }

        static async Task ObtenerPilotos()
        {
            try
            {
                var jsonResponse = await SendRequest("/api/f1/pilotos");
                var pilotos = JsonSerializer.Deserialize<List<Piloto>>(jsonResponse, jsonOptions);

                if (pilotos != null && pilotos.Count > 0)
                {
                    Console.WriteLine("\n=== PILOTOS DE FÓRMULA 1 ===\n");
                    foreach (var piloto in pilotos)
                    {
                        MostrarPiloto(piloto);
                    }
                }
                else
                {
                    Console.WriteLine("No se encontraron pilotos.");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error al obtener pilotos: {ex.Message}");
            }
        }

        static async Task ObtenerEquipos()
        {
            try
            {
                var jsonResponse = await SendRequest("/api/f1/equipos");
                var equipos = JsonSerializer.Deserialize<List<Equipo>>(jsonResponse, jsonOptions);

                if (equipos != null && equipos.Count > 0)
                {
                    Console.WriteLine("\n=== EQUIPOS DE FÓRMULA 1 ===\n");
                    foreach (var equipo in equipos)
                    {
                        MostrarEquipo(equipo);
                    }
                }
                else
                {
                    Console.WriteLine("No se encontraron equipos.");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error al obtener equipos: {ex.Message}");
            }
        }

        static async Task ObtenerCircuitos()
        {
            try
            {
                var jsonResponse = await SendRequest("/api/f1/circuitos");
                var circuitos = JsonSerializer.Deserialize<List<Circuito>>(jsonResponse, jsonOptions);

                if (circuitos != null && circuitos.Count > 0)
                {
                    Console.WriteLine("\n=== CIRCUITOS DE FÓRMULA 1 ===\n");
                    foreach (var circuito in circuitos)
                    {
                        MostrarCircuito(circuito);
                    }
                }
                else
                {
                    Console.WriteLine("No se encontraron circuitos.");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error al obtener circuitos: {ex.Message}");
            }
        }

        static async Task ObtenerRecords()
        {
            try
            {
                var jsonResponse = await SendRequest("/api/f1/records");
                var records = JsonSerializer.Deserialize<Records>(jsonResponse, jsonOptions);

                if (records != null)
                {
                    Console.WriteLine("\n=== RÉCORDS DE FÓRMULA 1 ===\n");
                    Console.WriteLine($"Mayor número de victorias: {records.MostWins}");
                    Console.WriteLine($"Mayor número de poles: {records.MostPoles}");
                    Console.WriteLine($"Mayor número de títulos: {records.MostTitles}");
                    Console.WriteLine($"Vuelta más rápida: {records.FastestLapEver}");
                    Console.WriteLine("------------------------");
                }
                else
                {
                    Console.WriteLine("No se encontraron récords.");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error al obtener récords: {ex.Message}");
            }
        }

        static async Task BuscarPiloto()
        {
            try
            {
                Console.Write("\nIntroduce el nombre del piloto: ");
                var nombre = Console.ReadLine();
                if (string.IsNullOrWhiteSpace(nombre))
                {
                    Console.WriteLine("El nombre no puede estar vacío.");
                    return;
                }

                var jsonResponse = await SendRequest($"/api/f1/pilotos/{Uri.EscapeDataString(nombre)}");
                var piloto = JsonSerializer.Deserialize<Piloto>(jsonResponse, jsonOptions);

                if (piloto != null)
                {
                    Console.WriteLine("\n=== INFORMACIÓN DEL PILOTO ===\n");
                    MostrarPiloto(piloto);
                }
                else
                {
                    Console.WriteLine("No se encontró el piloto.");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error al buscar piloto: {ex.Message}");
            }
        }

        static async Task BuscarEquipo()
        {
            try
            {
                Console.Write("\nIntroduce el nombre del equipo: ");
                var nombre = Console.ReadLine();
                if (string.IsNullOrWhiteSpace(nombre))
                {
                    Console.WriteLine("El nombre no puede estar vacío.");
                    return;
                }

                var jsonResponse = await SendRequest($"/api/f1/equipos/{Uri.EscapeDataString(nombre)}");
                var equipo = JsonSerializer.Deserialize<Equipo>(jsonResponse, jsonOptions);

                if (equipo != null)
                {
                    Console.WriteLine("\n=== INFORMACIÓN DEL EQUIPO ===\n");
                    MostrarEquipo(equipo);
                }
                else
                {
                    Console.WriteLine("No se encontró el equipo.");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error al buscar equipo: {ex.Message}");
            }
        }

        static async Task BuscarCircuitoPorPais()
        {
            try
            {
                Console.Write("\nIntroduce el país del circuito: ");
                var pais = Console.ReadLine();
                if (string.IsNullOrWhiteSpace(pais))
                {
                    Console.WriteLine("El país no puede estar vacío.");
                    return;
                }

                var jsonResponse = await SendRequest($"/api/f1/circuitos/pais/{Uri.EscapeDataString(pais)}");
                var circuitos = JsonSerializer.Deserialize<List<Circuito>>(jsonResponse, jsonOptions);

                if (circuitos != null && circuitos.Count > 0)
                {
                    Console.WriteLine($"\n=== CIRCUITOS EN {pais.ToUpper()} ===\n");
                    foreach (var circuito in circuitos)
                    {
                        MostrarCircuito(circuito);
                    }
                }
                else
                {
                    Console.WriteLine($"No se encontraron circuitos en {pais}.");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error al buscar circuitos: {ex.Message}");
            }
        }

        static void MostrarPiloto(Piloto piloto)
        {
            Console.WriteLine($"Piloto: {piloto.Name}");
            Console.WriteLine($"Nacionalidad: {piloto.Nationality}");
            Console.WriteLine($"Títulos mundiales: {piloto.Titles}");
            Console.WriteLine($"Victorias: {piloto.Wins}");
            Console.WriteLine($"Podios: {piloto.Podiums}");
            Console.WriteLine($"Vueltas rápidas: {piloto.FastestLaps}");
            Console.WriteLine($"Poles: {piloto.Poles}");
            Console.WriteLine($"Equipos: {string.Join(", ", piloto.Teams)}");
            Console.WriteLine("------------------------");
        }

        static void MostrarEquipo(Equipo equipo)
        {
            Console.WriteLine($"Equipo: {equipo.Name}");
            Console.WriteLine($"Nacionalidad: {equipo.Nationality}");
            Console.WriteLine($"Campeonatos: {equipo.Championships}");
            Console.WriteLine($"Victorias: {equipo.RaceWins}");
            Console.WriteLine($"Podios: {equipo.Podiums}");
            Console.WriteLine($"Pilotos: {string.Join(", ", equipo.Drivers)}");
            Console.WriteLine("------------------------");
        }

        static void MostrarCircuito(Circuito circuito)
        {
            Console.WriteLine($"Circuito: {circuito.Name}");
            Console.WriteLine($"Ubicación: {circuito.Location}");
            Console.WriteLine($"Primer GP: {circuito.FirstRace}");
            Console.WriteLine($"Longitud: {circuito.LengthKm:F3} km");
            Console.WriteLine($"Vuelta récord: {circuito.LapRecord}");
            Console.WriteLine($"Poseedor del récord: {circuito.RecordHolder}");
            Console.WriteLine("------------------------");
        }
    }
}
