using Common;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using System;

namespace AzureTestApplication.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class KeyVaultController : ControllerBase
    {
        private readonly IConfiguration _configuration;

        public KeyVaultController(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        [HttpGet]
        public SecretResponse Get()
        {
            var secret = this._configuration.GetValue<string>(Constants.KeyVaultSecretName);

            return new SecretResponse
            {
                DateTime = DateTime.UtcNow,
                Secret = secret
            };
        }
    }
}
