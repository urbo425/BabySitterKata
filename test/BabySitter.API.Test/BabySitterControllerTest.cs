using System;
using System.Collections.Generic;
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
                EndTime = new DateTime(2020, 01, 02, 18, 00, 00)
            };

            var response = (BadRequestObjectResult) sut.CalculatePay(request);

            response.StatusCode.Should().Be(400);
            response.Value.Should().Be("StartTime before 5:00PM is invalid.");
        }

        [Fact]
        public void ShouldReturnBadRequest_GivenEndTimeAfter0400TheNextDay()
        {
            var sut = GetDefaultBabySitterController();
            var request = new CalculatePayRequest
            {
                StartTime = new DateTime(2020, 01, 01, 18, 00, 00),
                EndTime = new DateTime(2020, 01, 02, 05, 00, 00)
            };

            var response = (BadRequestObjectResult) sut.CalculatePay(request);

            response.StatusCode.Should().Be(400);
            response.Value.Should().Be("EndTime after 4:00AM the next day is invalid.");
        }

        [Fact]
        public void ShouldReturnOk()
        {
            var sut = GetDefaultBabySitterController();
            var request = new CalculatePayRequest
            {
                StartTime = new DateTime(2020, 01, 01, 18, 00, 00),
                EndTime = new DateTime(2020, 01, 01, 20, 00, 00)
            };

            var response = (OkObjectResult) sut.CalculatePay(request);

            response.StatusCode.Should().Be(200);
        }

        [Fact]
        public void ShouldCalculateRateAt12DollarsPerHour_GivenTimeIsBeforeBedtime()
        {
            var configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string>
                {
                    {"BedtimeHour", "18"}
                }).Build();
            var sut = new BabySitterController(configuration);
            var request = new CalculatePayRequest
            {
                StartTime = new DateTime(2020, 01, 01, 17, 00, 00),
                EndTime = new DateTime(2020, 01, 01, 18, 00, 00)
            };

            var response = (OkObjectResult) sut.CalculatePay(request);

            response.StatusCode.Should().Be(200);
            response.Value.Should().Be("$12.00");
        }

        [Fact]
        public void ShouldCalculateRateAt8DollarsPerHour_GivenTimeIsAfterBedtimeAndBeforeMidnight()
        {
            var configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string>
                {
                    {"BedtimeHour", "18"}
                }).Build();
            var sut = new BabySitterController(configuration);
            var request = new CalculatePayRequest
            {
                StartTime = new DateTime(2020, 01, 01, 19, 00, 00),
                EndTime = new DateTime(2020, 01, 01, 20, 00, 00)
            };

            var response = (OkObjectResult) sut.CalculatePay(request);

            response.StatusCode.Should().Be(200);
            response.Value.Should().Be("$8.00");
        }

        [Fact]
        public void ShouldCalculateRateAt16DollarsPerHour_GivenTimeIsAfterMidnight()
        {
            var configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string>
                {
                    {"BedtimeHour", "18"}
                }).Build();
            var sut = new BabySitterController(configuration);
            var request = new CalculatePayRequest
            {
                StartTime = new DateTime(2020, 01, 01, 23, 00, 00),
                EndTime = new DateTime(2020, 01, 02, 01, 00, 00)
            };

            var response = (OkObjectResult) sut.CalculatePay(request);

            response.StatusCode.Should().Be(200);
            response.Value.Should().Be("$24.00");
        }

        [Fact]
        public void ShouldCalculatePayForAllRates()
        {
            var configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string>
                {
                    {"BedtimeHour", "20"}
                }).Build();
            var sut = new BabySitterController(configuration);
            var request = new CalculatePayRequest
            {
                StartTime = new DateTime(2020, 01, 01, 17, 00, 00),
                EndTime = new DateTime(2020, 01, 02, 04, 00, 00)
            };

            var response = (OkObjectResult) sut.CalculatePay(request);

            response.StatusCode.Should().Be(200);
            response.Value.Should().Be("$132.00");
        }

        private BabySitterController GetDefaultBabySitterController()
        {
            var configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string>
                {
                    {"BedtimeHour", "20"}
                }).Build();
            return new BabySitterController(configuration);
        }
    }
}
