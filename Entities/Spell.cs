namespace KubikDungeon.Entities;

public enum SpellType { Damage, Stun, Drain, Heal, Buff }

public class Spell
{
    public string Name { get; set; }
    public int Tier { get; set; }
    public int ManaCost { get; set; }
    public int BasePower { get; set; } 
    public SpellType Type { get; set; }
    
    // НОВОЕ: Если true, то скалируется от Атаки (для воинов)
    public bool IsPhysical { get; set; } 

    public Spell(string name, int tier, int cost, int power, SpellType type, bool isPhysical = false)
    {
        Name = name;
        Tier = tier;
        ManaCost = cost;
        BasePower = power;
        Type = type;
        IsPhysical = isPhysical;
    }

    public string GetTooltip(int heroMagicPower, int heroPhysicalDamage)
    {
        int val = CalculateEffect(heroMagicPower, heroPhysicalDamage);
        string effect = "";

        switch (Type)
        {
            case SpellType.Damage: 
                effect = IsPhysical ? $"⚔️ {val} Физ. Урона" : $"💥 {val} Маг. Урона"; 
                break;
            case SpellType.Stun:   effect = $"❄️ {val} Урн + Стан"; break;
            case SpellType.Drain:  effect = $"🩸 Кража {val} HP"; break;
            case SpellType.Heal:   effect = $"💚 +{val} HP"; break;
            case SpellType.Buff:   effect = $"⚡ +{val} MP/ход (Реген)"; break; // Изменили описание
        }

        return $"{Name} {ToRoman(Tier)} ({ManaCost} MP) :: {effect}";
    }

    public int CalculateEffect(int magicPower, int physDamage)
    {
        if (Type == SpellType.Buff) return BasePower; // Бафф дает фиксированный прирост регена

        if (IsPhysical)
        {
            // Урон воина: База скилла + (Атака героя * Тир)
            return BasePower + (physDamage * Tier); // Очень мощно на высоких тирах
        }
        else
        {
            // Урон мага: База + Магия
            return BasePower + magicPower;
        }
    }

    private string ToRoman(int n) => n switch { 1 => "I", 2 => "II", 3 => "III", _ => "IV" };
}