using System;
using System.Collections.Generic;
using System.Net;
using BabySitter.API.Controllers;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Xunit;

namespace BabySitter.API.Test
{
    public class BabySitterControllerTest
    {
        
        [Fact]
        public void ShouldReturnBadRequest_GivenStartTimeBefore1700()
        {
            var sut = GetDefaultBabySitterController();
            var request = new CalculatePayRequest
            {
                StartTime = new DateTime(2020, 01, 01, 16, 00, 00),
                EndTime = new DateTime()
            };

            var response = (BadRequestObjectResult) sut.CalculatePay(request);

            response.StatusCode.Should().Be(400);
            response.Value.Should().Be("StartTime before 5:00PM is invalid.");
        }

        [Fact]
        public void ShouldReturnBadRequest_GivenEndTimeAfter0400()
        {
            var sut = GetDefaultBabySitterController();
            var request = new CalculatePayRequest
            {
                StartTime = new DateTime(2020, 01, 01, 18, 00, 00),
                EndTime = new DateTime(2020, 01, 02, 05, 00, 00)
            };

            var response = (BadRequestObjectResult) sut.CalculatePay(request);

            response.StatusCode.Should().Be(400);
            response.Value.Should().Be("EndTime after 4:00AM is invalid.");
        }
        
        private BabySitterController GetDefaultBabySitterController()
        {
            var configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string>
                {
                    {"BedtimeHour", "8"}
                }).Build();
            return new BabySitterController(configuration);
        }
    }
}
