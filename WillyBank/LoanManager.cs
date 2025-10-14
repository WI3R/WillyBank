using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WillyBank
{
    public class LoanManager
    {
        public string Name { get; set; }
        public decimal LoanAmount { get; set; }
        public DateTime LoanDate { get; set; }
        public Guid AccountId { get; set; }

        public LoanManager(string name, decimal loanAmount, Guid accountId)
        {
            Name = name;
            LoanAmount = loanAmount;
            LoanDate = DateTime.Now;
            AccountId = accountId;
        }
    }
}
