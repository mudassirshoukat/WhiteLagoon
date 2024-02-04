using WhiteLagoon.Domain.Entities;

namespace WhiteLagoon.Application.Common.Interfaces
{
    public interface IBookingRepository:IRepository<Booking>
    {
        void UpdateBooking(Booking bookingStatus);
        Task UpdateStatusAsync(int bookingId,string status);
        Task UpdateStripePaymentIDAsync(int bookingId,string sessionId,string paymentIntentId);
    }
}
