using ConteoRecaudo.BLL.Interfaces;
using ConteoRecaudo.Models;
using Microsoft.AspNetCore.Mvc;

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
        ///  Obtiene el listado de recaudos por página
        /// </summary>
        /// <param name="pagina">Página a consultar</param>
        /// <returns></returns>
        /// <response code="200">Operación finalizada exitosamente.</response>
        /// <response code="404">No encontró recaudos</response>
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<ResponseReacudoModel>> Get(int pagina = 1)
        {
            ResponseReacudoModel response = await _recaudoBL.GetRecaudos(pagina);

            if (response?.ConteoRecaudoList?.Count == 0) {
                return NotFound();
            }

            return Ok(response);

        }

        /// <summary>
        ///  Guardar recaudos en base de datos
        /// </summary>
        /// <param name="token">Token</param>
        /// <param name="fechaInicio">Fecha inicio para la consulta formato(YYYY-MM-DD)</param>
        /// <param name="fechaFin">Fecha fin para la consulta formato(YYYY-MM-DD)</param>
        /// <returns></returns>
        /// <response code="200">Operación finalizada exitosamente.</response>
        /// <response code="500">Error al guardar recaudos en base de datos</response>
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<bool>> Post(string token, DateTime fechaInicio, DateTime fechaFin)
        {
            bool respuesta = await _recaudoBL.GuardarRecaudos(token, fechaInicio, fechaFin);
            if (!respuesta)
            {
                return StatusCode(StatusCodes.Status500InternalServerError);
            }
            return Ok();
        }

        /// <summary>
        /// Consulta un listado de recaudos por fecha y retorna el resultado de la consulta como arreglo de bytes en formato de excel
        /// </summary>
        /// <param name="fechaInicial">fecha inicial de la consulta</param>
        /// <param name="fechaFinal">fecha final de la consulta</param>
        /// <returns>Objeto con el arreglo de bytes y nombre del archivo.</returns>
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [HttpGet("GetBytesRecaudoExcel")]
        public async Task<ArchivoRecaudoExcel> ExportarExcel(DateTime fechaInicial, DateTime fechaFinal)
        {
            return await _recaudoBL.ExportarExcel(fechaInicial, fechaFinal);
        }
    }
}