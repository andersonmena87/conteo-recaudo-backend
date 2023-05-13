using ConteoRecaudo.BLL.Interfaces;
using ConteoRecaudo.Models;
using Microsoft.AspNetCore.Mvc;

namespace conteo_recaudo_backend.Controllers
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
    }
}