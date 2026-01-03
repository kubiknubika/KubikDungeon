using System.Text.Json;
using KubikDungeon.Entities;

namespace KubikDungeon.Services;

public class ScoreEntry
{
    public string Name { get; set; }
    public string ClassName { get; set; } // Новое поле
    public int Level { get; set; }
    public int Room { get; set; }
    public string Date { get; set; }
}

public static class LeaderboardService
{
    private static string FileName = "leaderboard.json";

    public static void SaveScore(string name, string className, int level, int room)
    {
        string cleanName = name.Trim();
        if (string.IsNullOrEmpty(cleanName)) cleanName = "Странник";

        List<ScoreEntry> scores = LoadScores();
        var existing = scores.FirstOrDefault(s => s.Name.Equals(cleanName, StringComparison.OrdinalIgnoreCase) && s.ClassName == className);

        // Теперь рекорд уникален для связки Имя + Класс (Кубик-Маг и Кубик-Воин - разные рекорды)
        if (existing != null)
        {
            if (room > existing.Room)
            {
                existing.Room = room;
                existing.Level = level;
                existing.Date = DateTime.Now.ToString("dd.MM.yyyy HH:mm");
            }
        }
        else
        {
            scores.Add(new ScoreEntry 
            { 
                Name = cleanName, 
                ClassName = className,
                Level = level, 
                Room = room, 
                Date = DateTime.Now.ToString("dd.MM.yyyy HH:mm") 
            });
        }

        scores = scores.OrderByDescending(s => s.Room).Take(15).ToList();
        string json = JsonSerializer.Serialize(scores, new JsonSerializerOptions { WriteIndented = true });
        File.WriteAllText(FileName, json);
    }

    public static List<ScoreEntry> LoadScores()
    {
        if (!File.Exists(FileName)) return new List<ScoreEntry>();
        try 
        {
            string json = File.ReadAllText(FileName);
            return JsonSerializer.Deserialize<List<ScoreEntry>>(json) ?? new List<ScoreEntry>();
        }
        catch { return new List<ScoreEntry>(); }
    }

    public static void Show()
    {
        Console.Clear();
        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine("🏆 === ЗАЛ СЛАВЫ === 🏆");
        Console.WriteLine("----------------------------------------------------------");
        Console.WriteLine("{0,-12} | {1,-8} | {2,-5} | {3,-5} | {4}", "ИМЯ", "КЛАСС", "LVL", "ROOM", "ДАТА");
        Console.WriteLine("----------------------------------------------------------");
        Console.ResetColor();

        var scores = LoadScores();
        if (scores.Count == 0) Console.WriteLine("Пока пусто.");

        foreach (var s in scores)
        {
            Console.WriteLine("{0,-12} | {1,-8} | {2,-5} | {3,-5} | {4}", s.Name, s.ClassName, s.Level, s.Room, s.Date);
        }
        
        Console.WriteLine("\nНажмите ENTER...");
        Console.ReadLine();
    }
}