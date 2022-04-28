using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using qei.Services;

namespace qei.Controllers
{
    [Route("q")]
    [ApiController]
    public class qeiController : ControllerBase
    {
        private IQeiService _queiService;

        public qeiController(IQeiService qeiService)
        {
            _queiService = qeiService;
        }

        [HttpGet("create")]
        public async Task<string> Create(string email)
        {
            return await _queiService.Create(email);
        }

        [HttpGet("add")]
        public async Task<bool> Add(string database, string key, string value)
        {
            return await _queiService.Add(database, key, value);
        }

        [HttpGet("get")]
        public async Task<string> Get(string database, string key)
        {
            return await _queiService.Get(database, key);
        }
    }
}
