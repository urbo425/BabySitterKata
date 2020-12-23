using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace BabySitter.API.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class BabySitterController : ControllerBase
    {
        private readonly IConfigurationRoot _configuration;

        public BabySitterController(IConfigurationRoot configuration)
        {
            _configuration = configuration;
        }

        [HttpPost("CalculatePay")]
        public ActionResult CalculatePay([FromBody] CalculatePayRequest calculatePayRequest)
        {
            var bedtimeHour = _configuration.GetValue<int>("BedtimeHour");
            if (calculatePayRequest.StartTime.Hour < 17)
            {
                return new BadRequestObjectResult("StartTime before 5:00PM is invalid.");
            }

            if (calculatePayRequest.EndTime.Hour > 04)
            {
                return new BadRequestObjectResult("EndTime after 4:00AM is invalid.");
            }
            
            return Ok();
        }
    }

    public class CalculatePayRequest
    {
        public DateTime StartTime { get; set; }

        public DateTime EndTime { get; set; }
    }
}