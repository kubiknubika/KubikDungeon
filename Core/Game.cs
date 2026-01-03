using KubikDungeon.Entities;
using KubikDungeon.Services;

namespace KubikDungeon.Core;

public class Game
{
    private Hero _player;
    private int _roomNumber = 1;
    private IController _controller;
    private bool _isBot;
    
    public bool IsHeadless { get; set; } = false; // РЕЖИМ ТИШИНЫ
    public int ResultScore { get; private set; } = 0; 
    
    private List<string> _journeyLog = new List<string>();

    public Game(IController controller)
    {
        _controller = controller;
        _isBot = controller is BotController || controller is NeuralBotController;
    }

    public void Start()
    {
        if (!IsHeadless)
        {
            Console.Clear();
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine(_isBot ? $"🤖 Бот {_controller.GetName()} начал забег..." : "⚔️  Kubik Dungeon v5 ⚔️");
            Console.ResetColor();
        }

        string name = _controller.GetName();
        HeroClass cls = _controller.ChooseClass();
        
        // ПЕРЕДАЕМ IsHeadless ПРЯМО ТУТ
        _player = new Hero(name, cls, IsHeadless); 
        
        _roomNumber = 1;

        GameLoop();
    }
    private void Wait() { if (!_isBot && !IsHeadless) Console.ReadLine(); }
    private void Sleep(int ms) { if (!_isBot && !IsHeadless) Thread.Sleep(ms); }

    private void Log(string text, bool isImportant = false)
    {
        if (IsHeadless) return; 
        if (!_isBot) Console.WriteLine(text);
        else if (isImportant) _journeyLog.Add($"[Комната {_roomNumber}] {text}");
    }

    private void GameLoop()
    {
        int watchdog = 0; // Счетчик действий для защиты от зависания

        while (!_player.IsDead())
        {
            watchdog++;
            if (IsHeadless && watchdog > 10000) 
            {
                // Если бот топчется на месте 10000 итераций - убиваем его
                _player.Health = -1;
                break;
            }
            if (!_isBot && !IsHeadless) Console.Clear();
            if (_isBot && !IsHeadless) Console.Write($"\r👣 Комната: {_roomNumber} | Lvl: {_player.Level} | HP: {_player.Health}   ");
            
            Log($"\n👣 УРОВЕНЬ #{_roomNumber}", false);
            
            if (!_isBot && !IsHeadless)
            {
                Console.WriteLine($"❤️ HP: {_player.Health}/{_player.GetTotalMaxHealth()} | 💧 MP: {_player.Mana}/{_player.MaxMana}");
                Console.WriteLine($"⚔️ Физ: {_player.GetTotalDamage()} | ✨ Маг: {_player.GetTotalMagicPower()}");
                Console.ForegroundColor = ConsoleColor.Cyan;
                Console.WriteLine($"🛡️ Экип: [{_player.EquippedWeapon?.Name??"Нет"}] [{_player.EquippedArmor?.Name??"Нет"}]");
                Console.ResetColor();
            }

            if (_roomNumber % 5 == 0)
            {
                Log("☠️ ЛОГОВО БОССА", true);
                if (!_isBot && !IsHeadless) { Console.WriteLine("Нажмите ENTER..."); Wait(); }
                ProcessFight(true); 
            }
            else
            {
                var doors = GenerateDoors();
                if (!_isBot && !IsHeadless)
                {
                    Console.WriteLine("\nВыберите путь:");
                    for (int i = 0; i < doors.Count; i++) Console.WriteLine($"[{i + 1}] {doors[i].Description}");
                    Console.WriteLine("[I] Инвентарь [0] Сбежать");
                }

                string input = _controller.ChooseMenuAction();

                if (input.ToUpper() == "I") { ManageInventory(); continue; }
                if (input == "0") { EndGame(true); return; }
                
                int choice = _controller.ChooseDoor(doors, _player);
                Log($"Выбрана дверь #{choice}", false);
                
                ProcessDoor(doors[choice - 1]);
            }

            if (!_player.IsDead()) _roomNumber++;
        }
        EndGame(false);
    }

    private void ProcessDoor(Door door)
    {
        if (door.Type == DoorType.Unknown)
        {
            door.ResolveMystery();
            Log($"🎲 Туман рассеивается... Это {door.GetTypeName()}!", false);
        }

        switch (door.Type)
        {
            case DoorType.Monster: ProcessFight(false); break;
            case DoorType.Merchant: 
                if (_isBot && !IsHeadless) Log("Торговец (авто).", false);
                Merchant.Trade(_player, _controller); 
                break;
            case DoorType.Heal: ProcessShrine(); break;
        }
    }

    private void ProcessShrine()
    {
        int heal = (int)(_player.GetTotalMaxHealth() * 0.3) + 10;
        int oldHp = _player.Health;
        _player.Health += heal;
        if (_player.Health > _player.GetTotalMaxHealth()) _player.Health = _player.GetTotalMaxHealth();
        
        Log($"✨ Источник восстановил {heal} HP.", true);
        Wait();
    }

