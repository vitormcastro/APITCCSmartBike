using APITCCSmartBike.Entities;
using APITCCSmartBike.Helpers;
using APITCCSmartBike.Interfaces;

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

        public async Task Execute(string email, string subject, string message)
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
            var client = new RestClient(_appSettings.Url + "/v2/entities/?options=keyValues&type=User");
            client.Timeout = -1;
            var request = new RestRequest(Method.GET);
            request.AddParameter("text/plain", "", ParameterType.RequestBody);
            IRestResponse response = client.Execute(request);
            //Console.WriteLine(response.Content);
            if (response == null)
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
    }
}
