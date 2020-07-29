using DataContext.Extensions;
using DataContext.Models;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;
using System.Xml;

namespace DataContext.Extensions
{
    public class Common
    {
        #region 加锁

        //加锁 防止 多人同时 登录时引发 Cache 对象已具有相同键值
        public static object lockCacheHelper = new object();

        //本地日志 线程获取信息锁
        public static object TimerLocalLogLocker = new object();
        //数据库日志 线程获取信息锁
        public static object TimerMessageLgLocker = new object();

        #endregion
    
        /// <summary>
        /// 获取当前命名空间
        /// </summary>
        private static string GetCurrentNamespace()
        {
            string NameSpaceStr = "";
            //取得当前方法命名空间
            NameSpaceStr = System.Reflection.MethodBase.GetCurrentMethod().DeclaringType.Namespace;
            return NameSpaceStr;
        }

        /// <summary>
        /// 获取当前类命名空间
        /// </summary>
        private static string GetCurrentNamespace_ClassName()
        {
            string NameSpace_ClassName = "";
            //取得当前方法类全名
            NameSpace_ClassName = System.Reflection.MethodBase.GetCurrentMethod().DeclaringType.FullName;
            return NameSpace_ClassName;
        }

        //当前程序集
        public static System.Reflection.Assembly Assembly
        {
            get
            {
                return System.Reflection.Assembly.GetExecutingAssembly();
            }
        }

        /// <summary>
        /// 存储Cache所有名称
        /// </summary>
        public enum CacheNameS
        {
            //Start 特殊缓存---------------------------
            [Display(Name = "dbContextMember", Description = "dbContext类的所有表")]
            dbContextMember,
            [Display(Name = "AllEntityAssembly", Description = "网站所有反射类")]
            AllEntityAssembly,
            [Display(Name = "SetDefaults", Description = "所有设置的默认值")]
            SetDefaults,
            [Display(Name = "ApplicationRole", Description = "所有角色")]
            ApplicationRole,
            [Display(Name = "ApplicationUser", Description = "所有账户")]
            ApplicationUser,
            [Display(Name = "LinqEnumerableMethods", Description = "Linq-Enumerable类反射的所有方法")]
            LinqEnumerableMethods,
            [Display(Name = "IListMethods", Description = "IList类反射的所有方法")]
            IListMethods,
            [Display(Name = "ExpressionMethods", Description = "Expression表达树的所有方法")]
            ExpressionMethods,
            [Display(Name = "AsyncWriteLog", Description = "异步写日志")]
            AsyncWriteLog,
            //------------------------------end 特殊缓存

        }

        #region 枚举操作

        /// <summary>
        /// 获取Session枚举值
        /// </summary>
        /// <param name="EnumValName">枚举键</param>
        /// <param name="enumName">枚举名称</param>
        /// <returns></returns>
        public static object GeSessionEnumByName(string FieldStr)
        {
            object EnumVal = null;
            EnumVal = GetEnumByName(FieldStr, "SessionNameS");
            return EnumVal;
        }

        /// <summary>
        /// 获取缓存枚举值
        /// </summary>
        /// <param name="EnumValName">枚举键</param>
        /// <param name="enumName">枚举名称</param>
        /// <returns></returns>
        public static object GeCacheEnumByName(string FieldStr)
        {
            object EnumVal = null;
            EnumVal = GetEnumByName(FieldStr, "CacheNameS");
            return EnumVal;
        }

        /// <summary>
        /// 获取枚举值
        /// </summary>
        /// <param name="EnumValName">枚举键</param>
        /// <param name="enumName">枚举名称</param>
        /// <returns></returns>
        public static object GetEnumByName(string FieldStr, string enumName)
        {
            object EnumVal = null;
            try
            {
                Assembly assem = Assembly.GetExecutingAssembly();
                Type type = assem.GetType(GetCurrentNamespace_ClassName() + "+" + enumName);

                try
                {
                    var fields = type.GetFields(BindingFlags.Static | BindingFlags.Public);
                    foreach (var fi in fields)
                    {
                        if (fi.Name == FieldStr)
                        {
                            DisplayAttribute attr;
                            attr = (DisplayAttribute)fi.GetCustomAttributes(typeof(DisplayAttribute), true).SingleOrDefault();

                            if (attr == null)
                            {
                                EnumVal = (object)FieldStr;
                            }
                            else
                                EnumVal = (object)((attr != null) ? attr.GetName() : String.Empty);
                            break;
                        }
                    }
                }
                catch
                {
                    EnumVal = null;
                }
            }
            catch
            {
                EnumVal = null;
            }
            return EnumVal;
        }

        /// <summary>
        /// Enum 转换成 字典类型
        /// </summary>
        /// <param name="enumName">枚举名称</param>
        /// <returns></returns>
        public static List<EnumModelType> GetEnumToDic(string enumName, string namespaseStr = "", string JoinCalc = "+")
        {
            List<EnumModelType> ArrEnumMember = new List<EnumModelType>();
            try
            {
                Assembly assem = Assembly.GetExecutingAssembly();
                namespaseStr = string.IsNullOrEmpty(namespaseStr) ? GetCurrentNamespace_ClassName() : namespaseStr;
                string CacheEnumName = namespaseStr + JoinCalc + enumName;
                var Cache_Obj = HttpRuntime.Cache[CacheEnumName];
                if (Cache_Obj != null)
                {
                    return (List<EnumModelType>)Cache_Obj;
                }
                Type type = assem.GetType(CacheEnumName);
                foreach (string FieldStr in Enum.GetNames(type))
                {
                    EnumModelType o = new EnumModelType();
                    string NameStr = "";
                    string DescptStr = "";
                    var field = type.GetField(FieldStr);
                    if (field != null)
                    {
                        if (field.Name == FieldStr)
                        {
                            DisplayAttribute attr;
                            attr = (DisplayAttribute)field.GetCustomAttributes(typeof(DisplayAttribute), true).SingleOrDefault();

                            if (attr != null)
                            {
                                NameStr = (attr != null) ? attr.GetName() : String.Empty;
                                DescptStr = (attr != null) ? attr.GetDescription() : String.Empty;
                            }
                        }
                    }
                    o.Key = FieldStr;
                    o.Value = (int)Enum.Parse(type, FieldStr);
                    o.DisplayName = NameStr;
                    o.DisplayDescription = DescptStr;

                    if (!string.IsNullOrEmpty(FieldStr))
                    {
                        ArrEnumMember.Add(o);
                    }
                }
                //设置缓存
                HttpRuntime.Cache.Insert(CacheEnumName, ArrEnumMember);
            }
            catch
            {
                ArrEnumMember = new List<EnumModelType>();
            }
            return ArrEnumMember;
        }

        /// <summary>
        /// 获取枚举
        /// </summary>
        /// <typeparam name="T">枚举</typeparam>
        /// <param name="enumVal">枚举名称/值</param>
        /// <returns></returns>
        public static T GetEnumVal<T>(string enumVal) where T : struct, IConvertible
        {
            int enumIntVal;
            if (!typeof(T).IsEnum)
            {
                throw new ArgumentException("T must be an enumerated type");
            }

            if (int.TryParse(enumVal, out enumIntVal))
            {
                T foo = (T)Enum.ToObject(typeof(T), enumIntVal);
                return foo;
            }
            else
            {
                T foo = (T)Enum.Parse(typeof(T), enumVal);
                // the foo.ToString().Contains(",") check is necessary for enumerations marked with an [Flags] attribute
                if (!Enum.IsDefined(typeof(T), foo) && !foo.ToString().Contains(","))
                    throw new InvalidOperationException(enumVal + " is not an underlying value of the YourEnum enumeration.");
                return foo;
            }
        }

        /// <summary>
        /// 根据枚举 键获取值
        /// </summary>
        /// <param name="enumName">枚举名称</param>
        /// <param name="ColumnName"></param>
        /// <returns></returns>
        public static int GetEnumVal(string enumName, string ColumnName)
        {
            Assembly assem = Assembly.GetExecutingAssembly();
            Type type = assem.GetType(GetCurrentNamespace_ClassName() + "+" + enumName);
            return (int)Enum.Parse(type, ColumnName);
        }

        /// <summary>
        /// 获取枚举描述
        /// </summary>
        /// <param name="EnumObj"></param>
        /// <returns></returns>
        public static string GetEnumDisplay(object EnumObj)
        {
            var type = EnumObj.GetType();
            if (type.IsEnum)
            {
                var EName = Enum.GetName(type, EnumObj);
                foreach (var FieldStr in Enum.GetNames(type))
                {
                    if (FieldStr == EName)
                    {
                        string NameStr = "";
                        string DescptStr = "";
                        var field = type.GetField(FieldStr);
                        if (field != null)
                        {
                            if (field.Name == FieldStr)
                            {
                                DisplayAttribute attr;
                                attr = (DisplayAttribute)field.GetCustomAttributes(typeof(DisplayAttribute), true).SingleOrDefault();

                                if (attr != null)
                                {
                                    NameStr = (attr != null) ? attr.GetName() : String.Empty;
                                    DescptStr = (attr != null) ? attr.GetDescription() : String.Empty;
                                }
                            }
                        }
                        return DescptStr;
                    }
                }
                return "";
            }
            else
                return "";
        }

        #endregion

        /// <summary>
        /// 获取 错误信息
        /// </summary>
        /// <param name="ex"></param>
        /// <returns></returns>
        public static string GetExceptionMsg(Exception ex)
        {
            if (ex is System.Data.SqlClient.SqlException)
            {
                var e = ex as System.Data.SqlClient.SqlException;
                return ex.InnerException == null ? ex.Message : (ex.InnerException.InnerException == null ? ex.Message : ex.InnerException.InnerException.Message);
            }
            else if (ex is Oracle.ManagedDataAccess.Client.OracleException)
            {
                var e = ex as Oracle.ManagedDataAccess.Client.OracleException;
                return ex.InnerException == null ? ex.Message : (ex.InnerException.InnerException == null ? ex.Message : ex.InnerException.InnerException.Message);
            }
            else if (ex is System.Data.Entity.Infrastructure.DbUpdateException)
            {
                var e = ex as System.Data.Entity.Infrastructure.DbUpdateException;
                return ex.InnerException == null ? ex.Message : (ex.InnerException.InnerException == null ? ex.Message : ex.InnerException.InnerException.Message);
            }
            else if (ex is System.Data.Entity.Validation.DbEntityValidationException)
            {
                var e = ex as System.Data.Entity.Validation.DbEntityValidationException;
                var ValidaErr = e.EntityValidationErrors;
                if (ValidaErr.Any())
                {
                    List<string> ArrEntityErr = new List<string>();
                    foreach (var item in ValidaErr)
                    {
                        if (item.ValidationErrors.Any())
                        {
                            foreach (var itemErrMsg in item.ValidationErrors)
                            {
                                string EntityFullName = item.Entry.Entity.GetType().ToString();
                                string EntityName = EntityFullName.Substring(EntityFullName.LastIndexOf('.') <= 0 ? 0 : EntityFullName.LastIndexOf('.') + 1);
                                ArrEntityErr.Add(EntityName + ":" + itemErrMsg.ErrorMessage);
                            }
                        }
                    }
                    return string.Join("<br/>", ArrEntityErr.Take(10)) + (ArrEntityErr.Count() > 10 ? "....." : "");
                }
                else
                    return ex.Message;
            }
            else
            {
                string ErrMsg = ex.Message;
                Exception e_x = ex.InnerException;
                while (e_x != null)
                {
                    try
                    {
                        if (!string.IsNullOrEmpty(e_x.Message))
                        {
                            ErrMsg = e_x.Message;
                        }
                        e_x = e_x.InnerException;
                    }
                    catch (Exception e)
                    {
                        if (string.IsNullOrEmpty(ErrMsg))
                            ErrMsg = string.IsNullOrEmpty(e.Message) ? "未知错误" : e.Message;
                        break;
                    }
                }
                return ErrMsg;
            }
        }

        /// <summary>
        /// String转换成Bool
        /// 如果是Int >0 为True
        /// 如果是String =true 为True
        /// </summary>
        /// <param name="Str"></param>
        /// <returns></returns>
        public static bool ChangStrToBool(string Str)
        {
            bool IsTrue = false;
            int Num = 0;
            if (int.TryParse(Str, out Num))
            {
                if (Num >= 1)
                    IsTrue = true;
            }
            else
            {
                if (Str.ToLower() == "true")
                    IsTrue = true;
            }
            return IsTrue;
        }

        #region 自动设置 两个类的 字段相同的数据

        /// <summary>
        /// 自动设置属性相同的值
        /// </summary>
        /// <typeparam name="TClass">要获取值的类</typeparam>
        /// <typeparam name="T">要添加的类</typeparam>
        /// <param name="objModel">要获取值的类数据</param>
        /// <param name="OldObjModel">要获取值的类未修改时的数据</param>
        /// <param name="_unitOfWork"></param>
        /// <param name="AutoInsert">是否自动添加</param>
        /// <returns></returns>
        public static T AutosetSameProtity<TClass, T>(object objModel, object OldObjModel, bool AutoInsert, bool IngoreFieldCase = false) where T : class, new()
        {
            //实例化 需要新增的类
            T TobjModel = new T();
            //try
            //{
            if (objModel != null)
            {
                #region 赋值相同项

                System.Reflection.PropertyInfo[] TClass_PropertyInfos = objModel == null ? new System.Reflection.PropertyInfo[] { } : objModel.GetType().GetProperties(System.Reflection.BindingFlags.Instance |
                            System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic);
                System.Reflection.PropertyInfo[] OldTClass_PropertyInfos = OldObjModel == null ? new System.Reflection.PropertyInfo[] { } : OldObjModel.GetType().GetProperties(System.Reflection.BindingFlags.Instance |
                            System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic);
                System.Reflection.PropertyInfo[] Change_PropertyInfos = TobjModel == null ? new System.Reflection.PropertyInfo[] { } : TobjModel.GetType().GetProperties(System.Reflection.BindingFlags.Instance |
                    System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic);

                var _ChangeProtitys = Change_PropertyInfos.Where(x => x.Name.EndsWith("_Change"));

                //遍历该model实体的所有字段
                foreach (System.Reflection.PropertyInfo fi in TClass_PropertyInfos)
                {
                    string DataType = fi.PropertyType.Name;

                    //泛型
                    if (fi.PropertyType.IsGenericType && fi.PropertyType.GetGenericTypeDefinition() == typeof(Nullable<>))
                    {
                        var Arguments = fi.PropertyType.GetGenericArguments();
                        if (Arguments.Count() == 1)
                        {
                            Type ChildType = Arguments[0];
                            DataType = Arguments[0].Name;
                            if (ChildType != null)
                            {
                                if (ChildType == typeof(DateTime) || ChildType == typeof(int) || ChildType == typeof(decimal) ||
                                    ChildType == typeof(double) || ChildType == typeof(float) || ChildType == typeof(bool))
                                {
                                    var Changefi_s = Change_PropertyInfos.Where(x => IngoreFieldCase ? (x.Name.ToUpper() == fi.Name.ToUpper()) : x.Name == fi.Name);// && x.PropertyType == fi.PropertyType
                                    if (Changefi_s.Any())
                                    {
                                        var Changefi = Changefi_s.First();
                                        object objval = fi.GetValue(objModel);
                                        //Changefi.SetValue(TobjModel, objval);
                                        setProtityValue(TobjModel, Changefi, objval);
                                    }
                                }
                            }
                        }
                    }
                    ////判断是否派生自IEnumerable
                    //else if (fi.PropertyType.GetInterface("IEnumerable", false) != null && DataType.ToLower().IndexOf("string") < 0)
                    //{
                    //}
                    else
                    {
                        var Changefi_s = Change_PropertyInfos.Where(x => IngoreFieldCase ? (x.Name.ToUpper() == fi.Name.ToUpper()) : x.Name == fi.Name);// && x.PropertyType == fi.PropertyType
                        if (Changefi_s.Any())
                        {
                            //带_Change的为变更后数据
                            var Where_ChangeProtitys = _ChangeProtitys.Where(x => IngoreFieldCase ? (x.Name.ToUpper() == (fi.Name + "_Change").ToUpper()) : x.Name == (fi.Name + "_Change"));
                            if (Where_ChangeProtitys.Any())
                            {
                                var Changefi = Where_ChangeProtitys.First();
                                Changefi.SetValue(TobjModel, fi.GetValue(objModel));

                                var WhereOldTClass_PropertyInfos = OldTClass_PropertyInfos.Where(x => x.Name == fi.Name);
                                if (WhereOldTClass_PropertyInfos.Any())
                                {
                                    Changefi = Changefi_s.First();
                                    object objval = WhereOldTClass_PropertyInfos.FirstOrDefault().GetValue(OldObjModel);
                                    //Changefi.SetValue(TobjModel, objval);
                                    setProtityValue(TobjModel, Changefi, objval);
                                }
                            }
                            else
                            {
                                var Changefi = Changefi_s.First();
                                object objval = fi.GetValue(objModel);
                                //Changefi.SetValue(TobjModel, objval);
                                setProtityValue(TobjModel, Changefi, objval);
                            }
                        }
                    }
                }

                #endregion

                if (AutoInsert)
                {
                    #region 自动新增

                    ////类的Type
                    //Type type = typeof(T);
                    ////UnitOfWork类的Type
                    //Type UnitOfWorkType = _unitOfWork.GetType();
                    ////获取UnitOfWork类的Repository泛型方法
                    //MethodInfo Method = UnitOfWorkType.GetMethod("Repository");
                    ////为Repository泛型方法 添加泛型类
                    //Method = Method.MakeGenericMethod(type);
                    ////类的IRepository对象
                    //var Rep = Method.Invoke(_unitOfWork, null);
                    //Type RepType = Rep.GetType();
                    ////类的IRepository对象的所有方法
                    //MethodInfo[] Methods = RepType.GetMethods();
                    //MethodInfo InsertMethod = Methods.Where(x => x.Name == "Insert").FirstOrDefault();
                    //if (InsertMethod != null)
                    //{
                    //    #region 判断必填项 是否已填写

                    //    foreach (System.Reflection.PropertyInfo Changefi in Change_PropertyInfos)
                    //    {
                    //        //属性数据类型
                    //        string DataType = Changefi.PropertyType.Name;
                    //        if (GetAttributeRequired(Changefi) || GetMetaRequired(Changefi))
                    //        {
                    //            //属性值
                    //            var valobj = Changefi.GetValue(TobjModel);
                    //            if (valobj == null)
                    //            {
                    //                if (Changefi.Name == "ID")
                    //                {
                    //                    Changefi.SetValue(TobjModel, 0);
                    //                }
                    //                if (Changefi.Name == "EMS_ORG_Type")
                    //                {
                    //                    var EMS_ORG_Type = typeof(TClass).ToString();
                    //                    var index = 0;
                    //                    index = EMS_ORG_Type.LastIndexOf('.');
                    //                    Changefi.SetValue(TobjModel, index > 0 ? EMS_ORG_Type.Substring(index + 1) : EMS_ORG_Type);
                    //                }
                    //            }
                    //            else
                    //            {
                    //                if (valobj.ToString() == "")
                    //                {
                    //                    if (Changefi.Name == "ID")
                    //                    {
                    //                        Changefi.SetValue(TobjModel, 0);
                    //                    }
                    //                    if (Changefi.Name == "EMS_ORG_Type")
                    //                    {
                    //                        var EMS_ORG_Type = typeof(TClass).ToString();
                    //                        var index = 0;
                    //                        index = EMS_ORG_Type.LastIndexOf('.');
                    //                        Changefi.SetValue(TobjModel, index > 0 ? EMS_ORG_Type.Substring(index + 1) : EMS_ORG_Type);
                    //                    }
                    //                }
                    //            }
                    //        }

                    //        //去除编辑状态字段值
                    //        if (SetEditPropertyNames.Contains(Changefi.Name))
                    //        {
                    //            if (DataType.IndexOf("String") >= 0)
                    //                Changefi.SetValue(TobjModel, "");
                    //            if (DataType.IndexOf("DateTime") >= 0)
                    //                Changefi.SetValue(TobjModel, null);
                    //        }
                    //    }

                    //    #endregion

                    //    List<object> ArrParam = new List<object>() { TobjModel };
                    //    InsertMethod.Invoke(Rep, ArrParam.ToArray());
                    //}

                    #endregion
                }
            }
            //}
            //catch(Exception ex)
            //{

            //}
            return TobjModel;
        }

