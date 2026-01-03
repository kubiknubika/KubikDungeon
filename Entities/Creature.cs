namespace KubikDungeon.Entities;

public class Creature
{
    public string Name { get; set; }
    public int Health { get; set; }
    public int MaxHealth { get; set; }
    public int Damage { get; set; }
    
    // НОВОЕ: Режим тишины
    public bool IsSilent { get; set; } = false;

    public Creature(string name, int health, int damage)
    {
        Name = name;
        Health = health;
        MaxHealth = health;
        Damage = damage;
    }

    public void Attack(Creature target)
    {
        Random rnd = new Random();
        bool isCritical = rnd.Next(1, 101) <= 20; 

        int baseDamage = rnd.Next(Damage - 2, Damage + 3);
        if (baseDamage < 0) baseDamage = 0;

        int finalDamage = isCritical ? baseDamage * 2 : baseDamage;

        target.Health -= finalDamage;

        // ВЫВОДИМ ТОЛЬКО ЕСЛИ НЕ SILENT
        if (!IsSilent)
        {
            if (isCritical)
            {
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine($"🔥 КРИТИЧЕСКИЙ УДАР! {Name} наносит {finalDamage} урона!");
                Console.ResetColor();
            }
            else
            {
                Console.WriteLine($"{Name} атакует {target.Name} и наносит {finalDamage} урона.");
            }
            Console.WriteLine($"   (HP врага: {target.Health}/{target.MaxHealth})");
        }
    }

    public bool IsDead()
    {
        return Health <= 0;
    }
}