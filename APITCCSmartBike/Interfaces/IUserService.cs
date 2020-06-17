using APITCCSmartBike.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace APITCCSmartBike.Interfaces
{
    public interface IUserService
    {
        User Authenticate(string username, string password, string type, ref string message);
        IEnumerable<User> GetAll();

        bool RecuperarSenha(string email, string username,ref string message);
    }
}
