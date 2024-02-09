using WhiteLagoon.Domain.Entities;

namespace WhiteLagoon.Application.Common.utility
{
    public static class SD
    {
        public const string Role_Admin = "Admin";
        public const string Role_Customer = "Customer";

        public const string StatusPending = "Pending";
        public const string StatusApproved = "Approved";
        public const string StatusCheckedIn = "CheckedIn";
        public const string StatusCompleted = "Completed";
        public const string StatusCancelled = "Cancelled";
        public const string StatusRefunded = "Refunded";

        public enum BookingStatus
        {
            Pending,
            Approved,
            CheckedIn,
            Completed,
            Cancelled,
            Refunded
        }

        public static int VillaRoomAvailable_Count(int villaId, IEnumerable<VillaNumber> villaNumberList,
            DateOnly checkInDate, int nights, IEnumerable<Booking> bookings)
        {

            List<int> bookingInDate = new();
            int finalizeAvailableRooms = int.MaxValue;  //it will get smallest value of available rooms in duration of booking
            int roomsInVilla = villaNumberList.Where(x => x.VillaId == villaId).Count();

            for (int i = 0; i < nights; i++)
            {
                var villaBooked = bookings.Where(x => x.CheckInDate <= checkInDate.AddDays(i)
                && x.CheckOutDate > checkInDate.AddDays(i) && x.VillaId == villaId).ToList();

                foreach (var booking in villaBooked)
                {
                    if (!bookingInDate.Contains(booking.Id))
                    {
                        bookingInDate.Add(booking.Id);
                    }
                }

                var availablerooms = roomsInVilla - bookingInDate.Count();

                if (availablerooms == 0)
                {
                    return 0;
                }
                else
                {
                    if (finalizeAvailableRooms > availablerooms)
                    {
                        finalizeAvailableRooms = availablerooms;
                    }
                }
            }//forloop

            return finalizeAvailableRooms;
        }

    }
}
