using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace APITCCSmartBike.Models
{
    public class UserModel
    {
        [Required]
        public string FirstName { get; set; }

        [Required]
        public string LastName { get; set; }

        [Required]
        public string Genero { get; set; }

        [Required]
        public string Nascimento { get; set; }

        [Required]
        public string Email { get; set; }

        [Required]
        public string ImgBase64 { get; set; }

        [Required]
        public string Username { get; set; }

        [Required]
        public string Password { get; set; }
    
    }
}
