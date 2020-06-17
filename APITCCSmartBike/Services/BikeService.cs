using APITCCSmartBike.Entities;
using APITCCSmartBike.Helpers;
using APITCCSmartBike.Interfaces;
using APITCCSmartBike.Models;

using Microsoft.EntityFrameworkCore.Query.Internal;
using Microsoft.Extensions.Options;

using MongoDB.Bson;
using MongoDB.Driver;
using MongoDB.Driver.Linq;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

using RestSharp;

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;

namespace APITCCSmartBike.Services
{
    public class BikeService : IBikeService
    {

        private readonly AppSettings _appSettings;

        private readonly BikeDatabaseSettings _bikeDatabaseSettings;

        public BikeService(IOptions<AppSettings> appSettings, IBikeDatabaseSettings settings)
        {
            _appSettings = appSettings.Value;

            _bikeDatabaseSettings = (BikeDatabaseSettings)settings;
        }
       
        string GetCorridaId()
        {
            var client = new RestClient(_appSettings.Url + "/v2/entities/?type=Corrida&options=keyValues&limit=1000");
            client.Timeout = -1;
            var request = new RestRequest(Method.GET);
            request.AddParameter("text/plain", "", ParameterType.RequestBody);
            IRestResponse response = client.Execute(request);
            //Console.WriteLine(response.Content);
            if (response == null)
                return string.Empty;
            Corrida[] vetor = JsonConvert.DeserializeObject<Corrida[]>(response.Content);
            if (vetor.Length > 98)
            {
                return "urn:ngsi-ld:Corrida:" + (vetor.Length + 1);
            }
            else if (vetor.Length > 8)
            {
                return "urn:ngsi-ld:Corrida:0" + (vetor.Length + 1);
            }
            else
            {
                return "urn:ngsi-ld:Corrida:00" + (vetor.Length + 1);
            }

        }

        IMongoCollection<Data> ConnectToMongoDB(string idBike)
        {
            var client = new MongoClient(_bikeDatabaseSettings.ConnectionString);
            var database = client.GetDatabase(_bikeDatabaseSettings.DatabaseName);

            return database.GetCollection<Data>("sth_/_" + idBike + "_iot");
        }

        List<Corrida> GetListRunFromUser(string idUser)
        {
            var client = new RestClient(_appSettings.Url + "/v2/entities/?q=refUser==" + idUser + "&type=Corrida&options=keyValues&limit=1000");
            client.Timeout = -1;
            var request = new RestRequest(Method.GET);
            request.AddParameter("text/plain", "", ParameterType.RequestBody);
            IRestResponse response = client.Execute(request);
            if (response == null)
            {
                return null;
            }
            Corrida[] runs = JsonConvert.DeserializeObject<Corrida[]>(response.Content);
            return new List<Corrida>(runs);
        }

        IRestResponse CreateOrUpdateRun(Corrida run)
        {
            var client = new RestClient(_appSettings.Url + "/v2/op/update");
            client.Timeout = -1;
            var request = new RestRequest(Method.POST);
            request.AddHeader("Content-Type", "application/json");
            request.AddParameter("application/json", "{\r\n  \"actionType\":\"APPEND\",\r\n  \"entities\":[\r\n    {\r\n      \"id\":\"" + run.Id + "\"," +
                                 " \"type\":\"Corrida\",\r\n      \"refUser\": { \r\n        \"type\": \"Relationship\",\r\n        \"value\": \"" + run.RefUser +
                                 "\"\r\n      },\r\n      \"refBike\": { \r\n        \"type\": \"Relationship\",\r\n        \"value\": \"" + run.RefBike +
                                 "\"\r\n      },\r\n      \"getBike\":{\r\n        \"type\":\"Text\", \"value\":\"" + run.GetBike + "\"\r\n      },\r\n" +
                                 "      \"leaveBike\":{\r\n        \"type\":\"Text\", \"value\":" + (string.IsNullOrWhiteSpace(run.LeaveBike) ? "null" : ("\"" + run.LeaveBike + "\"")) +
                                 "\r\n        },\r\n      \"distancia\":{\r\n        \"type\":\"float\", \"value\":" +
                                 (run.Distancia.HasValue ? run.Distancia.Value.ToString() : "null") + "\r\n      },\r\n      \"duracao\":{\r\n" +
                                 "        \"type\":\"float\", \"value\":" + (run.Duracao.HasValue ? run.Duracao.Value.ToString("F1").Replace(',', '.') : "null") + "\r\n      }\r\n    }\r\n  ]\r\n}",
                                 ParameterType.RequestBody);
            IRestResponse response = client.Execute(request);
            return response;
        }
        public bool AlterarStatus(string idBike, bool status, ref string message)
        {
            var client = new RestClient(_appSettings.Url + "/v2/entities/" + idBike + "/attrs/locked/value");
            client.Timeout = -1;
            var request = new RestRequest(Method.PUT);
            request.AddHeader("Content-Type", "text/plain");
            request.AddHeader("fiware-service", "helixiot");
            request.AddHeader("fiware-servicepath", "/");
            request.AddHeader("Accept", "text/plain");
            request.AddParameter("text/plain", status.ToString(), ParameterType.RequestBody);
            IRestResponse response = client.Execute(request);
            if (response == null)
            {
                message = "falha de Conexão";
                return false;
            }
            message = "Falha da Requisição";
            return response.StatusCode == System.Net.HttpStatusCode.NoContent;

        }

