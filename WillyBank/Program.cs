using System;
using System.Collections.Generic;

namespace WillyBank
{
    internal class Program
    {
        static BankSystem bank;
        static void Main()
        {
            bank = new BankSystem();
            bank.LoadData();

            bool running = true;
            while (running)
            {
                List<string> options = new List<string>
                {
                    "1. View all loans",
                    "2. Add a loan",
                    "3. Pay a loan",
                    "4. Add account",
                    "5. View accounts",
                    "6. Transfer money",
                    "7. Exit"
                };

                int selectedIndex = 0;
                Console.CursorVisible = false;
                bool optionChosen = false;

                while (!optionChosen)
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
                    switch (key)
                    {
                        case ConsoleKey.W:
                        case ConsoleKey.UpArrow:
                            selectedIndex = (selectedIndex > 0) ? selectedIndex - 1 : options.Count - 1;
                            break;
                        case ConsoleKey.S:
                        case ConsoleKey.DownArrow:
                            selectedIndex = (selectedIndex < options.Count - 1) ? selectedIndex + 1 : 0;
                            break;
                        case ConsoleKey.Enter:
                            Console.Clear();
                            optionChosen = true;
                            switch (selectedIndex)
                            {
                                case 0: bank.ViewAllLoans(); break;
                                case 1: AddNewLoan(); break;
                                case 2:
                                    Console.Write("Enter name: ");
                                    bank.PayLoan(Console.ReadLine());
                                    break;
                                case 3:
                                    Console.Write("Enter name: ");
                                    bank.AddAccount(Console.ReadLine());
                                    break;
                                case 4:
                                    Console.Write("Enter name: ");
                                    bank.ViewAccounts(Console.ReadLine());
                                    break;
                                case 5:
                                    Console.Write("Enter name: ");
                                    bank.TransferMoney(Console.ReadLine());
                                    break;
                                case 6:
                                    running = false;
                                    return;
                            }
                            break;
                        case ConsoleKey.Escape:
                            running = false;
                            optionChosen = true;
                            return;
                    }
                }
            }
        }

        static void AddNewLoan()
        {
            Console.Write("Enter name: ");
            string name = Console.ReadLine();

            Console.Write("Enter amount: ");
            if (!decimal.TryParse(Console.ReadLine(), out decimal amount))
            {
                Console.WriteLine("Invalid amount.");
                Console.ReadKey();
                return;
            }

            // Get all accounts that belong to this person
            var accounts = bank.Accounts
                .Where(a => a.OwnerName.Equals(name, StringComparison.OrdinalIgnoreCase))
                .ToList();

            if (accounts.Count == 0)
            {
                Console.WriteLine("This person has no accounts. Please create one first.");
                Console.ReadKey();
                return;
            }

            // Let user pick which account to attach the loan to
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
                    else
                    {
                        Console.WriteLine($"  {accounts[i].AccountId} | Balance: {accounts[i].Balance:C}");
                    }
                }

                key = Console.ReadKey(true).Key;
                if (key == ConsoleKey.Enter)
                    break;
                if (key == ConsoleKey.UpArrow || key == ConsoleKey.W)
                    selectedIndex = (selectedIndex > 0) ? selectedIndex - 1 : accounts.Count - 1;
                else if (key == ConsoleKey.DownArrow || key == ConsoleKey.S)
                    selectedIndex = (selectedIndex < accounts.Count - 1) ? selectedIndex + 1 : 0;
            }

            var chosenAccount = accounts[selectedIndex];
            bank.AddLoan(name, amount, chosenAccount.AccountId);

            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine($"Loan of {amount:C} added for {name} (Account {chosenAccount.AccountId})");
            Console.ResetColor();
            Console.ReadKey();
        }
    }
}