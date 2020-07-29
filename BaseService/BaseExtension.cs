using EntityInfrastructure;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BaseService
{
    public static class BaseExtension
    {
        public static IEnumerable<TEntity> SelectPage<TEntity>(this IQueryable<TEntity> IQuery, int page, int pageSize, out int totalCount) where TEntity : class, IEntityState
        {
            if (page >= 0)
            {
                var skipNum = (page - 1);
                totalCount = IQuery.Count();
                return IQuery.Skip(skipNum < 0 ? 0 : skipNum).Take(pageSize).ToList();
            }
            else
            {
                totalCount = 0;
                return null;
            }
        }
    }
}
