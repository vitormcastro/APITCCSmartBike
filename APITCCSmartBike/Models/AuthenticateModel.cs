using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace APITCCSmartBike.Models
{
    public class AuthenticateModel
    {
        [Required]
        public string Username { get; set; }

        
        public string Password { get; set; }

        public string Type { get; set; }

        public string Email { get; set; }
    }
}
