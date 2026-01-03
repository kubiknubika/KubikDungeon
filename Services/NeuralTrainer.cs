using KubikDungeon.Core;
using KubikDungeon.Entities;

namespace KubikDungeon.Services;

public static class NeuralTrainer
{
    public static void Train()
    {
        Console.Clear();
        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.WriteLine("🧠 === ЦЕНТР ОБУЧЕНИЯ НЕЙРОСЕТИ === 🧠");
        Console.ResetColor();

        int generations = ReadInt("Количество поколений (Enter = 50): ", 50);
        int populationSize = ReadInt("Размер популяции (Enter = 50): ", 50);

        Console.WriteLine("\nЗапуск симуляции (ОДНОПОТОЧНЫЙ РЕЖИМ)...");
        Console.WriteLine("Нажмите [ESC], чтобы остановить обучение досрочно.");
        Thread.Sleep(1000);
        
        List<NeuralGenome> population = new List<NeuralGenome>();
        for(int i=0; i<populationSize; i++) population.Add(new NeuralGenome());

        NeuralGenome bestGenome = population[0];
        int bestScore = 0;

        for (int gen = 1; gen <= generations; gen++)
        {
            if (Console.KeyAvailable)
            {
                var key = Console.ReadKey(true).Key;
                if (key == ConsoleKey.Escape || key == ConsoleKey.Q)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine($"\n\n🛑 ОСТАНОВЛЕНО.");
                    Console.ResetColor();
                    break;
                }
            }

            Console.Write($"\r🧬 Поколение {gen}/{generations} | Рекорд: {bestScore} | Обучение...   ");
            
            List<(NeuralGenome genome, int score)> results = new List<(NeuralGenome, int)>();

            // --- ОДНОПОТОЧНЫЙ ЦИКЛ (Стабильный) ---
            foreach (var genome in population)
            {
                NeuralBotController bot = new NeuralBotController(genome);
                Game game = new Game(bot);
                game.IsHeadless = true; 
                game.Start();
                
                results.Add((genome, game.ResultScore));
            }
            // ---------------------------------------

            results = results.OrderByDescending(x => x.score).ToList();
            
            var bestInGen = results[0];
            if (bestInGen.score > bestScore)
            {
                bestScore = bestInGen.score;
                bestGenome = bestInGen.genome;
            }

            List<NeuralGenome> nextGen = new List<NeuralGenome>();
            int elitesCount = Math.Max(2, populationSize / 5); 
            for(int i=0; i<elitesCount; i++) nextGen.Add(results[i].genome);

            Random rnd = new Random();
            while (nextGen.Count < populationSize)
            {
                var parent = results[rnd.Next(elitesCount)].genome;
                nextGen.Add(parent.Mutate());
            }
            population = nextGen;
        }

        Console.WriteLine("\n\n✨ ОБУЧЕНИЕ ЗАВЕРШЕНО!");
        Console.WriteLine($"🏆 Абсолютный рекорд: {bestScore} комнат.");
        
        Console.WriteLine("\nХарактеристики мозга победителя:");
        string className = bestGenome.Weights[0] > 0.3 ? "Mage" : (bestGenome.Weights[0] < -0.3 ? "Warrior" : "Rogue");
        Console.WriteLine($"[0] Класс:   {className} ({bestGenome.Weights[0]:F2})");
        Console.WriteLine($"[1] Хил:     Лечится при < {((bestGenome.Weights[1] + 1) / 2.0 * 100):F0}% HP");
        Console.WriteLine($"[2] Магия:   Кастует при > {((1.0 - (bestGenome.Weights[2] + 1) / 2.0) * 100):F0}% MP");
        Console.WriteLine($"[5] Шопинг:  {bestGenome.Weights[5]:F2}");
        
        Console.WriteLine("\nНажмите [ENTER], чтобы посмотреть Демо-игру.");
        while (Console.KeyAvailable) Console.ReadKey(true);

        var k = Console.ReadKey(true).Key;
        if (k == ConsoleKey.Enter)
        {
            NeuralBotController champion = new NeuralBotController(bestGenome);
            Game demoGame = new Game(champion);
            demoGame.IsHeadless = false; 
            demoGame.Start();
        }
    }

    private static int ReadInt(string prompt, int defaultValue)
    {
        Console.Write(prompt);
        string input = Console.ReadLine();
        if (int.TryParse(input, out int result) && result > 0) return result;
        return defaultValue;
    }
}