using KubikDungeon.Core; // Нужно для IController

namespace KubikDungeon.Entities;

public static class Merchant
{
    public static void Trade(Hero player, IController controller)
    {
        // Если это бот, не чистим экран и не выводим приветствие каждый раз, чтобы не спамить
        bool isBot = controller is BotController || controller is NeuralBotController;

        if (!isBot)
        {
            Console.Clear();
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("💰 ТОРГОВЕЦ АРТЕФАКТАМИ");
            Console.ResetColor();
        }

        while (true)
        {
            if (!isBot)
            {
                Console.WriteLine("\n------------------------------------------------");
                Console.WriteLine($"🧪 Зелий: {player.HealingPotions} | 🎒 Рюкзак: {player.Backpack.Count}/{player.MaxBackpackSize}");
                Console.WriteLine("[1] 📤 ПРОДАТЬ предмет (+1 Зелье)");
                Console.WriteLine("[2] 📥 КУПИТЬ предмет (-3 Зелья)");
                Console.WriteLine("[0] Уйти");
            }

            // СПРАШИВАЕМ У КОНТРОЛЛЕРА
            string choice = controller.ChooseMerchantAction(player);

            if (choice == "1") SellItem(player, controller, isBot);
            else if (choice == "2") BuyItem(player, isBot);
            else if (choice == "0") break; // <--- ВАЖНО: Выход из цикла
            
            // Если бот что-то сделал, прерываем цикл, чтобы он не застрял в бесконечных покупках/продажах
            if (isBot) break; // <--- ЭТА СТРОКА ОБЯЗАТЕЛЬНА
        }
    }

    private static void SellItem(Hero player, IController controller, bool isBot)
    {
        if (player.Backpack.Count == 0) return;

        int idx = 0;
        
        if (isBot)
        {
            // Бот продает первый попавшийся предмет (самый старый)
            idx = 1; 
        }
        else
        {
            Console.WriteLine("\nЧто продать?");
            for (int i = 0; i < player.Backpack.Count; i++)
                Console.WriteLine($"[{i + 1}] {player.Backpack[i].GetDescription()}");
            Console.Write("> ");
            int.TryParse(Console.ReadLine(), out idx);
        }

        if (idx > 0 && idx <= player.Backpack.Count)
        {
            Item item = player.Backpack[idx - 1];
            player.Backpack.RemoveAt(idx - 1);
            player.HealingPotions++;
            if (!isBot) Console.WriteLine($"✅ Продано: {item.Name}");
        }
    }

    private static void BuyItem(Hero player, bool isBot)
    {
        int cost = 3;
        if (player.HealingPotions < cost) return;

        player.HealingPotions -= cost;
        Item newItem = LootFactory.Generate(player.Level, false);

        if (!isBot) Console.WriteLine($"✅ Куплено: {newItem.GetDescription()}");

        if (newItem.Type == ItemType.SpellBook)
        {
            player.LearnSpell(newItem.SpellEffect!);
        }
        else
        {
            // Бот автоматически надевает, если лучше, или кладет в рюкзак
            if (isBot)
            {
                Item? current = player.GetEquippedItem(newItem.Type);
                if (current == null || (newItem.Value > current.Value))
                    player.EquipDirectly(newItem);
                else if (player.Backpack.Count < player.MaxBackpackSize)
                    player.Backpack.Add(newItem);
            }
            else
            {
                if (player.Backpack.Count < player.MaxBackpackSize) player.Backpack.Add(newItem);
                else Console.WriteLine("🎒 Рюкзак полон! Предмет потерян.");
            }
        }
    }
}