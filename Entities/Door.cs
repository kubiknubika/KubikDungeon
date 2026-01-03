namespace KubikDungeon.Entities;

public enum DoorType { Monster, Merchant, Heal, Unknown }

public class Door
{
    public DoorType Type { get; private set; }
    public string Description { get; private set; }

    public Door(DoorType type)
    {
        Type = type;
        if (type == DoorType.Monster) Description = "🔴 Опасная дверь";
        else Description = "⚫ Таинственная дверь";
    }

    public void ResolveMystery()
    {
        Random rnd = new Random();
        int roll = rnd.Next(1, 101);
        
        // Шансы гачи: 40% монстр, 30% торговец, 30% хил
        if (roll <= 40) Type = DoorType.Monster;
        else if (roll <= 70) Type = DoorType.Merchant;
        else Type = DoorType.Heal;
    }

    public string GetTypeName() => Type.ToString();
}