using KubikDungeon.Core;
using KubikDungeon.Services;

while (true)
{
    Console.Clear();
    Console.ForegroundColor = ConsoleColor.Yellow;
    Console.WriteLine("╔════════════════════════════════════╗");
    Console.WriteLine("║        KUBIK DUNGEON RPG v5        ║");
    Console.WriteLine("╚════════════════════════════════════╝");
    Console.ResetColor();
    
    Console.WriteLine("1. 🎮 Новая игра (Человек)");
    Console.WriteLine("2. 🤖 Бот: BEZUMEC (Crazy)");
    Console.WriteLine("3. 🤖 Бот: TRUS (Cautious)");
    Console.WriteLine("4. 🤖 Бот: FARMER (Гриндер)");
    Console.WriteLine("5. 🤖 Бот: GAMBLER (Азартный)");
    Console.ForegroundColor = ConsoleColor.Cyan;
    Console.WriteLine("6. 🧠 НЕЙРОСЕТЬ (Обучить и запустить)");
    Console.ResetColor();
    Console.WriteLine("7. 🏆 Зал Славы");
    Console.WriteLine("0. ❌ Выход");
    Console.Write("> ");

    string choice = Console.ReadLine();

    IController controller = null;

    if (choice == "1") controller = new HumanController();
    else if (choice == "2") controller = new BotController(BotPersonality.Crazy);
    else if (choice == "3") controller = new BotController(BotPersonality.Cautious);
    else if (choice == "4") controller = new BotController(BotPersonality.Farmer);
    else if (choice == "5") controller = new BotController(BotPersonality.Gambler);
    else if (choice == "6")
    {
        NeuralTrainer.Train();
        continue; // После тренировки возвращаемся в меню
    }
    else if (choice == "7") 
    { 
        LeaderboardService.Show(); 
        continue; 
    }
    else if (choice == "0") break;

    if (controller != null)
    {
        Game game = new Game(controller);
        game.Start();
    }
}