    private void ProcessFight(bool isBossFight)
    {
        _player.ResetCombatStats(); 
        
        Enemy enemy = EnemyFactory.Generate(_roomNumber);
        
        // ВАЖНО: Передаем настройку тишины врагу
        enemy.IsSilent = IsHeadless;

        Log($"⚠️ БИТВА: {enemy.Name} ({enemy.Health} HP)", isBossFight);
        int fightTurns = 0;

        while (!_player.IsDead() && !enemy.IsDead())
        {
            fightTurns++;
            _player.RegenMana(); 
            if (IsHeadless && fightTurns > 500)
            {
                // Бой затянулся (бесконечный хил?) - ничья, герой уходит (или умирает)
                _player.Health = -1; 
                break;
            }


            if (!_isBot && !IsHeadless)
            {
                Console.WriteLine($"\n👤 {_player.Health} HP | 🧪 {_player.HealingPotions}");
                Console.WriteLine($"👹 {enemy.Health} HP");
                Console.WriteLine("[1] Атака [2] Зелье [3] Магия");
            }

            string act = _controller.ChooseBattleAction(_player, enemy);
            bool stunned = false;

            if (act == "1") _player.Attack(enemy);
            else if (act == "2") _player.Heal();
            else if (act == "3") stunned = CastMagic(enemy);
            
            if (enemy.IsDead()) break;
            
            if (!stunned) { Sleep(200); enemy.Attack(_player); }
            else Log("❄️ Враг оглушен!", false);
        }

        if (!_player.IsDead())
        {
            Log($"🏆 Победа над {enemy.Name}!", isBossFight);
            _player.GainXp(enemy.MaxHealth);
            LootPhase(enemy.Name.Contains("БОСС"));
        }
    }

    private bool CastMagic(Enemy enemy)
    {
        if (_player.Spellbook.Count == 0) return false;
        
        if (!_isBot && !IsHeadless)
        {
            for(int i=0; i<_player.Spellbook.Count; i++)
                Console.WriteLine($"[{i+1}] {_player.Spellbook[i].GetTooltip(_player.GetTotalMagicPower(), _player.GetTotalDamage())}");
        }

        int idx = _controller.ChooseSpell(_player);
        if (idx > 0 && idx <= _player.Spellbook.Count)
            return _player.CastSpell(_player.Spellbook[idx-1], enemy);
        
        return false;
    }

    private void LootPhase(bool isBoss)
    {
        Random rnd = new Random();
        if (isBoss || rnd.Next(1, 101) <= 40)
        {
            Item loot = LootFactory.Generate(_player.Level, isBoss);
            Log($"🎁 ЛУТ: {loot.GetDescription()}", isBoss);

            if (_player.GetEquippedItem(loot.Type) == null && loot.Type != ItemType.SpellBook) 
            { 
                _player.EquipDirectly(loot); 
                Log($"   -> Надето автоматически: {loot.Name}", isBoss);
                Wait(); 
                return; 
            }

            string p = _controller.ChooseLootAction(loot, _player);
            if (p == "E") {
                _player.EquipDirectly(loot);
                if (isBoss) Log($"   -> Игрок надел: {loot.Name}", true);
            }
            else if (p == "T") {
                if (_player.Backpack.Count < _player.MaxBackpackSize) _player.Backpack.Add(loot);
            }
        }
        Wait();
    }

    private void ManageInventory()
    {
        if (_isBot) return; 
        while(true)
        {
            Console.Clear();
            Console.WriteLine("🎒 ИНВЕНТАРЬ:");
            for(int i=0; i<_player.Backpack.Count; i++)
                Console.WriteLine($"[{i+1}] {_player.Backpack[i].GetDescription()}");
            Console.WriteLine("0 - Выход");
            
            if (int.TryParse(Console.ReadLine(), out int c) && c > 0 && c <= _player.Backpack.Count)
            {
                _player.EquipFromBackpack(_player.Backpack[c-1]);
                Console.ReadLine();
            }
            else break;
        }
    }

    private void EndGame(bool escaped)
    {
        ResultScore = _roomNumber; 
        if (IsHeadless) return; 

        Console.WriteLine();
        Console.Clear();
        
        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine("📜 === КРАТКАЯ ИСТОРИЯ ПУТЕШЕСТВИЯ === 📜");
        Console.ResetColor();
        
        foreach (var entry in _journeyLog)
        {
            Console.WriteLine(entry);
        }
        
        Console.WriteLine("---------------------------------------------");
        if (escaped) Console.WriteLine("🏃 ИТОГ: ПОБЕГ");
        else Console.WriteLine($"💀 ИТОГ: СМЕРТЬ (Убийца: {_roomNumber} комната)");
        
        Console.WriteLine($"Герой: {_player.Name} | Класс: {_player.Class} | Уровень: {_player.Level}");
        
        LeaderboardService.SaveScore(_player.Name, _player.Class.ToString(), _player.Level, _roomNumber);
        
        Console.WriteLine("\nНажмите Enter...");
        Console.ReadLine();
    }

    private List<Door> GenerateDoors()
    {
        var list = new List<Door> { new Door(DoorType.Monster), new Door(DoorType.Unknown), new Door(DoorType.Unknown) };
        return list.OrderBy(x => Guid.NewGuid()).ToList(); 
    }
}