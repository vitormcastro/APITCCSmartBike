using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using APITCCSmartBike.Entities;
using APITCCSmartBike.Interfaces;
using APITCCSmartBike.Models;
using APITCCSmartBike.Services;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace APITCCSmartBike.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class BikeController : ControllerBase
    {

        private IBikeService _bikeService;

        public BikeController(IBikeService bikeService)
        {
            _bikeService = bikeService;
        }

        [HttpPost("alterarstatus")]
        public IActionResult AlterarStatus([FromBody] StatusBikeModel model)
        {
            var response = string.Empty;
            if (model.Status)
            {
                if (_bikeService.TravarBikeInCorrida(model.IdUser, ref response))
                {
                    response = "Travada com sucesso";
                    return Ok(new
                    {
                        status = "Sucess",
                        message = response
                    });
                }
            }
            else
            {
                if (string.IsNullOrWhiteSpace(model.IdBike))
                {
                    return NotFound(new
                    {
                        status = "Failed",
                        message = "idBike invalido!"
                    });
                }
                if (_bikeService.AlterarStatus(model.IdBike, model.Status, ref response))
                {
                    if (_bikeService.CreateCorrida(model.IdBike, model.IdUser, ref response))
                    {
                        response = "Destravada com sucesso";
                        return Ok(new
                        {
                            status = "Sucess",
                            message = response
                        });
                    }
                    _bikeService.AlterarStatus(model.IdBike, true, ref response);
                }
            }


            return BadRequest(new
            {
                status = "Failed",
                message = response
            });


        }


        [HttpGet("obterdados/{id}")]
        public IActionResult ObterDados(string id)
        {
            var response = string.Empty;
            var dados = _bikeService.ObterDados(id, ref response);

            if (dados == null)
            {
                return BadRequest(new
                {
                    status = "Failed",
                    message = response
                });
            }

            return Ok(new
            {
                status = response,
                dados
            });
        }

        [HttpGet("obterdados/{id}/{type}")]
        public IActionResult ObterDados(string id, string type)
        {
            var response = string.Empty;
            var dados = _bikeService.ObterDados(id, type, ref response);

            if (dados == null)
            {
                return BadRequest(new
                {
                    status = "Failed",
                    message = response
                });
            }

            return Ok(new
            {
                status = response,
                dados
            });
        }

        [HttpGet("obterbike/{id}")]
        public IActionResult ObterBike(string id)
        {
            var response = string.Empty;
            var bike = _bikeService.ObterBike(id, ref response);

            if (bike == null)
            {
                return BadRequest(new
                {
                    status = "Failed",
                    message = response
                });
            }

            return Ok(new
            {
                status = response,
                bike
            });
        }

        [HttpGet("obterparque")]
        public IActionResult ObterParque()
        {
            string response = string.Empty;
            var dados = _bikeService.ObterParque(ref response);
            if (response == "Sucess")
            {
                return Ok(new
                {
                    status = response,
                    dados
                });
            }
            return BadRequest(new
            {
                status = "Failed",
                message = response
            });

        }

        [HttpGet("checkuserinrun/{id}")]
        public IActionResult CheckUserInRun(string id)
        {
            string response = string.Empty;
            bool resp = _bikeService.CheckUserInRun(id, ref response);
            if (response == "Sucess")
            {
                return Ok(new
                {
                    status = response,
                    message = resp.ToString()
                });
            }
            return BadRequest(new
            {
                status = "Failed",
                message = response
            });
        }

        [HttpGet("corridahistorico/{id}")]
        public IActionResult CorridaHistorico(string id)
        {
            string response = string.Empty;

            var corridas = _bikeService.GetCorridaHistorico(id, 5, ref response);
            if (corridas != null)
            {
                return Ok(new
                {
                    status = response,
                    corridas
                });
            }

            return BadRequest(new
            {
                status = "Failed",
                message = response
            });
        }

        [HttpGet("corridahistorico/{id}/{qtdd}")]
        public IActionResult CorridaHistorico(string id, int? qtdd)
        {
            string response = string.Empty;
            int count = qtdd == null ? 5 : qtdd.Value;
            var corridas = _bikeService.GetCorridaHistorico(id, count, ref response);
            if (response == "Sucess")
            {
                return Ok(new
                {
                    status = response,
                    corridas
                });
            }

            return BadRequest(new
            {
                status = "Failed",
                message = response
            });
        }

        [HttpGet("all")]
        public IActionResult GetAll()
        {
            string response = string.Empty;
            var bikes = _bikeService.GetAll(ref response);
            if (bikes == null)
            {
                return BadRequest(new
                {
                    status = "Failed",
                    message = response
                });
            }
            return Ok(new
            {
                status = response,
                bikes
            });
        }

        [HttpGet("corrida/all")]
        public IActionResult GetAllCorrida()
        {
            string response = string.Empty;
            var corridas = _bikeService.GetAllCorrida(ref response);
            if (corridas == null)
            {
                return BadRequest(new
                {
                    status = "Failed",
                    message = response
                });
            }
            return Ok(new
            {
                status = response,
                corridas
            });
        }

        [HttpPost("obterdadosperiodo")]
        public IActionResult ObterDadosEmPeriodo([FromBody] DadosPorPeriodoModel model)
        {
            try
            {


                DateTime de = Convert.ToDateTime(model.DeDataHora);
                DateTime ate = DateTime.Now;
                if (!string.IsNullOrWhiteSpace(model.AteDataHora))
                {
                    ate = Convert.ToDateTime(model.AteDataHora);
                }
                if (ate <= de)
                {
                    return BadRequest(new
                    {
                        status = "Failed",
                        message = "Período invalido!"
                    });
                }
                string response = string.Empty;
                IEnumerable<Data> dados = null;
                if (string.IsNullOrWhiteSpace(model.Type))
                {
                    dados = _bikeService.ObterDados(model.IdBike, de, ate, ref response);
                }
                else
                {
                    dados = _bikeService.ObterDados(model.IdBike, de, ate, model.Type, ref response);
                }
                if (dados == null)
                {
                    return BadRequest(new
                    {
                        status = "Failed",
                        message = response
                    });
                }

                return Ok(new
                {
                    status = response,
                    dados
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new
                {
                    status = "Failed",
                    message = ex.Message
                });
            }

        }


    }
}