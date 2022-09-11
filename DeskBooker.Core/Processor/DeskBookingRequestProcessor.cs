using DeskBooker.Core.DataInterface;
using DeskBooker.Core.Domain;
using System;
using System.Linq;

namespace DeskBooker.Core.Processor
{
    public class DeskBookingRequestProcessor
    {
        private readonly IDeskBookingRepository _deskBookingRepository;
        private readonly IDeskRepository _deskRepository;

        public DeskBookingRequestProcessor()
        {
        }

        public DeskBookingRequestProcessor(IDeskBookingRepository deskBookingRepository, IDeskRepository deskRepository)
        {
            _deskBookingRepository = deskBookingRepository;
            _deskRepository = deskRepository;
        }

        public DeskBookingResult BookDesk(DeskBookingRequest request)
        {
            if (request == null) throw new ArgumentNullException(nameof(request));

            var availableDesks = _deskRepository.GetAvailableDesks(request.Date);
            var result = Create<DeskBookingResult>(request);
            if (availableDesks.FirstOrDefault() is Desk availableDesk)
            {
                var deskBooking = Create<DeskBooking>(request);
                deskBooking.DeskId = availableDesk.Id;
                _deskBookingRepository.Save(deskBooking);
                result.Code = DeskBookingResultCode.Success;
                result.DeskBookingId = deskBooking.Id;
            }
            else
            {
                result.Code = DeskBookingResultCode.NoDeskAvailable;
            }

            return result;
        }

        private static T Create<T>(DeskBookingRequest request) where T : DeskBookingBase, new()
        {
            return new T
            {
                FistName = request.FistName,
                LastName = request.LastName,
                Date = request.Date,
                Email = request.Email,
            };
        }
    }
}