using System;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;

namespace BabySitter.API.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class BabySitterController : ControllerBase
    {
        private readonly IConfiguration _configuration;

        public BabySitterController(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        [HttpPost("CalculatePay")]
        public ActionResult CalculatePay([FromBody] CalculatePayRequest calculatePayRequest)
        {
            var bedtimeHour = _configuration.GetValue<int>("BedtimeHour");
            var bedtime = calculatePayRequest.StartTime.Date.Add(new TimeSpan(bedtimeHour, 0, 0));
            if (calculatePayRequest.StartTime.Hour < 17)
            {
                return new BadRequestObjectResult("StartTime before 5:00PM is invalid.");
            }

            if (calculatePayRequest.EndTime.Hour > 04 && 
                calculatePayRequest.EndTime.Date == calculatePayRequest.StartTime.Date.AddDays(1))
            {
                return new BadRequestObjectResult("EndTime after 4:00AM the next day is invalid.");
            }

            var hoursWorkedBeforeBedtime = GetHoursWorkedBeforeBedtime(calculatePayRequest, bedtime);
            var hoursWorkedAfterMidnight = GetHoursWorkedAfterMidnight(calculatePayRequest);
            var hoursWorkedBetweenBedtimeAndMidnight =
                GetHoursWorkedBetweenBedtimeAndMidnight(calculatePayRequest, hoursWorkedBeforeBedtime, hoursWorkedAfterMidnight);
            var pay = CalculateTotalPay(hoursWorkedBeforeBedtime, hoursWorkedAfterMidnight,
                hoursWorkedBetweenBedtimeAndMidnight);
            return Ok(pay.ToString("C"));
        }

        private double CalculateTotalPay(double hoursWorkedBeforeBedtime, double hoursWorkedAfterMidnight, double hoursWorkedBetweenBedtimeAndMidnight)
        {
            return (hoursWorkedBeforeBedtime * 12) +
                   (hoursWorkedAfterMidnight * 16) +
                   (hoursWorkedBetweenBedtimeAndMidnight * 8);
        }

        private double GetHoursWorkedAfterMidnight(CalculatePayRequest calculatePayRequest)
        {
            var midnightOfNextDay = calculatePayRequest.StartTime.AddDays(1).Date;
            if (calculatePayRequest.EndTime >= midnightOfNextDay)
            {
                return (calculatePayRequest.EndTime - midnightOfNextDay).TotalHours;
            }

            return 0;
        }

        private double GetHoursWorkedBetweenBedtimeAndMidnight(CalculatePayRequest calculatePayRequest, double hoursWorkedBeforeBedtime, double hoursWorkedAfterMidnight)
        {
            var totalHoursWorked = (calculatePayRequest.EndTime - calculatePayRequest.StartTime).TotalHours;
            var hoursBetweenBedtimeAndMidnight = totalHoursWorked - (hoursWorkedBeforeBedtime + hoursWorkedAfterMidnight);
            return hoursBetweenBedtimeAndMidnight > 0 ? hoursBetweenBedtimeAndMidnight : 0;
        }

        private double GetHoursWorkedBeforeBedtime(CalculatePayRequest calculatePayRequest, DateTime bedtime)
        {
            if (calculatePayRequest.StartTime >= bedtime) return 0;

            if (calculatePayRequest.EndTime < bedtime)
            {
                return (calculatePayRequest.EndTime - calculatePayRequest.StartTime).TotalHours;
            }
           
            return (bedtime - calculatePayRequest.StartTime).TotalHours;
        }
    }

    public class CalculatePayRequest
    {
        public DateTime StartTime { get; set; }

        public DateTime EndTime { get; set; }
    }
}