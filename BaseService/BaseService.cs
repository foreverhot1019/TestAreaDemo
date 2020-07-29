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
    public abstract class BaseService<TEntity> : IBaseService<TEntity> where TEntity : class, IEntityState
    {
        public readonly DbContext MyDbContext;

        public BaseService(DbContext _MyDbContext)
        {
            MyDbContext = _MyDbContext;
        }

        public virtual IQueryable<TEntity> Query(Expression<Func<TEntity, bool>> query) { return MyDbContext.Set<TEntity>().Where(query); }

        public virtual IEnumerable<TEntity> QueryList(Expression<Func<TEntity, bool>> query) { return MyDbContext.Set<TEntity>().Where(query).ToList(); }

        public virtual TEntity Find(params object[] keyValues) { return MyDbContext.Set<TEntity>().Find(keyValues); }

        public virtual TEntity Find(Expression<Func<TEntity, bool>> query) { return MyDbContext.Set<TEntity>().Where(query).FirstOrDefault(); }

        public virtual void Insert(TEntity OTEntity) { MyDbContext.Entry(OTEntity).State = EntityState.Added; }

        public virtual void InsertRange(IEnumerable<TEntity> ArrTEntity)
        {
            foreach (var OTEntity in ArrTEntity)
            {
                MyDbContext.Entry(OTEntity).State = EntityState.Added;
            }
        }

        public virtual void Delete(TEntity OTEntity) { MyDbContext.Entry(OTEntity).State = EntityState.Deleted; }

        public virtual void Delete(int Id) { Delete(MyDbContext.Set<TEntity>().Find(Id)); }

        //public virtual void DeleteRange(IEnumerable<int> ArrKeyId) { Delete(MyDbContext.Set<TEntity>().Find(ArrId)); }

        public virtual void Update(TEntity OTEntity) { MyDbContext.Entry(OTEntity).State = EntityState.Modified; }

        public virtual IEnumerable<TEntity> SqlQuery(string query, params object[] parameters) { return MyDbContext.Set<TEntity>().SqlQuery(query, parameters).ToList(); }

    }
}