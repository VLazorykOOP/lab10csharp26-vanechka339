using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace HorseLifeEvents
{
    public class HorseEventArgs : EventArgs
    {
        public string Action { get; }
        public int Intensity { get; }
        public int Priority { get; }
        public string? Status { get; set; }

        public HorseEventArgs(string action, int intensity, int priority)
        {
            Action = action;
            Intensity = intensity;
            Priority = priority;
            Status = "Очікує обробки";
        }
    }

    public delegate void HorseEventHandler(object sender, HorseEventArgs e);

    public class Horse
    {
        public event HorseEventHandler? OnLifeEvent;

        private Random rnd = new Random();
        public Dictionary<string, int> Statistics = new Dictionary<string, int>();

        public async Task LiveDayAsync(int hours)
        {
            for (int i = 1; i <= hours; i++)
            {
                Console.WriteLine($"--- Година {i} ---");

                int eventType = rnd.Next(1, 4);
                HorseEventArgs args = eventType switch
                {
                    1 => new HorseEventArgs("Голод", rnd.Next(1, 10), 3),
                    2 => new HorseEventArgs("Втома", rnd.Next(1, 10), 2),
                    _ => new HorseEventArgs("Бажання бігати", rnd.Next(1, 10), 1)
                };

                await Task.Run(() => TriggerEvent(args));

                UpdateStats(args.Action);
                await Task.Delay(500);
            }
        }

        protected virtual void TriggerEvent(HorseEventArgs e)
        {
            Console.WriteLine($"Кінь: Потрібно -> {e.Action} (Пріоритет: {e.Priority})");

            OnLifeEvent?.Invoke(this, e);

            if (!string.IsNullOrEmpty(e.Status))
                Console.WriteLine($"Служби звітують: {e.Status}");
        }

        private void UpdateStats(string action)
        {
            if (Statistics.ContainsKey(action)) Statistics[action]++;
            else Statistics[action] = 1;
        }
    }

    public abstract class Receiver
    {
        protected Horse horse;
        public Receiver(Horse horse) { this.horse = horse; }
        public abstract void Handle(object sender, HorseEventArgs e);
    }

    public class Stable : Receiver
    {
        public Stable(Horse horse) : base(horse) { }
        public override void Handle(object sender, HorseEventArgs e)
        {
            if (e.Action == "Голод") e.Status = "Кінь нагодований у стайні.";
            if (e.Action == "Втома") e.Status = "Кінь відпочиває.";
        }
    }

    public class Trainer : Receiver
    {
        public Trainer(Horse horse) : base(horse) { }
        public override void Handle(object sender, HorseEventArgs e)
        {
            if (e.Action == "Бажання бігати") e.Status = "Тренер провів тренування.";
        }
    }

    class Program
    {
        static async Task Main(string[] args)
        {
            Console.OutputEncoding = System.Text.Encoding.UTF8;

            Horse myHorse = new Horse();
            Stable stable = new Stable(myHorse);
            Trainer trainer = new Trainer(myHorse);

            myHorse.OnLifeEvent += stable.Handle;
            myHorse.OnLifeEvent += trainer.Handle;

            await myHorse.LiveDayAsync(5);

            Console.WriteLine("\n=== ПІДСУМКОВА СТАТИСТИКА ===");
            foreach (var stat in myHorse.Statistics)
            {
                Console.WriteLine($"{stat.Key}: {stat.Value} раз(и)");
            }
        }
    }
}