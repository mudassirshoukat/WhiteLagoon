namespace WhiteLagoon.Application.Common.Interfaces
{
    public interface IUnitOfWork
    {
        public IVillaRepository VillaRepo { get; }
        public IVillaNumberRepository VillaNumberRepo { get; }
        public IAmenityRepository AmenityRepo { get; }
        public bool HasChanges();
        public Task<bool> SaveAllAsync();
    }
}
