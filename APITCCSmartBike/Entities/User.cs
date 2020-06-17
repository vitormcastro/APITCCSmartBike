using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace APITCCSmartBike.Entities
{
    public class User
    {
        public string Id { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Genero { get; set; }
        public string Nascimento { get; set; }
        public string Email { get; set; }
        public string ImgBase64 { get; set; }

        //Não foi implementado no sistema
        public float TotalRide { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }

        //Web Token para autenticação
        public string Token { get; set; }

        public string RefPermission { get; set; }
    }
}
