using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;

public enum Category { Food, Transport, Fun, School, Other }

public class ValidationException : Exception {
    public ValidationException(string msg) : base(msg) {}
}

public class Income {
    public DateTime Date { get; }
    public string Source { get; }
    public decimal Amount { get; }

    public Income(DateTime date, string source, decimal amount) {
        if (amount <= 0) throw new ValidationException("Summai jābūt > 0");
        if (string.IsNullOrWhiteSpace(source)) throw new ValidationException("Avots nedrīkst būt tukšs");
        Date = date;
        Source = source;
        Amount = amount;
    }
}

public class Expense {
    public DateTime Date { get; }
    public Category Category { get; }
    public decimal Amount { get; }
    public string Note { get; }

    public Expense(DateTime date, Category category, decimal amount, string note) {
        if (amount <= 0) throw new ValidationException("Summai jābūt > 0");
        if (string.IsNullOrWhiteSpace(note)) throw new ValidationException("Nevar būt tukša");
        Date = date;
        Category = category;
        Amount = amount;
        Note = note;
    }
}

public class Subscription {
    public string Name { get; }
    public decimal MonthlyPrice { get; }
    public DateTime StartDate { get; }
    public bool IsActive { get; private set; }

    public Subscription(string name, decimal monthlyPrice, DateTime startDate, bool isActive) {
        if (monthlyPrice <= 0) throw new ValidationException("Cenai jābūt > 0");
        if (string.IsNullOrWhiteSpace(name)) throw new ValidationException("Nosaukums nedrīkst būt tukšs");
        Name = name;
        MonthlyPrice = monthlyPrice;
        StartDate = startDate;
        IsActive = isActive;
    }

    public void Toggle() {
        IsActive = !IsActive;
    }
}

public static class Tools {
    public static string ReadNonEmptyString(string prompt) {
        string s;
        do {
            Console.Write(prompt);
            s = Console.ReadLine() ?? "";
        } while (string.IsNullOrWhiteSpace(s));
        return s;
    }

    public static DateTime ReadDate(string prompt) {
        DateTime d;
        while (true) {
            Console.Write(prompt);
            if (DateTime.TryParse(Console.ReadLine(), out d)) return d;
            Console.WriteLine("Nepareizs datums, mēģini vēlreiz (GGGG-MM-DD).");
        }
    }

    public static decimal ReadDecimal(string prompt) {
        decimal d;
        while (true) {
            Console.Write(prompt);
            if (decimal.TryParse(Console.ReadLine(), out d) && d > 0) return d;
            Console.WriteLine("Ievadi skaitli > 0.");
        }
    }

    public static decimal Percent(decimal part, decimal whole) {
        return whole == 0 ? 0 : (part / whole) * 100;
    }

    public static decimal SafeDivide(decimal a, decimal b) {
        return b == 0 ? 0 : a / b;
    }
}

public class Program {
    static List<Income> incomes = new List<Income>();
    static List<Expense> expenses = new List<Expense>();
    static List<Subscription> subscriptions = new List<Subscription>();

    public static void Main() {
        while (true) {
            Console.WriteLine("\n--- Personīgais finanšu plānotājs ---");
            Console.WriteLine("1) Ienākumi");
            Console.WriteLine("2) Izdevumi");
            Console.WriteLine("3) Abonementi");
            Console.WriteLine("4) Viss");
            Console.WriteLine("5) Filtri");
            Console.WriteLine("6) Mēneša pārskats");
            Console.WriteLine("7) Import/Export JSON");
            Console.WriteLine("0) Iziet");
            Console.Write("Izvēle: ");

            string choice = Console.ReadLine() ?? "";

            try {
                switch (choice) {
                    case "1": IncomeMenu(); break;
                    case "2": ExpenseMenu(); break;
                    case "3": SubscriptionMenu(); break;
                    case "4": ShowAll(); break;
                    case "5": Filters(); break;
                    case "6": MonthlyReport(); break;
                    case "7": JsonMenu(); break;
                    case "0": return;
                    default: Console.WriteLine("Nederīga izvēle"); break;
                }
            } catch (ValidationException ex) {
                Console.WriteLine("Kļūda: " + ex.Message);
            }
        }
    }

