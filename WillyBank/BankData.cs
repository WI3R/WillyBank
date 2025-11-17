using System.Collections.Generic;

namespace WillyBank
{
    public class BankData
    {
        public List<User> Users { get; set; } = new();
        public List<LoanManager> Loans { get; set; } = new();
        public List<BankAccount> Accounts { get; set; } = new();
    }
}
