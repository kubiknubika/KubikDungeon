namespace KubikDungeon.Entities;

public enum HeroClass { Warrior, Mage, Rogue }

public class Hero : Creature
{
    // ОСНОВНЫЕ СВОЙСТВА
    public HeroClass Class { get; private set; }
    public int HealingPotions { get; set; } = 3;
    public int Level { get; set; } = 1;
    public int Experience { get; set; } = 0;
    public int XpToNextLevel { get; set; } = 100;

    // МАГИЯ И ЭНЕРГИЯ
    public int Mana { get; set; }
    public int MaxMana { get; set; }
    public int BaseManaRegen { get; set; }
    public int CurrentManaRegen { get; set; }

    public List<Spell> Spellbook { get; private set; } = new List<Spell>();

    // ИНВЕНТАРЬ
    public int MaxBackpackSize { get; } = 10;
    public Item? EquippedWeapon { get; private set; }
    public Item? EquippedArmor { get; private set; }
    public Item? EquippedAmulet { get; private set; }
    public Item? EquippedRing { get; private set; }
    public List<Item> Backpack { get; private set; } = new List<Item>();

    // КОНСТРУКТОР
    public Hero(string name, HeroClass heroClass, bool isSilent = false) : base(name, 100, 10)
    {
        IsSilent = isSilent; // <--- Сразу включаем тишину, ДО надевания вещей
        Class = heroClass;

        switch (Class)
        {
            case HeroClass.Warrior:
                MaxHealth = 120; Damage = 18; MaxMana = 20; BaseManaRegen = 3;
                Spellbook.Add(new Spell("Сильный удар", 1, 8, 10, SpellType.Damage, true));
                EquipDirectly(new Item("Ржавый Меч", ItemType.Weapon, ItemRarity.Common, 5, 0));
                break;

            case HeroClass.Mage:
                MaxHealth = 80; Damage = 5; MaxMana = 60; BaseManaRegen = 8;
                Spellbook.Add(new Spell("Огненный шар", 1, 10, 25, SpellType.Damage, false));
                EquipDirectly(new Item("Старый Посох", ItemType.Weapon, ItemRarity.Common, 2, 10)); 
                break;

            case HeroClass.Rogue:
                MaxHealth = 100; Damage = 15; MaxMana = 40; BaseManaRegen = 5;
                Spellbook.Add(new Spell("Отравленный нож", 1, 10, 15, SpellType.Damage, true));
                EquipDirectly(new Item("Кинжал", ItemType.Weapon, ItemRarity.Common, 8, 0));
                break;
        }

        Health = MaxHealth;
        Mana = MaxMana;
        CurrentManaRegen = BaseManaRegen;
    }

    // --- ПОДСЧЕТ СТАТОВ ---
    public int GetTotalDamage() 
    {
        int total = Damage;
        if (EquippedWeapon != null) total += EquippedWeapon.Value;
        if (EquippedRing != null && EquippedRing.Type == ItemType.Ring) total += EquippedRing.Value;
        return total;
    }

    public int GetTotalMaxHealth() 
    {
        int total = MaxHealth;
        if (EquippedArmor != null) total += EquippedArmor.Value;
        if (EquippedRing != null && EquippedRing.Type == ItemType.Ring) total += EquippedRing.Value;
        return total;
    }

    public int GetTotalMagicPower()
    {
        int total = 0;
        if (EquippedWeapon != null) total += EquippedWeapon.MagicBonus;
        if (EquippedAmulet != null) total += EquippedAmulet.MagicBonus;
        if (EquippedRing != null) total += EquippedRing.MagicBonus;
        return total;
    }
    
    // Хелперы
    public Item? GetEquippedItem(ItemType type) => type switch {
        ItemType.Weapon => EquippedWeapon, ItemType.Armor => EquippedArmor,
        ItemType.Amulet => EquippedAmulet, ItemType.Ring => EquippedRing, _ => null
    };

    private void SetEquippedItem(Item item) {
        switch (item.Type) {
            case ItemType.Weapon: EquippedWeapon = item; break;
            case ItemType.Armor: EquippedArmor = item; break;
            case ItemType.Amulet: EquippedAmulet = item; break;
            case ItemType.Ring: EquippedRing = item; break;
        }
    }

    // --- ЛОГИКА ---

    public void ResetCombatStats() { CurrentManaRegen = BaseManaRegen; }

    public void RegenMana()
    {
        if (Mana < MaxMana)
        {
            Mana += CurrentManaRegen; 
            if (Mana > MaxMana) Mana = MaxMana;
            if (!IsSilent) // <--- ПРОВЕРКА ДЛЯ БОТА
            {
                Console.ForegroundColor = ConsoleColor.Blue;
                Console.WriteLine($"   (+{CurrentManaRegen} MP)"); 
                Console.ResetColor();
            }
        }
    }

