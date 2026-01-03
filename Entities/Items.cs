namespace KubikDungeon.Entities;

public enum ItemType { Weapon, Armor, Amulet, Ring, SpellBook }
public enum ItemRarity { Common, Rare, Epic, Legendary }

public class Item
{
    public string Name { get; set; }
    public ItemType Type { get; set; }
    public ItemRarity Rarity { get; set; }
    
    public int Value { get; set; }       // Физ. Урон или HP
    public int MagicBonus { get; set; }  // Магическая сила
    
    public Spell? SpellEffect { get; set; }

    public Item(string name, ItemType type, ItemRarity rarity, int value, int magicBonus = 0)
    {
        Name = name;
        Type = type;
        Rarity = rarity;
        Value = value;
        MagicBonus = magicBonus;
    }

    public Item(Spell spell, ItemRarity rarity)
    {
        Name = $"Том: {spell.Name} {ToRoman(spell.Tier)}";
        Type = ItemType.SpellBook;
        Rarity = rarity;
        Value = 0;
        SpellEffect = spell;
    }

    public string GetDescription()
    {
        if (Type == ItemType.SpellBook)
            return $"📖 {Name} (Тир {SpellEffect?.Tier} {SpellEffect?.Type})";

        string icon = Type switch 
        {
            ItemType.Weapon => "⚔️",
            ItemType.Armor => "🛡️",
            ItemType.Amulet => "📿",
            ItemType.Ring => "💍",
            _ => "?"
        };

        // Формируем строку статов
        List<string> stats = new List<string>();
        if (Value > 0)
        {
            if (Type == ItemType.Armor) stats.Add($"+{Value} HP");
            else stats.Add($"+{Value} Атк");
        }
        if (MagicBonus > 0) stats.Add($"✨ +{MagicBonus} Магии");

        return $"{icon} {Name} ({string.Join(", ", stats)})";
    }

    public ConsoleColor GetColor()
    {
        return Rarity switch
        {
            ItemRarity.Common => ConsoleColor.Gray,
            ItemRarity.Rare => ConsoleColor.Blue,
            ItemRarity.Epic => ConsoleColor.Magenta,
            ItemRarity.Legendary => ConsoleColor.Yellow,
            _ => ConsoleColor.White
        };
    }
    
    private string ToRoman(int n) => n switch { 1 => "I", 2 => "II", 3 => "III", 4 => "IV", _ => "V" };
}