using EntityInfrastructure;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace BaseService
{
    public interface IBaseService<TEntity> where TEntity : class, IEntityState
    {
        IQueryable<TEntity> Query(Expression<Func<TEntity, bool>> query);

        IEnumerable<TEntity> QueryList(Expression<Func<TEntity, bool>> query);

        TEntity Find(params object[] keyValues);

        TEntity Find(Expression<Func<TEntity, bool>> query);

        void Insert(TEntity OTEntity);

        void InsertRange(IEnumerable<TEntity> ArrTEntity);

        void Delete(TEntity OTEntity);

        void Delete(int Id);

        //DeleteRange(IEnumerable<int> ArrKeyId) { Delete(MyDbContext.Set<TEntity>().Find(ArrId)); }

        void Update(TEntity OTEntity);

    }
}
