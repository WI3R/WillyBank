using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace WillyBank
{
    internal class Program
    {
        static BankSystem bank;

        static void Main()
        {
            CultureInfo.CurrentCulture = new CultureInfo("sv-SE"); // Sets the currency symbol used later to SEK instead of GBP

            bank = new BankSystem();
            bank.LoadData();

            bool running = true;
            while (running)
            {
                List<string> options = new()
                {
                    "1. View a Person",
                    "2. Transfer Money",
                    "3. Exit"
                };

                int selectedIndex = 0;
                Console.CursorVisible = false;

                while (true)
                {
                    Console.Clear();
                    Console.WriteLine("==== Willy Bank ====");
                    for (int i = 0; i < options.Count; i++)
                    {
                        if (i == selectedIndex)
                        {
                            Console.ForegroundColor = ConsoleColor.Cyan;
                            Console.WriteLine($"> {options[i]}");
                            Console.ResetColor();
                        }
                        else
                        {
                            Console.WriteLine($"  {options[i]}");
                        }
                    }

                    Console.WriteLine("\nUse arrow up/down or w/s to navigate, Enter to select, Esc to exit.");

                    var key = Console.ReadKey(true).Key;
                    if (key == ConsoleKey.W || key == ConsoleKey.UpArrow)
                    {
                        selectedIndex = (selectedIndex > 0) ? selectedIndex - 1 : options.Count - 1;
                    }
                    else if (key == ConsoleKey.S || key == ConsoleKey.DownArrow)
                    {
                        selectedIndex = (selectedIndex < options.Count - 1) ? selectedIndex + 1 : 0;
                    }
                    else if (key == ConsoleKey.Escape)
                    {
                        return;
                    }
                    else if (key == ConsoleKey.Enter)
                    {
                        break;
                    }
                }

                Console.Clear();
                switch (selectedIndex)
                {
                    case 0:
                        ViewPerson();
                        break;
                    case 1:
                        Console.Write("Enter name: ");
                        bank.TransferMoney(Console.ReadLine());
                        break;
                    case 2:
                        running = false;
                        return;
                }
            }
        }

        static void ViewPerson()
        {
            while (true)
            {
                var people = bank.Accounts.Select(a => a.OwnerName).Distinct().ToList();
                people.Add("+ Add New Person");

                if (people.Count == 1) // only "+ Add New Person"
                {
                    Console.WriteLine("No people found yet.");
                }

                int selectedIndex = 0;
                ConsoleKey key;
                while (true)
                {
                    Console.Clear();
                    Console.WriteLine("Select a person:");
                    for (int i = 0; i < people.Count; i++)
                    {
                        if (i == selectedIndex)
                        {
                            Console.ForegroundColor = ConsoleColor.Cyan;
                            Console.WriteLine($"> {people[i]}");
                            Console.ResetColor();
                        }
                        else Console.WriteLine($"  {people[i]}");
                    }

                    key = Console.ReadKey(true).Key;
                    if (key == ConsoleKey.Enter)
                    {
                        break;
                    }
                    if (key == ConsoleKey.W || key == ConsoleKey.UpArrow)
                    {
                        selectedIndex = (selectedIndex > 0) ? selectedIndex - 1 : people.Count - 1;
                    }
                    else if (key == ConsoleKey.S || key == ConsoleKey.DownArrow)
                    {
                        selectedIndex = (selectedIndex < people.Count - 1) ? selectedIndex + 1 : 0;
                    }                    
                    else if (key == ConsoleKey.Escape)
                    {
                        return;
                    }
                }

                string selectedPerson = people[selectedIndex];
                if (selectedPerson == "+ Add New Person")
                {
                    Console.Clear();
                    Console.Write("Enter new person's name: ");
                    string newName = Console.ReadLine();

                    Console.Write("Enter starting balance (optional, press Enter for 0): ");
                    string balanceInput = Console.ReadLine();
                    decimal startingBalance = 0;
                    if (!string.IsNullOrWhiteSpace(balanceInput))
                    {
                        decimal.TryParse(balanceInput, out startingBalance);
                    }
                        

                    bank.AddAccount(newName, startingBalance);
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.WriteLine($"Created new person '{newName}' with starting balance {startingBalance:C}");
                    Console.ResetColor();
                    Console.ReadKey();
                    continue; // refresh the person list
                }

                PersonMenu(selectedPerson);
            }
        }

        static void PersonMenu(string name)
        {
            List<string> options = new()
            {
                "1. View Accounts",
                "2. View Loans",
                "3. Add Account",
                "4. Pay Loan",
                "5. Add Loan",
                "6. Add Balance",
                "7. Back"
            };

            int selectedIndex = 0;
            ConsoleKey key;

            while (true)
            {
                Console.Clear();
                Console.WriteLine($"=== {name}'s Menu ===");
                for (int i = 0; i < options.Count; i++)
                {
                    if (i == selectedIndex)
                    {
                        Console.ForegroundColor = ConsoleColor.Yellow;
                        Console.WriteLine($"> {options[i]}");
                        Console.ResetColor();
                    }
                    else Console.WriteLine($"  {options[i]}");
                }

                key = Console.ReadKey(true).Key;
                if (key == ConsoleKey.W || key == ConsoleKey.UpArrow)
                {
                    selectedIndex = (selectedIndex > 0) ? selectedIndex - 1 : options.Count - 1;
                }
                else if (key == ConsoleKey.S || key == ConsoleKey.DownArrow)
                {
                    selectedIndex = (selectedIndex < options.Count - 1) ? selectedIndex + 1 : 0;
                }
                else if (key == ConsoleKey.Escape)
                {
                    return;
                }
                else if (key == ConsoleKey.Enter)
                {
                    Console.Clear();
                    switch (selectedIndex)
                    {
                        case 0:
                            bank.ViewAccounts(name);
                            break;
                        case 1:
                            bank.ViewLoan(name);
                            Console.ReadKey();
                            break;
                        case 2:
                            bank.AddAccount(name);
                            Console.WriteLine("Account added.");
                            Console.ReadKey();
                            break;
                        case 3:
                            bank.PayLoan(name);
                            break;
                        case 4:
                            AddNewLoan(name);
                            break;
                        case 5:
                            AddBalance(name);
                            break;
                        case 6:
                            return;
                    }
                }
            }
        }

        static void AddNewLoan(string name)
        {
            Console.Write("Enter amount: ");
            if (!decimal.TryParse(Console.ReadLine(), out decimal amount))
            {
                Console.WriteLine("Invalid amount.");
                Console.ReadKey();
                return;
            }

            var accounts = bank.Accounts.Where(a => a.OwnerName.Equals(name, StringComparison.OrdinalIgnoreCase)).ToList();
            if (accounts.Count == 0)
            {
                Console.WriteLine("This person has no accounts. Please create one first.");
                Console.ReadKey();
                return;
            }

            int selectedIndex = 0;
            ConsoleKey key;
            while (true)
            {
                Console.Clear();
                Console.WriteLine($"Select account for {name}:");
                for (int i = 0; i < accounts.Count; i++)
                {
                    if (i == selectedIndex)
                    {
                        Console.ForegroundColor = ConsoleColor.Cyan;
                        Console.WriteLine($"> {accounts[i].AccountId} | Balance: {accounts[i].Balance:C}");
                        Console.ResetColor();
                    }
                    else Console.WriteLine($"  {accounts[i].AccountId} | Balance: {accounts[i].Balance:C}");
                }

                key = Console.ReadKey(true).Key;
                if (key == ConsoleKey.Enter)
                {
                    break;
                }
                if (key == ConsoleKey.W || key == ConsoleKey.UpArrow)
                {
                    selectedIndex = (selectedIndex > 0) ? selectedIndex - 1 : accounts.Count - 1;
                }
                else if (key == ConsoleKey.S || key == ConsoleKey.DownArrow)
                {
                    selectedIndex = (selectedIndex < accounts.Count - 1) ? selectedIndex + 1 : 0;
                }
            }

            var chosenAccount = accounts[selectedIndex];
            bank.AddLoan(name, amount, chosenAccount.AccountId);

            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine($"Loan of {amount:C} added for {name} (Account {chosenAccount.AccountId})");
            Console.ResetColor();
            Console.ReadKey();
        }

        static void AddBalance(string name)
        {
            var accounts = bank.Accounts.Where(a => a.OwnerName.Equals(name, StringComparison.OrdinalIgnoreCase)).ToList();
            if (accounts.Count == 0)
            {
                Console.WriteLine("This person has no accounts.");
                Console.ReadKey();
                return;
            }

            int selectedIndex = 0;
            ConsoleKey key;
            while (true)
            {
                Console.Clear();
                Console.WriteLine($"Select account to add balance to for {name}:");
                for (int i = 0; i < accounts.Count; i++)
                {
                    if (i == selectedIndex)
                    {
                        Console.ForegroundColor = ConsoleColor.Green;
                        Console.WriteLine($"> {accounts[i].AccountId} | Balance: {accounts[i].Balance:C}");
                        Console.ResetColor();
                    }
                    else
                    {
                        Console.WriteLine($"  {accounts[i].AccountId} | Balance: {accounts[i].Balance:C}");
                    }
                }

                key = Console.ReadKey(true).Key;
                if (key == ConsoleKey.Enter)
                {
                    break;
                }
                if (key == ConsoleKey.W || key == ConsoleKey.UpArrow)
                {
                    selectedIndex = (selectedIndex > 0) ? selectedIndex - 1 : accounts.Count - 1;
                }
                else if (key == ConsoleKey.S || key == ConsoleKey.DownArrow)
                {
                    selectedIndex = (selectedIndex < accounts.Count - 1) ? selectedIndex + 1 : 0;
                }
            }

            var account = accounts[selectedIndex];
            Console.Write("Enter amount to add: ");
            if (!decimal.TryParse(Console.ReadLine(), out decimal amount) || amount <= 0)
            {
                Console.WriteLine("Invalid amount.");
                Console.ReadKey();
                return;
            }

            account.Balance += amount;
            bank.SaveData();

            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine($"Added {amount:C} to {account.AccountId}. New balance: {account.Balance:C}");
            Console.ResetColor();
            Console.ReadKey();
        }
    }
}
