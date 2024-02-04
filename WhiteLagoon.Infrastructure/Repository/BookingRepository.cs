using Microsoft.EntityFrameworkCore;
using WhiteLagoon.Application.Common.Interfaces;
using WhiteLagoon.Application.Common.utility;
using WhiteLagoon.Domain.Entities;
using WhiteLagoon.Infrastructure.Data;

namespace WhiteLagoon.Infrastructure.Repository
{
    public class BookingRepository: Repository<Booking>,IBookingRepository
    {
        private readonly ApplicationDbContext context;

        public BookingRepository(ApplicationDbContext context):base(context)
        {
            this.context = context;
        }

        public void UpdateBooking(Booking booking)
        {
            context.bookings.Update(booking);
        }

        public async Task UpdateStatusAsync(int bookingId, string bookingStatus)
        {
           var booking=await context.bookings.FirstOrDefaultAsync(x=>x.Id==bookingId);
            if (booking != null)
            {
                booking.Status = bookingStatus;
                if (bookingStatus == SD.StatusCheckedIn)
                {
                    booking.ActualCheckInDate = DateTime.Now;
                }
                if (bookingStatus == SD.StatusCompleted)
                {
                    booking.ActualCheckOutDate = DateTime.Now;
                }
            }
        }

        public async Task UpdateStripePaymentIDAsync(int bookingId, string sessionId, string paymentIntentId)
        {
            var booking = await context.bookings.FirstOrDefaultAsync(x => x.Id == bookingId);
            if (booking != null)
            {
                if (!string.IsNullOrEmpty(sessionId))
                {
                    booking.StripeSessionId = sessionId;
                }

                if (!string.IsNullOrEmpty(paymentIntentId))
                {
                    booking.StripePaymentIntentId = paymentIntentId;
                    booking.PaymentDate = DateTime.Now;
                    booking.IsPaymentSuccessful= true;
                }

            }
        }
    }
}