        /// <summary>
        /// 自动设置属性相同的值
        /// </summary>
        /// <typeparam name="TSet">设置的Model类型</typeparam>
        /// <typeparam name="TGet">读取的Model类型</typeparam>
        /// <param name="SetobjModel">设置的Model</param>
        /// <param name="GetobjModel">读取的Model</param>
        /// <param name="IngoreFieldCase">不区字段分大小写</param>
        /// <param name="Set_PropertyInfos">设置的Model字段属性</param>
        /// <param name="Get_PropertyInfos">读取的Model字段属性</param>
        /// <param name="IngorNullGetVal">忽略空值</param>
        /// <returns></returns>
        public static TSet SetSamaProtity<TSet, TGet>(TSet SetobjModel, TGet GetobjModel, bool IngoreFieldCase = false,
            System.Reflection.PropertyInfo[] Set_PropertyInfos = null, System.Reflection.PropertyInfo[] Get_PropertyInfos = null,
            bool IngorNullGetVal = false)
            where TSet : class, new()
            where TGet : class, new()
        {
            #region 赋值相同项


            if (SetobjModel == null)
                SetobjModel = new TSet();

            if (Set_PropertyInfos == null)
                Set_PropertyInfos = SetobjModel == null ? new System.Reflection.PropertyInfo[] { } : SetobjModel.GetType().GetProperties(System.Reflection.BindingFlags.Instance |
                            System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic);

            if (Get_PropertyInfos == null)
                Get_PropertyInfos = GetobjModel == null ? new System.Reflection.PropertyInfo[] { } : GetobjModel.GetType().GetProperties(System.Reflection.BindingFlags.Instance |
                            System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic);

            //遍历该model实体的所有字段
            foreach (System.Reflection.PropertyInfo fi in Set_PropertyInfos)
            {
                string DataType = fi.PropertyType.Name;
                //泛型
                if (fi.PropertyType.IsGenericType && fi.PropertyType.GetGenericTypeDefinition() == typeof(Nullable<>))
                {
                    //实体泛型的类型
                    var Arguments = fi.PropertyType.GetGenericArguments();
                    if (Arguments.Count() == 1)
                    {
                        Type ChildType = Arguments[0];
                        DataType = Arguments[0].Name;
                        if (ChildType != null)
                        {
                            if (ChildType == typeof(DateTime) || ChildType == typeof(int) || ChildType == typeof(decimal) ||
                                ChildType == typeof(double) || ChildType == typeof(float) || ChildType == typeof(bool))
                            {
                                var WhereGetfi_s = Get_PropertyInfos.Where(x => IngoreFieldCase ? (x.Name.ToUpper() == fi.Name.ToUpper()) : x.Name == fi.Name);
                                if (WhereGetfi_s.Any())
                                {
                                    var Getfi = WhereGetfi_s.First();
                                    if (Getfi.CanRead && fi.CanWrite)
                                    {
                                        var GetVal = Getfi.GetValue(GetobjModel);
                                        if (IngorNullGetVal)
                                        {
                                            if (!string.IsNullOrWhiteSpace(GetVal.ToString()))
                                                setProtityValue(SetobjModel, fi, GetVal);
                                        }
                                        else
                                            setProtityValue(SetobjModel, fi, GetVal);
                                    }
                                }
                            }
                        }
                    }
                }
                ////判断是否派生自IEnumerable
                //else if (fi.PropertyType.GetInterface("IEnumerable", false) != null && DataType.ToLower().IndexOf("string") < 0)
                //{
                //}
                else
                {
                    var WhereGetfi_s = Get_PropertyInfos.Where(x => IngoreFieldCase ? (x.Name.ToUpper() == fi.Name.ToUpper()) : x.Name == fi.Name);
                    if (WhereGetfi_s.Any())
                    {
                        var Getfi = WhereGetfi_s.First();
                        if (Getfi.CanRead && fi.CanWrite)
                        {
                            var GetVal = Getfi.GetValue(GetobjModel);
                            if (IngorNullGetVal)
                            {
                                if (!string.IsNullOrWhiteSpace(GetVal.ToString()))
                                    setProtityValue(SetobjModel, fi, GetVal);
                            }
                            else
                                setProtityValue(SetobjModel, fi, GetVal);
                        }
                    }
                }
            }

            #endregion

            return SetobjModel;
        }

        /// <summary>
        /// 设置相同属性值
        /// </summary>
        /// <typeparam name="SetType">要设置的类型 Type</typeparam>
        /// <typeparam name="GetobjModel">获取相同值的 数据</typeparam>
        /// <param name="IngoreFieldCase">是否区分大小写</param>
        /// <returns></returns>
        public static object SetSamaProtity(Type SetType, Object GetobjModel, System.Reflection.Assembly assembly = null, bool IngoreFieldCase = false, System.Reflection.PropertyInfo[] Set_PropertyInfos = null, System.Reflection.PropertyInfo[] Get_PropertyInfos = null)
        {
            if (assembly == null)
                assembly = Assembly;

            #region 赋值相同项

            object SetobjModel = null;
            if (SetType != null)
            {
                SetobjModel = Activator.CreateInstance(SetType);
            }
            else
                return null;

            if (Set_PropertyInfos == null)
                Set_PropertyInfos = SetobjModel == null ? new System.Reflection.PropertyInfo[] { } : SetobjModel.GetType().GetProperties(System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic);
            if (Get_PropertyInfos == null)
                Get_PropertyInfos = new System.Reflection.PropertyInfo[] { };

            Type Get_Type = GetobjModel.GetType();
            bool IsDynamic = false;

            if (Get_Type.FullName.IndexOf("Dynamic") > 0)
            {
                IsDynamic = true;
            }
            else
            {
                if (GetobjModel != null)
                {
                    Get_PropertyInfos = GetobjModel.GetType().GetProperties(System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic);
                }
                else
                {
                    return null;
                }
            }

            if (!Set_PropertyInfos.Any())
            {
                System.Reflection.FieldInfo[] Set_FieldInfos = SetobjModel == null ? new System.Reflection.FieldInfo[] { } : SetobjModel.GetType().GetFields().Where(x => x.MemberType == MemberTypes.Field).ToArray();
                //遍历该model实体的所有字段
                foreach (System.Reflection.FieldInfo fi in Set_FieldInfos)
                {
                    //设置值
                    var SetObjVal = fi.GetValue(SetobjModel);
                    string DataType = fi.FieldType.Name;
                    //获取值
                    object GetObjVal = null;
                    if (IsDynamic)
                    {
                        var WhereGetfi_s = ((IDictionary<string, object>)GetobjModel).Where(x => IngoreFieldCase ? (x.Key.ToUpper() == fi.Name.ToUpper()) : x.Key == fi.Name);
                        if (WhereGetfi_s.Any())
                        {
                            var Getfi = WhereGetfi_s.First();
                            GetObjVal = Getfi.Value;
                        }
                    }
                    else
                    {
                        var WhereGetfi_s = Get_PropertyInfos.Where(x => IngoreFieldCase ? (x.Name.ToUpper() == fi.Name.ToUpper()) : x.Name == fi.Name);
                        if (WhereGetfi_s.Any())
                        {
                            var Getfi = WhereGetfi_s.First();
                            GetObjVal = Getfi.GetValue(GetobjModel);
                        }
                    }
                    if (GetObjVal == null)
                        continue;
                    //泛型
                    if (fi.FieldType.IsGenericType && fi.FieldType.GetGenericTypeDefinition() == typeof(Nullable<>))
                    {
                        //泛型 类型
                        var Arguments = fi.FieldType.GetGenericArguments();
                        if (Arguments.Count() == 1)
                        {
                            Type ChildType = Arguments[0];
                            DataType = Arguments[0].Name;
                            if (ChildType != null)
                            {
                                if (ChildType == typeof(DateTime) || ChildType == typeof(int) || ChildType == typeof(decimal) ||
                                    ChildType == typeof(double) || ChildType == typeof(float) || ChildType == typeof(bool))
                                {
                                    Common.setProtityValue(SetobjModel, fi, GetObjVal);
                                }
                            }
                        }
                    }
                    //判断是否派生自IEnumerable(string 是特殊的数组)
                    else //if (fi.FieldType.GetInterface("IEnumerable", false) != null && DataType.ToLower().IndexOf("string") < 0)
                        if (fi.FieldType.GetInterface("IEnumerable", false) != null &&
                        (fi.FieldType.Name.ToLower().IndexOf("string") < 0 ||
                        (fi.FieldType.Name.ToLower().IndexOf("string") >= 0 &&
                        (fi.FieldType.Name.ToLower().IndexOf("[]") > 0 || fi.FieldType.Name.ToLower().IndexOf("<") > 0))))
                        {
                            var Arrobjval = GetObjVal as System.Collections.IEnumerable;

                            //是List数组还是Array数组
                            bool IsList = true;

                            #region  创建List<T> 实例 并赋值

                            Type ListTType = null;//泛型类
                            var IEnumerableTypes = fi.FieldType.GetGenericArguments();
                            if (IEnumerableTypes.Any())
                            {
                                //List<> 数组
                                ListTType = IEnumerableTypes[0];
                            }
                            else
                            {
                                //数组
                                ListTType = null;//数组类型
                                ListTType = assembly.GetType(fi.FieldType.FullName.Replace("[]", ""));
                                IsList = false;
                            }

                            Type ListType = typeof(List<>);
                            ListType = ListType.MakeGenericType(ListTType);
                            //创建List数组实例
                            var ObjListT = Activator.CreateInstance(ListType);
                            Type argsType = GetObjVal.GetType();
                            //if (argsType.GetInterface("IEnumerable", false) != null && (argsType.Name.ToLower().IndexOf("string") < 0 && argsType.Name.ToLower().IndexOf("[]") < 0 && argsType.Name.ToLower().IndexOf("<") < 0))
                            if (argsType.GetInterface("IEnumerable", false) != null &&
                            (argsType.Name.ToLower().IndexOf("string") < 0 ||
                            (argsType.Name.ToLower().IndexOf("string") >= 0 &&
                            (argsType.Name.ToLower().IndexOf("[]") > 0 || argsType.Name.ToLower().IndexOf("<") > 0))))
                            {
                                MethodInfo AddMethodInfo = ListType.GetMethod("Add");
                                if (AddMethodInfo != null)
                                {
                                    foreach (var item in Arrobjval)
                                    {
                                        var obj = SetSamaProtity(ListTType, item, assembly, IngoreFieldCase);
                                        AddMethodInfo.Invoke(ObjListT, new object[] { obj });
                                    }
                                }

                                if (IsList)
                                {
                                    Common.setProtityValue(SetobjModel, fi, ObjListT);
                                }
                                else
                                {
                                    MethodInfo ToArrayMethodInfo = ListType.GetMethod("ToArray");
                                    if (ToArrayMethodInfo != null)
                                    {
                                        var ArrObj = ToArrayMethodInfo.Invoke(ObjListT, null);
                                        Common.setProtityValue(SetobjModel, fi, ArrObj);
                                    }
                                }
                            }

                            #endregion
                        }
                        //判断是否是 基元类型 string struct datetime decimal 为特殊的 基元类型
                        //基元类型：sbyte / byte / short / ushort /int / uint / long / ulong / char / float / double / bool
                        //if ((fi.FieldType.IsPrimitive || fi.FieldType.IsValueType || fi.FieldType == typeof(string) || fi.FieldType == typeof(decimal) || fi.FieldType == typeof(DateTime)) && fi.FieldType.Name.ToLower().IndexOf("struct") < 0)
                        else if (fi.FieldType.IsClass && !fi.FieldType.IsPrimitive && fi.FieldType.Name.ToLower().IndexOf("string") < 0)
                        {
                            var obj = SetSamaProtity(fi.FieldType, GetObjVal, assembly, IngoreFieldCase);
                            Common.setProtityValue(SetobjModel, fi, obj);
                        }
                        else
                        {
                            Common.setProtityValue(SetobjModel, fi, GetObjVal);
                        }
                }
            }
            else
            {
                //遍历该model实体的所有字段
                foreach (System.Reflection.PropertyInfo fi in Set_PropertyInfos)
                {
                    //设置值
                    var SetObjVal = fi.GetValue(SetobjModel);
                    string DataType = fi.PropertyType.Name;
                    //获取值
                    object GetObjVal = null;
                    if (IsDynamic)
                    {
                        var WhereGetfi_s = ((IDictionary<string, object>)GetobjModel).Where(x => IngoreFieldCase ? (x.Key.ToUpper() == fi.Name.ToUpper()) : x.Key == fi.Name);
                        if (WhereGetfi_s.Any())
                        {
                            var Getfi = WhereGetfi_s.First();
                            GetObjVal = Getfi.Value;
                        }
                    }
                    else
                    {
                        var WhereGetfi_s = Get_PropertyInfos.Where(x => IngoreFieldCase ? (x.Name.ToUpper() == fi.Name.ToUpper()) : x.Name == fi.Name);
                        if (WhereGetfi_s.Any())
                        {
                            var Getfi = WhereGetfi_s.First();
                            GetObjVal = Getfi.GetValue(GetobjModel);
                        }
                    }


                    if (GetObjVal == null)
                        continue;
                    //泛型
                    if (fi.PropertyType.IsGenericType && fi.PropertyType.GetGenericTypeDefinition() == typeof(Nullable<>))
                    {
                        //泛型 类型
                        var Arguments = fi.PropertyType.GetGenericArguments();
                        if (Arguments.Count() == 1)
                        {
                            Type ChildType = Arguments[0];
                            DataType = Arguments[0].Name;
                            if (ChildType != null)
                            {
                                if (ChildType == typeof(DateTime) || ChildType == typeof(int) || ChildType == typeof(decimal) ||
                                    ChildType == typeof(double) || ChildType == typeof(float) || ChildType == typeof(bool))
                                {
                                    Common.setProtityValue(SetobjModel, fi, GetObjVal);
                                }
                            }
                        }
                    }
                    //判断是否派生自IEnumerable(string 是特殊的数组)
                    else //if (fi.PropertyType.GetInterface("IEnumerable", false) != null && DataType.ToLower().IndexOf("string") < 0)
                        if (fi.PropertyType.GetInterface("IEnumerable", false) != null &&
                        (fi.PropertyType.Name.ToLower().IndexOf("string") < 0 ||
                        (fi.PropertyType.Name.ToLower().IndexOf("string") >= 0 &&
                        (fi.PropertyType.Name.ToLower().IndexOf("[]") > 0 || fi.PropertyType.Name.ToLower().IndexOf("<") > 0))))
                        {
                            var Arrobjval = GetObjVal as System.Collections.IEnumerable;

                            //是List数组还是Array数组
                            bool IsList = true;

                            #region  创建List<T> 实例 并赋值

                            Type ListTType = null;//泛型类
                            var IEnumerableTypes = fi.PropertyType.GetGenericArguments();
                            if (IEnumerableTypes.Any())
                            {
                                //List<> 数组
                                ListTType = IEnumerableTypes[0];
                            }
                            else
                            {
                                //数组
                                ListTType = null;//数组类型
                                ListTType = assembly.GetType(fi.PropertyType.FullName.Replace("[]", ""));
                                IsList = false;
                            }

                            Type ListType = typeof(List<>);
                            ListType = ListType.MakeGenericType(ListTType);
                            //创建List数组实例
                            var ObjListT = Activator.CreateInstance(ListType);
                            Type argsType = GetObjVal.GetType();
                            //if (argsType.GetInterface("IEnumerable", false) != null && (argsType.Name.ToLower().IndexOf("string") < 0 && argsType.Name.ToLower().IndexOf("[]") < 0 && argsType.Name.ToLower().IndexOf("<") < 0))
                            if (argsType.GetInterface("IEnumerable", false) != null &&
                            (argsType.Name.ToLower().IndexOf("string") < 0 ||
                            (argsType.Name.ToLower().IndexOf("string") >= 0 &&
                            (argsType.Name.ToLower().IndexOf("[]") > 0 || argsType.Name.ToLower().IndexOf("<") > 0))))
                            {
                                MethodInfo AddMethodInfo = ListType.GetMethod("Add");
                                if (AddMethodInfo != null)
                                {
                                    foreach (var item in Arrobjval)
                                    {
                                        var obj = SetSamaProtity(ListTType, item, assembly, IngoreFieldCase);
                                        AddMethodInfo.Invoke(ObjListT, new object[] { obj });
                                    }
                                }
                                if (IsList)
                                {
                                    Common.setProtityValue(SetobjModel, fi, ObjListT);
                                }
                                else
                                {
                                    MethodInfo ToArrayMethodInfo = ListType.GetMethod("ToArray");
                                    if (ToArrayMethodInfo != null)
                                    {
                                        var ArrObj = ToArrayMethodInfo.Invoke(ObjListT, null);
                                        Common.setProtityValue(SetobjModel, fi, ArrObj);
                                    }
                                }
                            }

                            #endregion
                        }
                        //判断是否是 基元类型 string struct datetime decimal 为特殊的 基元类型
                        //基元类型：sbyte / byte / short / ushort /int / uint / long / ulong / char / float / double / bool
                        else if (fi.PropertyType.IsClass && !fi.PropertyType.IsPrimitive && fi.PropertyType.Name.ToLower().IndexOf("string") < 0)
                        {
                            var obj = SetSamaProtity(fi.PropertyType, GetObjVal, assembly, IngoreFieldCase);
                            Common.setProtityValue(SetobjModel, fi, obj);
                        }
                        else
                        {
                            Common.setProtityValue(SetobjModel, fi, GetObjVal);
                        }
                }
            }

            #endregion

            return SetobjModel;
        }

        /// <summary>
        /// 两个结构相同的类进行赋值
        /// </summary>
        /// <typeparam name="T">赋值类</typeparam>
        /// <typeparam name="L">被赋值类</typeparam>
        /// <param name="t">赋值类的数据</param>
        /// <param name="Exclude">排除的字段</param>
        /// <returns></returns>
        public static L SetProperties<T, L>(T t, string Exclude) where L : new()
        {
            if (t == null)
            {
                return default(L);
            }
            if (string.IsNullOrEmpty(Exclude))
            {
                return default(L);
            }
            var _entity = Exclude.Split(',');

            System.Reflection.PropertyInfo[] propertiesT = typeof(T).GetProperties(System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public);
            System.Reflection.PropertyInfo[] propertiesL = typeof(L).GetProperties(System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public);
            if (propertiesT.Length != propertiesL.Length || propertiesL.Length == 0)
            {
                return default(L);
            }
            L setT = new L();
            foreach (System.Reflection.PropertyInfo itemL in propertiesL)
            {
                if (_entity.Contains(itemL.Name))
                {
                    continue;
                }
                foreach (System.Reflection.PropertyInfo itemT in propertiesT)
                {
                    if (itemL.Name == itemT.Name)
                    {
                        object value = itemT.GetValue(t, null);
                        itemL.SetValue(setT, value, null);
                    }
                }
            }
            return setT;
        }

        #endregion

