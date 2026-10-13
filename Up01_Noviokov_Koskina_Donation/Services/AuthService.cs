using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Up01_Noviokov_Koskina_Donation.Services
{
    public static class AuthService
    {
        public static User Authenticate(string login, string password)
        {
            string hash = Security.GetHash(password);
            return Entities.GetContext().User.FirstOrDefault(u => u.Login == login && u.Password == hash);
        }
    }
}
