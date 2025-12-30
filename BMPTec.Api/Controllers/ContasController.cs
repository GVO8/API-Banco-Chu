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
    public class ContasController : ControllerBase
    {
        private readonly IContaService _contaService;
        private readonly ILogger<ContasController> _logger;

        public ContasController(
            IContaService contaService,
            ILogger<ContasController> logger)
        {
            _contaService = contaService ?? throw new ArgumentNullException(nameof(contaService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Cria uma nova conta bancária
        /// </summary>
        /// <param name="request">Dados para criação da conta</param>
        /// <returns>Conta criada</returns>
        [HttpPost]
        [ProducesResponseType(typeof(ContaResponse), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
        [AllowAnonymous] // Permitir criação sem autenticação
        public async Task<IActionResult> CriarConta([FromBody] CriarContaRequest request)
        {
            try
            {
                _logger.LogInformation("Recebida requisição para criar conta para {Nome}", request.Nome);
                
                var contaCriada = await _contaService.CriarContaAsync(request);
                
                _logger.LogInformation("Conta criada com sucesso: {NumeroConta}", contaCriada.NumeroConta);
                
                return CreatedAtAction(
                    nameof(ObterContaPorId),
                    new { id = contaCriada.Id, version = "1.0" },
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
        /// Obtém uma conta pelo ID
        /// </summary>
        /// <param name="id">ID da conta</param>
        /// <returns>Dados da conta</returns>
        [HttpGet("{id:guid}")]
        [ProducesResponseType(typeof(ContaResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> ObterContaPorId(Guid id)
        {
            try
            {
                var conta = await _contaService.GetContaByIdAsync(id);
                return Ok(conta);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new ProblemDetails
                {
                    Title = "Conta não encontrada",
                    Detail = ex.Message,
                    Status = StatusCodes.Status404NotFound
                });
            }
        }
    }
}