        /// <summary>
        /// 根据 类 和 字段 设置值
        /// </summary>
        /// <param name="TableClass"></param>
        /// <param name="FiledName"></param>
        /// <param name="DefaultValue"></param>
        public static void setProtityValue(Object TableClass = null, string FiledName = "", object DefaultValue = null)
        {
            try
            {
                if (TableClass == null || string.IsNullOrEmpty(FiledName))
                    return;

                System.Reflection.PropertyInfo[] PropertyInfos = TableClass.GetType().GetProperties(System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic);
                //遍历该model实体的所有字段
                foreach (System.Reflection.PropertyInfo fi in PropertyInfos)
                {
                    //获取字段名，用于查找该字段对应的display数据，来源List<ColumValue>
                    String _FiledName = fi.Name;
                    object s = fi.GetValue(TableClass, null);
                    string DataType = "";
                    if (fi.Name.ToLower() == FiledName.ToLower())
                    {
                        DataType = fi.PropertyType.Name;
                        //如果是 泛型 decimal?、List<T> 等等
                        if (fi.PropertyType.IsGenericType)
                        {
                            //如果是decimal?等泛型
                            if (fi.PropertyType.GetGenericTypeDefinition() == typeof(Nullable<>))
                            {
                                var Arguments = fi.PropertyType.GetGenericArguments();
                                if (Arguments.Any())
                                {
                                    if (Arguments.Count() == 1)
                                    {
                                        DataType = Arguments[0].Name;
                                    }
                                }
                            }
                        }
                        switch (DataType.ToLower())
                        {
                            case "int":
                                int Dftint = 0;
                                if (int.TryParse(DefaultValue.ToString(), out Dftint))
                                {
                                    fi.SetValue(TableClass, Dftint, null);
                                }
                                break;
                            case "int32":
                                int Dftint32 = 0;
                                if (int.TryParse(DefaultValue.ToString(), out Dftint32))
                                {
                                    fi.SetValue(TableClass, Dftint32, null);
                                }
                                break;
                            case "int64":
                                Int64 Dftint64 = 0;
                                if (Int64.TryParse(DefaultValue.ToString(), out Dftint64))
                                {
                                    fi.SetValue(TableClass, Dftint64, null);
                                }
                                break;
                            case "decimal":
                                decimal Dftdecimal = 0;
                                if (decimal.TryParse(DefaultValue.ToString(), out Dftdecimal))
                                {
                                    fi.SetValue(TableClass, Dftdecimal, null);
                                }
                                break;
                            case "double":
                                double Dftdouble = 0;
                                if (double.TryParse(DefaultValue.ToString(), out Dftdouble))
                                {
                                    fi.SetValue(TableClass, Dftdouble, null);
                                }
                                break;
                            case "float":
                                float Dftfloat = 0;
                                if (float.TryParse(DefaultValue.ToString(), out Dftfloat))
                                {
                                    fi.SetValue(TableClass, Dftfloat, null);
                                }
                                break;
                            case "string":
                                fi.SetValue(TableClass, DefaultValue.ToString(), null);
                                break;
                            case "datetime":
                                int TDatetime = 0;
                                if (int.TryParse(DefaultValue.ToString(), out TDatetime))
                                {
                                    fi.SetValue(TableClass, DateTime.Now.AddDays(TDatetime), null);
                                }
                                else
                                {
                                    DateTime DftDateTime = new DateTime();
                                    if (DateTime.TryParse(DefaultValue.ToString(), out DftDateTime))
                                    {
                                        fi.SetValue(TableClass, DftDateTime, null);
                                    }
                                }
                                break;
                            case "bool":
                                bool Dftbool = false;
                                if (bool.TryParse(DefaultValue.ToString(), out Dftbool))
                                {
                                    fi.SetValue(TableClass, Dftbool, null);
                                }
                                break;
                            default:
                                fi.SetValue(TableClass, DefaultValue, null);
                                break;
                        }
                        break;
                    }
                }
            }
            catch
            {

            }
        }

        /// <summary>
        /// 根据 类 和 字段 设置值
        /// </summary>
        /// <param name="TableClass"></param>
        /// <param name="fi"></param>
        /// <param name="DefaultValue"></param>
        public static void setProtityValue(Object TableClass = null, PropertyInfo fi = null, object DefaultValue = null)
        {
            try
            {
                if (fi != null)
                {
                    //获取字段名，用于查找该字段对应的display数据，来源List<ColumValue>
                    String _FiledName = fi.Name;
                    object s = fi.GetValue(TableClass, null);
                    string DataType = "";
                    DataType = fi.PropertyType.Name;
                    //如果是 泛型 decimal?、List<T> 等等
                    if (fi.PropertyType.IsGenericType)
                    {
                        //如果是decimal?等泛型
                        if (fi.PropertyType.GetGenericTypeDefinition() == typeof(Nullable<>))
                        {
                            var Arguments = fi.PropertyType.GetGenericArguments();
                            if (Arguments.Any())
                            {
                                if (Arguments.Count() == 1)
                                {
                                    DataType = Arguments[0].Name;
                                }
                            }
                        }
                    }
                    var StrVal = DefaultValue == null ? "" : DefaultValue.ToString();
                    switch (DataType.ToLower())
                    {
                        case "int":
                        case "int32":
                            int Dftint = 0;
                            if (int.TryParse(StrVal, out Dftint))
                            {
                                fi.SetValue(TableClass, Dftint, null);
                            }
                            break;
                        case "long":
                        case "int64":
                            Int64 Dftint64 = 0;
                            if (Int64.TryParse(StrVal, out Dftint64))
                            {
                                fi.SetValue(TableClass, Dftint64, null);
                            }
                            break;
                        case "decimal":
                            decimal Dftdecimal = 0;
                            if (decimal.TryParse(StrVal, out Dftdecimal))
                            {
                                fi.SetValue(TableClass, Dftdecimal, null);
                            }
                            break;
                        case "double":
                            double Dftdouble = 0;
                            if (double.TryParse(StrVal, out Dftdouble))
                            {
                                fi.SetValue(TableClass, Dftdouble, null);
                            }
                            break;
                        case "float":
                            float Dftfloat = 0;
                            if (float.TryParse(StrVal, out Dftfloat))
                            {
                                fi.SetValue(TableClass, Dftfloat, null);
                            }
                            break;
                        case "string":
                            fi.SetValue(TableClass, StrVal, null);
                            break;
                        case "datetime":
                            int TDatetime = 0;
                            if (int.TryParse(StrVal, out TDatetime))
                            {
                                fi.SetValue(TableClass, DateTime.Now.AddDays(TDatetime), null);
                            }
                            else
                            {
                                DateTime DftDateTime = new DateTime();
                                if (DateTime.TryParse(StrVal, out DftDateTime))
                                {
                                    fi.SetValue(TableClass, DftDateTime, null);
                                }
                            }
                            break;
                        case "bool":
                            bool Dftbool = false;
                            if (bool.TryParse(StrVal, out Dftbool))
                            {
                                fi.SetValue(TableClass, Dftbool, null);
                            }
                            break;
                        default:
                            fi.SetValue(TableClass, DefaultValue, null);
                            break;
                    }
                }
            }
            catch
            {

            }
        }

        /// <summary>
        /// 根据 类 和 字段 设置值
        /// </summary>
        /// <param name="TableClass"></param>
        /// <param name="fi"></param>
        /// <param name="DefaultValue"></param>
        public static void setProtityValue(Object TableClass = null, FieldInfo fi = null, object DefaultValue = null)
        {
            try
            {
                if (fi != null)
                {
                    //获取字段名，用于查找该字段对应的display数据，来源List<ColumValue>
                    String _FiledName = fi.Name;
                    object s = fi.GetValue(TableClass);
                    string DataType = "";
                    DataType = fi.FieldType.Name;
                    //如果是 泛型 decimal?、List<T> 等等
                    if (fi.FieldType.IsGenericType)
                    {
                        //如果是decimal?等泛型
                        if (fi.FieldType.GetGenericTypeDefinition() == typeof(Nullable<>))
                        {
                            var Arguments = fi.FieldType.GetGenericArguments();
                            if (Arguments.Any())
                            {
                                if (Arguments.Count() == 1)
                                {
                                    DataType = Arguments[0].Name;
                                }
                            }
                        }
                    }
                    var StrVal = DefaultValue == null ? "" : DefaultValue.ToString();
                    switch (DataType.ToLower())
                    {
                        case "int":
                        case "int32":
                            int Dftint = 0;
                            if (int.TryParse(StrVal, out Dftint))
                            {
                                fi.SetValue(TableClass, Dftint);
                            }
                            break;
                        case "int64":
                        case "long":
                            Int64 Dftint64 = 0;
                            if (Int64.TryParse(StrVal, out Dftint64))
                            {
                                fi.SetValue(TableClass, Dftint64);
                            }
                            break;
                        case "decimal":
                            decimal Dftdecimal = 0;
                            if (decimal.TryParse(StrVal, out Dftdecimal))
                            {
                                fi.SetValue(TableClass, Dftdecimal);
                            }
                            break;
                        case "double":
                            double Dftdouble = 0;
                            if (double.TryParse(StrVal, out Dftdouble))
                            {
                                fi.SetValue(TableClass, Dftdouble);
                            }
                            break;
                        case "float":
                            float Dftfloat = 0;
                            if (float.TryParse(StrVal, out Dftfloat))
                            {
                                fi.SetValue(TableClass, Dftfloat);
                            }
                            break;
                        case "string":
                            fi.SetValue(TableClass, StrVal);
                            break;
                        case "datetime":
                            int TDatetime = 0;
                            if (int.TryParse(StrVal, out TDatetime))
                            {
                                fi.SetValue(TableClass, DateTime.Now.AddDays(TDatetime));
                            }
                            else
                            {
                                DateTime DftDateTime = new DateTime();
                                if (DateTime.TryParse(StrVal, out DftDateTime))
                                {
                                    fi.SetValue(TableClass, DftDateTime);
                                }
                            }
                            break;
                        case "bool":
                            bool Dftbool = false;
                            if (bool.TryParse(StrVal, out Dftbool))
                            {
                                fi.SetValue(TableClass, Dftbool);
                            }
                            break;
                        default:
                            fi.SetValue(TableClass, DefaultValue);
                            break;
                    }
                }
            }
            catch
            {

            }
        }

