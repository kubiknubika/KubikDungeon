using KubikDungeon.Entities;

namespace KubikDungeon.Core;

public class NeuralBotController : IController
{
    private NeuralGenome _brain;
    private Random _rnd = new Random();

    public NeuralBotController(NeuralGenome genome)
    {
        _brain = genome;
    }

    public string GetName() => "NeuroBot_v3";

    public HeroClass ChooseClass()
    {
        // ГЕН [0]: Класс
        double gene = _brain.Weights[0];
        if (gene < -0.3) return HeroClass.Warrior;
        if (gene > 0.3) return HeroClass.Mage;     
        return HeroClass.Rogue;
    }

    public string ChooseMenuAction() => "";

    public int ChooseDoor(List<Door> doors, Hero hero)
    {
        double hpPct = (double)hero.Health / hero.GetTotalMaxHealth();
        double riskGene = _brain.Weights[3]; // Любовь к риску

        for (int i = 0; i < doors.Count; i++)
        {
            var type = doors[i].Type;

            // Если бот "Трус" (Risk < -0.5) и ХП не полное - ищет хил приоритетно
            if (riskGene < -0.5 && hpPct < 0.9 && type == DoorType.Heal) return i + 1;

            // Стандартная логика
            if (riskGene > 0.2 && hpPct > 0.6 && type == DoorType.Monster) return i + 1;
            if (riskGene < 0 && hpPct < 0.6 && type == DoorType.Heal) return i + 1;
            if (_brain.Weights[4] > 0.3 && type == DoorType.Merchant) return i + 1;
        }
        return _rnd.Next(1, 4);
    }

    public string ChooseBattleAction(Hero hero, Enemy enemy)
    {
        double hpPct = (double)hero.Health / hero.GetTotalMaxHealth();
        double manaPct = (double)hero.Mana / hero.MaxMana;
        
        // 1. ПРОВЕРКА НА ХИЛ (Зелье)
        // Ген [1] определяет порог паники.
        double potionThreshold = (_brain.Weights[1] + 1) / 2.0; // 0..1
        if (hpPct < potionThreshold && hero.HealingPotions > 0) return "2";

        // 2. ВЫБОР: АТАКА или МАГИЯ
        // Если нет маны или заклинаний - бьем рукой
        if (hero.Spellbook.Count == 0 || hero.Mana < 5) return "1";

        // Если это Маг, он почти всегда хочет кастовать, если есть мана
        if (hero.Class == HeroClass.Mage && manaPct > 0.1) return "3";

        // Для воина/плута: используем ману, если Ген [2] (Агрессия магии) высокий
        double magicDesire = _brain.Weights[2]; 
        if (magicDesire > 0 && manaPct > 0.3) return "3";

        return "1";
    }

    public int ChooseSpell(Hero hero)
    {
        // УМНЫЙ ВЫБОР ЗАКЛИНАНИЯ
        // Мы проходим по книге и ищем самое полезное прямо сейчас
        
        double hpPct = (double)hero.Health / hero.GetTotalMaxHealth();
        double manaPct = (double)hero.Mana / hero.MaxMana;

        int bestSpellIdx = 0;
        double bestScore = -100;

        for (int i = 0; i < hero.Spellbook.Count; i++)
        {
            var spell = hero.Spellbook[i];
            double score = 0;

            // Не хватает маны - сразу нет
            if (hero.Mana < spell.ManaCost) 
            {
                if (score > bestScore) { bestScore = -1; bestSpellIdx = i; } // На крайний случай
                continue;
            }

            switch (spell.Type)
            {
                case SpellType.Heal:
                case SpellType.Drain:
                    // Ценность растет, если мало ХП
                    score = (1.0 - hpPct) * 100; 
                    break;

                case SpellType.Buff: // Концентрация
                    // Ценность растет, если МАЛО маны (восстановить ресурс)
                    score = (1.0 - manaPct) * 80;
                    break;

                case SpellType.Stun:
                    // Всегда полезно, особенно если много маны
                    score = 50; 
                    break;

                case SpellType.Damage:
                    // Базовая полезность. Если высокий Тир - приоритет выше.
                    score = 30 + (spell.Tier * 10);
                    break;
            }

            // Корректировка на "Любимый стиль" из генов
            // Ген [2] поощряет дорогие спеллы
            score += (_brain.Weights[2] * spell.ManaCost);

            if (score > bestScore)
            {
                bestScore = score;
                bestSpellIdx = i;
            }
        }

        return bestSpellIdx + 1;
    }

    public string ChooseLootAction(Item loot, Hero hero)
    {
        // Всегда надеваем лучшее
        Item? current = hero.GetEquippedItem(loot.Type);
        if (current == null) return "E";

        // Сравнение: Для мага важна Магия, для воина Атака/ХП
        int valNew = (hero.Class == HeroClass.Mage) ? loot.MagicBonus : loot.Value;
        int valOld = (hero.Class == HeroClass.Mage) ? current.MagicBonus : current.Value;
        
        // Если предмет универсальный (Кольцо), смотрим сумму
        if (loot.Type == ItemType.Ring) valNew = loot.Value + loot.MagicBonus;
        if (current.Type == ItemType.Ring) valOld = current.Value + current.MagicBonus;

        if (valNew > valOld) return "E";

        // Если не лучше - берем в рюкзак (если жадный) или продать потом
        return "T";
    }

    public int ChooseInventoryItem(Hero hero) => 0;

    public string ChooseMerchantAction(Hero hero)
    {
        // Ген [4]: Торговец
        // Если > 0, бот любит продавать хлам
        if (_brain.Weights[4] > 0 && hero.Backpack.Count > 0) return "1";

        // Ген [5]: Шопоголик
        // Если > 0.5, тратит зелья на шмот
        if (_brain.Weights[5] > 0.5 && hero.HealingPotions >= 3) return "2";

        return "0";
    }
}