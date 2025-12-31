using Microsoft.AspNetCore.Mvc;
using BMPTec.Application.DTOs;
using BMPTec.Application.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;

namespace BMPTec.API.Controllers.v1
{
    [ApiController]
    [Route("api/v{version:apiVersion}/[controller]")]
    [ApiVersion("1.0")]
    [Authorize]
    public class ExtratoController : ControllerBase
    {
        private readonly IExtratoAppService _extratoAppService;
        private readonly ILogger<ExtratoController> _logger;

        public ExtratoController(
            IExtratoAppService extratoAppService,
            ILogger<ExtratoController> logger)
        {
            _extratoAppService = extratoAppService ?? throw new ArgumentNullException(nameof(extratoAppService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        [HttpPost]
        [AllowAnonymous]
        public async Task<IActionResult> GerarExtrato([FromBody] ExtratoRequest request)
        {
            try
            {
                // Validar se usuário tem acesso à conta
                var usuarioId = User.FindFirst("sub")?.Value;
                await ValidarAcessoConta(request.ContaId, usuarioId);
                
                var extrato = await _extratoAppService.GerarExtratoAsync(request);
                return Ok(extrato);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao gerar extrato");
                return BadRequest(new { error = ex.Message });
            }
        }

        [HttpPost("txt")]
        [AllowAnonymous]
        public async Task<IActionResult> GerarExtratoTxt([FromBody] ExtratoRequest request)
        {
            try
            {
                var usuarioId = User.FindFirst("sub")?.Value;
                await ValidarAcessoConta(request.ContaId, usuarioId);
                
                var txtStream = await _extratoAppService.GerarExtratoTxtAsync(request);
                return File(txtStream, "application/txt", $"extrato_{DateTime.Now:yyyyMMddHHmmss}.txt");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao gerar txt do extrato");
                return BadRequest(new { error = ex.Message });
            }
        }

        private async Task ValidarAcessoConta(Guid contaId, string usuarioId)
        {
            // Implementar lógica de validação
            // Ex: verificar se conta pertence ao usuário
        }
    }
}