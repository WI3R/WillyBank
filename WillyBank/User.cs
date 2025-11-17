using System;
using System.Collections.Generic;

namespace WillyBank
{
    public class User
    {
        public string Username { get; set; }
        public string Password { get; set; }
        public DateTime LastInterestUpdate { get; set; } = DateTime.Now;
        public List<Guid> AccountIds { get; set; } = new();

        public User(string username, string password)
        {
            Username = username;
            Password = password;
            LastInterestUpdate = DateTime.Now;
        }
    }
}
