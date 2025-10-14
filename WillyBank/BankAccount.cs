using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WillyBank
{
    public class BankAccount
    {
        public Guid AccountId { get; set; }
        public string OwnerName { get; set; }
        public decimal Balance { get; set; }

        public BankAccount(Guid accountId, string ownerName, decimal balance)
        {
            AccountId = accountId;
            OwnerName = ownerName;
            Balance = balance;
        }
    }
}
