using System;
using System.Globalization;
using System.Linq;

namespace WillyBank
{
    internal class Program
    {
        static BankSystem bank;   // Main bank system instance

        static void Main()
        {
            // Set culture to Swedish for currency formatting
            CultureInfo.CurrentCulture = new CultureInfo("sv-SE");

            bank = new BankSystem();
            bank.LoadData(); // Makes sure that previously saved users, loans and accounts are avalible on startup

            while (true)
            {
                Console.Clear();
                Console.WriteLine("==== Willy Bank Login ====\n");

                // Build a user list + option to add a new user
                var userList = bank.Users.Select(u => u.Username).ToList();
                userList.Add("+ Add User");

                int selectedIndex = 0;
                ConsoleKey key;
                Console.CursorVisible = false; // Hide cursor for cleaner look and navigation

                // User selection menu
                while (true)
                {
                    Console.Clear();
                    Console.WriteLine("Select a user:");
                    for (int i = 0; i < userList.Count; i++)
                    {
                        // Highlight selected user
                        if (i == selectedIndex)
                        {
                            Console.ForegroundColor = ConsoleColor.Cyan;
                            Console.WriteLine($"> {userList[i]}");
                            Console.ResetColor();
                        }
                        else Console.WriteLine($"  {userList[i]}");
                    }

                    key = Console.ReadKey(true).Key;

                    // Confirm selection
                    if (key == ConsoleKey.Enter)
                    {
                        break;
                    }

                    // Navigation up
                    if (key == ConsoleKey.W || key == ConsoleKey.UpArrow)
                    {
                        selectedIndex = (selectedIndex > 0) ? selectedIndex - 1 : userList.Count - 1;
                    }
                    // Navigation down
                    else if (key == ConsoleKey.S || key == ConsoleKey.DownArrow)
                    {
                        selectedIndex = (selectedIndex < userList.Count - 1) ? selectedIndex + 1 : 0;
                    }
                    // Exit program
                    else if (key == ConsoleKey.Escape)
                    {
                        return;
                    }
                }
                Console.CursorVisible = true;

                string selectedUser = userList[selectedIndex];

                // Create a new user
                if (selectedUser == "+ Add User")
                {
                    CreateNewUser();
                    continue;
                }

                // Password prompt
                Console.Write($"Enter password for {selectedUser}: ");
                string password = ReadPassword();

                // Authenticate user
                var user = bank.Authenticate(selectedUser, password);
                if (user == null)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("\nInvalid password.");
                    Console.ResetColor();
                    Console.ReadKey();
                    continue;
                }

                // Open logged-in menu
                UserMenu(user);
            }
        }

        // Handles creating new users
        static void CreateNewUser()
        {
            Console.Clear();
            Console.Write("Enter username: ");
            string username = Console.ReadLine();

            // Have to enter the same password twice when creating a user for non accidental passwords
            Console.Write("Enter password: ");
            string pass1 = ReadPassword();
            Console.Write("\nConfirm password: ");
            string pass2 = ReadPassword();

            // Password mismatch check
            if (pass1 != pass2)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("\nPasswords do not match.");
                Console.ResetColor();
                Console.ReadKey();
                return;
            }

