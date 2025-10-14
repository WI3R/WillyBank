using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace WillyBank
{
    public class BankSystem
    {
        private string filePath = "bank_data.json";
        public List<LoanManager> Loans { get; private set; } = new();
        public List<BankAccount> Accounts { get; private set; } = new();

        /// <summary>
        /// Load from file
        /// </summary>
        public void LoadData()
        {
            if (File.Exists(filePath))
            {
                string json = File.ReadAllText(filePath);
                var data = JsonSerializer.Deserialize<BankData>(json);
                Loans = data?.Loans ?? new();
                Accounts = data?.Accounts ?? new();
            }
            else
            {
                Loans = new();
            }
        }

        /// <summary>
        /// Save to file
        /// </summary>
        public void SaveData()
        {
            var data = new BankData { Loans = Loans, Accounts = Accounts };
            string json = JsonSerializer.Serialize(data, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(filePath, json);
        }


        // Loans
        public void AddLoan(string name, decimal amount, Guid accountID)
        {
            var record = new LoanManager(name, amount, accountID);
            Loans.Add(record);
            SaveData();
        }

        public void PayLoan(string name)
        {
            var matchingPeople = Loans
                .Where(l => l.Name.Equals(name, StringComparison.OrdinalIgnoreCase))
                .Select(l => l.Name)
                .Distinct()
                .ToList();

            if (matchingPeople.Count == 0)
            {
                Console.WriteLine("No loan found for that person.");
                Console.ReadKey();
                return;
            }

            // If there are multiple people with same name, pick which one
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
            var selectedLoans = Loans.Where(l => l.Name == selectedName).ToList();

            if (selectedLoans.Count == 0)
            {
                Console.WriteLine("No loans found.");
                Console.ReadKey();
                return;
            }

            // Choose which loan to pay
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
                    break;
                if (key == ConsoleKey.UpArrow || key == ConsoleKey.W)
                    selectedIndex = (selectedIndex > 0) ? selectedIndex - 1 : selectedLoans.Count - 1;
                else if (key == ConsoleKey.DownArrow || key == ConsoleKey.S)
                    selectedIndex = (selectedIndex < selectedLoans.Count - 1) ? selectedIndex + 1 : 0;
            }

            var loan = selectedLoans[selectedIndex];
            Console.Write("Enter payment amount: ");
            if (!decimal.TryParse(Console.ReadLine(), out decimal payment))
            {
                Console.WriteLine("Invalid amount.");
                Console.ReadKey();
                return;
            }

            loan.LoanAmount -= payment;
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

        public void ViewLoan(string name)
        {
            var loan = Loans.Find(l => l.Name.Equals(name, StringComparison.OrdinalIgnoreCase));
            if (loan == null)
            {
                Console.WriteLine("No active loan found for that name");
            }
            else
            {
                Console.WriteLine($"{loan.Name} has an active loan for {loan.LoanAmount:C}");
            }        
        }

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

        // Accounts
        public void AddAccount(string name, decimal startingBalance = 0)
        {
            var account = new BankAccount(Guid.NewGuid(), name, startingBalance);
            Accounts.Add(account);
            SaveData();
        }

        public void ViewAccounts(string name)
        {
            var accounts = Accounts.Where(a => a.OwnerName.Equals(name, StringComparison.OrdinalIgnoreCase)).ToList();
            if (accounts.Count == 0)
            {
                Console.WriteLine("No accounts found for that name!");
                Console.ReadKey();
                return;
            }

            Console.WriteLine($"Accounts for {name}:");
            foreach(var account in accounts)
            {
                Console.WriteLine($"{account.AccountId} | Balance: {account.Balance:C}");
            }
            Console.ReadKey();
        }

        public void TransferMoney(string name)
        {
            var accounts = Accounts.Where(a => a.OwnerName.Equals(name, StringComparison.OrdinalIgnoreCase)).ToList();
            if (accounts.Count < 2)
            {
                Console.WriteLine("You need at least 2 accounts to transfer between.");
                Console.ReadKey();
                return;
            }

            Console.WriteLine("Select source account:");
            var source = SelectAccount(accounts);
            Console.WriteLine("Select destination account:");
            var destination = SelectAccount(accounts.Where(a => a != source).ToList());

            Console.Write("Enter transfer amount: ");
            if (!decimal.TryParse(Console.ReadLine(), out decimal amount) || amount <= 0)
            {
                Console.WriteLine("Invalid amount.");
                Console.ReadKey();
                return;
            }

            if (source.Balance < amount)
            {
                Console.WriteLine("Insufficient funds.");
                Console.ReadKey();
                return;
            }

            source.Balance -= amount;
            destination.Balance += amount;
            SaveData();

            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine($"Transferred {amount:C} from {source.AccountId} to {destination.AccountId}");
            Console.ResetColor();
            Console.ReadKey();
        }

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
                    else Console.WriteLine($"  {accounts[i].AccountId} | Balance: {accounts[i].Balance:C}");
                }

                key = Console.ReadKey(true).Key;
                if (key == ConsoleKey.Enter)
                    break;
                if (key == ConsoleKey.UpArrow || key == ConsoleKey.W)
                    selectedIndex = (selectedIndex > 0) ? selectedIndex - 1 : accounts.Count - 1;
                else if (key == ConsoleKey.DownArrow || key == ConsoleKey.S)
                    selectedIndex = (selectedIndex < accounts.Count - 1) ? selectedIndex + 1 : 0;
            }

            return accounts[selectedIndex];
        }
    }

    // Used to serialize all data in one JSON file
    public class BankData
    {
        public List<LoanManager> Loans { get; set; } = new();
        public List<BankAccount> Accounts { get; set; } = new();
    }
}