    static void IncomeMenu() {
        Console.WriteLine("1) Pievienot ienākumu 2) Rādīt ienākumus 3) Dzēst");
        var ch = Console.ReadLine();
        if (ch == "1") {
            var date = Tools.ReadDate("Datums: ");
            var src = Tools.ReadNonEmptyString("Avots: ");
            var amt = Tools.ReadDecimal("Summa: ");
            incomes.Add(new Income(date, src, amt));
        } else if (ch == "2") {
            foreach (var i in incomes) Console.WriteLine($"{i.Date:d} {i.Source} {i.Amount}€");
        } else if (ch == "3") {
            for (int i=0;i<incomes.Count;i++) Console.WriteLine($"{i}) {incomes[i].Source} {incomes[i].Amount}€");
            Console.Write("Indekss: ");
            if (int.TryParse(Console.ReadLine(), out int idx) && idx>=0 && idx<incomes.Count)
                incomes.RemoveAt(idx);
        }
    }

    static void ExpenseMenu() {
        Console.WriteLine("1) Pievienot izdevumu 2) Rādīt izdevumus 3) Dzēst");
        var ch = Console.ReadLine();
        if (ch == "1") {
            var date = Tools.ReadDate("Datums: ");
            Console.WriteLine("Kategorija: 0=Food 1=Transport 2=Fun 3=School 4=Other");
            var cat = (Category)int.Parse(Console.ReadLine() ?? "0");
            var amt = Tools.ReadDecimal("Summa: ");
            var note = Tools.ReadNonEmptyString("Piezīme: ");
            expenses.Add(new Expense(date, cat, amt, note));
        } else if (ch == "2") {
            foreach (var e in expenses) Console.WriteLine($"{e.Date:d} {e.Category} {e.Amount}€ {e.Note}");
        } else if (ch == "3") {
            for (int i=0;i<expenses.Count;i++) Console.WriteLine($"{i}) {expenses[i].Category} {expenses[i].Amount}€");
            Console.Write("Indekss: ");
            if (int.TryParse(Console.ReadLine(), out int idx) && idx>=0 && idx<expenses.Count)
                expenses.RemoveAt(idx);
        }
    }

    static void SubscriptionMenu() {
        Console.WriteLine("1) Pievienot abonementu 2) Rādīt abonementus 3) Pārslēgt aktīvs/neaktīvs 4) Dzēst");
        var ch = Console.ReadLine();
        if (ch == "1") {
            var name = Tools.ReadNonEmptyString("Nosaukums: ");
            var price = Tools.ReadDecimal("Cena mēnesī: ");
            var date = Tools.ReadDate("Sākuma datums: ");
            subscriptions.Add(new Subscription(name, price, date, true));
        } else if (ch == "2") {
            foreach (var s in subscriptions) Console.WriteLine($"{s.Name} {s.MonthlyPrice}€ {(s.IsActive?"Aktīvs":"Neaktīvs")}");
        } else if (ch == "3") {
            for (int i=0;i<subscriptions.Count;i++) Console.WriteLine($"{i}) {subscriptions[i].Name} {subscriptions[i].IsActive}");
            Console.Write("Indekss: ");
            if (int.TryParse(Console.ReadLine(), out int idx) && idx>=0 && idx<subscriptions.Count)
                subscriptions[idx].Toggle();
        } else if (ch == "4") {
            for (int i=0;i<subscriptions.Count;i++) Console.WriteLine($"{i}) {subscriptions[i].Name}");
            Console.Write("Indekss: ");
            if (int.TryParse(Console.ReadLine(), out int idx) && idx>=0 && idx<subscriptions.Count)
                subscriptions.RemoveAt(idx);
        }
    }

    static void ShowAll() {
        Console.WriteLine("Visi ieraksti pēc datuma (dilstoši):");
        var allDates = incomes.Select(i => (i.Date, $"Ienākums {i.Source} {i.Amount}€"))
            .Concat(expenses.Select(e => (e.Date, $"Izdevums {e.Category} {e.Amount}€ {e.Note}")))
            .Concat(subscriptions.Select(s => (s.StartDate, $"Abonements {s.Name} {s.MonthlyPrice}€ {(s.IsActive?"Aktīvs":"Neaktīvs")}")))
            .OrderByDescending(x => x.Item1);
        foreach (var x in allDates) Console.WriteLine($"{x.Item1:d} {x.Item2}");
    }

