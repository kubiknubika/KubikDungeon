using KubikDungeon.Entities;

namespace KubikDungeon.Core;

public class HumanController : IController
{
    public string GetName()
    {
        Console.Write("Введите имя героя: ");
        return Console.ReadLine() ?? "Странник";
    }

    public HeroClass ChooseClass()
    {
        while (true)
        {
            Console.WriteLine("ВЫБЕРИТЕ КЛАСС: [1] Воин [2] Маг [3] Плут");
            Console.Write("> ");
            string c = Console.ReadLine();
            if (c == "1") return HeroClass.Warrior;
            if (c == "2") return HeroClass.Mage;
            if (c == "3") return HeroClass.Rogue;
        }
    }

    public int ChooseDoor(List<Door> doors, Hero hero)
    {
        Console.Write("Выберите дверь (1-3): ");
        if (int.TryParse(Console.ReadLine(), out int c) && c >= 1 && c <= 3) return c;
        return 1;
    }

    public string ChooseBattleAction(Hero hero, Enemy enemy)
    {
        Console.Write("> ");
        return Console.ReadLine(); 
    }

    public int ChooseSpell(Hero hero)
    {
        Console.Write("Выберите заклинание (номер): ");
        if (int.TryParse(Console.ReadLine(), out int c)) return c;
        return 0;
    }

    public string ChooseLootAction(Item loot, Hero hero)
    {
        Console.Write("> ");
        return Console.ReadLine().ToUpper();
    }

    public int ChooseInventoryItem(Hero hero)
    {
        if (int.TryParse(Console.ReadLine(), out int c)) return c;
        return 0;
    }

    public string ChooseMenuAction()
    {
        Console.Write("> ");
        return Console.ReadLine();
    }

    // НОВЫЙ МЕТОД
    public string ChooseMerchantAction(Hero hero)
    {
        Console.Write("> ");
        return Console.ReadLine();
    }
}