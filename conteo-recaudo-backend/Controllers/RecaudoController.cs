using ConteoRecaudo.BLL;
using ConteoRecaudo.BLL.Interfaces;
using ConteoRecaudo.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Primitives;

namespace ConteoRecaudo.Controllers
{
    [ApiController]
    [Route("[controller]")]
    [Produces("application/json")]
    public class RecaudoController : ControllerBase
    {
        
        private readonly IRecaudoBL _recaudoBL;

        public RecaudoController(IRecaudoBL recaudoBL)
        {
            _recaudoBL = recaudoBL;
        }

        /// <summary>
        ///  Obtiene el listado de recaudos
        /// </summary>
        /// <returns></returns>
        /// <response code="200">Operación finalizada exitosamente.</response>
        /// <response code="404">No encontró recaudos</response>
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<List<RecaudoModel>>> Get()
        {
            List<RecaudoModel> recaudos = await _recaudoBL.GetRecaudos();

            if (recaudos.Count == 0) {
                return NotFound();
            }

            return Ok(recaudos);

        }

        /// <summary>
        ///  Guardar recaudos
        /// </summary>
        /// <param name="token">fecha inicial para la consulta</param>
        /// <param name="fechaInicio">fecha  final para la consulta</param>
        /// <param name="fechaFin">Maquina que registró el error</param>
        /// <returns></returns>
        /// <response code="200">Operación finalizada exitosamente.</response>
        /// <response code="404">No encontró recaudos</response>
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<bool>> Post(string token, DateTime fechaIncio, DateTime fechaFin )
        {
            bool respuesta = await _recaudoBL.GuardarRecaudos(token, fechaIncio, fechaFin);
            return Ok();

        }
    }
}