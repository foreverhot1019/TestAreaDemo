using DataContext.Extensions;
using DataContext.Models;
using Microsoft.AspNet.Identity.EntityFramework;
using System;

namespace DataContext
{
    public class DataContextAppUser : IdentityDbContext<ApplicationUser>, IDataContext
    {
        #region Private Fields
        private readonly Guid _instanceId;
        private bool _disposed;
        #endregion Private Fields

        public DataContextAppUser(string nameOrConnectionString)
            : base(nameOrConnectionString)
        {
            _instanceId = Guid.NewGuid();
            Configuration.LazyLoadingEnabled = false;
            Configuration.ProxyCreationEnabled = false;
        }

        public Guid InstanceId { get { return _instanceId; } }

        /// <summary>
        /// 开启全文检索功能
        /// </summary>
        protected bool IsWriteDataToLunece { get; set; }

        /// <summary>
        /// 开启Redis缓存
        /// </summary>
        protected bool IsWriteDataToRedis { get; set; }

        /// <summary>
        /// 设置
        /// </summary>
        /// <param name="enabled"></param>
        public void SetAutoDetectChangesEnabled(bool enabled)
        {
            this.Configuration.AutoDetectChangesEnabled = enabled;
        }

        /// <summary>
        /// Oracle 表所有者，（SQL 改成 dbo(默认)，也可删除此设置）
        /// </summary>
        public string DbSchema
        {
            get
            {
                if (System.Configuration.ConfigurationManager.AppSettings["DbSchema"] == null)
                    return "dbo";
                else
                    return System.Configuration.ConfigurationManager.AppSettings["DbSchema"].ToString();
            }
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
                //var Entitys = ChangeTracker.Entries();
                //List<Object> ArrRedisInsertUpdateObj = new List<object>();

                ////没有变动跳过
                //if (!Entitys.Any(_e => _e.State != EntityState.Unchanged))
                //    return 0;

                //string CurrentUserId = TestAreaDemo.Controllers.Utility.CurrentAppUser == null ? "" : TestAreaDemo.Controllers.Utility.CurrentAppUser.Id;
                //string CurrentUserName = TestAreaDemo.Controllers.Utility.CurrentAppUser == null ? "" : TestAreaDemo.Controllers.Utility.CurrentAppUser.UserName;

                //#region 获取所有新增的数据

                //var _entitiesAdded = Entitys.Where(_e => _e.State == EntityState.Added).ToList();

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

                //#endregion

                //#region 获取所有更新的数据

                //var _entitiesChanged = ChangeTracker.Entries().Where(_e => _e.State == EntityState.Modified).ToList();

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

                //#endregion

                //#region 获取所有删除的数据

                //var _entitiesDeleted = Entitys.Where(_e => _e.State == EntityState.Deleted).ToList();
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

                //#endregion

                //ret = base.SaveChanges();

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
                string ErrMsg = Common.GetExceptionMsg(ex);
                WriteLogHelper.WriteLog(ErrMsg, "SaveChanges", true);
                throw ex;
            }
            return ret;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="disposing"></param>
        protected override void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    // free other managed objects that implement
                    // IDisposable only
                }

                // release any unmanaged objects
                // set object references to null

                _disposed = true;
            }

            base.Dispose(disposing);
        }

    }
}