        public Bike ObterBike(string idBike, ref string message)
        {
            var client = new RestClient(_appSettings.Url + "/v2/entities/" + idBike + "/?options=keyValues");
            client.Timeout = -1;
            var request = new RestRequest(Method.GET);
            request.AddHeader("fiware-service", "helixiot");
            request.AddHeader("fiware-servicepath", "/");
            request.AddHeader("Accept", "application/json");
            IRestResponse response = client.Execute(request);
            if (response == null || response.StatusCode != System.Net.HttpStatusCode.OK)
            {
                message = "falha de Conexão no Helix";
                return null;
            }

            var sensores = JsonConvert.DeserializeObject<Sensor>(response.Content);

            client = new RestClient(_appSettings.Url + "/v2/entities/" + idBike + "/?options=keyValues");
            client.Timeout = -1;
            request = new RestRequest(Method.GET);
            request.AddParameter("text/plain", "", ParameterType.RequestBody);
            response = client.Execute(request);
            if (response == null || response.StatusCode != System.Net.HttpStatusCode.OK)
            {
                message = "falha de Conexão no Orion";
                return null;
            }

            var bike = JsonConvert.DeserializeObject<Bike>(response.Content);
            bike.BatteryLevel = sensores.BatteryLevel;
            bike.Coordinates = sensores.Location;
            bike.Locked = sensores.Locked;
            bike.Speed = sensores.Speed;
            message = "Sucess";
            return bike;
        }

        public IEnumerable<Data> ObterDados(string idBike, ref string message)
        {
           
            try
            {
                var data = ConnectToMongoDB(idBike);
                if (data != null)
                {
                    message = "Sucess";
                    return data.Find(data => true).ToList();
                   
                }
            }
            catch (Exception ex)
            {
                message = ex.Message;
                return null;
            }
          

            message = "Não foi possível pegar as informação no banco";
            return null;
        }

        public IEnumerable<Data> ObterDados(string idBike,string type, ref string message)
        {

            try
            {
                var data = ConnectToMongoDB(idBike);
                if (data != null)
                {
                    message = "Sucess";
                    return data.Find(data => data.AttrName == type).ToList();

                }
            }
            catch (Exception ex)
            {
                message = ex.Message;
                return null;
            }


            message = "Não foi possível pegar as informação no banco";
            return null;
        }

        public IEnumerable<CoordinateModel> ObterParque(ref string message)
        {
            var client = new RestClient(_appSettings.Url + "/v2/entities/?options=keyValues&attrs=location,locked");
            client.Timeout = -1;
            var request = new RestRequest(Method.GET);
            request.AddHeader("fiware-service", "helixiot");
            request.AddHeader("fiware-servicepath", "/");
            request.AddHeader("Accept", "application/json");
            IRestResponse response = client.Execute(request);
            if (response == null)
            {
                message = "Falha de Conexão";
                return null;
            }
            if (response.StatusCode != System.Net.HttpStatusCode.OK)
            {
                message = "Não foi possível buscar os dados!";
                return null;
            }

            var dict = JsonConvert.DeserializeObject<Sensor[]>(response.Content);
            List<CoordinateModel> coordinates = new List<CoordinateModel>();
            foreach (var o in dict)
            {
                if (o.Locked == true)
                {
                    coordinates.Add(o.Location);
                }

                // texts.Add();
            }
            message = "Sucess";

            return coordinates;
        }





