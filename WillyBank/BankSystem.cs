using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection.Metadata.Ecma335;
using System.Security.Cryptography.X509Certificates;
using System.Text.Json;

namespace WillyBank
{
    public class BankSystem
    {
        // Path to save all bank data
        private string filePath = "bank_data.json";

        // Main data structures
        public List<User> Users { get; private set; } = new();
        public List<LoanManager> Loans { get; private set; } = new();
        public List<BankAccount> Accounts { get; private set; } = new();

        // Load all stored data from JSON file
        public void LoadData()
        {
            // If file missing or empty, create fresh data
            if (!File.Exists(filePath) || new FileInfo(filePath).Length == 0)
            {
                Console.WriteLine("No valid data file found. Creating new data...");
                Users = new();
                Loans = new();
                Accounts = new();
                SaveData();
                ApplyInterest();
                return;
            }

            try
            {
                string json = File.ReadAllText(filePath);

                // If empty JSON, start fresh
                if (string.IsNullOrWhiteSpace(json))
                {
                    Console.WriteLine("Data file is empty. Initializing new data...");
                    Users = new();
                    Loans = new();
                    Accounts = new();
                }
                else
                {
                    // Try to deserialize
                    var data = JsonSerializer.Deserialize<BankData>(json);

                    // If corrupted or invalid, reset
                    if (data == null)
                    {
                        Console.WriteLine("Data file invalid. Starting fresh...");
                        Users = new();
                        Loans = new();
                        Accounts = new();
                    }
                    else
                    {
                        Users = data.Users ?? new();
                        Loans = data.Loans ?? new();
                        Accounts = data.Accounts ?? new();
                    }
                }
            }
            catch (JsonException ex)
            {
                // JSON reading failed, reset data
                Console.WriteLine("JSON error: " + ex.Message);
                Console.WriteLine("Resetting data...");
                Users = new();
                Loans = new();
                Accounts = new();
            }

            // Apply daily interest changes
            ApplyInterest();
        }

