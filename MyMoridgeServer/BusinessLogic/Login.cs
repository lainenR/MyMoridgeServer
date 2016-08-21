using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using MyMoridgeServer.Models;

namespace MyMoridgeServer.BusinessLogic
{
    public class Login
    {
        private MyMoridgeServerModelContainer1 db = new MyMoridgeServerModelContainer1();
        private User User = null;

        public Login()
        {

        }

        public bool DoLogin(string userName, string password)       
        {
            bool isValid = false;
            User = db.Users.FirstOrDefault(u => (u.UserName == userName) && (u.Password == password));

            if(User != null)
            {
                isValid = true;
                User.LastLogin = DateTime.Now;
                db.SaveChanges();
            }

            return isValid;
        }
    }
}