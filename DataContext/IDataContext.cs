using System;

namespace DataContext
{
    public interface IDataContext : IDisposable
    {
        int SaveChanges();
        void SetAutoDetectChangesEnabled(bool enabled);
    }
}