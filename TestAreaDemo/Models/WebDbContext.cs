using DataContext;
using DataContext.Extensions;
using Microsoft.AspNet.Identity.EntityFramework;
using System;
using System.Collections.Generic;
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
using System.Web;

namespace TestAreaDemo.Models
{
    public interface IWebDbContext : IDataContext
    {
        Guid GetInstence();
    }

    public class WebDbContext : DataContextAppUser, IWebDbContext//IdentityDbContext<ApplicationUser>
    {
        public WebDbContext()
            : base("DefaultConnection")
        {
            string ConfigName = "IsWriteDataToLunece";
            string IsWriteDataToLuneceStr = System.Configuration.ConfigurationManager.AppSettings[ConfigName] ?? "";
            IsWriteDataToLunece = Common.ChangStrToBool(IsWriteDataToLuneceStr);

            ConfigName = "IsWriteDataToRedis";
            string IsWriteDataToRedisStr = System.Configuration.ConfigurationManager.AppSettings["IsWriteDataToRedis"] ?? "";
            bool IsWriteDataToRedis = Common.ChangStrToBool(IsWriteDataToRedisStr);
            //Configuration.LazyLoadingEnabled = false;
            //Configuration.ProxyCreationEnabled = false;
        }

        public Guid GetInstence()
        {
            return this.InstanceId;
        }

        public static WebDbContext Create()
        {
            return new WebDbContext();
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
        /// 模型创建时
        /// </summary>
        /// <param name="modelBuilder"></param>
        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            //Oracle 表所有者，（SQL 改成 dbo(默认)，也可删除此设置）
            modelBuilder.HasDefaultSchema(DbSchema);
            //将数据列转换成大写
            modelBuilder.Properties().Configure(x => x.HasColumnName(StringUtil.GetColumnName(x.ClrPropertyInfo)));
            //将TableName转大写,TableName 指定的除外
            modelBuilder.Types().Configure(c => c.ToTable(StringUtil.GetTableName(c.ClrType)));
            //设置联合主键
            //modelBuilder.Entity<ModelName>().HasKey(t => new { t.VERDORCODE, t.MFLAG });

            #region AirOut 特殊数据格式设置

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
            Database.SetInitializer<WebDbContext>(null);

            base.OnModelCreating(modelBuilder);
        }

        public DbSet<Message> Messages { get; set; }
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