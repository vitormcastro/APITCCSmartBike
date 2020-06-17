using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace APITCCSmartBike.Models
{
    public class StatusBikeModel
    {
        public string IdBike { get; set; }
        
        [Required]
        public string IdUser { get; set; }

        [Required]
        public bool Status { get; set; }
    }
}
