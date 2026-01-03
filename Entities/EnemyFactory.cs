namespace KubikDungeon.Entities;

public static class EnemyFactory
{
    private static Random _rnd = new Random();

    public static Enemy Generate(int roomNumber)
    {
        // --- БОССЫ (Каждые 5 комнат) ---
        if (roomNumber % 5 == 0)
        {
            string bossName = roomNumber switch
            {
                5 => "👑 КОРОЛЬ СЛИЗНЕЙ",
                10 => "☠️ КОРОЛЬ-ЛИЧ",
                15 => "🐉 ДРЕВНИЙ ДРАКОН",
                _ => "👹 ПОВЕЛИТЕЛЬ ДЕМОНОВ"
            };
            // Урон босса сбалансирован, чтобы не ваншотать
            return new Enemy($"{bossName} (БОСС)", 120 + (roomNumber * 8), 10 + (roomNumber / 2));
        }

        // --- ОБЫЧНЫЕ МОБЫ ---
        int hpBonus = roomNumber * 2;
        int dmgBonus = roomNumber / 3;

        // ЭТАП 1: НАЧАЛО (Комнаты 1-3)
        if (roomNumber <= 3)
        {
            int roll = _rnd.Next(1, 4);
            return roll switch
            {
                1 => new Enemy("Гигантская Крыса", 25 + hpBonus, 4 + dmgBonus),
                2 => new Enemy("Пещерная Мышь", 20 + hpBonus, 5 + dmgBonus),
                _ => new Enemy("Зеленый Слизень", 35 + hpBonus, 6 + dmgBonus)
            };
        }
        // ЭТАП 2: СЕРЕДИНА (Комнаты 4-9)
        else if (roomNumber <= 9)
        {
            int roll = _rnd.Next(1, 5);
            return roll switch
            {
                1 => new Enemy("Гоблин-Разбойник", 50 + hpBonus, 8 + dmgBonus),
                2 => new Enemy("Дикий Волк", 45 + hpBonus, 10 + dmgBonus),
                3 => new Enemy("Гремящий Скелет", 60 + hpBonus, 9 + dmgBonus),
                _ => new Enemy("Орк-Воин", 70 + hpBonus, 12 + dmgBonus)
            };
        }
        // ЭТАП 3: ГЛУБИНА (Комнаты 10+)
        else
        {
            int roll = _rnd.Next(1, 5);
            return roll switch
            {
                1 => new Enemy("Ядовитый Паук", 80 + hpBonus, 14 + dmgBonus),
                2 => new Enemy("Злой Призрак", 70 + hpBonus, 16 + dmgBonus),
                3 => new Enemy("Пещерный Тролль", 110 + hpBonus, 18 + dmgBonus),
                _ => new Enemy("Темный Рыцарь", 100 + hpBonus, 20 + dmgBonus)
            };
        }
    }
}