        /// <summary>
        /// 根据 类 和 字段 获取值
        /// </summary>
        /// <param name="TableClass"></param>
        /// <param name="FiledName"></param>
        /// <param name="IngoreFieldCase">不区分 字段名 大小写</param>
        /// <returns></returns>
        public static object GetProtityValue(Object TableClass = null, string FiledName = "", bool IngoreFieldCase = true)
        {
            try
            {
                object retValue = "";
                Dictionary<string, object> dict = new Dictionary<string, object>();
                if (TableClass == null || string.IsNullOrEmpty(FiledName))
                    return null;

                System.Reflection.PropertyInfo[] PropertyInfos = TableClass.GetType().GetProperties(System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic);
                //遍历该model实体的所有字段
                foreach (System.Reflection.PropertyInfo fi in PropertyInfos)
                {
                    //获取字段名，用于查找该字段对应的display数据，来源List<ColumValue>
                    String _FiledName = fi.Name;
                    object fival = fi.GetValue(TableClass, null);
                    string DataType = "";
                    if (IngoreFieldCase ? fi.Name.ToLower() == FiledName.ToLower() : fi.Name == FiledName)
                    {
                        var retVal = fi.GetValue(TableClass);
                        DataType = fi.PropertyType.Name;
                        //判断是否是泛型
                        if (fi.PropertyType.IsGenericType && fi.PropertyType.GetGenericTypeDefinition() == typeof(Nullable<>))
                        {
                            var Arguments = fi.PropertyType.GetGenericArguments();
                            if (Arguments.Any())
                            {
                                if (Arguments.Count() == 1)
                                {
                                    DataType = Arguments[0].Name;
                                }
                            }
                        }
                        var StrVal = retVal == null ? "" : retVal.ToString();
                        switch (DataType.ToLower())
                        {
                            case "int":
                            case "int32":
                                int Dftint = 0;
                                if (int.TryParse(StrVal, out Dftint))
                                {
                                    retVal = (object)Dftint;
                                    retValue = retVal;
                                }
                                break;
                            case "int64":
                            case "long":
                                Int64 Dftint64 = 0;
                                if (Int64.TryParse(StrVal, out Dftint64))
                                {
                                    retVal = (object)Dftint64;
                                    retValue = retVal;
                                }
                                break;
                            case "string":
                                retValue = retVal;
                                break;
                            case "datetime":
                                int TDatetime = 0;
                                if (int.TryParse(StrVal, out TDatetime))
                                {
                                    retVal = (object)TDatetime;
                                    retValue = retVal;
                                }
                                else
                                {
                                    DateTime DftDateTime = new DateTime();
                                    if (DateTime.TryParse(StrVal, out DftDateTime))
                                    {
                                        retVal = (object)DftDateTime;
                                        retValue = retVal;
                                    }
                                }
                                break;
                            case "bool":
                                bool Dftbool = false;
                                if (bool.TryParse(StrVal, out Dftbool))
                                {
                                    retVal = (object)Dftbool;
                                    retValue = retVal;
                                }
                                break;
                            case "decimal":
                                decimal Dftdecimal = 0;
                                if (decimal.TryParse(StrVal, out Dftdecimal))
                                {
                                    if (FiledName == "Volume_CK" && TableClass.ToString() == "AirOut.Web.Controllers.Warehouse_receiptsController+pdfBF")
                                    {
                                        string str = Dftdecimal.ToString("f3");
                                        retVal = (object)str;
                                        retValue = retVal;
                                    }
                                    else
                                    {
                                        string str = Dftdecimal.ToString("f2");
                                        retVal = (object)str;
                                        retValue = retVal;
                                    }
                                }
                                break;
                            case "double":
                                double Dftdouble = 0;
                                if (double.TryParse(StrVal, out Dftdouble))
                                {
                                    string str = Dftdouble.ToString("f2");
                                    retVal = (object)str;
                                    retValue = retVal;
                                }
                                break;
                            case "float":
                                float Dftfloat = 0;
                                if (float.TryParse(StrVal, out Dftfloat))
                                {
                                    string str = Dftfloat.ToString("f2");
                                    retVal = (object)str;
                                    retValue = retVal;
                                }
                                break;
                            default:
                                retValue = retVal;
                                break;
                        }
                        dict.Add(DataType, fival);
                        break;
                    }
                }
                return retValue;// dict;
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// 根据 类 的 字段的 ProtityInfo
        /// </summary>
        /// <param name="TableClass"></param>
        /// <param name="FiledName"></param>
        /// <param name="IngoreFieldCase">不区分 字段名 大小写</param>
        /// <returns></returns>
        public static PropertyInfo GetProtityInfoByFieldName(Object TableClass = null, string FiledName = "", bool IngoreFieldCase = true)
        {
            try
            {
                PropertyInfo retPropertyInfo = null;
                Dictionary<string, object> dict = new Dictionary<string, object>();

                if (TableClass == null || string.IsNullOrEmpty(FiledName))
                    return null;

                retPropertyInfo = TableClass.GetType().GetProperties(System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic).
                    Where(x => IngoreFieldCase ? x.Name.ToLower() == FiledName.ToLower() : x.Name == FiledName).FirstOrDefault();

                return retPropertyInfo;// dict;
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// 获取属性是否时必须的
        /// </summary>
        /// <param name="property"></param>
        /// <returns></returns>
        public static bool GetAttributeRequired(PropertyInfo property)
        {
            var atts = property.GetCustomAttributes(typeof(System.ComponentModel.DataAnnotations.RequiredAttribute), true);
            if (atts.Length == 0)
                return false;
            return true;
        }

        /// <summary>
        /// 获取属性的MetaType是否是必须的
        /// </summary>
        /// <param name="property"></param>
        /// <returns></returns>
        public static bool GetMetaRequired(PropertyInfo property)
        {
            var atts = property.DeclaringType.GetCustomAttributes(typeof(MetadataTypeAttribute), true);
            if (atts.Length == 0)
                return false;

            var metaAttr = atts[0] as MetadataTypeAttribute;
            var metaProperty = metaAttr.MetadataClassType.GetProperty(property.Name);
            if (metaProperty == null)
                return false;
            return GetAttributeRequired(metaProperty);
        }

        /// <summary>
        /// 反射获取 主键值
        /// </summary>
        /// <returns></returns>
        public static string GetKeyValue(object entity, PropertyInfo[] _entityProptys)
        {
            string KeyVal = "";
            foreach (var propinfo in _entityProptys)
            {
                var ArrKeyAttr = propinfo.GetCustomAttributes(typeof(KeyAttribute), false);
                if (ArrKeyAttr.Any())
                {
                    KeyVal = propinfo.GetValue(entity).ToString();
                    break;
                }
            }
            if (string.IsNullOrEmpty(KeyVal))
            {
                var KeyProptyS = _entityProptys.Where(x => x.Name.ToLower() == "ID");
                if (KeyProptyS.Any())
                {
                    KeyVal = KeyProptyS.FirstOrDefault().GetValue(entity).ToString();
                }
            }
            return KeyVal;
        }

        #region 根据 类，获取类所有字段的DisplayName,MetadataDisplayName

        /// <summary>
        /// 根据 类，获取类所有字段的DisplayName,MetadataDisplayName
        /// </summary>
        /// <typeparam name="T">Model类</typeparam>
        /// <returns></returns>
        public static IList GetAllFieldNameByModel<T>() where T : class, new()
        {
            IList ClassNames = null;
            try
            {
                T Obj_Table = new T();
                if (Obj_Table != null)
                {
                    Type type = Obj_Table.GetType();
                    System.Reflection.PropertyInfo[] PropertyInfos = type.GetProperties(System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic);
                    ClassNames = PropertyInfos.Select(x => new
                    {
                        Name = x.Name,
                        DisplayName = Common.GetDisplayName(type, x.Name),//(x.GetCustomAttributes(typeof(DisplayAttribute), true).Length > 0 ? (x.GetCustomAttributes(typeof(DisplayAttribute), true)[0] as System.ComponentModel.DataAnnotations.DisplayAttribute).Name : ""),
                        MetaDataTypeName = Common.GetMetaDataDisplayName(type, x.Name)
                    }).ToList();
                    //var query = PropertyInfos.Select(x => new
                    //{
                    //    Name = x.Name,
                    //    DisplayName = Common.GetDisplayName(type, x.Name),//(x.GetCustomAttributes(typeof(DisplayAttribute), true).Length > 0 ? (x.GetCustomAttributes(typeof(DisplayAttribute), true)[0] as System.ComponentModel.DataAnnotations.DisplayAttribute).Name : ""),
                    //    MetaDataTypeName = Common.GetMetaDataDisplayName(type, x.Name)
                    //}).ToList();
                    //foreach (var item in query)
                    //{
                    //    dynamic d = new System.Dynamic.ExpandoObject();
                    //    ((IDictionary<string, object>)d).Add(item.Name.ToString(), item.Name);
                    //    ((IDictionary<string, object>)d).Add(item.DisplayName.ToString(), item.DisplayName);
                    //    ((IDictionary<string, object>)d).Add(item.MetaDataTypeName.ToString(), item.MetaDataTypeName);
                    //    ClassNames.Add(d);
                    //}

                    //ConvertObject(query,);
                }
            }
            catch
            {

            }
            return ClassNames;
        }

        /// <summary>
        /// 根据 类，获取类所有字段的DisplayName,MetadataDisplayName
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static DataTable GetAllColumnByTable_Column_dt<T>() where T : class, new()
        {
            DataTable ClassNames = null;
            try
            {
                T Obj_Table = new T();

                if (Obj_Table != null)
                {
                    Type type = Obj_Table.GetType();
                    var Cache_PropertyInfos = CacheHelper.GetCache(type.FullName);
                    System.Reflection.PropertyInfo[] PropertyInfos = new PropertyInfo[] { };
                    if (Cache_PropertyInfos == null)
                    {
                        PropertyInfos = type.GetProperties(System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic);
                        CacheHelper.SetCache(type.FullName, PropertyInfos);
                    }
                    else
                    {
                        PropertyInfos = Cache_PropertyInfos as PropertyInfo[];
                    }
                    ClassNames = PropertyInfos.Select(x => new
                    {
                        x.Name,
                        DisplayName = Common.GetDisplayName(type, x.Name),
                        MetaDataTypeName = Common.GetMetaDataDisplayName(type, x.Name)
                    }).ToDataTable();
                }
            }
            catch
            {

            }
            return ClassNames;
        }

        #endregion

        #region 获取/设置 类的默认值，存储在XML文件中

        //private static DateTime? LastEditTime = null;//最后修改时间
        /// <summary>
        /// 获取费用-计费条件XML
        /// </summary>
        /// <returns></returns>
        public static List<dynamic> GetBillFormulaXML()
        {
            string XMLPath = System.Configuration.ConfigurationManager.AppSettings["BillFormulaXml"] ?? "/App_Data/BillFormula.xml";
            List<dynamic> ArrBillFormula = new List<dynamic>();
            try
            {
                FileInfo FInfo = new FileInfo(HttpContext.Current.Server.MapPath(XMLPath));
                if (FInfo.Exists)
                {
                    //已在CacheHelper中设置文件依赖（文件修改 清空缓存）
                    //if (LastEditTime == null)
                    //    LastEditTime = FInfo.LastWriteTime;
                    //else
                    //{
                    //    var ObjBillFormulaXML = CacheHelper.Get_SetCache(CacheNameS.BillFormulaXML);
                    //    if (ObjBillFormulaXML != null)
                    //    {
                    //        if (LastEditTime >= FInfo.LastWriteTime)
                    //            return (List<dynamic>)ObjBillFormulaXML;
                    //    }
                    //}

                    XmlDocument doc = new XmlDocument();
                    doc.Load(FInfo.FullName);
                    XmlNode xmlTableNode = doc.SelectSingleNode("BillFormula");
                    if (xmlTableNode != null)
                    {
                        if (xmlTableNode.HasChildNodes)
                        {
                            XmlNodeList xmltableNodes = xmlTableNode.SelectNodes("item");
                            if (xmltableNodes != null)
                            {
                                foreach (XmlNode xmlAutoSetNode in xmltableNodes)
                                {
                                    dynamic dyObj = new System.Dynamic.ExpandoObject();
                                    dyObj.ID = dyObj.Name = xmlAutoSetNode.Attributes["Name"].Value;
                                    dyObj.TEXT = dyObj.Desc = xmlAutoSetNode.Attributes["desc"].Value;
                                    dyObj.Display = xmlAutoSetNode.InnerText;
                                    ArrBillFormula.Add(dyObj);
                                }
                            }
                        }
                    }
                }
            }
            catch
            {
                ArrBillFormula = new List<dynamic>();
            }
            return ArrBillFormula;
        }

        /// <summary>
        /// 获取所有设置默认值的Table名称
        /// </summary>
        /// <returns></returns>
        public static List<string> GetAllDefaultTabName()
        {
            string XMLPath = System.Configuration.ConfigurationManager.AppSettings["SetDefaultsXml"] ?? "/App_Data/SetDefaults.xml";
            List<string> ArrSetDefaultTabName = new List<string>();
            try
            {
                XmlDocument doc = new XmlDocument();
                doc.Load(HttpContext.Current.Server.MapPath(XMLPath));
                XmlNode xmlTableNode = doc.SelectSingleNode("WebSettings/DefaultTables");
                if (xmlTableNode != null)
                {
                    if (xmlTableNode.HasChildNodes)
                    {
                        XmlNodeList xmltableNodes = xmlTableNode.SelectNodes("table");
                        if (xmltableNodes != null)
                        {
                            foreach (XmlNode xmlAutoSetNode in xmltableNodes)
                            {
                                var TabName = xmlAutoSetNode.Attributes["name"].Value;
                                var TabNameChs = xmlAutoSetNode.Attributes["value"].Value;
                                ArrSetDefaultTabName.Add(TabName);
                            }
                        }
                    }
                }
            }
            catch
            {
                ArrSetDefaultTabName = new List<string>();
            }
            return ArrSetDefaultTabName;
        }

        /// <summary>
        /// 根据表名 获取表的所有默认值 并返回为 List<SetDefaults>
        /// </summary>
        /// <param name="TableName"></param>
        /// <returns></returns>
        public static List<SetDefaults> getAllSetDefaultsByTable(string TableName = "")
        {
            string XMLPath = System.Configuration.ConfigurationManager.AppSettings["SetDefaultsXml"] ?? "/App_Data/SetDefaults.xml";
            List<SetDefaults> ArrSetDefaults = new List<SetDefaults>();
            SetDefaults OSetDefaults = new SetDefaults();
            if (TableName == "")
            {
                ArrSetDefaults = new List<SetDefaults>();
            }
            else
            {
                try
                {
                    OSetDefaults = null;
                    XmlDocument doc = new XmlDocument();
                    doc.Load(HttpContext.Current.Server.MapPath(XMLPath));
                    XmlNode xmlTableNode = doc.SelectSingleNode("WebSettings/table[@name='" + TableName + "']");
                    if (xmlTableNode != null)
                    {
                        if (xmlTableNode.HasChildNodes)
                        {
                            XmlNodeList xmlAutoSetNodes = xmlTableNode.SelectNodes("AutoSet");
                            if (xmlAutoSetNodes != null)
                            {
                                foreach (XmlNode xmlAutoSetNode in xmlAutoSetNodes)
                                {
                                    OSetDefaults = new SetDefaults();
                                    OSetDefaults.TableName = xmlAutoSetNode.ParentNode == null ? "" : xmlAutoSetNode.ParentNode.Attributes["name"].Value;
                                    OSetDefaults.TableNameChs = xmlAutoSetNode.ParentNode == null ? "" : xmlAutoSetNode.ParentNode.Attributes["value"].Value;
                                    OSetDefaults.ColumnName = xmlAutoSetNode.Attributes["colum"].Value;
                                    OSetDefaults.ColumnNameChs = xmlAutoSetNode.Attributes["name"].Value;
                                    OSetDefaults.DataType = xmlAutoSetNode.Attributes["type"].Value;
                                    OSetDefaults.DefaultValue = xmlAutoSetNode.Attributes["value"].Value;
                                    ArrSetDefaults.Add(OSetDefaults);
                                }
                            }
                        }
                    }
                }
                catch
                {
                    ArrSetDefaults = new List<SetDefaults>();
                }
            }
            return ArrSetDefaults;
        }

        /// <summary>
        /// 为类设置 默认值
        /// </summary>
        /// <param name="TableName"></param>
        /// <param name="TableClass"></param>
        public static void SetDefaultValueToModel(string TableName = "", Object TableClass = null)
        {
            if (TableName == "")
                return;
            if (TableClass == null)
                return;
            List<SetDefaults> list = getAllSetDefaultsByTable(TableName);
            if (!list.Any())
                return;
            System.Reflection.PropertyInfo[] PropertyInfos = TableClass.GetType().GetProperties(System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic);
            //遍历该model实体的所有字段
            foreach (System.Reflection.PropertyInfo fi in PropertyInfos)
            {
                //获取字段名，用于查找该字段对应的display数据，来源List<ColumValue>
                String FiledName = fi.Name;
                object s = fi.GetValue(TableClass, null);
                //获取display属性操作对象
                DisplayAttribute disAttr = (DisplayAttribute)fi.GetCustomAttributes(typeof(DisplayAttribute), true).SingleOrDefault();

                foreach (var item in list)
                {
                    if (item.ColumnName == FiledName)
                    {
                        switch (item.DataType.ToLower())
                        {
                            case "int":
                                int Dftint = 0;
                                if (int.TryParse(item.DefaultValue, out Dftint))
                                {
                                    fi.SetValue(TableClass, Dftint, null);
                                }
                                break;
                            case "int32":
                                int Dftint32 = 0;
                                if (int.TryParse(item.DefaultValue, out Dftint32))
                                {
                                    fi.SetValue(TableClass, Dftint32, null);
                                }
                                break;
                            case "int64":
                                Int64 Dftint64 = 0;
                                if (Int64.TryParse(item.DefaultValue, out Dftint64))
                                {
                                    fi.SetValue(TableClass, Dftint64, null);
                                }
                                break;
                            case "decimal":
                                decimal Dftdecimal = 0;
                                if (decimal.TryParse(item.DefaultValue, out Dftdecimal))
                                {
                                    fi.SetValue(TableClass, Dftdecimal, null);
                                }
                                break;
                            case "double":
                                double Dftdouble = 0;
                                if (double.TryParse(item.DefaultValue.ToString(), out Dftdouble))
                                {
                                    fi.SetValue(TableClass, Dftdouble, null);
                                }
                                break;
                            case "float":
                                float Dftfloat = 0;
                                if (float.TryParse(item.DefaultValue.ToString(), out Dftfloat))
                                {
                                    fi.SetValue(TableClass, Dftfloat, null);
                                }
                                break;
                            case "string":
                                fi.SetValue(TableClass, item.DefaultValue, null);
                                break;
                            case "datetime":
                                int TDatetime = 0;
                                if (int.TryParse(item.DefaultValue, out TDatetime))
                                {
                                    fi.SetValue(TableClass, DateTime.Now.AddDays(TDatetime), null);
                                }
                                else
                                {
                                    DateTime DftDateTime = new DateTime();
                                    if (DateTime.TryParse(item.DefaultValue, out DftDateTime))
                                    {
                                        fi.SetValue(TableClass, DftDateTime, null);
                                    }
                                }
                                break;
                            case "boolean":
                                bool Dftboolean = false;
                                if (bool.TryParse(item.DefaultValue, out Dftboolean))
                                {
                                    fi.SetValue(TableClass, Dftboolean, null);
                                }
                                break;
                            case "bool":
                                bool Dftbool = false;
                                if (bool.TryParse(item.DefaultValue, out Dftbool))
                                {
                                    fi.SetValue(TableClass, Dftbool, null);
                                }
                                break;
                            default:
                                break;
                        }
                    }
                }


            }

        }

        #endregion

        /// <summary>
        /// 将字符串时间 转换成 时间格式
        /// </summary>
        /// <param name="dateStr">字符串时间</param>
        /// <param name="ArrFormat">时间格式（如：yyyy-MM-dd HH:mm:ss）</param>
        /// <returns></returns>
        public static DateTime? ParseStrToDateTime(string dateStr, params string[] ArrFormat)
        {
            if (ArrFormat == null || !ArrFormat.Any())
            {
                ArrFormat = new string[]{
                   "yyyy/M/d", 
                   "yyyy/M/d h:mm",
                   "yyyy/M/d hh:mm",
                   "yyyy/M/d HH:mm",
                   "yyyy/M/d h:mm:ss", 
                   "yyyy/M/d hh:mm:ss", 
                   "yyyy/M/d HH:mm:ss", 
                   "yyyy/MM/dd", 
                   "yyyy/MM/dd h:mm",
                   "yyyy/MM/dd hh:mm",
                   "yyyy/MM/dd HH:mm",
                   "yyyy/MM/dd h:mm:ss", 
                   "yyyy/MM/dd hh:mm:ss", 
                   "yyyy/MM/dd HH:mm:ss"
                };
            }

            try
            {
                if (dateStr.IndexOf("-") > 0)
                {
                    //dateStr = Regex.Replace(dateStr, "/-/g", "/", RegexOptions.None);
                    dateStr = dateStr.Replace("-", "/");
                }
                DateTime dateVal;
                if (DateTime.TryParseExact(dateStr, ArrFormat, new System.Globalization.CultureInfo("zh-CN"), System.Globalization.DateTimeStyles.None, out dateVal))
                //if (DateTime.TryParseExact(dateStr, ArrFormat, System.Globalization.CultureInfo.CurrentCulture, System.Globalization.DateTimeStyles.None, out dateVal))
                {
                    return dateVal;
                }
                else
                {
                    return null;
                }
            }
            catch (Exception ex)
            {
                //Console.WriteLine("Unable to convert '{0}' to a date.", dateStr);
                WriteLogHelper.WriteLogByLog4Net(ex, EnumType.Log4NetMsgType.Error);
                return null;
            }
        }

        /// <summary>
        /// 获取类的中文名
        /// </summary>
        /// <param name="dataType"></param>
        /// <param name="fieldName"></param>
        /// <returns></returns>
        public static string GetDisplayName(Type dataType, string fieldName)
        {
            // First look into attributes on a type and it's parents
            DisplayAttribute attr;
            attr = (DisplayAttribute)dataType.GetProperty(fieldName).GetCustomAttributes(typeof(DisplayAttribute), true).SingleOrDefault();

            if (attr == null)
            {
                return String.Empty;
            }
            else
                return (attr != null) ? attr.GetName() : String.Empty;
        }

        /// <summary>
        /// 获取类的Metadata中设置的Display中文名
        /// 先取 类中的 Display然后去 Metadata类中的Display
        /// </summary>
        /// <param name="dataType"></param>
        /// <param name="fieldName"></param>
        /// <returns>有MetadataType取Metadata,没有取类中的</returns>
        public static string GetMetaDataDisplayName(Type dataType, string fieldName)
        {
            // First look into attributes on a type and it's parents
            DisplayAttribute attr = null;
            attr = (DisplayAttribute)dataType.GetProperty(fieldName).GetCustomAttributes(typeof(DisplayAttribute), true).SingleOrDefault();

            MetadataTypeAttribute metadataType = (MetadataTypeAttribute)dataType.GetCustomAttributes(typeof(MetadataTypeAttribute), true).FirstOrDefault();
            if (metadataType != null)
            {
                var property = metadataType.MetadataClassType.GetProperty(fieldName);
                if (property != null)
                {
                    attr = (DisplayAttribute)property.GetCustomAttributes(typeof(DisplayAttribute), true).SingleOrDefault();
                }
            }

            return (attr != null) ? attr.Name : String.Empty;
        }

        /// <summary>
        /// 获取类的Metadata中设置的Display中文名
        /// 先取 类中的 Display然后去 Metadata类中的Display
        /// </summary>
        /// <param name="dataType"></param>
        /// <param name="fieldName"></param>
        /// <returns>有MetadataType取Metadata,没有取类中的</returns>
        public static string GetDataDisplayName(PropertyInfo pi)
        {
            // First look into attributes on a type and it's parents
            DisplayAttribute attr = null;
            attr = (DisplayAttribute)pi.GetCustomAttributes(typeof(DisplayAttribute), true).SingleOrDefault();
            if (attr != null)
                return attr.Name;
            else
            {
                MetadataTypeAttribute metadataType = (MetadataTypeAttribute)pi.ReflectedType.GetCustomAttributes(typeof(MetadataTypeAttribute), true).FirstOrDefault();
                if (metadataType != null)
                {
                    var property = metadataType.MetadataClassType.GetProperty(pi.Name);
                    if (property != null)
                    {
                        attr = (DisplayAttribute)property.GetCustomAttributes(typeof(DisplayAttribute), true).SingleOrDefault();
                        if (attr != null)
                            return attr.Name;
                    }
                }

                return String.Empty;

            }
        }

        /// <summary>
        /// 先取 类中的 Display属性，没有再去然后去 Metadata类中的Display属性
        /// 获取类的Metadata中设置的Display中文名
        /// </summary>
        /// <param name="dataType"></param>
        /// <param name="fieldName"></param>
        /// <returns>有MetadataType取Metadata,没有取类中的</returns>
        public static DisplayAttribute GetDataDisplayAttr(PropertyInfo pi)
        {
            // First look into attributes on a type and it's parents
            DisplayAttribute attr = null;
            attr = (DisplayAttribute)pi.GetCustomAttributes(typeof(DisplayAttribute), true).SingleOrDefault();
            if (attr == null)
            {
                MetadataTypeAttribute metadataType = (MetadataTypeAttribute)pi.ReflectedType.GetCustomAttributes(typeof(MetadataTypeAttribute), true).FirstOrDefault();
                if (metadataType != null)
                {
                    var property = metadataType.MetadataClassType.GetProperty(pi.Name);
                    if (property != null)
                    {
                        attr = (DisplayAttribute)property.GetCustomAttributes(typeof(DisplayAttribute), true).SingleOrDefault();
                    }
                }
            }
            return attr;
        }

        /// <summary>
        /// 获取类的Metadata中设置的Display中文名
        /// 先取 类中的 Display然后去 Metadata类中的Display
        /// </summary>
        /// <param name="dataType"></param>
        /// <param name="fieldName"></param>
        /// <returns>有MetadataType取Metadata,没有取类中的</returns>
        public static string GetOnlyMetaDataDisplayName(Type dataType, string fieldName)
        {
            // First look into attributes on a type and it's parents
            DisplayAttribute attr = null;
            // Look for [MetadataType] attribute in type hierarchy
            // http://stackoverflow.com/questions/1910532/attribute-isdefined-doesnt-see-attributes-applied-with-metadatatype-class

            MetadataTypeAttribute metadataType = (MetadataTypeAttribute)dataType.GetCustomAttributes(typeof(MetadataTypeAttribute), true).OfType<MetadataTypeAttribute>().FirstOrDefault();
            if (metadataType != null)
            {
                var property = metadataType.MetadataClassType.GetProperty(fieldName);
                if (property != null)
                {
                    var ss = property.GetCustomAttributes();
                    attr = (DisplayAttribute)property.GetCustomAttributes(typeof(System.ComponentModel.DataAnnotations.DisplayAttribute), true).SingleOrDefault();
                }
            }

            return (attr != null) ? attr.Name : String.Empty;
        }

        #region DataTable 与 匿名类 转换

        /// <summary>
        /// 匿名类的转换方式
        /// 因为匿名类是不能够 Activator.CreateInstance进行反射实例化的
        /// </summary>
        /// <param name="GenericType"></param>
        /// <param name="dataTable"></param>
        /// <returns></returns>
        public static IList FromTable(Type GenericType, DataTable dataTable)
        {
            Type typeMaster = typeof(List<>);
            Type listType = typeMaster.MakeGenericType(GenericType);
            IList list = Activator.CreateInstance(listType) as IList;
            if (dataTable == null || dataTable.Rows.Count == 0)
                return list;
            var constructor = GenericType.GetConstructors(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)
                           .OrderBy(c => c.GetParameters().Length).First();
            var parameters = constructor.GetParameters();

            #region  没有 多参数 构造函数时 反射调用 ConvertTo 方法

            if (!parameters.Any())
            {
                MethodInfo[] ArrMethodInfo = typeof(Common).GetMethods();
                MethodInfo _method = ArrMethodInfo.Where(x => x.Name == "ConvertTo" && x.IsGenericMethod).FirstOrDefault();
                if (_method != null)
                {
                    _method = _method.MakeGenericMethod(GenericType);
                    var retobj = _method.Invoke(null, new object[] { dataTable });
                    if (retobj != null)
                    {
                        list = retobj as IList;
                        return list;
                    }
                }
            }

            #endregion

            var values = new object[parameters.Length];
            foreach (DataRow dr in dataTable.Rows)
            {
                int index = 0;
                foreach (ParameterInfo item in parameters)
                {
                    object itemValue = null;
                    if (dr[item.Name] != null && dr[item.Name] != DBNull.Value)
                    {
                        itemValue = Convert.ChangeType(dr[item.Name], item.ParameterType.GetType());//.GetEnumUnderlyingType()
                    }
                    values[index++] = itemValue;
                }
                list.Add(constructor.Invoke(values));
            }
            return list;
        }

        /// <summary>
        /// 匿名类的转换方式
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="dataTable"></param>
        /// <returns></returns>
        public static List<T> FromTable<T>(DataTable dataTable)
        {
            List<T> list = new List<T>();
            if (dataTable == null || dataTable.Rows.Count == 0)
                return list;
            //取当前匿名类的构造函数
            var constructor = typeof(T).GetConstructors(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)
                           .OrderBy(c => c.GetParameters().Length).First();
            //取当前构造函数的参数
            var parameters = constructor.GetParameters();

            #region  没有 多参数 构造函数时 反射调用 ConvertTo 方法

            if (!parameters.Any())
            {
                MethodInfo[] ArrMethodInfo = typeof(Common).GetMethods();
                MethodInfo _method = ArrMethodInfo.Where(x => x.Name == "ConvertTo" && x.IsGenericMethod).FirstOrDefault();
                if (_method != null)
                {
                    _method = _method.MakeGenericMethod(typeof(T));
                    var retobj = _method.Invoke(null, new object[] { dataTable });
                    if (retobj != null)
                    {
                        list = retobj as List<T>;
                        return list;
                    }
                }
            }

            #endregion

            var values = new object[parameters.Length];
            foreach (DataRow dr in dataTable.Rows)
            {
                int index = 0;
                foreach (ParameterInfo item in parameters)
                {
                    object itemValue = null;
                    if (dr[item.Name] != null)
                    {
                        itemValue = Convert.ChangeType(dr[item.Name], item.ParameterType.GetType());//.GetUnderlyingType()
                    }
                    values[index++] = itemValue;
                }
                T entity = (T)constructor.Invoke(values);
                list.Add(entity);
            }
            return list;
        }

        #endregion

        #region 文件及图片处理

        /// <summary>
        /// 根据文件头判断上传的文件类型
        /// </summary>
        /// <param name="filePath">filePath是文件的完整路径 </param>
        /// <returns>返回true或false</returns>
        public static string GetByteType(int id, byte[] bytedata)
        {
            try
            {
                string FileType = "";
                int Num = 0;
                string fileClass;
                byte buffer = bytedata[Num];
                string str = buffer.ToString();
                fileClass = str;
                buffer = bytedata[Num++];
                str = buffer.ToString();
                fileClass += str;

                //255216是jpg;7173是gif;6677是BMP,13780是PNG;7790是exe,8297是rar 
                switch (fileClass)
                {
                    case "255216":
                        FileType = "jpg";
                        break;
                    case "7173":
                        FileType = "gif";
                        break;
                    case "13780":
                        FileType = "BMP";
                        break;
                    case "6677":
                        FileType = "PNG";
                        break;
                    case "7790":
                        FileType = "exe";
                        break;
                    case "8297":
                        FileType = "rar";
                        break;
                }

                return FileType;

                //FileStream fs = new FileStream(filePath, FileMode.Open, FileAccess.Read);
                //System.IO.BinaryReader reader = new System.IO.BinaryReader(fs);
                //string fileClass;
                //byte buffer;
                //buffer = reader.ReadByte();
                //fileClass = buffer.ToString();
                //buffer = reader.ReadByte();
                //fileClass += buffer.ToString();
                //reader.Close();
                //fs.Close();
                //if (fileClass == "255216" || fileClass == "7173" || fileClass == "13780" || fileClass == "6677")
                ////255216是jpg;7173是gif;6677是BMP,13780是PNG;7790是exe,8297是rar 
                //{
                //    return true;
                //}
                //else
                //{
                //    return false;
                //}
            }
            catch
            {
                return "";
            }
        }

        /// <summary>
        /// 获取文件真实扩展名
        /// </summary>
        /// <param name="dirpath">要检测文件真实扩展名的路径</param>
        /// <returns></returns>
        public static Dictionary<string, string> GetFileType(string dirpath = "/Log")
        {
            Dictionary<string, string> dirFile = new Dictionary<string, string>();
            System.IO.DirectoryInfo dir = new System.IO.DirectoryInfo(HttpContext.Current.Server.MapPath(dirpath));
            if (dir.Exists)
            {
                var ArrFiles = dir.GetFiles();
                if (ArrFiles.Any())
                {
                    foreach (var item in ArrFiles)
                    {
                        System.IO.FileStream fs = new System.IO.FileStream(item.FullName, System.IO.FileMode.Open, System.IO.FileAccess.Read);
                        System.IO.BinaryReader r = new System.IO.BinaryReader(fs);
                        string fileClass = "";
                        byte buffer;
                        try
                        {
                            buffer = r.ReadByte();
                            fileClass = buffer.ToString();
                            buffer = r.ReadByte();
                            fileClass += buffer.ToString();
                            dirFile.Add(item.FullName, "图片：" + (IsImage(fileClass) ? "是" : "否") + "---" + fileClass);
                        }
                        catch (Exception exc)
                        {
                            Console.WriteLine(exc.Message);
                        }
                        r.Close();
                        fs.Close();
                    }
                }
            }
            return dirFile;
        }

        /// <summary>
        /// byte[] 是否是图片
        /// </summary>
        /// <param name="id"></param>
        /// <param name="bytedata"></param>
        /// <returns></returns>
        public static bool GetByteIsImage(int id, byte[] bytedata)
        {
            System.IO.MemoryStream ms = new System.IO.MemoryStream(bytedata);
            System.IO.BinaryReader r = new System.IO.BinaryReader(ms);
            string fileClass = "";
            byte buffer;
            try
            {
                buffer = r.ReadByte();
                fileClass = buffer.ToString();
                buffer = r.ReadByte();
                fileClass += buffer.ToString();
                return IsImage(fileClass);
            }
            catch (Exception exc)
            {
                Console.WriteLine(exc.Message);
                return false;
            }
            finally
            {
                r.Close();
                ms.Close();
            }
        }

        /// <summary>
        /// 是否是图片
        /// </summary>
        /// <param name="fileClass"></param>
        /// <returns></returns>
        public static bool IsImage(string fileClass = "")
        {
            bool ret = false;
            var dic = GetEnumToDic("ImageFileClass");
            //var b = Enum.Parse(typeof(ImageFileClass), fileClass).ToString();
            if (dic.Any(x => x.Value.ToString() == fileClass.Trim()))
            {
                ret = true;
            }
            return ret;
        }

        /// <summary>
        /// 获取文件流 前2位 数据
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public static string GetFileClass(string path)
        {
            System.IO.FileInfo fileinfo = new System.IO.FileInfo(path);
            if (path.IndexOf(':') < 0)
                fileinfo = new System.IO.FileInfo(HttpContext.Current.Server.MapPath(path));
            if (fileinfo.Exists)
            {
                try
                {
                    System.IO.FileStream fs = fileinfo.Open(System.IO.FileMode.Open, System.IO.FileAccess.Read, System.IO.FileShare.ReadWrite);
                    byte[] bytedata = new byte[fs.Length];
                    fs.Read(bytedata, 0, (int)fs.Length);
                    fs.Close();
                    return GetFileClass(bytedata);
                }
                catch
                {
                    return "";
                }
            }
            else
                return "";
        }

        /// <summary>
        /// 获取文件流 前2位 数据
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public static string GetFileClass(byte[] bytedata)
        {
            System.IO.MemoryStream ms = new System.IO.MemoryStream(bytedata);
            System.IO.BinaryReader r = new System.IO.BinaryReader(ms);
            string fileClass = "";
            byte buffer;
            try
            {
                buffer = r.ReadByte();
                fileClass = buffer.ToString();
                buffer = r.ReadByte();
                fileClass += buffer.ToString();
                return fileClass;
            }
            catch (Exception exc)
            {
                Console.WriteLine(exc.Message);
                return "";
            }
            finally
            {
                r.Close();
                ms.Close();
            }
        }

        //图片文件 byte流 前2位 数据
        public enum ImageFileClass
        {
            JPG = 255216,
            GIF = 7173,
            PNG = 13780,
            BMP = 6677
        }

        //文件byte流 前2位 数据 图片 测试 正常 文件 rar 测试正常 office 文件无法判断
        public enum FileExtension
        {
            /*文件扩展名说明
             * 255216 jpg
             * 208207 doc xls ppt wps
             * 8075 docx pptx xlsx zip
             * 5150 txt
             * 8297 rar
             * 7790 exe
             * 3780 pdf      
             * 
             * 4946/104116 txt
             * 7173        gif 
             * 255216      jpg
             * 13780       png
             * 6677        bmp
             * 239187      txt,aspx,asp,sql
             * 208207      xls.doc.ppt
             * 6063        xml
             * 6033        htm,html
             * 4742        js
             * 8075        xlsx,zip,pptx,mmap,zip
             * 8297        rar   
             * 01          accdb,mdb
             * 7790        exe,dll
             * 5666        psd 
             * 255254      rdp 
             * 10056       bt种子 
             * 64101       bat 
             * 4059        sgf    
             */
            //JPG = 255216,
            //GIF = 7173,
            //BMP = 6677,
            //PNG = 13780,
            //COM = 7790,
            //EXE = 7790,
            //DLL = 7790,
            //RAR = 8297,
            //ZIP = 8075,
            //XML = 6063,
            //HTML = 6033,
            //ASPX = 239187,
            //CS = 117115,
            //JS = 119105,
            //TXT = 210187,
            //SQL = 255254,
            //BAT = 64101,
            //BTSEED = 10056,
            //RDP = 255254,
            //PSD = 5666,
            //PDF = 3780,
            //CHM = 7384,
            //LOG = 70105,
            //REG = 8269,
            //HLP = 6395,
            //DOC = 208207,
            //XLS = 208207,
            //DOCX = 208207,
            //XLSX = 208207
        }

        #endregion

        /// <summary>
        /// 取 img a标签中的 src或href url中的 参数
        /// </summary>
        /// <param name="HtmlStr">要获取 img a标签的 html字符串</param>
        /// <returns>img a标签中的 src或href 以及 url中的 参数</returns>
        public static Dictionary<string, Dictionary<string, string>> Get_A_Img_Src(string HtmlStr)
        {
            Dictionary<string, Dictionary<string, string>> retDic = new Dictionary<string, Dictionary<string, string>>();
            //取图片/附件src
            string nmm_strFileSrc = "(src|href|targetUrl)=(\"|')?(?<filesrcstring>[^ \"'>]+)";
            //取src|href 里的参数
            string nmm_FileSrcParam = "[\\?&]{1}(?<param>\\w+)=(?<paramval>[A-Za-z0-9_-]+)";// "([^\\?&])=?(?<param>[^&\"']+)";

            Regex nmm_regFileSrc = new Regex(nmm_strFileSrc, RegexOptions.IgnoreCase);
            MatchCollection nmm_matchesFileSrc = nmm_regFileSrc.Matches(HtmlStr);

            foreach (Match m in nmm_matchesFileSrc)
            {
                //取图片<img  ...... > <a .......>
                string strfileSrc = m.Groups["filesrcstring"].Value;
                string DeCodestrfileSrc = HttpContext.Current.Server.HtmlDecode(strfileSrc);
                Dictionary<string, string> dicParam = new Dictionary<string, string>();
                //取图片src <img style=' width:30px; height:30px' a src='/EMS_HEADS/GetImgByte?id=0&column=APPROVAL_DOCUMENTS&columnVal=40949263-2123-4c47-9e7c-b3e4dfc2ad50&FileAttachID=22&s=0.7691376628936399'>
                Regex nmm_regFileSrcParam = new Regex(nmm_FileSrcParam, RegexOptions.IgnoreCase);
                MatchCollection nmm_matchesFileSrcParam = nmm_regFileSrcParam.Matches(DeCodestrfileSrc);
                foreach (Match m1 in nmm_matchesFileSrcParam)
                {
                    dicParam.Add(m1.Groups["param"].Value, m1.Groups["paramval"].Value);
                }
                if (!retDic.Any(x => x.Key == strfileSrc))
                    retDic.Add(strfileSrc, dicParam);
            }
            return retDic;
        }

        #region 图片操作 缩放等

        #region 正方型裁剪并缩放

        /// <summary>
        /// 正方型裁剪
        /// 以图片中心为轴心，截取正方型，然后等比缩放
        /// 用于头像处理
        /// </summary>
        /// <remarks>吴剑 2012-08-08</remarks>
        /// <param name="fromFile">原图Stream对象</param>
        /// <param name="fileSaveUrl">缩略图存放地址</param>
        /// <param name="side">指定的边长（正方型）</param>
        /// <param name="quality">质量（范围0-100）</param>
        public static void CutImageForSquare(System.IO.Stream fromFile, string fileSaveUrl, int side, int quality)
        {
            //创建目录
            string dir = Path.GetDirectoryName(fileSaveUrl);
            if (!Directory.Exists(dir))
                Directory.CreateDirectory(dir);

            //原始图片（获取原始图片创建对象，并使用流中嵌入的颜色管理信息）
            System.Drawing.Image initImage = System.Drawing.Image.FromStream(fromFile, true);

            //原图宽高均小于模版，不作处理，直接保存
            if (initImage.Width <= side && initImage.Height <= side)
            {
                initImage.Save(fileSaveUrl, System.Drawing.Imaging.ImageFormat.Jpeg);
            }
            else
            {
                //原始图片的宽、高
                int initWidth = initImage.Width;
                int initHeight = initImage.Height;

                //非正方型先裁剪为正方型
                if (initWidth != initHeight)
                {
                    //截图对象
                    System.Drawing.Image pickedImage = null;
                    System.Drawing.Graphics pickedG = null;

                    //宽大于高的横图
                    if (initWidth > initHeight)
                    {
                        //对象实例化
                        pickedImage = new System.Drawing.Bitmap(initHeight, initHeight);
                        pickedG = System.Drawing.Graphics.FromImage(pickedImage);
                        //设置质量
                        pickedG.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
                        pickedG.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighQuality;
                        //定位
                        Rectangle fromR = new Rectangle((initWidth - initHeight) / 2, 0, initHeight, initHeight);
                        Rectangle toR = new Rectangle(0, 0, initHeight, initHeight);
                        //画图
                        pickedG.DrawImage(initImage, toR, fromR, System.Drawing.GraphicsUnit.Pixel);
                        //重置宽
                        initWidth = initHeight;
                    }
                    //高大于宽的竖图
                    else
                    {
                        //对象实例化
                        pickedImage = new System.Drawing.Bitmap(initWidth, initWidth);
                        pickedG = System.Drawing.Graphics.FromImage(pickedImage);
                        //设置质量
                        pickedG.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
                        pickedG.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighQuality;
                        //定位
                        Rectangle fromR = new Rectangle(0, (initHeight - initWidth) / 2, initWidth, initWidth);
                        Rectangle toR = new Rectangle(0, 0, initWidth, initWidth);
                        //画图
                        pickedG.DrawImage(initImage, toR, fromR, System.Drawing.GraphicsUnit.Pixel);
                        //重置高
                        initHeight = initWidth;
                    }

                    //将截图对象赋给原图
                    initImage = (System.Drawing.Image)pickedImage.Clone();
                    //释放截图资源
                    pickedG.Dispose();
                    pickedImage.Dispose();
                }

                //缩略图对象
                System.Drawing.Image resultImage = new System.Drawing.Bitmap(side, side);
                System.Drawing.Graphics resultG = System.Drawing.Graphics.FromImage(resultImage);
                //设置质量
                resultG.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
                resultG.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighQuality;
                //用指定背景色清空画布
                resultG.Clear(Color.White);
                //绘制缩略图
                resultG.DrawImage(initImage, new System.Drawing.Rectangle(0, 0, side, side), new System.Drawing.Rectangle(0, 0, initWidth, initHeight), System.Drawing.GraphicsUnit.Pixel);

                //关键质量控制
                //获取系统编码类型数组,包含了jpeg,bmp,png,gif,tiff
                ImageCodecInfo[] icis = ImageCodecInfo.GetImageEncoders();
                ImageCodecInfo ici = null;
                foreach (ImageCodecInfo i in icis)
                {
                    if (i.MimeType == "image/jpeg" || i.MimeType == "image/bmp" || i.MimeType == "image/png" || i.MimeType == "image/gif")
                    {
                        ici = i;
                    }
                }
                EncoderParameters ep = new EncoderParameters(1);
                ep.Param[0] = new EncoderParameter(System.Drawing.Imaging.Encoder.Quality, (long)quality);

                //保存缩略图
                resultImage.Save(fileSaveUrl, ici, ep);

                //释放关键质量控制所用资源
                ep.Dispose();

                //释放缩略图资源
                resultG.Dispose();
                resultImage.Dispose();

                //释放原始图片资源
                initImage.Dispose();
            }
        }

        /// <summary>
        /// 正方型裁剪
        /// 以图片中心为轴心，截取正方型，然后等比缩放
        /// 用于头像处理
        /// </summary>
        /// <remarks>吴剑 2012-08-08</remarks>
        /// <param name="fromFile">原图Stream对象</param>
        /// <param name="fileSaveUrl">缩略图存放地址</param>
        /// <param name="side">指定的边长（正方型）</param>
        /// <param name="quality">质量（范围0-100）</param>
        public static Stream CutImageForSquare(System.IO.Stream fromFile, int side, int quality)
        {
            MemoryStream retst = new MemoryStream();
            //原始图片（获取原始图片创建对象，并使用流中嵌入的颜色管理信息）
            System.Drawing.Image initImage = System.Drawing.Image.FromStream(fromFile, true);

            //原图宽高均小于模版，不作处理，直接保存
            if (initImage.Width <= side && initImage.Height <= side)
            {
                retst = (MemoryStream)fromFile;
                //initImage.Save(fileSaveUrl, System.Drawing.Imaging.ImageFormat.Jpeg);
            }
            else
            {
                //原始图片的宽、高
                int initWidth = initImage.Width;
                int initHeight = initImage.Height;

                //非正方型先裁剪为正方型
                if (initWidth != initHeight)
                {
                    //截图对象
                    System.Drawing.Image pickedImage = null;
                    System.Drawing.Graphics pickedG = null;

                    //宽大于高的横图
                    if (initWidth > initHeight)
                    {
                        //对象实例化
                        pickedImage = new System.Drawing.Bitmap(initHeight, initHeight);
                        pickedG = System.Drawing.Graphics.FromImage(pickedImage);
                        //设置质量
                        pickedG.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
                        pickedG.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighQuality;
                        //定位
                        Rectangle fromR = new Rectangle((initWidth - initHeight) / 2, 0, initHeight, initHeight);
                        Rectangle toR = new Rectangle(0, 0, initHeight, initHeight);
                        //画图
                        pickedG.DrawImage(initImage, toR, fromR, System.Drawing.GraphicsUnit.Pixel);
                        //重置宽
                        initWidth = initHeight;
                    }
                    //高大于宽的竖图
                    else
                    {
                        //对象实例化
                        pickedImage = new System.Drawing.Bitmap(initWidth, initWidth);
                        pickedG = System.Drawing.Graphics.FromImage(pickedImage);
                        //设置质量
                        pickedG.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
                        pickedG.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighQuality;
                        //定位
                        Rectangle fromR = new Rectangle(0, (initHeight - initWidth) / 2, initWidth, initWidth);
                        Rectangle toR = new Rectangle(0, 0, initWidth, initWidth);
                        //画图
                        pickedG.DrawImage(initImage, toR, fromR, System.Drawing.GraphicsUnit.Pixel);
                        //重置高
                        initHeight = initWidth;
                    }

                    //将截图对象赋给原图
                    initImage = (System.Drawing.Image)pickedImage.Clone();
                    //释放截图资源
                    pickedG.Dispose();
                    pickedImage.Dispose();
                }

                //缩略图对象
                System.Drawing.Image resultImage = new System.Drawing.Bitmap(side, side);
                System.Drawing.Graphics resultG = System.Drawing.Graphics.FromImage(resultImage);
                //设置质量
                resultG.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
                resultG.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighQuality;
                //用指定背景色清空画布
                resultG.Clear(Color.White);
                //绘制缩略图
                resultG.DrawImage(initImage, new System.Drawing.Rectangle(0, 0, side, side), new System.Drawing.Rectangle(0, 0, initWidth, initHeight), System.Drawing.GraphicsUnit.Pixel);

                //关键质量控制
                //获取系统编码类型数组,包含了jpeg,bmp,png,gif,tiff
                ImageCodecInfo[] icis = ImageCodecInfo.GetImageEncoders();
                ImageCodecInfo ici = null;
                foreach (ImageCodecInfo i in icis)
                {
                    if (i.MimeType == "image/jpeg" || i.MimeType == "image/bmp" || i.MimeType == "image/png" || i.MimeType == "image/gif")
                    {
                        ici = i;
                    }
                }
                EncoderParameters ep = new EncoderParameters(1);
                ep.Param[0] = new EncoderParameter(System.Drawing.Imaging.Encoder.Quality, (long)quality);

                //保存缩略图
                //resultImage.Save(fileSaveUrl, ici, ep);
                resultImage.Save(retst, ici, ep);
                //释放关键质量控制所用资源
                ep.Dispose();
                //释放缩略图资源
                resultG.Dispose();
                resultImage.Dispose();
                //释放原始图片资源
                initImage.Dispose();
            }

            return retst;
        }

        #endregion

        #region 自定义裁剪并缩放

        /// <summary>
        /// 指定长宽裁剪
        /// 按模版比例最大范围的裁剪图片并缩放至模版尺寸
        /// </summary>
        /// <remarks>吴剑 2012-08-08</remarks>
        /// <param name="fromFile">原图Stream对象</param>
        /// <param name="fileSaveUrl">保存路径</param>
        /// <param name="maxWidth">最大宽(单位:px)</param>
        /// <param name="maxHeight">最大高(单位:px)</param>
        /// <param name="quality">质量（范围0-100）</param>
        public static void CutImageForCustom(System.IO.Stream fromFile, string fileSaveUrl, int maxWidth, int maxHeight, int quality)
        {
            //从文件获取原始图片，并使用流中嵌入的颜色管理信息
            System.Drawing.Image initImage = System.Drawing.Image.FromStream(fromFile, true);

            //原图宽高均小于模版，不作处理，直接保存
            if (initImage.Width <= maxWidth && initImage.Height <= maxHeight)
            {
                initImage.Save(fileSaveUrl, System.Drawing.Imaging.ImageFormat.Jpeg);
            }
            else
            {
                //模版的宽高比例
                double templateRate = (double)maxWidth / maxHeight;
                //原图片的宽高比例
                double initRate = (double)initImage.Width / initImage.Height;

                //原图与模版比例相等，直接缩放
                if (templateRate == initRate)
                {
                    //按模版大小生成最终图片
                    System.Drawing.Image templateImage = new System.Drawing.Bitmap(maxWidth, maxHeight);
                    System.Drawing.Graphics templateG = System.Drawing.Graphics.FromImage(templateImage);
                    templateG.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.High;
                    templateG.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighQuality;
                    templateG.Clear(Color.White);
                    templateG.DrawImage(initImage, new System.Drawing.Rectangle(0, 0, maxWidth, maxHeight), new System.Drawing.Rectangle(0, 0, initImage.Width, initImage.Height), System.Drawing.GraphicsUnit.Pixel);
                    templateImage.Save(fileSaveUrl, System.Drawing.Imaging.ImageFormat.Jpeg);
                }
                //原图与模版比例不等，裁剪后缩放
                else
                {
                    //裁剪对象
                    System.Drawing.Image pickedImage = null;
                    System.Drawing.Graphics pickedG = null;

                    //定位
                    Rectangle fromR = new Rectangle(0, 0, 0, 0);//原图裁剪定位
                    Rectangle toR = new Rectangle(0, 0, 0, 0);//目标定位

                    //宽为标准进行裁剪
                    if (templateRate > initRate)
                    {
                        //裁剪对象实例化
                        pickedImage = new System.Drawing.Bitmap(initImage.Width, (int)System.Math.Floor(initImage.Width / templateRate));
                        pickedG = System.Drawing.Graphics.FromImage(pickedImage);

                        //裁剪源定位
                        fromR.X = 0;
                        fromR.Y = (int)System.Math.Floor((initImage.Height - initImage.Width / templateRate) / 2);
                        fromR.Width = initImage.Width;
                        fromR.Height = (int)System.Math.Floor(initImage.Width / templateRate);

                        //裁剪目标定位
                        toR.X = 0;
                        toR.Y = 0;
                        toR.Width = initImage.Width;
                        toR.Height = (int)System.Math.Floor(initImage.Width / templateRate);
                    }
                    //高为标准进行裁剪
                    else
                    {
                        pickedImage = new System.Drawing.Bitmap((int)System.Math.Floor(initImage.Height * templateRate), initImage.Height);
                        pickedG = System.Drawing.Graphics.FromImage(pickedImage);

                        fromR.X = (int)System.Math.Floor((initImage.Width - initImage.Height * templateRate) / 2);
                        fromR.Y = 0;
                        fromR.Width = (int)System.Math.Floor(initImage.Height * templateRate);
                        fromR.Height = initImage.Height;

                        toR.X = 0;
                        toR.Y = 0;
                        toR.Width = (int)System.Math.Floor(initImage.Height * templateRate);
                        toR.Height = initImage.Height;
                    }

                    //设置质量
                    pickedG.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
                    pickedG.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighQuality;

                    //裁剪
                    pickedG.DrawImage(initImage, toR, fromR, System.Drawing.GraphicsUnit.Pixel);

                    //按模版大小生成最终图片
                    System.Drawing.Image templateImage = new System.Drawing.Bitmap(maxWidth, maxHeight);
                    System.Drawing.Graphics templateG = System.Drawing.Graphics.FromImage(templateImage);
                    templateG.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.High;
                    templateG.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighQuality;
                    templateG.Clear(Color.White);
                    templateG.DrawImage(pickedImage, new System.Drawing.Rectangle(0, 0, maxWidth, maxHeight), new System.Drawing.Rectangle(0, 0, pickedImage.Width, pickedImage.Height), System.Drawing.GraphicsUnit.Pixel);

                    //关键质量控制
                    //获取系统编码类型数组,包含了jpeg,bmp,png,gif,tiff
                    ImageCodecInfo[] icis = ImageCodecInfo.GetImageEncoders();
                    ImageCodecInfo ici = null;
                    foreach (ImageCodecInfo i in icis)
                    {
                        if (i.MimeType == "image/jpeg" || i.MimeType == "image/bmp" || i.MimeType == "image/png" || i.MimeType == "image/gif")
                        {
                            ici = i;
                        }
                    }
                    EncoderParameters ep = new EncoderParameters(1);
                    ep.Param[0] = new EncoderParameter(System.Drawing.Imaging.Encoder.Quality, (long)quality);

                    //保存缩略图
                    templateImage.Save(fileSaveUrl, ici, ep);
                    //templateImage.Save(fileSaveUrl, System.Drawing.Imaging.ImageFormat.Jpeg);

                    //释放资源
                    templateG.Dispose();
                    templateImage.Dispose();

                    pickedG.Dispose();
                    pickedImage.Dispose();
                }
            }

            //释放资源
            initImage.Dispose();
        }

        /// <summary>
        /// 指定长宽裁剪
        /// 按模版比例最大范围的裁剪图片并缩放至模版尺寸
        /// </summary>
        /// <remarks>吴剑 2012-08-08</remarks>
        /// <param name="fromFile">原图Stream对象</param>
        /// <param name="maxWidth">最大宽(单位:px)</param>
        /// <param name="maxHeight">最大高(单位:px)</param>
        /// <param name="quality">质量（范围0-100）</param>
        public static MemoryStream CutImageForCustom(System.IO.Stream fromFile, int maxWidth, int maxHeight, int quality)
        {
            MemoryStream retst = new MemoryStream();
            //从文件获取原始图片，并使用流中嵌入的颜色管理信息
            System.Drawing.Image initImage = System.Drawing.Image.FromStream(fromFile, true);

            //原图宽高均小于模版，不作处理，直接保存
            if (initImage.Width <= maxWidth && initImage.Height <= maxHeight)
            {
                retst = (MemoryStream)fromFile;
                //retbyte = new byte[fromFile.Length];
                //fromFile.Write(retbyte, 0, (int)fromFile.Length);
            }
            else
            {
                //模版的宽高比例
                double templateRate = (double)maxWidth / maxHeight;
                //原图片的宽高比例
                double initRate = (double)initImage.Width / initImage.Height;

                //原图与模版比例相等，直接缩放
                if (templateRate == initRate)
                {
                    //按模版大小生成最终图片
                    System.Drawing.Image templateImage = new System.Drawing.Bitmap(maxWidth, maxHeight);
                    System.Drawing.Graphics templateG = System.Drawing.Graphics.FromImage(templateImage);
                    templateG.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.High;
                    templateG.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighQuality;
                    templateG.Clear(Color.White);
                    templateG.DrawImage(initImage, new System.Drawing.Rectangle(0, 0, maxWidth, maxHeight), new System.Drawing.Rectangle(0, 0, initImage.Width, initImage.Height), System.Drawing.GraphicsUnit.Pixel);
                    templateImage.Save(retst, System.Drawing.Imaging.ImageFormat.Jpeg);
                }
                //原图与模版比例不等，裁剪后缩放
                else
                {
                    //裁剪对象
                    System.Drawing.Image pickedImage = null;
                    System.Drawing.Graphics pickedG = null;

                    //定位
                    Rectangle fromR = new Rectangle(0, 0, 0, 0);//原图裁剪定位
                    Rectangle toR = new Rectangle(0, 0, 0, 0);//目标定位

                    //宽为标准进行裁剪
                    if (templateRate > initRate)
                    {
                        //裁剪对象实例化
                        pickedImage = new System.Drawing.Bitmap(initImage.Width, (int)System.Math.Floor(initImage.Width / templateRate));
                        pickedG = System.Drawing.Graphics.FromImage(pickedImage);

                        //裁剪源定位
                        fromR.X = 0;
                        fromR.Y = (int)System.Math.Floor((initImage.Height - initImage.Width / templateRate) / 2);
                        fromR.Width = initImage.Width;
                        fromR.Height = (int)System.Math.Floor(initImage.Width / templateRate);

                        //裁剪目标定位
                        toR.X = 0;
                        toR.Y = 0;
                        toR.Width = initImage.Width;
                        toR.Height = (int)System.Math.Floor(initImage.Width / templateRate);
                    }
                    //高为标准进行裁剪
                    else
                    {
                        pickedImage = new System.Drawing.Bitmap((int)System.Math.Floor(initImage.Height * templateRate), initImage.Height);
                        pickedG = System.Drawing.Graphics.FromImage(pickedImage);

                        fromR.X = (int)System.Math.Floor((initImage.Width - initImage.Height * templateRate) / 2);
                        fromR.Y = 0;
                        fromR.Width = (int)System.Math.Floor(initImage.Height * templateRate);
                        fromR.Height = initImage.Height;

                        toR.X = 0;
                        toR.Y = 0;
                        toR.Width = (int)System.Math.Floor(initImage.Height * templateRate);
                        toR.Height = initImage.Height;
                    }

                    //设置质量
                    pickedG.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
                    pickedG.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighQuality;

                    //裁剪
                    pickedG.DrawImage(initImage, toR, fromR, System.Drawing.GraphicsUnit.Pixel);

                    //按模版大小生成最终图片
                    System.Drawing.Image templateImage = new System.Drawing.Bitmap(maxWidth, maxHeight);
                    System.Drawing.Graphics templateG = System.Drawing.Graphics.FromImage(templateImage);
                    templateG.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.High;
                    templateG.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighQuality;
                    templateG.Clear(Color.White);
                    templateG.DrawImage(pickedImage, new System.Drawing.Rectangle(0, 0, maxWidth, maxHeight), new System.Drawing.Rectangle(0, 0, pickedImage.Width, pickedImage.Height), System.Drawing.GraphicsUnit.Pixel);

                    //关键质量控制
                    //获取系统编码类型数组,包含了jpeg,bmp,png,gif,tiff
                    ImageCodecInfo[] icis = ImageCodecInfo.GetImageEncoders();
                    ImageCodecInfo ici = null;
                    foreach (ImageCodecInfo i in icis)
                    {
                        if (i.MimeType == "image/jpeg" || i.MimeType == "image/bmp" || i.MimeType == "image/png" || i.MimeType == "image/gif")
                        {
                            ici = i;
                        }
                    }
                    EncoderParameters ep = new EncoderParameters(1);
                    ep.Param[0] = new EncoderParameter(System.Drawing.Imaging.Encoder.Quality, (long)quality);

                    //保存缩略图
                    templateImage.Save(retst, ici, ep);
                    //templateImage.Save(fileSaveUrl, System.Drawing.Imaging.ImageFormat.Jpeg);

                    //释放资源
                    templateG.Dispose();
                    templateImage.Dispose();

                    pickedG.Dispose();
                    pickedImage.Dispose();
                }
            }

            //释放资源
            initImage.Dispose();

            return retst;
        }

        #endregion

        #region 等比缩放

        /// <summary>
        /// 图片等比缩放
        /// </summary>
        /// <remarks>吴剑 2012-08-08</remarks>
        /// <param name="fromFile">原图Stream对象</param>
        /// <param name="savePath">缩略图存放地址</param>
        /// <param name="targetWidth">指定的最大宽度</param>
        /// <param name="targetHeight">指定的最大高度</param>
        /// <param name="watermarkText">水印文字(为""表示不使用水印)</param>
        /// <param name="watermarkImage">水印图片路径(为""表示不使用水印)</param>
        public static void ZoomImageAuto(System.IO.Stream fromFile, string savePath, System.Double targetWidth, System.Double targetHeight, string watermarkText, string watermarkImage)
        {
            //创建目录
            string dir = Path.GetDirectoryName(savePath);
            if (!Directory.Exists(dir))
                Directory.CreateDirectory(dir);

            //原始图片（获取原始图片创建对象，并使用流中嵌入的颜色管理信息）
            System.Drawing.Image initImage = System.Drawing.Image.FromStream(fromFile, true);

            //原图宽高均小于模版，不作处理，直接保存
            if (initImage.Width <= targetWidth && initImage.Height <= targetHeight)
            {
                //文字水印
                if (watermarkText != "")
                {
                    using (System.Drawing.Graphics gWater = System.Drawing.Graphics.FromImage(initImage))
                    {
                        System.Drawing.Font fontWater = new Font("黑体", 10);
                        System.Drawing.Brush brushWater = new SolidBrush(Color.White);
                        gWater.DrawString(watermarkText, fontWater, brushWater, 10, 10);
                        gWater.Dispose();
                    }
                }

                //透明图片水印
                if (watermarkImage != "")
                {
                    if (File.Exists(watermarkImage))
                    {
                        //获取水印图片
                        using (System.Drawing.Image wrImage = System.Drawing.Image.FromFile(watermarkImage))
                        {
                            //水印绘制条件：原始图片宽高均大于或等于水印图片
                            if (initImage.Width >= wrImage.Width && initImage.Height >= wrImage.Height)
                            {
                                Graphics gWater = Graphics.FromImage(initImage);

                                //透明属性
                                ImageAttributes imgAttributes = new ImageAttributes();
                                ColorMap colorMap = new ColorMap();
                                colorMap.OldColor = Color.FromArgb(255, 0, 255, 0);
                                colorMap.NewColor = Color.FromArgb(0, 0, 0, 0);
                                ColorMap[] remapTable = { colorMap };
                                imgAttributes.SetRemapTable(remapTable, ColorAdjustType.Bitmap);

                                float[][] colorMatrixElements = {
                                   new float[] {1.0f,  0.0f,  0.0f,  0.0f, 0.0f},
                                   new float[] {0.0f,  1.0f,  0.0f,  0.0f, 0.0f},
                                   new float[] {0.0f,  0.0f,  1.0f,  0.0f, 0.0f},
                                   new float[] {0.0f,  0.0f,  0.0f,  0.5f, 0.0f},//透明度:0.5
                                   new float[] {0.0f,  0.0f,  0.0f,  0.0f, 1.0f}
                                };

                                ColorMatrix wmColorMatrix = new ColorMatrix(colorMatrixElements);
                                imgAttributes.SetColorMatrix(wmColorMatrix, ColorMatrixFlag.Default, ColorAdjustType.Bitmap);
                                gWater.DrawImage(wrImage, new Rectangle(initImage.Width - wrImage.Width, initImage.Height - wrImage.Height, wrImage.Width, wrImage.Height), 0, 0, wrImage.Width, wrImage.Height, GraphicsUnit.Pixel, imgAttributes);

                                gWater.Dispose();
                            }
                            wrImage.Dispose();
                        }
                    }
                }

                //保存
                initImage.Save(savePath, System.Drawing.Imaging.ImageFormat.Jpeg);
            }
            else
            {
                //缩略图宽、高计算
                double newWidth = initImage.Width;
                double newHeight = initImage.Height;

                //宽大于高或宽等于高（横图或正方）
                if (initImage.Width > initImage.Height || initImage.Width == initImage.Height)
                {
                    //如果宽大于模版
                    if (initImage.Width > targetWidth)
                    {
                        //宽按模版，高按比例缩放
                        newWidth = targetWidth;
                        newHeight = initImage.Height * (targetWidth / initImage.Width);
                    }
                }
                //高大于宽（竖图）
                else
                {
                    //如果高大于模版
                    if (initImage.Height > targetHeight)
                    {
                        //高按模版，宽按比例缩放
                        newHeight = targetHeight;
                        newWidth = initImage.Width * (targetHeight / initImage.Height);
                    }
                }

                //生成新图
                //新建一个bmp图片
                System.Drawing.Image newImage = new System.Drawing.Bitmap((int)newWidth, (int)newHeight);
                //新建一个画板
                System.Drawing.Graphics newG = System.Drawing.Graphics.FromImage(newImage);

                //设置质量
                newG.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
                newG.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighQuality;

                //置背景色
                newG.Clear(Color.White);
                //画图
                newG.DrawImage(initImage, new System.Drawing.Rectangle(0, 0, newImage.Width, newImage.Height), new System.Drawing.Rectangle(0, 0, initImage.Width, initImage.Height), System.Drawing.GraphicsUnit.Pixel);

                //文字水印
                if (watermarkText != "")
                {
                    using (System.Drawing.Graphics gWater = System.Drawing.Graphics.FromImage(newImage))
                    {
                        System.Drawing.Font fontWater = new Font("宋体", 10);
                        System.Drawing.Brush brushWater = new SolidBrush(Color.White);
                        gWater.DrawString(watermarkText, fontWater, brushWater, 10, 10);
                        gWater.Dispose();
                    }
                }

                //透明图片水印
                if (watermarkImage != "")
                {
                    if (File.Exists(watermarkImage))
                    {
                        //获取水印图片
                        using (System.Drawing.Image wrImage = System.Drawing.Image.FromFile(watermarkImage))
                        {
                            //水印绘制条件：原始图片宽高均大于或等于水印图片
                            if (newImage.Width >= wrImage.Width && newImage.Height >= wrImage.Height)
                            {
                                Graphics gWater = Graphics.FromImage(newImage);

                                //透明属性
                                ImageAttributes imgAttributes = new ImageAttributes();
                                ColorMap colorMap = new ColorMap();
                                colorMap.OldColor = Color.FromArgb(255, 0, 255, 0);
                                colorMap.NewColor = Color.FromArgb(0, 0, 0, 0);
                                ColorMap[] remapTable = { colorMap };
                                imgAttributes.SetRemapTable(remapTable, ColorAdjustType.Bitmap);

                                float[][] colorMatrixElements = {
                                   new float[] {1.0f,  0.0f,  0.0f,  0.0f, 0.0f},
                                   new float[] {0.0f,  1.0f,  0.0f,  0.0f, 0.0f},
                                   new float[] {0.0f,  0.0f,  1.0f,  0.0f, 0.0f},
                                   new float[] {0.0f,  0.0f,  0.0f,  0.5f, 0.0f},//透明度:0.5
                                   new float[] {0.0f,  0.0f,  0.0f,  0.0f, 1.0f}
                                };

                                ColorMatrix wmColorMatrix = new ColorMatrix(colorMatrixElements);
                                imgAttributes.SetColorMatrix(wmColorMatrix, ColorMatrixFlag.Default, ColorAdjustType.Bitmap);
                                gWater.DrawImage(wrImage, new Rectangle(newImage.Width - wrImage.Width, newImage.Height - wrImage.Height, wrImage.Width, wrImage.Height), 0, 0, wrImage.Width, wrImage.Height, GraphicsUnit.Pixel, imgAttributes);
                                gWater.Dispose();
                            }
                            wrImage.Dispose();
                        }
                    }
                }

                //保存缩略图
                newImage.Save(savePath, System.Drawing.Imaging.ImageFormat.Jpeg);

                //释放资源
                newG.Dispose();
                newImage.Dispose();
                initImage.Dispose();
            }
        }

        /// <summary>
        /// 图片等比缩放
        /// </summary>
        /// <remarks>吴剑 2012-08-08</remarks>
        /// <param name="fromFile">原图Stream对象</param>
        /// <param name="savePath">缩略图存放地址</param>
        /// <param name="targetWidth">指定的最大宽度</param>
        /// <param name="targetHeight">指定的最大高度</param>
        /// <param name="watermarkText">水印文字(为""表示不使用水印)</param>
        /// <param name="watermarkImage">水印图片路径(为""表示不使用水印)</param>
        public static Stream ZoomImageAuto(System.IO.Stream fromFile, System.Double targetWidth, System.Double targetHeight, string watermarkText, string watermarkImage)
        {
            MemoryStream retst = new MemoryStream();
            //原始图片（获取原始图片创建对象，并使用流中嵌入的颜色管理信息）
            System.Drawing.Image initImage = System.Drawing.Image.FromStream(fromFile, true);

            //原图宽高均小于模版，不作处理，直接保存
            if (initImage.Width <= targetWidth && initImage.Height <= targetHeight)
            {
                //文字水印
                if (watermarkText != "")
                {
                    using (System.Drawing.Graphics gWater = System.Drawing.Graphics.FromImage(initImage))
                    {
                        System.Drawing.Font fontWater = new Font("黑体", 10);
                        System.Drawing.Brush brushWater = new SolidBrush(Color.White);
                        gWater.DrawString(watermarkText, fontWater, brushWater, 10, 10);
                        gWater.Dispose();
                    }
                }

                //透明图片水印
                if (watermarkImage != "")
                {
                    if (File.Exists(watermarkImage))
                    {
                        //获取水印图片
                        using (System.Drawing.Image wrImage = System.Drawing.Image.FromFile(watermarkImage))
                        {
                            //水印绘制条件：原始图片宽高均大于或等于水印图片
                            if (initImage.Width >= wrImage.Width && initImage.Height >= wrImage.Height)
                            {
                                Graphics gWater = Graphics.FromImage(initImage);

                                //透明属性
                                ImageAttributes imgAttributes = new ImageAttributes();
                                ColorMap colorMap = new ColorMap();
                                colorMap.OldColor = Color.FromArgb(255, 0, 255, 0);
                                colorMap.NewColor = Color.FromArgb(0, 0, 0, 0);
                                ColorMap[] remapTable = { colorMap };
                                imgAttributes.SetRemapTable(remapTable, ColorAdjustType.Bitmap);

                                float[][] colorMatrixElements = {
                                   new float[] {1.0f,  0.0f,  0.0f,  0.0f, 0.0f},
                                   new float[] {0.0f,  1.0f,  0.0f,  0.0f, 0.0f},
                                   new float[] {0.0f,  0.0f,  1.0f,  0.0f, 0.0f},
                                   new float[] {0.0f,  0.0f,  0.0f,  0.5f, 0.0f},//透明度:0.5
                                   new float[] {0.0f,  0.0f,  0.0f,  0.0f, 1.0f}
                                };

                                ColorMatrix wmColorMatrix = new ColorMatrix(colorMatrixElements);
                                imgAttributes.SetColorMatrix(wmColorMatrix, ColorMatrixFlag.Default, ColorAdjustType.Bitmap);
                                gWater.DrawImage(wrImage, new Rectangle(initImage.Width - wrImage.Width, initImage.Height - wrImage.Height, wrImage.Width, wrImage.Height), 0, 0, wrImage.Width, wrImage.Height, GraphicsUnit.Pixel, imgAttributes);

                                gWater.Dispose();
                            }
                            wrImage.Dispose();
                        }
                    }
                }

                //保存
                initImage.Save(retst, System.Drawing.Imaging.ImageFormat.Jpeg);
            }
            else
            {
                //缩略图宽、高计算
                double newWidth = initImage.Width;
                double newHeight = initImage.Height;

                //宽大于高或宽等于高（横图或正方）
                if (initImage.Width > initImage.Height || initImage.Width == initImage.Height)
                {
                    //如果宽大于模版
                    if (initImage.Width > targetWidth)
                    {
                        //宽按模版，高按比例缩放
                        newWidth = targetWidth;
                        newHeight = initImage.Height * (targetWidth / initImage.Width);
                    }
                }
                //高大于宽（竖图）
                else
                {
                    //如果高大于模版
                    if (initImage.Height > targetHeight)
                    {
                        //高按模版，宽按比例缩放
                        newHeight = targetHeight;
                        newWidth = initImage.Width * (targetHeight / initImage.Height);
                    }
                }

                //生成新图
                //新建一个bmp图片
                System.Drawing.Image newImage = new System.Drawing.Bitmap((int)newWidth, (int)newHeight);
                //新建一个画板
                System.Drawing.Graphics newG = System.Drawing.Graphics.FromImage(newImage);

                //设置质量
                newG.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
                newG.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighQuality;

                //置背景色
                newG.Clear(Color.White);
                //画图
                newG.DrawImage(initImage, new System.Drawing.Rectangle(0, 0, newImage.Width, newImage.Height), new System.Drawing.Rectangle(0, 0, initImage.Width, initImage.Height), System.Drawing.GraphicsUnit.Pixel);

                //文字水印
                if (watermarkText != "")
                {
                    using (System.Drawing.Graphics gWater = System.Drawing.Graphics.FromImage(newImage))
                    {
                        System.Drawing.Font fontWater = new Font("宋体", 10);
                        System.Drawing.Brush brushWater = new SolidBrush(Color.White);
                        gWater.DrawString(watermarkText, fontWater, brushWater, 10, 10);
                        gWater.Dispose();
                    }
                }

                //透明图片水印
                if (watermarkImage != "")
                {
                    if (File.Exists(watermarkImage))
                    {
                        //获取水印图片
                        using (System.Drawing.Image wrImage = System.Drawing.Image.FromFile(watermarkImage))
                        {
                            //水印绘制条件：原始图片宽高均大于或等于水印图片
                            if (newImage.Width >= wrImage.Width && newImage.Height >= wrImage.Height)
                            {
                                Graphics gWater = Graphics.FromImage(newImage);

                                //透明属性
                                ImageAttributes imgAttributes = new ImageAttributes();
                                ColorMap colorMap = new ColorMap();
                                colorMap.OldColor = Color.FromArgb(255, 0, 255, 0);
                                colorMap.NewColor = Color.FromArgb(0, 0, 0, 0);
                                ColorMap[] remapTable = { colorMap };
                                imgAttributes.SetRemapTable(remapTable, ColorAdjustType.Bitmap);

                                float[][] colorMatrixElements = {
                                   new float[] {1.0f,  0.0f,  0.0f,  0.0f, 0.0f},
                                   new float[] {0.0f,  1.0f,  0.0f,  0.0f, 0.0f},
                                   new float[] {0.0f,  0.0f,  1.0f,  0.0f, 0.0f},
                                   new float[] {0.0f,  0.0f,  0.0f,  0.5f, 0.0f},//透明度:0.5
                                   new float[] {0.0f,  0.0f,  0.0f,  0.0f, 1.0f}
                                };

                                ColorMatrix wmColorMatrix = new ColorMatrix(colorMatrixElements);
                                imgAttributes.SetColorMatrix(wmColorMatrix, ColorMatrixFlag.Default, ColorAdjustType.Bitmap);
                                gWater.DrawImage(wrImage, new Rectangle(newImage.Width - wrImage.Width, newImage.Height - wrImage.Height, wrImage.Width, wrImage.Height), 0, 0, wrImage.Width, wrImage.Height, GraphicsUnit.Pixel, imgAttributes);
                                gWater.Dispose();
                            }
                            wrImage.Dispose();
                        }
                    }
                }

                //保存缩略图
                newImage.Save(retst, System.Drawing.Imaging.ImageFormat.Jpeg);

                //释放资源
                newG.Dispose();
                newImage.Dispose();
                initImage.Dispose();
            }
            return retst;
        }

        #endregion

        #region GIF图片打水印处理

        /// <summary>
        /// 会产生graphics异常的PixelFormat
        /// </summary>
        public static PixelFormat[] indexedPixelFormats = { PixelFormat.Undefined, PixelFormat.DontCare,
                PixelFormat.Format16bppArgb1555, PixelFormat.Format1bppIndexed, PixelFormat.Format4bppIndexed,
                PixelFormat.Format8bppIndexed};

        /// <summary>
        /// 判断图片的PixelFormat 是否在 引发异常的 PixelFormat 之中
        /// </summary>
        /// <param name="imgPixelFormat">原图片的PixelFormat</param>
        /// <returns></returns>
        public static bool IsPixelFormatIndexed(PixelFormat imgPixelFormat)
        {
            foreach (PixelFormat pf in indexedPixelFormats)
            {
                if (pf.Equals(imgPixelFormat)) return true;
            }
            return false;
        }

        public static void WaterMarkGIF(string SourceFilePath, MemoryStream ms, int PicAlign, int PicAlpha, int PicLeft, int PicTop, string DestFilePath)
        {
            //使用
            using (System.Drawing.Image img = System.Drawing.Image.FromFile(SourceFilePath))
            {
                //如果原图片是索引像素格式之列的，则需要转换
                if (IsPixelFormatIndexed(img.PixelFormat))
                {
                    Bitmap bmp = new Bitmap(img.Width, img.Height, PixelFormat.Format32bppArgb);
                    using (Graphics gs = Graphics.FromImage(bmp))
                    {
                        gs.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
                        gs.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighQuality;
                        gs.CompositingQuality = System.Drawing.Drawing2D.CompositingQuality.HighQuality;
                        gs.DrawImage(img, 0, 0);
                    }

                    //原图
                    System.Drawing.Bitmap SourceImage = new System.Drawing.Bitmap(SourceFilePath);
                    int SourceWidth = SourceImage.Width;
                    int SourceHeight = SourceImage.Height;
                    //水印图
                    System.Drawing.Bitmap WaterImage = new System.Drawing.Bitmap(ms);
                    int WaterWidth = WaterImage.Width;
                    int WaterHeight = WaterImage.Height;
                    int TestLeft = PicLeft < 0 ? (-PicLeft) : PicLeft;
                    int TestTop = PicTop < 0 ? (-PicTop) : PicTop;
                    if (TestLeft + TestLeft + WaterWidth > SourceWidth)
                    {
                        SourceImage.Dispose();
                        WaterImage.Dispose();
                        return;
                    }
                    if (TestTop + TestTop + WaterHeight > SourceHeight)
                    {
                        SourceImage.Dispose();
                        WaterImage.Dispose();
                        return;
                    }

                    //下面的水印操作，就直接对 bmp 进行了
                    System.Drawing.Graphics g = System.Drawing.Graphics.FromImage(bmp);

                    int x; //临时变量
                    int y; //临时变量
                    int x1 = 0; //原图和水印图的宽度差,即开始绘图的X位置 
                    int y1 = 0; //原图和水印图的高度差,即开始绘图的Y位置 
                    int w = 0; //生成的水印图的宽度,即结束绘图的X位置 
                    int h = 0; //生成的水印图的高度,即结束绘图的Y位置 
                    int al; //alpha 
                    int rl; //Red 
                    int gl; //Green 
                    int bl; //Blue
                    //校验透明度
                    if (PicAlpha > 100 || PicAlpha < 0)
                    {
                        al = 100;
                    }
                    else
                    {
                        al = PicAlpha;
                    }
                    if (SourceWidth > WaterWidth && SourceHeight > WaterHeight) //如果源图比水印图大 
                    {
                        switch (PicAlign)
                        {
                            case 14: //上左
                                x1 = 0;
                                y1 = 0;
                                x1 = x1 + PicLeft;
                                y1 = y1 + PicTop;
                                break;
                            case 34: //下左 
                                x1 = 0;
                                if ((SourceHeight - WaterHeight) > 0) //源图比水印图高
                                {
                                    y1 = SourceHeight - WaterHeight;
                                }
                                else
                                {
                                    y1 = SourceWidth;
                                }
                                x1 = x1 + PicLeft;
                                y1 = y1 - PicTop;
                                break;
                            case 12: //上右
                                y1 = 0;
                                if ((SourceWidth - WaterWidth) > 0) // 源图比水印图宽
                                {
                                    x1 = SourceWidth - WaterWidth;
                                }
                                else
                                {
                                    x1 = SourceWidth;
                                }
                                x1 = x1 - PicLeft;
                                y1 = y1 + PicTop;
                                break;
                            case 32: //下右 
                                //计算高度
                                if ((SourceHeight - WaterHeight) > 0) //源图比水印图高
                                {
                                    y1 = SourceHeight - WaterHeight;
                                }
                                else
                                {
                                    y1 = SourceWidth;
                                }
                                //计算宽度 
                                if ((SourceWidth - WaterWidth) > 0) // 源图比水印图宽
                                {
                                    x1 = SourceWidth - WaterWidth;
                                }
                                else
                                {
                                    x1 = SourceWidth;
                                }
                                x1 = x1 - PicLeft;
                                y1 = y1 - PicTop;
                                break;
                            case 22: //中中 
                                //计算高度 
                                if ((SourceHeight - WaterHeight) > 0) //源图比水印图高
                                {
                                    y1 = (SourceHeight - WaterHeight) / 2;
                                }
                                else
                                {
                                    y1 = SourceWidth;
                                }
                                //计算宽度 
                                if ((SourceWidth - WaterWidth) > 0) // 源图比水印图宽
                                {
                                    x1 = (SourceWidth - WaterWidth) / 2;
                                }
                                else
                                {
                                    x1 = SourceWidth;
                                }
                                x1 = x1 + PicLeft;
                                y1 = y1 + PicTop;
                                break;
                            case 40://左中
                                x1 = 0;
                                if ((SourceHeight - WaterHeight) > 0) //源图比水印图高
                                {
                                    y1 = (SourceHeight - WaterHeight) / 2;
                                }
                                else
                                {
                                    y1 = SourceWidth;
                                }
                                x1 = x1 + PicLeft;
                                y1 = y1 + PicTop;
                                break;
                            case 20://右中
                                //计算高度
                                if ((SourceHeight - WaterHeight) > 0) //源图比水印图高
                                {
                                    y1 = (SourceHeight - WaterHeight) / 2;
                                }
                                else
                                {
                                    y1 = SourceWidth;
                                }
                                //计算宽度 
                                if ((SourceWidth - WaterWidth) > 0) // 源图比水印图宽
                                {
                                    x1 = SourceWidth - WaterWidth;
                                }
                                else
                                {
                                    x1 = SourceWidth;
                                }
                                x1 = x1 - PicLeft;
                                y1 = y1 + PicTop;
                                break;
                            case 10://上中
                                y1 = 0;
                                if ((SourceWidth - WaterWidth) > 0) // 源图比水印图宽
                                {
                                    x1 = (SourceWidth - WaterWidth) / 2;
                                }
                                else
                                {
                                    x1 = SourceWidth;
                                }
                                x1 = x1 + PicLeft;
                                y1 = y1 + PicTop;
                                break;
                            case 30://下中
                                //计算高度
                                if ((SourceHeight - WaterHeight) > 0) //源图比水印图高
                                {
                                    y1 = SourceHeight - WaterHeight;
                                }
                                else
                                {
                                    y1 = SourceWidth;
                                }
                                //计算宽度 
                                if ((SourceWidth - WaterWidth) > 0) // 源图比水印图宽
                                {
                                    x1 = (SourceWidth - WaterWidth) / 2;
                                }
                                else
                                {
                                    x1 = SourceWidth;
                                }
                                x1 = x1 + PicLeft;
                                y1 = y1 - PicTop;
                                break;
                        }
                        if ((SourceHeight - WaterHeight) > 0)
                        {
                            h = WaterHeight;
                        }
                        else
                        {
                            h = SourceHeight;
                        }
                        if ((SourceWidth - WaterWidth) > 0)
                        {
                            w = WaterWidth;
                        }
                        else
                        {
                            w = SourceWidth;
                        }
                    }
                    else//源图比水印图小 
                    {
                        x1 = 0;
                        y1 = 0;
                        x1 = x1 + PicLeft;
                        y1 = y1 + PicTop;
                        w = SourceWidth;
                        h = SourceHeight;
                    }
                    al = Convert.ToInt32(al * 2.55);
                    //开始绘图 
                    for (x = 1; x < w; x++)
                    {
                        for (y = 1; y < h; y++)
                        {
                            try
                            {
                                rl = WaterImage.GetPixel(x, y).R;
                                gl = WaterImage.GetPixel(x, y).G;
                                bl = WaterImage.GetPixel(x, y).B;
                            }
                            catch
                            {
                                continue;
                            }
                            g.DrawEllipse(new Pen(new SolidBrush(Color.FromArgb(al, rl, gl, bl))), x1 + x, y1 + y, 1, 1);
                        }
                    }
                    g.Save();
                    g.Dispose();
                    WaterImage.Dispose();
                    SourceImage.Save(DestFilePath);
                    SourceImage.Dispose();
                }
            }
        }

        /// <summary>   
        /// 给图片上水印   
        /// </summary>   
        /// <param name="ImgStream">原图片地址</param>   
        /// <param name="WaterStream">水印图片地址</param>   
        public static MemoryStream MarkWater(MemoryStream ImgStream, MemoryStream WaterStream, string FileName, int imgWidth, int imgHeight, int imgWaterWidth, int imgWaterHeight)
        {
            //GIF不水印   
            int i = FileName.LastIndexOf(".");
            string ex = FileName.Substring(i, FileName.Length - i);
            if (string.Compare(ex, ".gif", true) == 0)
            {
                return null;
            }
            string[] allowImageType = { ".jpg", ".gif", ".png", ".bmp", ".tiff", ".wmf", ".ico", ".jpeg" };
            System.Drawing.Imaging.ImageFormat imageType = System.Drawing.Imaging.ImageFormat.Jpeg;
            switch (ex.ToLower())
            {
                case ".jpg": imageType = System.Drawing.Imaging.ImageFormat.Jpeg; break;
                case ".gif": imageType = System.Drawing.Imaging.ImageFormat.Gif; break;
                case ".png": imageType = System.Drawing.Imaging.ImageFormat.Png; break;
                case ".bmp": imageType = System.Drawing.Imaging.ImageFormat.Bmp; break;
                case ".tif": imageType = System.Drawing.Imaging.ImageFormat.Tiff; break;
                case ".wmf": imageType = System.Drawing.Imaging.ImageFormat.Wmf; break;
                case ".ico": imageType = System.Drawing.Imaging.ImageFormat.Icon; break;
                default: break;
            }

            int lucencyPercent = 25;//透明度
            System.Drawing.Image modifyImage = null;
            System.Drawing.Image drawedImage = null;
            System.Drawing.Graphics g = null;
            System.Drawing.Bitmap BasicBitmap = null;
            System.Drawing.Bitmap WaterBitmap = null;
            System.Drawing.Image.GetThumbnailImageAbort callb = null;
            try
            {
                //建立图形对象   (图片不能过大)
                modifyImage = System.Drawing.Image.FromStream(ImgStream, true);
                drawedImage = System.Drawing.Image.FromFile(HttpContext.Current.Server.MapPath("/WaterMarks/TestWater.jpg"));

                //调整图片大小
                if (imgWidth <= 100 || imgHeight <= 100)
                {
                    imgWidth = modifyImage.Width;
                    imgHeight = modifyImage.Height;
                }
                //调整原图大小
                //BasicBitmap = new System.Drawing.Bitmap(modifyImage, imgWidth, imgHeight);
                BasicBitmap = new System.Drawing.Bitmap(modifyImage.GetThumbnailImage(imgWidth, imgHeight, callb, new System.IntPtr()));
                //设置分辨率
                BasicBitmap.SetResolution(modifyImage.HorizontalResolution, modifyImage.VerticalResolution);

                //创建图片画布
                //g = System.Drawing.Graphics.FromImage(modifyImage);
                g = System.Drawing.Graphics.FromImage(BasicBitmap);
                //消除锯齿
                g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;

                //水印图片大小
                if (imgWaterWidth == 0 || imgWaterHeight == 0)
                {
                    imgWaterWidth = drawedImage.Width;
                    imgWaterHeight = drawedImage.Height;
                }
                if (imgWaterWidth >= imgWidth / 5)
                {
                    imgWaterWidth = imgWidth / 5;
                }
                else
                {
                    imgWaterWidth = drawedImage.Width;
                }
                if (imgWaterHeight >= imgHeight / 5)
                {
                    imgWaterHeight = imgHeight / 5;
                }
                else
                {
                    imgWaterHeight = drawedImage.Height;
                }
                //调整水印大小
                //WaterBitmap = new System.Drawing.Bitmap(drawedImage, imgWaterWidth, imgWaterHeight);
                WaterBitmap = new System.Drawing.Bitmap(drawedImage.GetThumbnailImage(imgWaterWidth, imgWaterHeight, callb, new System.IntPtr()));//设置分辨率
                BasicBitmap.SetResolution(drawedImage.HorizontalResolution, drawedImage.VerticalResolution);

                //获取要绘制图形坐标   
                int x = imgWidth - imgWaterWidth;
                int y = imgHeight - imgWaterHeight;

                //设置颜色矩阵   
                float[][] matrixItems ={
                new float[] {1, 0, 0, 0, 0},
                new float[] {0, 1, 0, 0, 0},
                new float[] {0, 0, 1, 0, 0},
                new float[] {0, 0, 0, (float)lucencyPercent/100f, 0},
                new float[] {0, 0, 0, 0, 1}};

                System.Drawing.Imaging.ColorMatrix colorMatrix = new System.Drawing.Imaging.ColorMatrix(matrixItems);
                System.Drawing.Imaging.ImageAttributes imgAttr = new System.Drawing.Imaging.ImageAttributes();

                //将颜色矩阵添加到属性
                imgAttr.SetColorMatrix(colorMatrix, System.Drawing.Imaging.ColorMatrixFlag.Default, System.Drawing.Imaging.ColorAdjustType.Bitmap);

                System.Drawing.Point[] Points = new System.Drawing.Point[] {
                    new System.Drawing.Point(0, 0)
                };

                //绘制阴影图像   
                g.DrawImage(WaterBitmap, new System.Drawing.Rectangle(x, y, imgWaterWidth, imgWaterHeight), 10, 10, imgWaterWidth, imgWaterHeight, System.Drawing.GraphicsUnit.Pixel, imgAttr);

                //保存文件
                g.Save();
                MemoryStream ms = new MemoryStream();
                BasicBitmap.Save(ms, imageType);
                BasicBitmap.Dispose();
                WaterBitmap.Dispose();
                modifyImage.Dispose();
                drawedImage.Dispose();
                g.Dispose();

                return ms;
            }
            catch
            {
                return ImgStream;
            }
            finally
            {
                try
                {
                    BasicBitmap.Dispose();
                    WaterBitmap.Dispose();
                    modifyImage.Dispose();
                    drawedImage.Dispose();
                    g.Dispose();
                }
                catch
                {
                }
            }
        }

        /// <summary>
        /// 添加水印
        /// </summary>
        /// <param name="img">需要添加水印的原图</param>
        /// <param name="WaterType">Text：文字 Temp是文字，Image：图片 Temp是水印图片地址</param>
        /// <returns>添加好水印的图片</returns>
        /// <param name="TextFontFamily">文字字体</param>
        /// <param name="Bold">是否粗体</param>
        /// <param name="TextColor">字体颜色</param>
        /// <param name="WaterText">水印图片位置</param>
        /// <param name="MarkX">标记位置横坐标</param>
        /// <param name="MarkY">标记位置纵坐标</param>
        /// <param name="Alpha">值的范围是0~255，0表示完全透明，255表示不透明。</param>
        public static System.Drawing.Image MarkWaterText(System.Drawing.Image img, string WaterText, int MarkX, int MarkY, string TextFontFamily, bool Bold, System.Drawing.Color TextColor, int Alpha)
        {
            System.Drawing.Bitmap newBitmap = null;
            System.Drawing.Graphics g = null;
            try
            {
                //根据字数判断字体大小
                int[] sizes = new int[] { 96, 48, 32, 16, 8, 6, 4 };
                if (string.IsNullOrEmpty(WaterText))
                {
                    WaterText = "测试数据";
                }
                //根据源图片生成新的Bitmap对象作为作图区，为了给gif图片添加水印，才有此周折
                newBitmap = new System.Drawing.Bitmap(img.Width, img.Height, System.Drawing.Imaging.PixelFormat.Format24bppRgb);
                //设置新建位图得分辨率
                newBitmap.SetResolution(img.HorizontalResolution, img.VerticalResolution);
                //创建Graphics对象，以对该位图进行操作
                g = System.Drawing.Graphics.FromImage(newBitmap);
                //消除图像锯齿
                g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
                //将原图拷贝到作图区
                g.DrawImage(img, new System.Drawing.Rectangle(0, 0, img.Width, img.Height), 10, 10, img.Width, img.Height, System.Drawing.GraphicsUnit.Pixel);
                //声明字体对象
                System.Drawing.Font cFont = null;
                //用来测试水印文本长度得尺子
                System.Drawing.SizeF size = new System.Drawing.SizeF();
                //探测出一个适合图片大小得字体大小，以适应水印文字大小得自适应
                for (int i = 0; i < 6; i++)
                {
                    //创建一个字体对象
                    cFont = new System.Drawing.Font(TextFontFamily, sizes[i]);
                    //是否加粗
                    if (!Bold)
                    {
                        cFont = new System.Drawing.Font(TextFontFamily, sizes[i], System.Drawing.FontStyle.Regular);
                    }
                    else
                    {
                        cFont = new System.Drawing.Font(TextFontFamily, sizes[i], System.Drawing.FontStyle.Bold);
                    }
                    //测量文本大小
                    size = g.MeasureString(WaterText, cFont);
                    //匹配第一个符合要求得字体大小
                    if ((ushort)size.Width < (ushort)img.Width)
                    {
                        if ((ushort)size.Height < (ushort)(img.Height - MarkY))
                            break;
                    }
                }

                //如Color.FromArgb(120,255,255,255)。FromArgb有四个参数，第一个就指定了Alpha值。
                //后面三个是颜色值RGB。
                //Alpha值的范围是0~255，0表示完全透明，255表示不透明。
                //创建刷子对象，准备给图片写上文字
                System.Drawing.Brush brush = new System.Drawing.SolidBrush(System.Drawing.Color.FromArgb(80, TextColor.R, TextColor.G, TextColor.B));

                //消除文字锯齿
                g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.AntiAlias;
                //在指定得位置写上文字
                g.DrawString(WaterText, cFont, brush, MarkX, MarkY);
                //释放Graphics对象
                g.Dispose();
                //将生成得图片读入MemoryStream
                System.IO.MemoryStream ms = new System.IO.MemoryStream();
                newBitmap.Save(ms, System.Drawing.Imaging.ImageFormat.Jpeg);
                //重新生成Image对象
                img = System.Drawing.Image.FromStream(ms);
                //返回新的Image对象
                return img;
            }
            catch
            {
                return img;
            }
            finally
            {
                try
                {
                    newBitmap.Dispose();
                    g.Dispose();
                }
                catch
                {
                }
            }
        }

        /// <summary>
        /// 添加水印
        /// </summary>
        /// <param name="img">需要添加水印的原图</param>
        /// <param name="WaterType">Text：文字 Temp是文字，Image：图片 Temp是水印图片地址</param>
        /// <returns>添加好水印的图片</returns>
        /// <param name="TextFontFamily">文字字体</param>
        /// <param name="Bold">是否粗体</param>
        /// <param name="TextColor">字体颜色</param>
        /// <param name="Temp">水印图片位置</param>
        /// <param name="MarkX">标记位置横坐标</param>
        /// <param name="MarkY">标记位置纵坐标</param>
        public static System.Drawing.Image Mark(System.Drawing.Image img, String WaterType, string Temp, int MarkX, int MarkY, string TextFontFamily, bool Bold, System.Drawing.Color TextColor)
        {
            //根据字数判断字体大小
            //int[] sizes = new int[] { 16, 16, 16, 16, 16, 16 };
            int[] sizes = new int[] { 48, 32, 16, 8, 6, 4 };
            if (string.IsNullOrEmpty(WaterType))
            {
                WaterType = "Text";
            }
            if (string.IsNullOrEmpty(Temp))
            {
                Temp = "测试数据";
            }
            try
            {
                //添加文字水印
                if (WaterType == "Text")
                {
                    //根据源图片生成新的Bitmap对象作为作图区，为了给gif图片添加水印，才有此周折
                    System.Drawing.Bitmap newBitmap = new System.Drawing.Bitmap(img.Width, img.Height, System.Drawing.Imaging.PixelFormat.Format24bppRgb);
                    //设置新建位图得分辨率
                    newBitmap.SetResolution(img.HorizontalResolution, img.VerticalResolution);
                    //创建Graphics对象，以对该位图进行操作
                    System.Drawing.Graphics g = System.Drawing.Graphics.FromImage(newBitmap);
                    //消除图像锯齿
                    g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
                    //将原图拷贝到作图区
                    g.DrawImage(img, new System.Drawing.Rectangle(0, 0, img.Width, img.Height), 10, 10, img.Width, img.Height, System.Drawing.GraphicsUnit.Pixel);
                    //声明字体对象
                    System.Drawing.Font cFont = null;
                    //用来测试水印文本长度得尺子
                    System.Drawing.SizeF size = new System.Drawing.SizeF();
                    //探测出一个适合图片大小得字体大小，以适应水印文字大小得自适应
                    for (int i = 0; i < 6; i++)
                    {
                        //创建一个字体对象
                        cFont = new System.Drawing.Font(TextFontFamily, sizes[i]);
                        //是否加粗
                        if (!Bold)
                        {
                            cFont = new System.Drawing.Font(TextFontFamily, sizes[i], System.Drawing.FontStyle.Regular);
                        }
                        else
                        {
                            cFont = new System.Drawing.Font(TextFontFamily, sizes[i], System.Drawing.FontStyle.Bold);
                        }
                        //测量文本大小
                        size = g.MeasureString(Temp, cFont);
                        //匹配第一个符合要求得字体大小
                        if ((ushort)size.Width < (ushort)img.Width)
                        {
                            break;
                        }
                    }

                    //如Color.FromArgb(120,255,255,255)。FromArgb有四个参数，第一个就指定了Alpha值。
                    //后面三个是颜色值RGB。
                    //Alpha值的范围是0~255，0表示完全透明，255表示不透明。
                    //创建刷子对象，准备给图片写上文字
                    System.Drawing.Brush brush = new System.Drawing.SolidBrush(System.Drawing.Color.FromArgb(80, TextColor.R, TextColor.G, TextColor.B));

                    //消除文字锯齿
                    g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.AntiAlias;
                    //在指定得位置写上文字
                    g.DrawString(Temp, cFont, brush, 10, 10);
                    //释放Graphics对象
                    g.Dispose();
                    //将生成得图片读入MemoryStream
                    System.IO.MemoryStream ms = new System.IO.MemoryStream();
                    newBitmap.Save(ms, System.Drawing.Imaging.ImageFormat.Jpeg);
                    //重新生成Image对象
                    img = System.Drawing.Image.FromStream(ms);
                    //返回新的Image对象
                    return img;
                }
                //添加图像水印
                if (WaterType == "Image")
                {
                    FileInfo nmmFileInfo = new FileInfo(HttpContext.Current.Server.MapPath(Temp));
                    if (!nmmFileInfo.Exists)
                    {
                        return img;
                    }
                    //获得水印图像
                    System.Drawing.Image markImg = System.Drawing.Image.FromFile(nmmFileInfo.FullName);
                    //创建颜色矩阵
                    float[][] ptsArray ={
                    new float[] {1, 0, 0, 0, 0},
                    new float[] {0, 1, 0, 0, 0},
                    new float[] {0, 0, 1, 0, 0},
                    new float[] {0, 0, 0, 0.3f, 0}, //注意：此处为0.0f为完全透明，1.0f为完全不透明
                    new float[] {0, 0, 0, 0, 1}};
                    System.Drawing.Imaging.ColorMatrix colorMatrix = new System.Drawing.Imaging.ColorMatrix(ptsArray);
                    //新建一个Image属性
                    System.Drawing.Imaging.ImageAttributes imageAttributes = new System.Drawing.Imaging.ImageAttributes();
                    //将颜色矩阵添加到属性
                    imageAttributes.SetColorMatrix(colorMatrix, System.Drawing.Imaging.ColorMatrixFlag.Default, System.Drawing.Imaging.ColorAdjustType.Default);
                    //生成位图作图区
                    System.Drawing.Bitmap newBitmap = new System.Drawing.Bitmap(img.Width, img.Height, System.Drawing.Imaging.PixelFormat.Format24bppRgb);
                    //设置分辨率
                    newBitmap.SetResolution(img.HorizontalResolution, img.VerticalResolution);
                    //创建Graphics
                    System.Drawing.Graphics g = System.Drawing.Graphics.FromImage(newBitmap);
                    //消除锯齿
                    g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
                    //拷贝原图到作图区
                    g.DrawImage(img, new System.Drawing.Rectangle(0, 0, img.Width, img.Height), 0, 0, img.Width, img.Height, System.Drawing.GraphicsUnit.Pixel);
                    //如果原图过小
                    if (markImg.Width > img.Width || markImg.Height > img.Height)
                    {
                        System.Drawing.Image.GetThumbnailImageAbort callb = null;
                        //对水印图片生成缩略图,缩小到原图得1/4
                        System.Drawing.Image new_img = markImg.GetThumbnailImage(img.Width / 4, markImg.Height * img.Width / markImg.Width, callb, new System.IntPtr());
                        //添加水印
                        g.DrawImage(new_img, new System.Drawing.Rectangle(MarkX, MarkY, new_img.Width, new_img.Height), 10, 10, new_img.Width, new_img.Height, System.Drawing.GraphicsUnit.Pixel, imageAttributes);
                        //释放缩略图
                        new_img.Dispose();
                        //释放Graphics
                        g.Dispose();
                        //将生成得图片读入MemoryStream
                        System.IO.MemoryStream ms = new System.IO.MemoryStream();
                        newBitmap.Save(ms, System.Drawing.Imaging.ImageFormat.Jpeg);
                        //返回新的Image对象
                        img = System.Drawing.Image.FromStream(ms);
                        return img;
                    }
                    //原图足够大
                    else
                    {
                        //添加水印
                        g.DrawImage(markImg, new System.Drawing.Rectangle(MarkX, MarkY, markImg.Width, markImg.Height), 10, 10, markImg.Width, markImg.Height, System.Drawing.GraphicsUnit.Pixel, imageAttributes);
                        //释放Graphics
                        g.Dispose();
                        //将生成得图片读入MemoryStream
                        System.IO.MemoryStream ms = new System.IO.MemoryStream();
                        newBitmap.Save(ms, System.Drawing.Imaging.ImageFormat.Jpeg);
                        //返回新的Image对象
                        img = System.Drawing.Image.FromStream(ms);
                        return img;
                    }
                }
                return img;
            }
            catch
            {
                return img;
            }
        }

        /// <summary>
        /// 在图片上画 圆形透明图标
        /// </summary>
        /// <param name="ImgStream">图片文件流</param>
        /// <param name="Str">原型图标内的文字</param>
        /// <returns></returns>
        public static MemoryStream IamgeMakeRound(MemoryStream ImgStream, string Str)
        {
            System.Drawing.Graphics g = null;
            System.Drawing.Bitmap BasicBitmap = null;
            System.Drawing.Image.GetThumbnailImageAbort callb = null;

            System.Drawing.Image modifyImage = System.Drawing.Image.FromStream(ImgStream, true);

            //调整原图大小
            BasicBitmap = new System.Drawing.Bitmap(modifyImage.GetThumbnailImage(modifyImage.Width, modifyImage.Height, callb, new System.IntPtr()));
            //设置分辨率
            BasicBitmap.SetResolution(modifyImage.HorizontalResolution, modifyImage.VerticalResolution);

            //创建图片画布
            g = System.Drawing.Graphics.FromImage(BasicBitmap);
            //消除锯齿
            g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;

            System.Drawing.Brush brush = new System.Drawing.SolidBrush(System.Drawing.Color.FromArgb(80, System.Drawing.Color.Green.R, System.Drawing.Color.Green.G, System.Drawing.Color.Green.B));

            System.Drawing.Pen DPen = new System.Drawing.Pen(brush, (float)10);

            //三个点，O为原点，A、B为圆上另外两点
            System.Drawing.Point Point_O = new System.Drawing.Point(200, 200);
            System.Drawing.Point Point_A = new System.Drawing.Point(100, 100);
            System.Drawing.Point Point_B = new System.Drawing.Point(300, 100);

            g.DrawArc(DPen, new System.Drawing.Rectangle(100, 100, 200, 200), (float)0, (float)152);

            g.FillEllipse(brush, 100, 100, 200, 200);//画填充椭圆的方法，x坐标、y坐标、宽、高，如果是100，则半径为50

            //用来测试水印文本长度得尺子
            System.Drawing.SizeF size = new System.Drawing.SizeF();
            System.Drawing.Font cFont = new System.Drawing.Font("宋体", 32, System.Drawing.FontStyle.Bold);
            //测量文本大小
            size = g.MeasureString(Str, cFont);
            //int x = 0;
            //int y = 0;
            g.DrawString(Str, cFont, new System.Drawing.SolidBrush(System.Drawing.Color.FromArgb(255, System.Drawing.Color.Red.R, System.Drawing.Color.Red.G, System.Drawing.Color.Red.B)), 200 - size.Width / 2, 200 - size.Height / 2);

            g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;

            //保存文件
            g.Save();
            MemoryStream ms = new MemoryStream();
            BasicBitmap.Save(ms, System.Drawing.Imaging.ImageFormat.Jpeg);
            BasicBitmap.Dispose();
            modifyImage.Dispose();
            g.Dispose();

            return ms;
        }

        #endregion

        /// <summary>
        /// 按照参数设定对过大的源文件进行等比例缩放
        /// </summary>
        /// <param name="SourceFilePath">源文件路径</param>
        /// <param name="Width">宽度</param>
        /// <param name="Height">高度</param>
        /// <param name="DestFilePath">缩放文件保存路径</param>
        public static void ResizeImageSize(string SourceFilePath, double Width, double Height, string DestFilePath)
        {
            System.Drawing.Image image = System.Drawing.Image.FromFile(SourceFilePath);
            //计算等比例缩放后图片大小
            double NewWidth;
            double NewHeight;
            if (image.Width > image.Height)
            {
                NewWidth = Width;
                NewHeight = image.Height * (NewWidth / image.Width);
            }
            else
            {
                NewHeight = Height;
                NewWidth = image.Width * (NewHeight / image.Height);
            }
            if (NewWidth > Width)
            {
                NewWidth = Width;
            }
            if (NewHeight > Height)
            {
                NewHeight = Height;
            }
            //取得图片大小
            System.Drawing.Size size = new System.Drawing.Size((int)NewWidth, (int)NewHeight);
            //新建一个bmp图片
            System.Drawing.Image bitmap = new System.Drawing.Bitmap(size.Width, size.Height);
            System.Drawing.Graphics graphics = System.Drawing.Graphics.FromImage(bitmap);
            graphics.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.High;
            graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighQuality;
            graphics.Clear(System.Drawing.Color.White);
            graphics.DrawImage(image, new System.Drawing.Rectangle(0, 0, bitmap.Width, bitmap.Height), new System.Drawing.Rectangle(0, 0, image.Width, image.Height), System.Drawing.GraphicsUnit.Pixel);
            try
            {
                bitmap.Save(DestFilePath);
            }
            catch
            { }
            graphics.Dispose();
            image.Dispose();
            bitmap.Dispose();
        }

        /// <summary>
        /// 处理图片灰度
        /// </summary>
        /// <param name="original"></param>
        /// <returns></returns>
        public static Bitmap Image2Gray(Bitmap original)
        {
            //create a blank bitmap the same size as original
            Bitmap newBitmap = new Bitmap(original.Width, original.Height);
            //get a graphics object from the new image
            Graphics g = Graphics.FromImage(newBitmap);
            //create the grayscale ColorMatrix
            System.Drawing.Imaging.ColorMatrix colorMatrix = new System.Drawing.Imaging.ColorMatrix(
               new float[][]
              {
                 new float[] {.3f, .3f, .3f, 0, 0},
                 new float[] {.59f, .59f, .59f, 0, 0},
                 new float[] {.11f, .11f, .11f, 0, 0},
                 new float[] {0, 0, 0, 1, 0},
                 new float[] {0, 0, 0, 0, 1}
              });

            //create some image attributes
            ImageAttributes attributes = new ImageAttributes();
            //set the color matrix attribute
            attributes.SetColorMatrix(colorMatrix);
            //draw the original image on the new image
            //using the grayscale color matrix
            g.DrawImage(original, new Rectangle(0, 0, original.Width, original.Height),
               0, 0, original.Width, original.Height, GraphicsUnit.Pixel, attributes);
            //dispose the Graphics object
            g.Dispose();
            return newBitmap;
        }

        #endregion
    }
}