    public bool CastSpell(Spell spell, Creature target)
    {
        if (Mana < spell.ManaCost)
        {
            if (!IsSilent) Console.WriteLine("❌ Недостаточно маны!");
            return false;
        }

        Mana -= spell.ManaCost;
        if (!IsSilent)
        {
            Console.ForegroundColor = ConsoleColor.Blue;
            Console.Write($"✨ {Name} использует {spell.Name}! ");
        }

        int val = spell.CalculateEffect(GetTotalMagicPower(), GetTotalDamage());
        bool enemyStunned = false;

        // ЛОГИ
        if (!IsSilent)
        {
            switch (spell.Type)
            {
                case SpellType.Damage: Console.WriteLine($"Нанесено {val} урона!"); break;
                case SpellType.Stun:   Console.WriteLine($"Урон {val}. ❄️ ВРАГ ОГЛУШЕН!"); break;
                case SpellType.Drain:  Console.WriteLine($"Поглощено {val} HP!"); break;
                case SpellType.Heal:   Console.WriteLine($"Восстановлено {val} HP."); break;
                case SpellType.Buff:   Console.WriteLine($"Реген MP +{val}!"); break;
            }
            Console.ResetColor();
        }

        // ЭФФЕКТЫ
        switch (spell.Type)
        {
            case SpellType.Damage: target.Health -= val; break;
            case SpellType.Stun:   target.Health -= val; enemyStunned = true; break;
            case SpellType.Drain:  target.Health -= val; Health += val; if (Health > GetTotalMaxHealth()) Health = GetTotalMaxHealth(); break;
            case SpellType.Heal:   Health += val; if (Health > GetTotalMaxHealth()) Health = GetTotalMaxHealth(); break;
            case SpellType.Buff:   CurrentManaRegen += val; break;
        }
        return enemyStunned;
    }

    public void GainXp(int amount)
    {
        Experience += amount;
        if (Experience >= XpToNextLevel) LevelUp();
    }

    private void LevelUp()
    {
        Level++;
        Experience -= XpToNextLevel;
        XpToNextLevel = (int)(XpToNextLevel * 1.5);

        int hpGain=0, dmgGain=0, manaGain=0;
        switch(Class){
            case HeroClass.Warrior: hpGain=30; dmgGain=4; manaGain=2; break;
            case HeroClass.Mage:    hpGain=10; dmgGain=1; manaGain=15; break;
            case HeroClass.Rogue:   hpGain=20; dmgGain=5; manaGain=5; break;
        }

        MaxHealth += hpGain; Damage += dmgGain; MaxMana += manaGain;
        Health = GetTotalMaxHealth(); Mana = MaxMana;

        if (!IsSilent)
        {
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine($"\n🌟🌟🌟 НОВЫЙ УРОВЕНЬ: {Level}! ({Class}) 🌟🌟🌟");
            Console.WriteLine($"HP +{hpGain} | ATK +{dmgGain} | MP +{manaGain}");
            Console.ResetColor();
        }
    }

    public void Heal()
    {
        if (HealingPotions > 0)
        {
            int tier = (Level <= 3) ? 1 : (Level <= 7 ? 2 : 3);
            int healAmount = tier switch { 1 => 40, 2 => 90, _ => 200 };
            int manaAmount = tier * 20;

            Health += healAmount; if (Health > GetTotalMaxHealth()) Health = GetTotalMaxHealth();
            Mana += manaAmount; if (Mana > MaxMana) Mana = MaxMana;

            HealingPotions--;
            
            if (!IsSilent)
            {
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine($"✨ Зелье (Тир {tier}): +{healAmount} HP и +{manaAmount} MP.");
                Console.ResetColor();
            }
        }
        else if (!IsSilent) Console.WriteLine("🎒 Зелья закончились!");
    }

    public void LearnSpell(Spell newSpell)
    {
        var existing = Spellbook.FirstOrDefault(s => s.Name == newSpell.Name);
        if (existing != null)
        {
            if (newSpell.Tier > existing.Tier)
            {
                Spellbook.Remove(existing); Spellbook.Add(newSpell);
                if (!IsSilent) { Console.ForegroundColor = ConsoleColor.Magenta; Console.WriteLine($"📖 НАВЫК УЛУЧШЕН! {existing.Name} -> {ToRoman(newSpell.Tier)}"); Console.ResetColor(); }
            }
            else
            {
                if (!IsSilent) Console.WriteLine($"Уже знаем.");
                MaxMana += 5; Mana = MaxMana;
            }
        }
        else
        {
            Spellbook.Add(newSpell);
            if (!IsSilent) { Console.ForegroundColor = ConsoleColor.Magenta; Console.WriteLine($"📖 ВЫУЧЕНО: {newSpell.Name} {ToRoman(newSpell.Tier)}!"); Console.ResetColor(); }
        }
    }

    public void EquipFromBackpack(Item item)
    {
        if (item.Type == ItemType.SpellBook) { LearnSpell(item.SpellEffect!); Backpack.Remove(item); return; }

        Item? oldItem = GetEquippedItem(item.Type);
        SetEquippedItem(item);
        Backpack.Remove(item);
        
        if (oldItem != null) Backpack.Add(oldItem);
        if (GetTotalMaxHealth() < Health) Health = GetTotalMaxHealth(); 
        
        if (!IsSilent) Console.WriteLine($"🆗 Вы надели: {item.Name}");
    }

    public void EquipDirectly(Item newItem)
    {
        if (newItem.Type == ItemType.SpellBook) { LearnSpell(newItem.SpellEffect!); return; }

        Item? oldItem = GetEquippedItem(newItem.Type);
        SetEquippedItem(newItem);
        if (!IsSilent) Console.WriteLine($"⚔️ Вы надели: {newItem.Name}");

        if (oldItem != null)
        {
            if (Backpack.Count < MaxBackpackSize) Backpack.Add(oldItem);
            else if (!IsSilent) { Console.ForegroundColor = ConsoleColor.DarkRed; Console.WriteLine($"🎒 Рюкзак полон! Старое выброшено."); Console.ResetColor(); }
        }
        if (GetTotalMaxHealth() < Health) Health = GetTotalMaxHealth(); 
    }

    private string ToRoman(int n) => n switch { 1 => "I", 2 => "II", 3 => "III", _ => "IV" };
}