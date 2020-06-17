using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;

namespace APITCCSmartBike.Models
{
    public class CoordinateModel
    {
            

        public float[] Coordinates { get; set; }

        public string WriteCoordinate()
        {
            if (Coordinates.Length == 2)
            {
                return (new
                {
                    latitude = Coordinates[0].ToString(),
                    longitude = Coordinates[1].ToString()
                }).ToString();
            }

            return string.Empty;
        }
    }
}
