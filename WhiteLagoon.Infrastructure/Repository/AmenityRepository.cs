using WhiteLagoon.Application.Common.Interfaces;
using WhiteLagoon.Domain.Entities;
using WhiteLagoon.Infrastructure.Data;

namespace WhiteLagoon.Infrastructure.Repository
{
    public class AmenityRepository : Repository<Amenity>, IAmenityRepository
    {
        private readonly ApplicationDbContext context;

        public AmenityRepository(ApplicationDbContext context):base(context) 
        {
            this.context = context;
        }


        public void UpdateAmenity(Amenity amenity)
        {
           context.Amenities.Update(amenity);
        }
    }
}
