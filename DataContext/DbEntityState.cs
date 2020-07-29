using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity;

namespace EntityInfrastructure
{
    /// <summary>
    /// 为了区分 数据库表类和普通类
    /// </summary>
    public interface IEntityState
    {
        EntityState EntityState { get; set; }
    }

    /// <summary>
    /// 为了区分 数据库表类和普通类
    /// </summary>
    public abstract class DbEntity : IEntityState
    {
        public DbEntity()
        {
            EntityState = EntityState.Unchanged;
        }

        [NotMapped]
        public EntityState EntityState { get; set; }
    }
}