using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace APITCCSmartBike.Models
{
    public class DadosPorPeriodoModel
    {
        [Required]
        public string IdBike { get; set; }

        [Required]

        public string DeDataHora { get; set; }


        public string AteDataHora { get; set; }

        public string Type { get; set; }
    }
}