            // Try to add user
            if (!bank.AddUser(username, pass1)) 
            // Returns false if there is already a username with that name. 
            //Makes so there are not any conflicts later on in the code when using the name to filter in methods.
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("\nUser already exists.");
                Console.ResetColor();
                Console.ReadKey();
                return;
            }

            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("\nUser created successfully!");
            Console.ResetColor();
            Console.ReadKey();
        }

        // Menu shown after login
        static void UserMenu(User user)
        {
            while (true)
            {
                Console.Clear();
                Console.WriteLine($"=== {user.Username}'s Menu ===");

                string[] options = {
                    "1. View Accounts",
                    "2. Add Account",
                    "3. Transfer Money",
                    "4. View Loans",
                    "5. Add Loan",
                    "6. Pay Loan",
                    "7. Add Balance",
                    "8. Logout"
                };

                int selectedIndex = 0;
                ConsoleKey key;
                Console.CursorVisible = false;

                // User selects option
                while (true)
                {
                    Console.Clear();
                    Console.WriteLine($"=== {user.Username}'s Menu ===");

                    for (int i = 0; i < options.Length; i++)
                    {
                        // Highlight selection
                        if (i == selectedIndex)
                        {
                            Console.ForegroundColor = ConsoleColor.Yellow;
                            Console.WriteLine($"> {options[i]}");
                            Console.ResetColor();
                        }
                        else Console.WriteLine($"  {options[i]}");
                    }

                    key = Console.ReadKey(true).Key;

                    if (key == ConsoleKey.Enter)
                    {
                        break;
                    }

                    // Navigation
                    if (key == ConsoleKey.W || key == ConsoleKey.UpArrow)
                    {
                        selectedIndex = (selectedIndex > 0) ? selectedIndex - 1 : options.Length - 1;
                    }
                    else if (key == ConsoleKey.S || key == ConsoleKey.DownArrow)
                    {
                        selectedIndex = (selectedIndex < options.Length - 1) ? selectedIndex + 1 : 0;
                    }
                    else if (key == ConsoleKey.Escape)
                    {
                        return;
                    }
                }
                Console.CursorVisible = true;

                ConsoleKey key1;
                // Handle selected option
                switch (selectedIndex)
                {
                    case 0:
                        // Show accounts
                        var accs = bank.GetAccounts(user.Username);
                        foreach (var acc in accs)
                        {
                            Console.WriteLine($"{acc.AccountId} | {acc.Balance:C}");
                        }

                        Console.WriteLine("Press spacebar to continue");
                        while ((key1 = Console.ReadKey(true).Key) != ConsoleKey.Spacebar) { }
                        break;

                    case 1:
                        // Add account
                        Console.Write("Enter starting balance: ");
                        decimal start = decimal.Parse(Console.ReadLine());
                        bank.AddAccount(user.Username, start);
                        break;

                    case 2:
                        // Transfer money
                        bank.TransferMoney(user.Username);
                        break;

                    case 3:
                        // View loan
                        bank.ViewLoan(user.Username);
                        Console.ReadKey();
                        break;

                    case 4:
                        // Add loan
                        AddNewLoan(user.Username);
                        break;

                    case 5:
                        // Pay loan
                        bank.PayLoan(user.Username);
                        break;

                    case 6:
                        // Add balance to first account
                        AddBalance(user.Username);
                        break;

                    case 7:
                        // Logout
                        return;
                }
            }
        }

        // Create new loan for a user
        static void AddNewLoan(string username)
        {
            Console.Write("Enter loan amount: ");
            if (!decimal.TryParse(Console.ReadLine(), out decimal amount))
                return;

            var accs = bank.GetAccounts(username);

            // Need at least one account
            if (accs.Count == 0)
            {
                Console.WriteLine("You must have an account first.");
                Console.ReadKey();
                return;
            }

            var acc = accs.First();
            bank.AddLoan(username, amount, acc.AccountId);
        }

        // Add balance to user's first account
        static void AddBalance(string username)
        {
            var accs = bank.GetAccounts(username);
            if (accs.Count == 0)
            {
                Console.WriteLine("You must have an account first.");
                Console.ReadKey();
                return;
            }

            var acc = accs.First();

            Console.Write("Enter amount: ");
            if (!decimal.TryParse(Console.ReadLine(), out decimal amt))
            {
                AddBalance(username); // Re-prompt recursively
            }

            acc.Balance += amt;
            bank.SaveData();
        }

        // Reads password input with * masking
        static string ReadPassword()
        {
            string pass = "";
            ConsoleKey key;

            while ((key = Console.ReadKey(true).Key) != ConsoleKey.Enter)
            {
                // Handle backspace
                if (key == ConsoleKey.Backspace && pass.Length > 0)
                {
                    pass = pass.Substring(0, pass.Length - 1);
                    Console.Write("\b \b");
                }
                // Add character if valid
                else if (!char.IsControl((char)key))
                {
                    pass += (char)key;
                    Console.Write("*");
                }
            }
            return pass;
        }
    }
}