        /// <summary>
        /// Saves the entire database to disk
        /// </summary>
        public void SaveData()
        {
            var data = new BankData { Users = Users, Loans = Loans, Accounts = Accounts };
            string json = JsonSerializer.Serialize(data, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(filePath, json);
        }

        // Add a new user if username is available
        public bool AddUser(string username, string password)
        {
            foreach (User u in Users)
            {
                if (u.Username.Equals(username, StringComparison.OrdinalIgnoreCase))
                {
                    return false; // username exists
                }
            }

            Users.Add(new User(username, password));
            SaveData();
            return true;
        }

        // Login validation
        public User Authenticate(string username, string password)
        {
            foreach (User u in Users)
            {
                // Case-insensitive username, case-sensitive password
                if (u.Username.Equals(username, StringComparison.OrdinalIgnoreCase) && u.Password == password)
                {
                    return u;
                }
            }

            return null; // login failed
        }

        // Adds daily interest to accounts and loans since last update
        private void ApplyInterest()
        {
            DateTime now = DateTime.Now;

            foreach (User user in Users)
            {
                TimeSpan diff = now - user.LastInterestUpdate;
                int days = (int)diff.TotalDays;

                // Skip if no days passed
                if (days <= 0) continue;

                // 1% per day for accounts
                foreach (BankAccount acc in Accounts)
                {
                    if (user.AccountIds.Contains(acc.AccountId))
                    {
                        for (int i = 0; i < days; i++)
                        {
                            acc.Balance *= 1.01m;
                        }
                    }
                }

                // 5% per day for loans
                foreach (LoanManager loan in Loans)
                {
                    if (loan.Name == user.Username)
                    {
                        for (int i = 0; i < days; i++)
                        {
                            loan.LoanAmount *= 1.05m;
                        }
                    }
                }

                user.LastInterestUpdate = now;
            }

            SaveData();
        }

        // Create a new loan under a user
        public void AddLoan(string name, decimal amount, Guid accountID)
        {
            var record = new LoanManager(name, amount, accountID);
            Loans.Add(record);
            SaveData();
        }

        // Pay off a loan for a user
        public void PayLoan(string name)
        {
            List<string> matchingPeople = new List<string>();

            // Find all people with loans matching input name
            foreach (LoanManager l in Loans)
            {
                if (l.Name.Equals(name, StringComparison.OrdinalIgnoreCase))
                {
                    bool exists = false;

                    // Avoid duplicates
                    foreach (string existing in matchingPeople)
                    {
                        if (existing.Equals(name, StringComparison.OrdinalIgnoreCase))
                        {
                            exists = true;
                            break;
                        }
                    }

                    if (!exists)
                    {
                        matchingPeople.Add(l.Name);
                    }
                }
            }

            // No loans found
            if (matchingPeople.Count == 0)
            {
                Console.WriteLine("No loan found for that person.");
                Console.ReadKey();
                return;
            }

            // Select which user to pay loan for
            int selectedIndex = 0;
            ConsoleKey key;

            while (true)
            {
                Console.Clear();
                Console.WriteLine("Select which person:");
                for (int i = 0; i < matchingPeople.Count; i++)
                {
                    if (i == selectedIndex)
                    {
                        Console.ForegroundColor = ConsoleColor.Cyan;
                        Console.WriteLine($"> {matchingPeople[i]}");
                        Console.ResetColor();
                    }
                    else Console.WriteLine($"  {matchingPeople[i]}");
                }

                key = Console.ReadKey(true).Key;

                if (key == ConsoleKey.Enter)
                {
                    break;
                }
                if (key == ConsoleKey.UpArrow || key == ConsoleKey.W)
                {
                    selectedIndex = (selectedIndex > 0) ? selectedIndex - 1 : matchingPeople.Count - 1;
                }
                else if (key == ConsoleKey.DownArrow || key == ConsoleKey.S)
                {
                    selectedIndex = (selectedIndex < matchingPeople.Count - 1) ? selectedIndex + 1 : 0;
                }
            }

            string selectedName = matchingPeople[selectedIndex];

            // Collect all loans under the selected user
            List<LoanManager> selectedLoans = new List<LoanManager>();
            foreach (LoanManager l in Loans)
            {
                if (l.Name == selectedName)
                {
                    selectedLoans.Add(l);
                }
            }

            if (selectedLoans.Count == 0)
            {
                Console.WriteLine("No loans found.");
                Console.ReadKey();
                return;
            }

            // Select the loan to pay off
            selectedIndex = 0;
            while (true)
            {
                Console.Clear();
                Console.WriteLine($"Select loan to pay for {selectedName}:");
                for (int i = 0; i < selectedLoans.Count; i++)
                {
                    if (i == selectedIndex)
                    {
                        Console.ForegroundColor = ConsoleColor.Yellow;
                        Console.WriteLine($"> Loan {i + 1}: {selectedLoans[i].LoanAmount:C}");
                        Console.ResetColor();
                    }
                    else Console.WriteLine($"  Loan {i + 1}: {selectedLoans[i].LoanAmount:C}");
                }

                key = Console.ReadKey(true).Key;

                if (key == ConsoleKey.Enter)
                {
                    break;
                }
                if (key == ConsoleKey.UpArrow || key == ConsoleKey.W)
                {
                    selectedIndex = (selectedIndex > 0) ? selectedIndex - 1 : selectedLoans.Count - 1;
                }
                else if (key == ConsoleKey.DownArrow || key == ConsoleKey.S)
                {
                    selectedIndex = (selectedIndex < selectedLoans.Count - 1) ? selectedIndex + 1 : 0;
                }
            }

            var loan = selectedLoans[selectedIndex];

            // Enter amount to pay
            Console.Write("Enter payment amount: ");
            if (!decimal.TryParse(Console.ReadLine(), out decimal payment))
            {
                Console.WriteLine("Invalid amount.");
                Console.ReadKey();
                return;
            }

            // Reduce loan amount
            loan.LoanAmount -= payment;

            // Fully paid
            if (loan.LoanAmount <= 0)
            {
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine($"{loan.Name} has fully repaid their loan.");
                Console.ResetColor();
                Console.ReadKey();
                Loans.Remove(loan);
            }

            SaveData();
        }

        // Show a single user's loan
        public void ViewLoan(string name)
        {
            LoanManager loan = null;

            // Find first matching loan
            foreach (LoanManager l in Loans)
            {
                if (l.Name.Equals(name, StringComparison.OrdinalIgnoreCase))
                {
                    loan = l;
                    break;
                }
            }

            if (loan == null)
            {
                Console.WriteLine("No active loan found for that name");
            }
            else
            {
                Console.WriteLine($"{loan.Name} has an active loan for {loan.LoanAmount:C}");
            }
        }

        // View all loans for debugging/admin
        public void ViewAllLoans()
        {
            if (Loans.Count == 0)
            {
                Console.WriteLine("No active loans.");
                Console.ReadKey();
                return;
            }

            foreach (var loan in Loans)
            {
                Console.WriteLine($"{loan.Name} | {loan.LoanAmount:C} | Account: {loan.AccountId}");
            }

            Console.ReadKey();
        }

        // Create a new bank account for a user
        public void AddAccount(string username, decimal startingBalance = 0)
        {
            var account = new BankAccount(Guid.NewGuid(), username, startingBalance);
            Accounts.Add(account);

            // Link account to user
            User accountUser = null;

            foreach (User u in Users)
            {
                if (u.Username == username)
                {
                    accountUser = u;
                    break;
                }
            }

            if (accountUser != null)
            {
                accountUser.AccountIds.Add(account.AccountId);
            }

            SaveData();
        }

        // Get all accounts belonging to a user
        public List<BankAccount> GetAccounts(string username)
        {
            List<BankAccount> results = new List<BankAccount>();

            foreach (BankAccount a in Accounts)
            {
                if (a.OwnerName.Equals(username, StringComparison.OrdinalIgnoreCase))
                {
                    results.Add(a);
                }
            }

            return results;
        }

        // View account summary for user
        public void ViewAccounts(string name)
        {
            List<BankAccount> accounts = new List<BankAccount>();

            foreach (BankAccount a in Accounts)
            {
                if (a.OwnerName.Equals(name, StringComparison.OrdinalIgnoreCase))
                {
                    accounts.Add(a);
                }
            }

            if (accounts.Count == 0)
            {
                Console.WriteLine("No accounts found for that name!");
                Console.ReadKey();
                return;
            }

            Console.WriteLine($"Accounts for {name}:");
            foreach (var account in accounts)
            {
                Console.WriteLine($"{account.AccountId} | Balance: {account.Balance:C}");
            }

            Console.ReadKey();
        }

        // Transfer money between accounts
        public void TransferMoney(string name)
        {
            List<BankAccount> accounts = new List<BankAccount>();

            // Filter accounts belonging to the user
            foreach (BankAccount a in Accounts)
            {
                if (a.OwnerName.Equals(name, StringComparison.OrdinalIgnoreCase))
                {
                    accounts.Add(a);
                }
            }

            if (accounts.Count < 2)
            {
                Console.WriteLine("You need at least 2 accounts to transfer between.");
                Console.ReadKey();
                return;
            }

            // Pick source account
            Console.WriteLine("Select source account:");
            var source = SelectAccount(accounts);

            // Build destination list excluding source
            List<BankAccount> destinationOptions = new List<BankAccount>();
            foreach (BankAccount a in Accounts)
            {
                if (a != source)
                {
                    destinationOptions.Add(a);
                }
            }

            Console.WriteLine("Select destination account: ");
            BankAccount destination = SelectAccount(destinationOptions);

            // Enter transfer amount
            Console.Write("Enter transfer amount: ");
            if (!decimal.TryParse(Console.ReadLine(), out decimal amount) || amount <= 0)
            {
                Console.WriteLine("Invalid amount.");
                Console.ReadKey();
                return;
            }

            // Check funds
            if (source.Balance < amount)
            {
                Console.WriteLine("Insufficient funds.");
                Console.ReadKey();
                return;
            }

            // Perform transfer
            source.Balance -= amount;
            destination.Balance += amount;
            SaveData();

            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine($"Transferred {amount:C} from {source.AccountId} to {destination.AccountId}");
            Console.ResetColor();
            Console.ReadKey();
        }

        // Menu selector for choosing accounts
        private BankAccount SelectAccount(List<BankAccount> accounts)
        {
            int selectedIndex = 0;
            ConsoleKey key;

            while (true)
            {
                Console.Clear();
                Console.WriteLine("Select an account:");

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
                {
                    break;
                }
                if (key == ConsoleKey.UpArrow || key == ConsoleKey.W)
                {
                    selectedIndex = (selectedIndex > 0) ? selectedIndex - 1 : accounts.Count - 1;
                }
                else if (key == ConsoleKey.DownArrow || key == ConsoleKey.S)
                {
                    selectedIndex = (selectedIndex < accounts.Count - 1) ? selectedIndex + 1 : 0;
                }
            }

            return accounts[selectedIndex];
        }
    }
}
