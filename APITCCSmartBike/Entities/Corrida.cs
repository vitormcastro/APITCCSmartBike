using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace APITCCSmartBike.Entities
{
    public class Corrida
    {
        public string Id { get; set; }

        public string RefUser { get; set; }

        public string RefBike { get; set; }

        public string GetBike { get; set; }

        public string LeaveBike { get; set; }

        //Não foi implementado
        public double? Distancia { get; set; }

        public double? Duracao { get; set; }
    }
}
