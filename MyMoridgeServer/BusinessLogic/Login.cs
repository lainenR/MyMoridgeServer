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
        private User user = null;

        public Login()
        {

        }

        public bool DoLogin(string userName, string password)       
        {
            bool isValid = false;
            user = db.Users.First(u => (u.UserName == userName) && (u.Password == password));

            if(user != null)
            {
                isValid = true;
                user.LastLogin = DateTime.Now;
                db.SaveChanges();
            }

            return isValid;
        }
    }
}