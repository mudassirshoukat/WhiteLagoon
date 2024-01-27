
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;
using WhiteLagoon.Application.Common.Interfaces;
using WhiteLagoon.Domain.Entities;
using WhiteLagoon.Infrastructure.Data;

namespace WhiteLagoon.Infrastructure.Repository
{
    public class VillaRepository : Repository<Villa>, IVillaRepository
    {
        private readonly ApplicationDbContext context;

        public VillaRepository(ApplicationDbContext context):base(context)
        {
            this.context = context;
        }

     

    

        public void UpdateVilla(Villa villa)
        {
            context.Villas.Update(villa);
        }
    }
}
