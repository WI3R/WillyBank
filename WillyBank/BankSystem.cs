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

        /// <summary>
        /// Load from file
        /// </summary>
        public void LoadData()
        {
            if (File.Exists(filePath))
                {
                string json = File.ReadAllText(filePath);
                    Loans = JsonSerializer.Deserialize<List<LoanManager>>(json) ?? new();
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
            string json = JsonSerializer.Serialize(Loans, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(filePath, json);
        }

        public void AddLoan(string name, decimal amount)
        {
            var record = new LoanManager(name, amount);
            Loans.Add(record);
            SaveData();
        }

        public void PayLoan(string name, decimal payment)
        {
            var loan = Loans.Find(l => l.Name.Equals(name, StringComparison.OrdinalIgnoreCase));

            if (loan == null)
            {
                Console.WriteLine("No loan found for that person");
                return;
            }

            loan.LoanAmount -= payment;
            if (loan.LoanAmount <= 0)
            {
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine($"{name} has fully repaid their loan");
                Console.ResetColor();
                Console.ReadKey();
                Loans.Remove(loan);
                loan = null;
            }

            SaveData();
        }

        public void ViewLoan(string name)
        {
            var thing = Loans.Find(l => l.Name.Equals(name));
            Console.WriteLine(name + " has an active loan for " + thing.LoanAmount);
        }

        public void ViewAllLoans()
        {
            if (Loans.Count == 0)
            {
                Console.WriteLine("No active loans.");
                return;
            }

            foreach (var loan in Loans)
            {
                Console.WriteLine(loan.Name + " | " + loan.LoanAmount);
            }

            Console.ReadKey();
        }
    }
}
