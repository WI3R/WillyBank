using System;
using System.Runtime.CompilerServices;

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
                    "4. Exit"
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
                    Console.WriteLine("\nUse arrow up/down or w/s for navigation, enter to confirm selection and escape to exit");

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
                            string method = options[selectedIndex];
                            switch (method)
                            {
                                case "1. View all loans":
                                    optionChosen = true;
                                    bank.ViewAllLoans();
                                    break;

                                case "2. Add a loan":
                                    AddNewLoan();
                                    optionChosen = true;
                                    break;

                                case "3. Pay a loan":
                                    optionChosen = true;
                                    Console.Write("Enter name: ");
                                    string payName = Console.ReadLine();
                                    bank.ViewLoan(payName);
                                    Console.Write("Enter payment amount: ");
                                    decimal paymentAmount = decimal.Parse(Console.ReadLine());
                                    bank.PayLoan(payName, paymentAmount);
                                    break;

                                case "4. Exit":
                                    optionChosen = true;
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
            decimal amount = decimal.Parse(Console.ReadLine());
            bank.AddLoan(name, amount);
        }
    }
}
