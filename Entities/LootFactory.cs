namespace KubikDungeon.Entities;

public static class LootFactory
{
    private static Random _rnd = new Random();

    public static Item Generate(int heroLevel, bool isBoss)
    {
        int roll = _rnd.Next(1, 101);
        ItemRarity rarity;
        
        if (isBoss) rarity = (roll <= 30) ? ItemRarity.Rare : (roll <= 80 ? ItemRarity.Epic : ItemRarity.Legendary);
        else rarity = (roll <= 60) ? ItemRarity.Common : (roll <= 90 ? ItemRarity.Rare : ItemRarity.Epic);

        // 1. КНИГИ (15%)
        if (_rnd.Next(1, 101) <= 15) return GenerateSpellbook(rarity, heroLevel);

        // 2. ЭКИПИРОВКА
        // 30% Оружие, 30% Броня, 20% Амулет, 20% Кольцо
        int typeRoll = _rnd.Next(1, 101);
        ItemType type;
        if (typeRoll <= 30) type = ItemType.Weapon;
        else if (typeRoll <= 60) type = ItemType.Armor;
        else if (typeRoll <= 80) type = ItemType.Amulet;
        else type = ItemType.Ring;

        // Базовые статы
        int baseStat = heroLevel * 2; 
        int multiplier = rarity switch { ItemRarity.Common => 1, ItemRarity.Rare => 2, ItemRarity.Epic => 4, ItemRarity.Legendary => 7, _ => 1 };
        int statBudget = baseStat + (multiplier * 3);

        string name = "Предмет";
        int value = 0;      // Физ/ХП
        int magic = 0;      // Магия

        switch (type)
        {
            case ItemType.Weapon:
                // 30% шанс на Посох
                bool isStaff = _rnd.Next(0, 3) == 0;
                if (isStaff) { magic = statBudget + 5; value = heroLevel; name = GenerateName("Staff", rarity); }
                else { value = statBudget; name = GenerateName("Weapon", rarity); }
                break;

            case ItemType.Armor:
                value = statBudget * 2; // Броня дает много ХП
                name = GenerateName("Armor", rarity);
                break;

            case ItemType.Amulet:
                // Амулет - чисто магический слот
                magic = statBudget + (heroLevel * 2); 
                name = GenerateName("Amulet", rarity);
                break;

            case ItemType.Ring:
                // Кольцо - универсальное. Либо Атака, либо Магия, либо ХП.
                int r = _rnd.Next(0, 3);
                if (r == 0) { value = statBudget; name = GenerateName("RingPhys", rarity); } // Кольцо силы
                else if (r == 1) { magic = statBudget; name = GenerateName("RingMag", rarity); } // Кольцо магии
                else { value = statBudget * 2; /*HP*/ name = GenerateName("RingHP", rarity); } // Кольцо жизни
                break;
        }

        return new Item(name, type, rarity, value, magic);
    }

    // (Метод GenerateSpellbook оставляем старый, он был хорош)
    private static Item GenerateSpellbook(ItemRarity rarity, int heroLevel)
    {
        int tier = (heroLevel - 1) / 3 + 1; 
        if (tier < 1) tier = 1;
        if (heroLevel >= 5) tier = 2;
        if (heroLevel >= 10) tier = 3;

        SpellType spellType = (SpellType)_rnd.Next(0, 5); 
        
        string name = spellType switch {
            SpellType.Damage => "Огненный шар",
            SpellType.Stun   => "Ледяной шип",
            SpellType.Drain  => "Вампиризм",
            SpellType.Heal   => "Исцеление",
            SpellType.Buff   => "Концентрация",
            _ => "Магия"
        };

        int cost = 8 + (tier * 4); 
        int power = 0;

        switch (spellType)
        {
            case SpellType.Damage: power = 15 + (tier * 10); break; 
            case SpellType.Stun:   power = 10 + (tier * 8);  break; 
            case SpellType.Drain:  power = 10 + (tier * 8);  break; 
            case SpellType.Heal:   power = 30 + (tier * 15); break; 
            case SpellType.Buff:   power = 20 + (tier * 10); cost = 10; break;
        }
        
        // Воинам тоже генерим техники иногда (как в прошлом коде)
        // Для краткости я оставил только магию, но ты можешь вернуть блок с WarriorTech

        Spell spell = new Spell(name, tier, cost, power, spellType);
        return new Item(spell, rarity);
    }

    private static string GenerateName(string category, ItemRarity rarity)
    {
        string[] adj = rarity switch {
            ItemRarity.Common => new[] { "Простой", "Старый" },
            ItemRarity.Rare => new[] { "Зачарованный", "Редкий" },
            ItemRarity.Epic => new[] { "Мифический", "Древний" },
            ItemRarity.Legendary => new[] { "БОЖЕСТВЕННЫЙ", "ЗВЕЗДНЫЙ" },
            _ => new[] { "" }
        };

        string baseName = category switch {
            "Weapon" => new[] { "Меч", "Топор", "Клинок", "Молот" }[_rnd.Next(4)],
            "Staff" => new[] { "Посох", "Жезл", "Скипетр" }[_rnd.Next(3)],
            "Armor" => new[] { "Куртка", "Кольчуга", "Латы", "Мантия" }[_rnd.Next(4)],
            "Amulet" => new[] { "Амулет", "Ожерелье", "Талисман", "Четки" }[_rnd.Next(4)],
            "RingPhys" => "Кольцо Силы",
            "RingMag" => "Кольцо Магии",
            "RingHP" => "Кольцо Жизни",
            _ => "Предмет"
        };

        return $"{adj[_rnd.Next(adj.Length)]} {baseName}";
    }
}