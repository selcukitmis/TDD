using DeskBooker.Core.DataInterface;
using DeskBooker.Core.Domain;
using DeskBooker.Core.Processor;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace DeskBooker.Core.Tests.Processor
{
    public class DeskBookingRequestProcessorTests
    {
        private readonly DeskBookingRequestProcessor _processor;
        private readonly Mock<IDeskBookingRepository> _deskBookingRepositoryMock;
        private readonly Mock<IDeskRepository> _deskRepositoryMock;
        private readonly List<Desk> _availableDesks;
        private readonly DeskBookingRequest _request;

        public DeskBookingRequestProcessorTests()
        {
            _deskBookingRepositoryMock = new Mock<IDeskBookingRepository>();
            _deskRepositoryMock = new Mock<IDeskRepository>();

            _availableDesks = new List<Desk> { new Desk { Id = 7 } };
            _request = new DeskBookingRequest
            {
                FistName = "Selçuk",
                LastName = "İTMİŞ",
                Email = "selcuk.itmis@outlook.com",
                Date = new DateTime(2022, 6, 12)
            };
            _deskRepositoryMock.Setup(s => s.GetAvailableDesks(_request.Date)).Returns(_availableDesks);
            _processor = new DeskBookingRequestProcessor(_deskBookingRepositoryMock.Object, _deskRepositoryMock.Object);


        }

        [Fact]
        public void ShouldReturnDeskBookingResultWithRequestValues()
        {
            DeskBookingResult result = _processor.BookDesk(_request);

            Assert.NotNull(result);
            Assert.Equal(_request.FistName, result.FistName);
            Assert.Equal(_request.LastName, result.LastName);
            Assert.Equal(_request.Email, result.Email);
            Assert.Equal(_request.Date, result.Date);
        }

        [Fact]
        public void ShouldThrowExceptionIfRequestIsNull()
        {
            var exception = Assert.Throws<ArgumentNullException>(() => _processor.BookDesk(null));

            Assert.Equal("request", exception.ParamName);
        }

        [Fact]
        public void ShouldSaveBooking()
        {
            if (_availableDesks.Count > 0)
            {

            }
            DeskBooking savedDeskBooking = null;
            _deskBookingRepositoryMock.Setup(x => x.Save(It.IsAny<DeskBooking>())).Callback<DeskBooking>(x => savedDeskBooking = x);
            _processor.BookDesk(_request);

            _deskBookingRepositoryMock.Verify(x => x.Save(It.IsAny<DeskBooking>()), Times.Once);

            Assert.NotNull(savedDeskBooking);
            Assert.Equal(_request.FistName, savedDeskBooking.FistName);
            Assert.Equal(_request.LastName, savedDeskBooking.LastName);
            Assert.Equal(_request.Email, savedDeskBooking.Email);
            Assert.Equal(_request.Date, savedDeskBooking.Date);
            Assert.Equal(_availableDesks.First().Id, savedDeskBooking.DeskId);
        }

        [Fact]
        public void ShouldNotSaveDeskBookingIfNoIsAvailable()
        {
            _availableDesks.Clear();

            _processor.BookDesk(_request);

            _deskBookingRepositoryMock.Verify(x => x.Save(It.IsAny<DeskBooking>()), Times.Never);
        }

        [Theory]
        [InlineData(DeskBookingResultCode.Success, true)]
        [InlineData(DeskBookingResultCode.NoDeskAvailable, false)]
        public void ShouldReturnExpectedResultCode(DeskBookingResultCode expectedResultCode, bool isDeskAvailable)
        {
            if (!isDeskAvailable)
            {
                _availableDesks.Clear();
            }
            var result = _processor.BookDesk(_request);
            Assert.Equal(expectedResultCode, result.Code);
        }

        [Theory]
        [InlineData(5, true)]
        [InlineData(null, false)]
        public void ShouldReturnExpectedDeskBookingId(int? expectedDeskBookingId, bool isDeskAvailable)
        {
            if (!isDeskAvailable)
            {
                _availableDesks.Clear();
            }
            else
            {
                _deskBookingRepositoryMock.Setup(s => s.Save(It.IsAny<DeskBooking>())).Callback<DeskBooking>(t =>
                {
                    t.Id = expectedDeskBookingId.Value;
                });
            }
            var result = _processor.BookDesk(_request);
            Assert.Equal(expectedDeskBookingId, result.DeskBookingId);
        }
    }
}
