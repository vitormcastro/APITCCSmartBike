using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using APITCCSmartBike.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace APITCCSmartBike.Models
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class UserController : ControllerBase
    {
        private IUserService _userService;

        public UserController(IUserService userService)
        {
            _userService = userService;
        }

        [HttpGet ("getall")]
        public IActionResult GetAll()
        {
            try
            {               
                var users = _userService.GetAll();
          
                return Ok(users);
            }
            catch(Exception err)
            {
                return BadRequest(err);
            }
          
        }

        [AllowAnonymous]
        [HttpPost("create")]
        public IActionResult CreateUser([FromBody]UserModel model)
        {
            string responde = string.Empty;
            if(_userService.CreateUser(model,ref responde))
            {
                return Ok(new
                {
                    status = responde,
                    message = "Criado com Sucesso!"
                });
            }
            return BadRequest(new
            {
                status = "Failed",
                message = responde
            });
        }
    }
}