using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WhiteLagoon.Application.Common.Interfaces;
using WhiteLagoon.Domain.Entities;
using WhiteLagoon.Infrastructure.Data;

namespace WhiteLagoon.Infrastructure.Repository
{
    internal class VillaNumberRepository:Repository<VillaNumber>,IVillaNumberRepository
    {
        private readonly ApplicationDbContext context;

        public VillaNumberRepository(ApplicationDbContext context):base(context) {
            this.context = context;
        }

        public void UpdateVillaNumber(VillaNumber villaNumber)
        {
            context.VillaNumbers.Update(villaNumber);
        }
    }
}
