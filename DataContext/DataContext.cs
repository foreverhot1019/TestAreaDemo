using DataContext.Extensions;
using System;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity;
using System.Data.Entity.Core.Common.CommandTrees;
using System.Data.Entity.Core.Common.CommandTrees.ExpressionBuilder;
using System.Data.Entity.Core.Metadata.Edm;
using System.Data.Entity.Infrastructure;
using System.Data.Entity.Infrastructure.Interception;
using System.Data.Entity.ModelConfiguration.Conventions;
using System.Diagnostics;
using System.Linq;
using System.Reflection;

namespace DataContext
{
    public class DataContext : DbContext, IDataContext
    {
        #region Private Fields
        private readonly Guid _instanceId;
        private bool _disposed;
        #endregion Private Fields

        public DataContext(string nameOrConnectionString)
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

    //配置公约
    public class EFConfiguration : DbConfiguration
    {
        public EFConfiguration()
        {
            //AddInterceptor(new StringTrimmerInterceptor());
            ////或者注册在Global.asax中的Application_Start 
            //DbInterception.Add(new EFIntercepterLogging());
        }
    }

    #region 使 EntityFrameWork 自动 去除字符串 末尾 空格

    /// <summary>
    /// 设置 SQL 条件 "123 "与"123"比对的问题
    /// 使 EntityFrameWork 自动 去除字符串 末尾 空格
    /// SQL 默认去除 最末尾的空格 "123 " 等价于 "123"
    /// </summary>
    public class StringTrimmerInterceptor : IDbCommandTreeInterceptor
    {
        public void TreeCreated(DbCommandTreeInterceptionContext interceptionContext)
        {
            if (interceptionContext.OriginalResult.DataSpace == DataSpace.SSpace)
            {
                var queryCommand = interceptionContext.Result as DbQueryCommandTree;
                if (queryCommand != null)
                {
                    var newQuery = queryCommand.Query.Accept(new StringTrimmerQueryVisitor());
                    interceptionContext.Result = new DbQueryCommandTree(
                        queryCommand.MetadataWorkspace,
                        queryCommand.DataSpace,
                        newQuery);
                }
            }
        }

        private class StringTrimmerQueryVisitor : DefaultExpressionVisitor
        {
            private static readonly string[] _typesToTrim = { "nvarchar", "varchar", "char", "nchar" };

            public override DbExpression Visit(DbNewInstanceExpression expression)
            {
                var arguments = expression.Arguments.Select(a =>
                {
                    var propertyArg = a as DbPropertyExpression;
                    if (propertyArg != null && _typesToTrim.Contains(propertyArg.Property.TypeUsage.EdmType.Name))
                    {
                        return EdmFunctions.Trim(a);
                    }

                    return a;
                });

                return DbExpressionBuilder.New(expression.ResultType, arguments);
            }
        }
    }

    #endregion

    #region 记录 EntityFrameWork 生成的SQL 以及 性能

    /// <summary>
    /// 记录 EntityFrameWork 生成的SQL 以及 性能
    /// 使用EntityFramework6.1的DbCommandInterceptor拦截生成的SQL语句
    /// </summary>
    public class EFIntercepterLogging : DbCommandInterceptor
    {
        private readonly Stopwatch _stopwatch = new Stopwatch();

        public override void ScalarExecuting(System.Data.Common.DbCommand command, DbCommandInterceptionContext<object> interceptionContext)
        {
            base.ScalarExecuting(command, interceptionContext);
            _stopwatch.Restart();
        }

        public override void ScalarExecuted(System.Data.Common.DbCommand command, DbCommandInterceptionContext<object> interceptionContext)
        {
            _stopwatch.Stop();
            if (interceptionContext.Exception != null)
            {
                Trace.TraceError("Exception:{1} \r\n --> Error executing command: {0}", command.CommandText, interceptionContext.Exception.ToString());
            }
            else
            {
                Trace.TraceInformation("\r\n执行时间:{0} 毫秒\r\n-->ScalarExecuted.Command:{1}\r\n", _stopwatch.ElapsedMilliseconds, command.CommandText);
            }
            base.ScalarExecuted(command, interceptionContext);
        }

        public override void NonQueryExecuting(System.Data.Common.DbCommand command, DbCommandInterceptionContext<int> interceptionContext)
        {
            base.NonQueryExecuting(command, interceptionContext);
            _stopwatch.Restart();
        }

        public override void NonQueryExecuted(System.Data.Common.DbCommand command, DbCommandInterceptionContext<int> interceptionContext)
        {
            _stopwatch.Stop();
            if (interceptionContext.Exception != null)
            {
                Trace.TraceError("Exception:{1} \r\n --> Error executing command:\r\n {0}", command.CommandText, interceptionContext.Exception.ToString());
            }
            else
            {
                Trace.TraceInformation("\r\n执行时间:{0} 毫秒\r\n-->NonQueryExecuted.Command:\r\n{1}", _stopwatch.ElapsedMilliseconds, command.CommandText);
            }
            base.NonQueryExecuted(command, interceptionContext);
        }

        public override void ReaderExecuting(System.Data.Common.DbCommand command, DbCommandInterceptionContext<System.Data.Common.DbDataReader> interceptionContext)
        {
            base.ReaderExecuting(command, interceptionContext);
            _stopwatch.Restart();
        }

        public override void ReaderExecuted(System.Data.Common.DbCommand command, DbCommandInterceptionContext<System.Data.Common.DbDataReader> interceptionContext)
        {
            _stopwatch.Stop();
            if (interceptionContext.Exception != null)
            {
                Trace.TraceError("Exception:{1} \r\n --> Error executing command:\r\n {0}", command.CommandText, interceptionContext.Exception.ToString());
            }
            else
            {
                Trace.TraceInformation("\r\n执行时间:{0} 毫秒 \r\n -->ReaderExecuted.Command:\r\n{1}", _stopwatch.ElapsedMilliseconds, command.CommandText);
            }
            base.ReaderExecuted(command, interceptionContext);
        }
    }

    #endregion
}