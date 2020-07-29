using DataContext;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Data.Entity.ModelConfiguration.Conventions;
using System.Linq;
using System.Web;

namespace TestAreaDemo.Areas.Movie.Models
{
    public interface IMovieDbContext : IDataContext
    {
        Guid GetInstence();
    }

    public class MovieDbContext : DataContext.DataContext, IMovieDbContext
    {
        public MovieDbContext()
            : base("DefaultConnection")
        {
            //Configuration.LazyLoadingEnabled = false;
            //Configuration.ProxyCreationEnabled = false;
        }

        public Guid GetInstence()
        {
            return this.InstanceId;
        }

        public static MovieDbContext Create()
        {
            return new MovieDbContext();
        }

        /// <summary>
        /// Entity FrameWork 保存时
        /// </summary>
        /// <returns></returns>
        public override int SaveChanges()
        {
            int ret = 0;
            try
            {
                var Entitys = ChangeTracker.Entries();
                List<Object> ArrRedisInsertUpdateObj = new List<object>();

                //没有变动跳过
                if (!Entitys.Any(_e => _e.State != EntityState.Unchanged))
                    return 0;

                //string CurrentUserId = TestAreaDemo.Controllers.Utility.CurrentAppUser == null ? "" : TestAreaDemo.Controllers.Utility.CurrentAppUser.Id;
                //string CurrentUserName = TestAreaDemo.Controllers.Utility.CurrentAppUser == null ? "" : TestAreaDemo.Controllers.Utility.CurrentAppUser.UserName;

                #region 获取所有新增的数据

                var _entitiesAdded = Entitys.Where(_e => _e.State == EntityState.Added).ToList();

                //    #region 全文检索

                //    try
                //    {
                //        if (IsWriteDataToLunece)
                //            LuneceManager.IndexManager.OIndexManager.LuneceInsert(_entity, _entityProptys);
                //    }
                //    catch (Exception ex)
                //    {
                //        SQLDALHelper.WriteLogHelper.WriteLog("全文检索错误（LuneceInsert）：" + TestAreaDemo.Extensions.Common.GetExceptionMsg(ex), "LuneceError", true);
                //    }

                //    #endregion

                #endregion

                #region 获取所有更新的数据

                var _entitiesChanged = ChangeTracker.Entries().Where(_e => _e.State == EntityState.Modified).ToList();

                //    #region 全文检索&Redis缓存

                //    try
                //    {
                //        LuneceManager.IndexManager.OIndexManager.LuneceModify(_entity, _entityProptys);
                //    }
                //    catch (Exception ex)
                //    {
                //        SQLDALHelper.WriteLogHelper.WriteLog("全文检索错误（LuneceModify）：" + TestAreaDemo.Extensions.Common.GetExceptionMsg(ex), "LuneceError", true);
                //    }

                //    #endregion

                #endregion

                #region 获取所有删除的数据

                var _entitiesDeleted = Entitys.Where(_e => _e.State == EntityState.Deleted).ToList();
                //foreach (var entityitem in _entitiesChanged)
                //{
                //    var _entity = entityitem.Entity;
                //    var _entityProptys = _entity.GetType().GetProperties();
                //    try
                //    {
                //        if (IsWriteDataToLunece)
                //            LuneceManager.IndexManager.OIndexManager.LuneceDelete(_entity, _entityProptys);
                //    }
                //    catch (Exception ex)
                //    {
                //        SQLDALHelper.WriteLogHelper.WriteLog("全文检索错误（LuneceDelete）：" + TestAreaDemo.Extensions.Common.GetExceptionMsg(ex), "LuneceError", true);
                //    }
                //}
                //try
                //{
                //    RedisCacheManager.RedisManager.ORedisManager.AnalysisEntity(_entitiesDeleted.Select(x => x.Entity).ToList(), RedisType.Delete);
                //}
                //catch
                //{

                //}

                #endregion

                ret = base.SaveChanges();

                //#region  Redis缓存

                //try
                //{
                //    if (IsWriteDataToRedis)
                //        RedisCacheManager.RedisManager.ORedisManager.AnalysisEntity(ArrRedisInsertUpdateObj, RedisType.Insert_Update);
                //}
                //catch
                //{

                //}

                //#endregion
            }
            catch (Exception ex)
            {
                throw ex;
            }
            return ret;
        }

        /// <summary>
        /// 模型创建时
        /// </summary>
        /// <param name="modelBuilder"></param>
        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            //Oracle 表所有者，（SQL 改成 dbo(默认)，也可删除此设置）
            modelBuilder.HasDefaultSchema(DbSchema);
            //设置联合主键
            //modelBuilder.Entity<ModelName>().HasKey(t => new { t.VERDORCODE, t.MFLAG });

            #region AirOut 特殊数据格式设置

            #region 设置数据关系

            modelBuilder.Entity<Movie>().
                HasMany(p => p.ActorList).
                WithMany(s => s.MovieList).
                Map(m =>
                {
                    m.MapLeftKey("MovieId");
                    m.MapRightKey("ActorId");
                    m.ToTable("MovieActor");
                });
            #endregion

            #region 设置删除数据关系

            //modelBuilder.Entity<EMS_HEAD>()
            //    .HasMany(e => e.EMS_EXGS)
            //    .WithRequired(e => e.EMS_HEAD)
            //    .WillCascadeOnDelete(false);

            #endregion

            #region 已经 手动设置过 数据位数

            //统一设置Decimal 长度（数据库实际位数可以缩短）
            modelBuilder.Properties<decimal>().Configure(c => c.HasPrecision(28, 9));
            //手动特殊话设置
            //modelBuilder.Entity<EMS_EXG>()//类
            //    .Property(e => e.FACTOR_1)//字段
            //    .HasPrecision(18, 9);//长度

            #endregion

            #endregion

            //关闭一对多的级联删除。
            modelBuilder.Conventions.Remove<OneToManyCascadeDeleteConvention>();
            //关闭多对多的级联删除。
            modelBuilder.Conventions.Remove<ManyToManyCascadeDeleteConvention>();
            //移除EF的表名公约  
            modelBuilder.Conventions.Remove<PluralizingTableNameConvention>();
            //移除对MetaData表的查询验证，要不然每次都要访问Metadata这个表
            modelBuilder.Conventions.Remove<IncludeMetadataConvention>();

            ////数据库不存在时重新创建数据库
            //Database.SetInitializer<WebDbContext>(new CreateDatabaseIfNotExists<WebDbContext>());
            ////每次启动应用程序时创建数据库
            //Database.SetInitializer<WebDbContext>(new DropCreateDatabaseAlways<WebDbContext>());
            //模型更改时重新创建数据库
            //Database.SetInitializer<WebDbContext>(new DropCreateDatabaseIfModelChanges<WebDbContext>());
            //从不创建数据库
            Database.SetInitializer<MovieDbContext>(null);

            base.OnModelCreating(modelBuilder);
        }

        //----------------------------------- 数据表 -------------------------------------
        public DbSet<Movie> Movie { get; set; }

        public DbSet<Actor> Actor { get; set; }
    }
}