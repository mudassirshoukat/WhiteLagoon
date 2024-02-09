using WhiteLagoon.Domain.Entities;

namespace WhiteLagoon.Application.Common.Interfaces
{
    public interface IBookingRepository:IRepository<Booking>
    {
        void UpdateBooking(Booking bookingStatus);
        Task UpdateStatusAsync(int bookingId,string status,int villaNumber);
        Task UpdateStripePaymentIDAsync(int bookingId,string sessionId,string paymentIntentId);
    }
}
