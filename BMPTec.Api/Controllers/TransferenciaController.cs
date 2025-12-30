using BMPTec.Application.DTOs.Requests;
using BMPTec.Application.DTOs.Responses;
using BMPTec.Application.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BMPTec.API.Controllers.v1
{
    [ApiController]
    [Route("api/v{version:apiVersion}/[controller]")]
    [ApiVersion("1.0")]
    [Authorize]
    public class TransferenciaController : ControllerBase
    {
        private readonly ITransferenciaService _transferenciaService;
        private readonly ILogger<ContasController> _logger;

        public TransferenciaController(
            ITransferenciaService transferenciaService,
            ILogger<ContasController> logger)
        {
            _transferenciaService = transferenciaService ?? throw new ArgumentNullException(nameof(transferenciaService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Cria uma nova conta bancária
        /// </summary>
        /// <param name="request">Dados para criação da conta</param>
        /// <returns>Conta criada</returns>
        [HttpPost("TransferirSaldoAsync")]
        [ProducesResponseType(typeof(TransferenciaSaldoResponse), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
        [AllowAnonymous] // Permitir criação sem autenticação
        public async Task<IActionResult> TransferirSaldoAsync([FromBody] TransferenciaSaldoRequest request)
        {
            try
            {
                var contaCriada = await _transferenciaService.TransferirSaldoAsync(request);
                
                return Created(
                    contaCriada.Id.ToString(),
                    contaCriada);
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Erro de validação ao criar conta");
                return BadRequest(new ProblemDetails
                {
                    Title = "Erro de validação",
                    Detail = ex.Message,
                    Status = StatusCodes.Status400BadRequest
                });
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "Operação inválida ao criar conta");
                return BadRequest(new ProblemDetails
                {
                    Title = "Operação inválida",
                    Detail = ex.Message,
                    Status = StatusCodes.Status400BadRequest
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro inesperado ao criar conta");
                return StatusCode(StatusCodes.Status500InternalServerError, new ProblemDetails
                {
                    Title = "Erro interno",
                    Detail = "Ocorreu um erro ao processar sua solicitação",
                    Status = StatusCodes.Status500InternalServerError
                });
            }
        }

        /// <summary>
        /// Cria uma nova conta bancária
        /// </summary>
        /// <param name="request">Dados para criação da conta</param>
        /// <returns>Conta criada</returns>
        [HttpPost("RealizarDeposito")]
        [ProducesResponseType(typeof(DepositoResponse), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
        [AllowAnonymous] // Permitir criação sem autenticação
        public async Task<IActionResult> RealizarDeposito([FromBody] DepositoRequest request)
        {
            try
            {   
                var contaCriada = await _transferenciaService.RealizarDeposito(request);
                
                return Created(
                    contaCriada.Id.ToString(),
                    contaCriada);
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Erro de validação ao criar conta");
                return BadRequest(new ProblemDetails
                {
                    Title = "Erro de validação",
                    Detail = ex.Message,
                    Status = StatusCodes.Status400BadRequest
                });
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "Operação inválida ao criar conta");
                return BadRequest(new ProblemDetails
                {
                    Title = "Operação inválida",
                    Detail = ex.Message,
                    Status = StatusCodes.Status400BadRequest
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro inesperado ao criar conta");
                return StatusCode(StatusCodes.Status500InternalServerError, new ProblemDetails
                {
                    Title = "Erro interno",
                    Detail = "Ocorreu um erro ao processar sua solicitação",
                    Status = StatusCodes.Status500InternalServerError
                });
            }
        }
    }
}