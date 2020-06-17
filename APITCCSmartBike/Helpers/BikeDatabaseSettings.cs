using APITCCSmartBike.Interfaces;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace APITCCSmartBike.Helpers
{
    public class BikeDatabaseSettings : IBikeDatabaseSettings
    {
        public string ConnectionString { get; set; }
        public string DatabaseName { get; set; }
    }
}