    static void Filters() {
        Console.WriteLine("1) Izdevumi pēc kategorijas 2) Izdevumi pēc datuma");
        var ch = Console.ReadLine();
        if (ch=="1") {
            Console.WriteLine("Kategorija: 0=Food 1=Transport 2=Fun 3=School 4=Other");
            var cat = (Category)int.Parse(Console.ReadLine()??"0");
            var filt = expenses.Where(e=>e.Category==cat);
            Console.WriteLine($"Kopā {filt.Sum(e=>e.Amount)}€");
        } else if (ch=="2") {
            var from = Tools.ReadDate("No: ");
            var to = Tools.ReadDate("Līdz: ");
            var filt = expenses.Where(e=>e.Date>=from && e.Date<=to);
            Console.WriteLine($"Kopā {filt.Sum(e=>e.Amount)}€");
        }
    }

    static void MonthlyReport() {
        Console.Write("Ievadi mēnesi (GGGG-MM): ");
        var input = Console.ReadLine() ?? "";
        if (!DateTime.TryParse(input+"-01", out var month)) {
            Console.WriteLine("Nepareizs formāts");
            return;
        }
        var inSum = incomes.Where(i=>i.Date.Year==month.Year && i.Date.Month==month.Month).Sum(i=>i.Amount);
        var exSum = expenses.Where(e=>e.Date.Year==month.Year && e.Date.Month==month.Month).Sum(e=>e.Amount);
        var subsSum = subscriptions.Where(s=>s.IsActive).Sum(s=>s.MonthlyPrice);
        var net = inSum - exSum - subsSum;

        Console.WriteLine($"Ienākumi: {inSum}€");
        Console.WriteLine($"Izdevumi: {exSum}€");
        Console.WriteLine($"Abonementi: {subsSum}€");
        Console.WriteLine($"Neto: {net}€");

        foreach (Category c in Enum.GetValues(typeof(Category))) {
            var catSum = expenses.Where(e=>e.Date.Year==month.Year && e.Date.Month==month.Month && e.Category==c).Sum(e=>e.Amount);
            Console.WriteLine($"{c}: {Tools.Percent(catSum, exSum):0.0}%");
        }
        var monthExpenses = expenses.Where(e=>e.Date.Year==month.Year && e.Date.Month==month.Month).OrderByDescending(e=>e.Amount);
        if (monthExpenses.Any()) {
            var maxExp = monthExpenses.First();
            Console.WriteLine($"Lielākais izdevums: {maxExp.Category} {maxExp.Amount}€");
        }
        var days = DateTime.DaysInMonth(month.Year, month.Month);
        Console.WriteLine($"Vidēji dienā: {Tools.SafeDivide(exSum, days):0.00}€");
    }

    static void JsonMenu() {
        Console.WriteLine("1) Eksportēt 2) Importēt");
        var ch = Console.ReadLine();
        if (ch=="1") {
            var obj = new { incomes, expenses, subscriptions };
            Console.WriteLine(JsonSerializer.Serialize(obj, new JsonSerializerOptions{WriteIndented=true}));
        } else if (ch=="2") {
            Console.WriteLine("Ievadi JSON (vienu rindu vai kompaktu formātu):");
            var txt = Console.ReadLine() ?? "";
            if (string.IsNullOrWhiteSpace(txt)) {
                Console.WriteLine("Tukšs ievads, darbība atcelta.");
                return;
            }
            try {
                var obj = JsonSerializer.Deserialize<JsonData>(txt);
                if (obj!=null) {
                    incomes = obj.incomes ?? new List<Income>();
                    expenses = obj.expenses ?? new List<Expense>();
                    subscriptions = obj.subscriptions ?? new List<Subscription>();
                    Console.WriteLine("Dati veiksmīgi importēti!");
                }
            } catch {
                Console.WriteLine("Nepareizs JSON, dati netika nomainīti.");
            }
        }
    }

    public class JsonData {
        public List<Income> incomes {get;set;}
        public List<Expense> expenses {get;set;}
        public List<Subscription> subscriptions {get;set;}
    }
}
