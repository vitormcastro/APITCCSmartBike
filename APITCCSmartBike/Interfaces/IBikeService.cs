using APITCCSmartBike.Entities;
using APITCCSmartBike.Models;

using MongoDB.Bson;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace APITCCSmartBike.Interfaces
{
    public interface IBikeService
    {
        bool AlterarStatus(string idBike, bool status, ref string message);

        IEnumerable<Data> ObterDados(string idBike, ref string message);

        IEnumerable<Data> ObterDados(string idBike, string type, ref string message);

        Bike ObterBike(string idBike, ref string message);

        IEnumerable<CoordinateModel> ObterParque(ref string message);

        bool CreateCorrida(string idBike, string idUser, ref string message);

        bool CheckUserInRun(string idUser, ref string message);

        bool TravarBikeInCorrida(string idUser, ref string message);

        IEnumerable<Corrida> GetCorridaHistorico(string idUser, int qtdd, ref string message);
    }
}
