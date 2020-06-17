using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using APITCCSmartBike.Interfaces;
using APITCCSmartBike.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace APITCCSmartBike.Controllers
{

    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class AccountController : Controller
    {

        private IUserService _userService;

        public AccountController(IUserService userService)
        {
            _userService = userService;
        }

        [AllowAnonymous]
        [HttpPost("authenticate")]
        public IActionResult Authenticate([FromBody]AuthenticateModel model)
        {
            string response = string.Empty;
            var user = _userService.Authenticate(model.Username, model.Password, model.Type, ref response);

            if (user == null)
            {
                return BadRequest(new
                {
                    Status = "Failed",
                    Message = response
                });
            }

            return Ok(new
            {
                Status = response,
                user
            });
        }

        [HttpPost("recuperarsenha")]
        public IActionResult RecuperarSenha([FromBody]AuthenticateModel model)
        {
            string response = string.Empty;
            if(_userService.RecuperarSenha(model.Email,model.Username,ref response))
            {
                return Ok(new
                {
                    Status = "Sucess",
                    Message = response
                });
            }
            return BadRequest(new
            {
                Status = "Failed",
                Message = response
            });
        }

    }
}
