using KubikDungeon.Entities;

namespace KubikDungeon.Core;

public enum BotPersonality { Crazy, Cautious, Farmer, Gambler }

public class BotController : IController
{
    private BotPersonality _personality;
    private Random _rnd = new Random();

    public BotController(BotPersonality personality)
    {
        _personality = personality;
    }

    public string GetName() => $"Bot_{_personality}";

    public HeroClass ChooseClass()
    {
        return _personality switch
        {
            BotPersonality.Crazy => HeroClass.Warrior,
            BotPersonality.Cautious => HeroClass.Mage, 
            BotPersonality.Farmer => HeroClass.Warrior,
            BotPersonality.Gambler => HeroClass.Rogue,
            _ => HeroClass.Warrior
        };
    }

    public string ChooseMenuAction() => "";

    public int ChooseDoor(List<Door> doors, Hero hero)
    {
        for (int i = 0; i < doors.Count; i++)
        {
            var door = doors[i];
            
            switch (_personality)
            {
                case BotPersonality.Cautious:
                    if (hero.Health < hero.GetTotalMaxHealth() * 0.6 && door.Type == DoorType.Heal) return i + 1;
                    if (hero.Health < hero.GetTotalMaxHealth() * 0.3 && door.Type == DoorType.Monster) continue;
                    break;

                case BotPersonality.Crazy:
                    if (door.Type == DoorType.Monster || door.Type == DoorType.Unknown) return i + 1;
                    break;

                case BotPersonality.Farmer:
                    if (door.Type == DoorType.Monster) return i + 1;
                    break;

                case BotPersonality.Gambler:
                    if (door.Type == DoorType.Unknown) return i + 1;
                    if (door.Type == DoorType.Merchant) return i + 1;
                    break;
            }
        }
        return _rnd.Next(1, 4);
    }

    public string ChooseBattleAction(Hero hero, Enemy enemy)
    {
        if (hero.Health < hero.GetTotalMaxHealth() * 0.3 && hero.HealingPotions > 0) return "2";
        if (hero.Mana >= 10 && hero.Spellbook.Count > 0) return "3";
        return "1";
    }

    public int ChooseSpell(Hero hero)
    {
        if (hero.Health < hero.GetTotalMaxHealth() * 0.5)
        {
            var heal = hero.Spellbook.FirstOrDefault(s => s.Type == SpellType.Heal);
            if (heal != null) return hero.Spellbook.IndexOf(heal) + 1;
        }
        return 1; 
    }

    public string ChooseLootAction(Item loot, Hero hero)
    {
        if (_personality == BotPersonality.Gambler) return "E";

        Item? current = hero.GetEquippedItem(loot.Type);
        if (current == null) return "E";

        int newVal = Math.Max(loot.Value, loot.MagicBonus);
        int oldVal = Math.Max(current.Value, current.MagicBonus);

        return newVal > oldVal ? "E" : "T";
    }

    public int ChooseInventoryItem(Hero hero) => 0;

    // НОВЫЙ МЕТОД
    public string ChooseMerchantAction(Hero hero)
    {
        if (hero.Backpack.Count >= hero.MaxBackpackSize) return "1"; // Продать
        if (hero.HealingPotions >= 5) return "2"; // Купить
        return "0"; // Уйти
    }
}