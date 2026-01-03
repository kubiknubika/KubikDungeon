using KubikDungeon.Entities;

namespace KubikDungeon.Core;

public interface IController
{
    string GetName();
    HeroClass ChooseClass();
    int ChooseDoor(List<Door> doors, Hero hero);
    string ChooseBattleAction(Hero hero, Enemy enemy);
    int ChooseSpell(Hero hero);
    string ChooseLootAction(Item loot, Hero hero);
    int ChooseInventoryItem(Hero hero);
    string ChooseMenuAction();
    
    // --- НОВОЕ ---
    string ChooseMerchantAction(Hero hero);
}