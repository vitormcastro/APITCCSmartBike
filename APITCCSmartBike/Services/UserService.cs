using APITCCSmartBike.Entities;
using APITCCSmartBike.Helpers;
using APITCCSmartBike.Interfaces;
using APITCCSmartBike.Models;

using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

using RestSharp;

using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace APITCCSmartBike.Services
{
    public class UserService : IUserService
    {

        private readonly AppSettings _appSettings;

        private readonly EmailSettings _emailSettings;

        public UserService(IOptions<AppSettings> appSettings, IOptions<EmailSettings> emailSettings)
        {
            _appSettings = appSettings.Value;
            _emailSettings = emailSettings.Value;
        }

        IRestResponse GetUser(string username)
        {
            var client = new RestClient(_appSettings.Url + "/v2/entities/?q=Username==" + username + "&type=User&options=keyValues");
            client.Timeout = -1;
            var request = new RestRequest(Method.GET);
            request.AddParameter("text/plain", "", ParameterType.RequestBody);
            IRestResponse response = client.Execute(request);
            return response;
        }

        async Task Execute(string email, string subject, string message)
        {
            try
            {
                string toEmail = email;

                MailMessage mail = new MailMessage()
                {
                    From = new MailAddress(_emailSettings.UsernameEmail, "Vitor Castro")
                };

                mail.To.Add(new MailAddress(toEmail));
                mail.CC.Add(new MailAddress(_emailSettings.CcEmail));

                mail.Subject = subject;
                mail.Body = message;
                mail.IsBodyHtml = true;
                mail.Priority = MailPriority.High;

                //outras opções
                //mail.Attachments.Add(new Attachment(arquivo));
                //

                using (SmtpClient smtp = new SmtpClient(_emailSettings.PrimaryDomain, _emailSettings.PrimaryPort))
                {
                    smtp.Credentials = new NetworkCredential(_emailSettings.UsernameEmail, _emailSettings.UsernamePassword);
                    smtp.EnableSsl = true;
                    await smtp.SendMailAsync(mail);
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        string GetIDUser()
        {
            var client = new RestClient("http://13.84.179.36:1026/v2/entities/?type=User&options=keyValues&limit=1000&attrs=id");
            client.Timeout = -1;
            var request = new RestRequest(Method.GET);
            request.AddParameter("text/plain", "", ParameterType.RequestBody);
            IRestResponse response = client.Execute(request);
            if (response == null || response.StatusCode != HttpStatusCode.OK)
            {
                return null;
            }
            var vetor = JsonConvert.DeserializeObject<User[]>(response.Content);
            if (vetor.Length > 98)
            {
                return "urn:ngsi-ld:User:" + (vetor.Length + 1);
            }
            else if (vetor.Length > 8)
            {
                return "urn:ngsi-ld:User:0" + (vetor.Length + 1);
            }
            else
            {
                return "urn:ngsi-ld:User:00" + (vetor.Length + 1);
            }
        }

        public User Authenticate(string username, string password, string type, ref string message)
        {

            try
            {
                IRestResponse response = GetUser(username);

                if (response == null || response.StatusCode != HttpStatusCode.OK)
                {
                    message = "Falha ao conectar ao sistema!";
                    return null;
                }
                User[] usuario = JsonConvert.DeserializeObject<User[]>(response.Content);
                if (usuario.Length == 0)
                {
                    message = "Usuario ou senha incorreto!";
                    return null;
                }
                if (usuario[0].Password != password)
                {
                    message = "Usuario ou senha incorreto!";
                    return null;
                }
                string refPermission = usuario[0].RefPermission;
                switch (type.ToLower())
                {
                    case "user":
                        {
                            if (refPermission != "urn:ngsi-ld:Permission:003")
                            {
                                message = "Usuario ou senha incorreto!";
                                return null;
                            }
                        }
                        break;

                    case "admin":
                        {
                            if (refPermission != "urn:ngsi-ld:Permission:001" && refPermission != "urn:ngsi-ld:Permission:002")
                            {
                                message = "Usuario ou senha incorreto!";
                                return null;
                            }
                        }
                        break;

                    default:
                        {
                            message = "Usuario ou senha incorreto!";
                            return null;
                        }
                }
                User user = usuario[0];

                // authentication successful so generate jwt token
                var tokenHandler = new JwtSecurityTokenHandler();
                var key = Encoding.ASCII.GetBytes(_appSettings.Secret);
                var tokenDescriptor = new SecurityTokenDescriptor
                {
                    Subject = new ClaimsIdentity(new Claim[]
                    {
                    new Claim(ClaimTypes.Name, user.Username.ToString())
                    }),
                    Expires = DateTime.UtcNow.AddHours(2),
                    SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
                };
                var token = tokenHandler.CreateToken(tokenDescriptor);
                user.Token = tokenHandler.WriteToken(token);
                user.RefPermission = string.Empty;
                message = "Sucess";
                return user.WithoutPassword();
            }
            catch (Exception ex)
            {
                message = ex.Message;
                return null;
            }
        }

        public IEnumerable<User> GetAll()
        {
            var client = new RestClient(_appSettings.Url + "/v2/entities/?options=keyValues&type=User&limit=1000");
            client.Timeout = -1;
            var request = new RestRequest(Method.GET);
            request.AddParameter("text/plain", "", ParameterType.RequestBody);
            IRestResponse response = client.Execute(request);
            //Console.WriteLine(response.Content);
            if (response == null || response.StatusCode != HttpStatusCode.OK)
                return null;
            User[] users = JsonConvert.DeserializeObject<User[]>(response.Content);
            /*var arrayResponse = JArray.Parse(response.Content);
            List<User> lista = new List<User>(); 
            foreach(JObject item in arrayResponse)
            {
                lista.Add(GetUser(item));
            }*/

            List<User> lista = new List<User>(users);
            return lista.WithoutPasswords();
        }

        public bool RecuperarSenha(string email, string username, ref string message)
        {
            try
            {
                var response = GetUser(username);
                if (response == null || response.StatusCode != HttpStatusCode.OK)
                {
                    message = response.ErrorMessage;
                    return false;
                }
                User[] usuario = JsonConvert.DeserializeObject<User[]>(response.Content);
                if (usuario.Length == 0)
                {
                    message = "Usuario ou e-mail incorreto!";
                    return true;
                }
                if (usuario[0].Email != email)
                {
                    message = "Usuario ou e-mail incorreto!";
                    return true;
                }
                string corpo = string.Format(" <table style=\"width: 750px; background - color: #ffffff;\" align=\"center\"><tr><td><p>Olá {0}!</p><br/><p>Sua senha é |  " +
                                             "<strong>{1}</strong>   |</p><p>Atenciosamente<p><p>Time Smart Bike</p></td></tr></table >",
                                             usuario[0].FirstName, usuario[0].Password);

                Execute(email, "Recuperar a Senha", corpo).Wait();
                message = "Enviamos sua senha para seu e-mail!";
                return true;
            }
            catch (Exception ex)
            {
                message = ex.Message;
                return false;
            }
        }

        public bool CreateUser(UserModel user, ref string message)
        {

            string id = GetIDUser();
            if (string.IsNullOrEmpty(id))
            {
                message = "Falha ao conectar ao Banco de Dados";
                return false;
            }


            var client = new RestClient(_appSettings.Url+"/v2/op/update");
            client.Timeout = -1;
            var request = new RestRequest(Method.POST);
            request.AddHeader("Content-Type", "application/json");
            request.AddParameter("application/json", "{\r\n  \"actionType\":\"APPEND\",\r\n  \"entities\":[\r\n    {\r\n      \"id\":\"" + id + "\", \"type\":\"User\"," +
                "\r\n      \"FirstName\":{\r\n        \"type\":\"Text\", \"value\":\"" + user.FirstName + "\"\r\n      },\r\n      \"LastName\":{\r\n        " +
                "\"type\":\"Text\", \"value\": \"" + user.LastName + "\"\r\n      },\r\n      \"Genero\":{\r\n        \"type\":\"Text\", \"value\": \"" + user.Genero + "\"\r\n" +
                "      },\r\n      \"Nascimento\":{\r\n        \"type\":\"Text\", \"value\": \"" + user.Nascimento + "\"\r\n      },\r\n      \"Email\":{\r\n        " +
                "\"type\":\"Text\", \"value\": \"" + user.Email + "\"\r\n      },\r\n      \"ImgBase64\":{\r\n        \"type\":\"Text\", \"value\": \"" + user.ImgBase64 + "\"" +
                "\r\n      },\r\n      \"TotalRide\":{\r\n        \"type\":\"float\", \"value\": \"0\"\r\n      },\r\n      \"Username\":{\r\n        " +
                "\"type\":\"Text\", \"value\": \"" + user.Username + "\"\r\n      },\r\n      \"Password\":{\r\n        \"type\":\"Text\", \"value\": \"" + user.Password +
                "\"\r\n},\r\n       \"refPermission\": { \r\n        \"type\": \"Relationship\",\r\n        " +
                                                      "\"value\": \"urn:ngsi-ld:Permission:003\"\r\n      }        \r\n    }\r\n  ]\r\n}", ParameterType.RequestBody);
            IRestResponse response = client.Execute(request);
            if (response == null || response.StatusCode != HttpStatusCode.NoContent)
            {
                message = "Falha ao Salvar o Usuário!";
                return false;
            }

            client = new RestClient(_appSettings.Url+"/v2/op/update");
            client.Timeout = -1;
            request = new RestRequest(Method.POST);
            request.AddHeader("Content-Type", "application/json");
            request.AddParameter("application/json", "{\r\n  \"actionType\":\"APPEND\",\r\n  \"entities\":[\r\n     \r\n    {\r\n      \"id\":\""+id+"\", " +
                                                      "\"type\":\"User\"\r\n    }\r\n  ]\r\n}", ParameterType.RequestBody);
            response = client.Execute(request);
            if (response == null || response.StatusCode != HttpStatusCode.NoContent)
            {
                message = "Falha ao Criar a Referencia do usuario!";
                return false;
            }
            message = "Sucess";
            return true;
        }
    }
}