        public bool CreateCorrida(string idBike, string idUser, ref string message)
        {
            string idCorrida = GetCorridaId();
            if (string.IsNullOrWhiteSpace(idCorrida))
            {
                message = "Falha de Conexão na criação do Id da entidade Corrida";
                return false;
            }
            Corrida run = new Corrida()
            {
                Distancia = null,
                Duracao = null,
                GetBike = DateTime.Now.ToString(),
                Id = idCorrida,
                LeaveBike = null,
                RefBike = idBike,
                RefUser = idUser
            };
            var response = CreateOrUpdateRun(run);
            if (response == null)
            {
                message = "Falha de Conexão na criação da entidade Corrida";
                return false;
            }
            message = "Falha da Requisição";
            return response.StatusCode == System.Net.HttpStatusCode.NoContent;

        }

        public bool CheckUserInRun(string idUser, ref string message)
        {


            List<Corrida> runs = GetListRunFromUser(idUser);
            if (runs == null)
            {
                message = "falha de Conexão";
                return false;
            }
            if (runs.Count == 0)
            {
                message = "Sucess";
                return false;
            }
            for (int i = runs.Count - 1; i >= 0; i--)
            {
                if (runs[i].LeaveBike == null)
                {
                    message = "Sucess";
                    return true;
                }
            }
            message = "Sucess";
            return false;
        }




        public bool TravarBikeInCorrida(string idUser, ref string message)
        {
            List<Corrida> runs = GetListRunFromUser(idUser);
            if (runs == null)
            {
                message = "falha de Conexão";
                return false;
            }
            if (runs.Count == 0)
            {
                message = "Usuario não possui corrida regristrada";
                return false;
            }
            for (int i = runs.Count - 1; i >= 0; i--)
            {
                if (runs[i].LeaveBike == null)
                {
                    if (AlterarStatus(runs[i].RefBike, true, ref message))
                    {
                        DateTime finish = DateTime.Now;
                        DateTime start = Convert.ToDateTime(runs[i].GetBike);
                        TimeSpan duracao = finish - start;
                        runs[i].Duracao = duracao.TotalMinutes;
                        runs[i].LeaveBike = finish.ToString();
                        var response = CreateOrUpdateRun(runs[i]);
                        if (response == null)
                        {
                            message = "Falha de Conexão na autalização da entidade Corrida";
                            return false;
                        }
                        message = "Falha da Requisição";
                        if (response.StatusCode != System.Net.HttpStatusCode.NoContent)
                        {
                            AlterarStatus(runs[i].RefBike, false, ref message);
                            return false;
                        }
                        return true;
                    }
                    return false;
                }
            }
            message = "Falha de encontrar a corrida do usuario";
            return false;
        }

        public IEnumerable<Corrida> GetCorridaHistorico(string idUser, int qtdd, ref string message)
        {
            List<Corrida> runs = GetListRunFromUser(idUser);
            if (runs == null)
            {
                message = "falha de Conexão";
                return null;
            }
            if (runs.Count == 0)
            {
                message = "Usuario não possui corrida regristrada";
                return runs;
            }
            message = "Sucess";
            List<Corrida> corridas = new List<Corrida>();
            int count = 0;
            for (int i = (runs.Count - 1); i >= 0; i--)
            {
                if (count == qtdd)
                {
                    break;
                }
                if (!string.IsNullOrWhiteSpace(runs[i].LeaveBike))
                {
                    corridas.Add(runs[i]);
                    count++;
                }
            }
            if (corridas.Count == 0)
            {
                message = "Usuario não possui corrida regristrada";
                return runs;
            }
            return corridas;
        }
    }
}
