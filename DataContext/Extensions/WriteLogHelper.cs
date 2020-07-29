using log4net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using DataContext.Models;
using System.Threading;
using System.Diagnostics;
using System.IO;
using System.Text.RegularExpressions;
using System.Text;
using Oracle.ManagedDataAccess.Client;
using System.Data;

namespace DataContext.Extensions
{
    public static class WriteLogHelper
    {
        /// <summary>
        /// 本地日志锁
        /// </summary>
        public static ReaderWriterLock ORWLogLocker = new ReaderWriterLock();

        #region Redis键值

        #region Redis队列键值

        //日志队列
        public static readonly string RedisKeyMessageLog = "WebSrvc_MessageLog";
        public static readonly string RedisKeyLocalLog = "WebSrvc_LocalLog";

        #endregion

        #endregion

        #region 日志

        #region 插入Redis日志

        /// <summary>
        /// 将日志数据写入Redis
        /// </summary>
        /// <param name="OMsg">错误内容</param>
        /// <param name="ORedisMsgType">错误类型（本地，Message类）</param>
        /// <param name="RedisKeyName">Redis得String类型Key值</param>
        /// <param name="FolderPath">本地日志文件地址</param>
        /// <param name="ORedisHelp">Redis帮助类</param>
        /// <returns></returns>
        public static bool AddMessageToRedis(object OMsg, EnumType.RedisLogMsgType ORedisLogMsgType, String RedisKeyName, string FolderPath = "Log", RedisHelp.RedisHelper ORedisHelp = null)
        {
            bool IsOK = false;

            try
            {
                MessageToRedis OMessageToRedis = new MessageToRedis(ORedisLogMsgType);
                OMessageToRedis.OMsg = OMsg;
                OMessageToRedis.FolderPath = FolderPath;
                if (ORedisHelp == null)
                    ORedisHelp = new RedisHelp.RedisHelper();
                //插入Redis队列
                ORedisHelp.ListRightPush<MessageToRedis>(RedisKeyName, OMessageToRedis);
                IsOK = true;
            }
            catch (Exception ex)
            {
                string ErrMsgStr = Common.GetExceptionMsg(ex);
                WriteLog_Local(ErrMsgStr, "Log\\AddMessageToRedis");
            }

            return IsOK;
        }

        #endregion

        #region 写入本地日志

        /// <summary>
        /// 写入本地日志（加锁）
        /// </summary>
        /// <param name="ErrMsgStr">错误信息</param>
        /// <param name="FolderPath">日志文件地址</param>
        /// <param name="DayFilePathName">日志文件地址按天文件夹记录</param>
        /// <param name="AddToRedis">日志存储到Redis</param>
        /// <param name="HourFileName">日志文件地址按小时文件夹记录</param>
        /// <param name="FileName">日志文件名称</param>
        /// <param name="AddDate">写入日志时间Redis</param>
        /// <returns></returns>
        public static bool WriteLog_Local(string ErrMsgStr, string FolderPath = "Log", bool DayFilePathName = true, bool AddToRedis = false, bool HourFileName = false, string FileName = "", DateTime? AddDate = null)
        {
            bool retTF = true;
            if (AddToRedis)
            {
                try
                {
                    retTF = AddMessageToRedis(ErrMsgStr, EnumType.RedisLogMsgType.LocalLog, RedisKeyLocalLog, FolderPath);
                }
                catch (Exception)
                {
                    retTF = false;
                }
            }
            else
            {
                try
                {
                    ORWLogLocker.AcquireWriterLock(1000);
                    try
                    {
                        if (AddDate == null)
                            WriteLog(ErrMsgStr, FolderPath, DayFilePathName, HourFileName, FileName);
                        else
                            NewWriteLog(ErrMsgStr, FolderPath, AddDate, DayFilePathName, HourFileName, FileName);
                    }
                    catch (Exception ex)
                    {
                        if (string.IsNullOrWhiteSpace(FileName))
                            FileName = Guid.NewGuid().ToString();
                        else
                            FileName = Guid.NewGuid().ToString() + FileName;

                        string ErrMsg = Common.GetExceptionMsg(ex);
                        if (AddDate == null)
                            WriteLog(ErrMsg + "-" + ErrMsgStr, FolderPath, DayFilePathName, HourFileName, FileName);
                        else
                            NewWriteLog(ErrMsg + "-" + ErrMsgStr, FolderPath, AddDate, DayFilePathName, HourFileName, FileName);
                    }
                    finally
                    {
                        if (ORWLogLocker.IsWriterLockHeld)
                            ORWLogLocker.ReleaseWriterLock();
                    }
                }
                catch (Exception ex)
                {
                    if (ORWLogLocker.IsWriterLockHeld)
                        ORWLogLocker.ReleaseWriterLock();
                    retTF = false;
                    ErrMsgStr += Common.GetExceptionMsg(ex);

                    if (string.IsNullOrWhiteSpace(FileName))
                        FileName = Guid.NewGuid().ToString();
                    else
                        FileName = Guid.NewGuid().ToString() + FileName;
                    if (AddDate == null)
                        WriteLogHelper.WriteLog(ErrMsgStr, FolderPath, DayFilePathName, HourFileName, FileName);
                    else
                        WriteLogHelper.NewWriteLog(ErrMsgStr, FolderPath, AddDate, DayFilePathName, HourFileName, FileName);
                }
            }
            return retTF;
        }

        /// <summary>
        /// 写入本地日志（加锁）
        /// </summary>
        /// <param name="ErrMsgStr">错误信息</param>
        /// <param name="FolderPath">日志文件地址</param>
        /// <param name="DayFilePathName">日志文件地址按天文件夹记录</param>
        /// <param name="HourFileName">日志文件地址按小时文件夹记录</param>
        /// <param name="FileName">日志文件名称</param>
        /// <param name="AddDate">写入日志时间</param>
        /// <param name="_ORWLogLocker">读写锁</param>
        /// <returns></returns>
        public static bool WriteLog_LocalByRWLogLocker(string ErrMsgStr, string FolderPath = "Log", bool DayFilePathName = true,
            bool HourFileName = false, string FileName = "", DateTime? AddDate = null, ReaderWriterLock _ORWLogLocker = null)
        {
            bool retTF = true;
            ReaderWriterLock New_ORWLogLocker = ORWLogLocker;

            try
            {
                if (_ORWLogLocker != null)
                    New_ORWLogLocker = _ORWLogLocker;
                //开启写入锁
                New_ORWLogLocker.AcquireWriterLock(1000);
                try
                {
                    if (AddDate == null)
                        WriteLogHelper.WriteLog(ErrMsgStr, FolderPath, DayFilePathName, HourFileName, FileName);
                    else
                        WriteLogHelper.NewWriteLog(ErrMsgStr, FolderPath, AddDate, DayFilePathName, HourFileName, FileName);
                }
                catch (Exception ex)
                {
                    if (string.IsNullOrWhiteSpace(FileName))
                        FileName = Guid.NewGuid().ToString();
                    else
                        FileName = Guid.NewGuid().ToString() + FileName;

                    string ErrMsg = Common.GetExceptionMsg(ex);
                    if (AddDate == null)
                        WriteLogHelper.WriteLog(ErrMsg + "-" + ErrMsgStr, FolderPath, DayFilePathName, HourFileName, FileName);
                    else
                        WriteLogHelper.NewWriteLog(ErrMsg + "-" + ErrMsgStr, FolderPath, AddDate, DayFilePathName, HourFileName, FileName);
                }
                finally
                {
                    if (New_ORWLogLocker.IsWriterLockHeld)
                        New_ORWLogLocker.ReleaseWriterLock();
                }
            }
            catch (Exception ex)
            {
                if (New_ORWLogLocker.IsWriterLockHeld)
                    New_ORWLogLocker.ReleaseWriterLock();
                retTF = false;
                ErrMsgStr += Common.GetExceptionMsg(ex);

                if (string.IsNullOrWhiteSpace(FileName))
                    FileName = Guid.NewGuid().ToString();
                else
                    FileName = Guid.NewGuid().ToString() + FileName;
                if (AddDate == null)
                    WriteLogHelper.WriteLog(ErrMsgStr, FolderPath, DayFilePathName, HourFileName, FileName);
                else
                    WriteLogHelper.NewWriteLog(ErrMsgStr, FolderPath, AddDate, DayFilePathName, HourFileName, FileName);
            }

            return retTF;
        }

        #endregion

        #region Log4Net

        /// <summary>
        /// Log4Net写日志
        /// level（级别）：标识这条日志信息的重要级别None>Fatal>ERROR>WARN>DEBUG>INFO>ALL，设定一个
        /// </summary>
        /// <param name="ErrMSg">错误信息</param>
        /// <param name="OLog4NetMsgType">错误类型</param>
        /// <param name="ex">错误堆栈</param>
        public static void WriteLogByLog4Net(string ErrMSg, EnumType.Log4NetMsgType OLog4NetMsgType = EnumType.Log4NetMsgType.Info, Exception ex = null)
        {
            ILog log = log4net.LogManager.GetLogger("WebApp");
            switch (OLog4NetMsgType)
            {
                case EnumType.Log4NetMsgType.Fatal:
                    if (ex != null)
                        log.Fatal(ErrMSg, ex);//严重错误
                    else
                        log.Fatal(ErrMSg);//严重错误
                    break;
                case EnumType.Log4NetMsgType.Error:
                    if (ex != null)
                        log.Error(ErrMSg, ex);//错误
                    else
                        log.Error(ErrMSg);//错误
                    break;
                case EnumType.Log4NetMsgType.Warn:
                    if (ex != null)
                        log.Warn(ErrMSg, ex);//记录警告信息
                    else
                        log.Warn(ErrMSg);//记录警告信息
                    break;
                case EnumType.Log4NetMsgType.Debug:
                    if (ex != null)
                        log.Debug(ErrMSg, ex);//记录调试信息
                    else
                        log.Debug(ErrMSg);//记录调试信息
                    break;
                case EnumType.Log4NetMsgType.Info:
                    if (ex != null)
                        log.Info(ErrMSg, ex); //记录一般信息
                    else
                        log.Info(ErrMSg); //记录一般信息
                    break;
                default:
                    break;
            }
        }

        /// <summary>
        /// 写日志
        /// </summary>
        /// <param name="ex">错误堆栈</param>
        /// <param name="OLog4NetMsgType">错误类型</param>
        public static void WriteLogByLog4Net(Exception ex, EnumType.Log4NetMsgType OLog4NetMsgType = EnumType.Log4NetMsgType.Info)
        {
            ILog log = log4net.LogManager.GetLogger("WebApp");
            string ErrMSg = "";
            if (ex == null)
            {
                ErrMSg = Common.GetExceptionMsg(ex);
            }
            switch (OLog4NetMsgType)
            {
                case EnumType.Log4NetMsgType.Fatal:
                    if (ex != null)
                        log.Fatal(ErrMSg, ex);//严重错误
                    else
                        log.Fatal(ErrMSg);//严重错误
                    break;
                case EnumType.Log4NetMsgType.Error:
                    if (ex != null)
                        log.Error(ErrMSg, ex);//错误
                    else
                        log.Error(ErrMSg);//错误
                    break;
                case EnumType.Log4NetMsgType.Warn:
                    if (ex != null)
                        log.Warn(ErrMSg, ex);//记录警告信息
                    else
                        log.Warn(ErrMSg);//记录警告信息
                    break;
                case EnumType.Log4NetMsgType.Debug:
                    if (ex != null)
                        log.Debug(ErrMSg, ex);//记录调试信息
                    else
                        log.Debug(ErrMSg);//记录调试信息
                    break;
                case EnumType.Log4NetMsgType.Info:
                    if (ex != null)
                        log.Info(ErrMSg, ex); //记录一般信息
                    else
                        log.Info(ErrMSg); //记录一般信息
                    break;
                default:
                    break;
            }
        }

        #endregion

        #endregion

        #region Redis异步推送

        /// <summary>
        /// 添加 错误信息然后推送
        /// </summary>
        /// <param name="ErrMsg">错误信息</param>
        /// <param name="Receiver">错误信息</param>
        /// <param name="SunChannel">推送频道</param>
        /// <param name="ORedisHelp">redis 链接</param>
        /// <param name="delay">延迟推送时间（毫秒，默认 0：不延迟）</param>
        public static void AddRedisToSubscribe(string ErrMsg, string Receiver, string SunChannel, RedisHelp.RedisHelper ORedisHelp = null, string Sender = "TM_Auto", int delay = 0)
        {
            if (!string.IsNullOrWhiteSpace(ErrMsg))
            {
                if (ORedisHelp == null)
                    ORedisHelp = new RedisHelp.RedisHelper();
                AutoSubscribeMsg OMsg = new AutoSubscribeMsg();
                OMsg.MSG = ErrMsg;
                OMsg.reciver = Receiver;
                OMsg.sender = Sender;
                OMsg.delay = delay;
                OMsg.type = "";
                ORedisHelp.Publish(SunChannel, Newtonsoft.Json.JsonConvert.SerializeObject(OMsg));
            }
        }

        #endregion

        #region 执行windows程序

        /// <summary>
        /// 运行新的进程（CMD，WinRar命令行等）
        /// 不显示命令窗口
        /// </summary>
        /// <param name ="processName">进程名称(完整路径，环境变量里 有配置时，不要完整路径)</param>
        /// <param name="cmdPath">命令行跳转路径(cmd.exe时，可能需要)</param>
        /// <param name="cmdStr">执行命令行参数</param>
        /// <param name="WaitForExit">执行命令行超时时间（毫秒）</param>
        public static Tuple<bool, string> RunCmd(string processName, string cmdStr, string cmdPath = "", int WaitForExit = 300000)
        {
            bool result = false;
            try
            {
                //没有命令 不执行
                if (string.IsNullOrEmpty(processName) || string.IsNullOrEmpty(cmdStr))
                    return new Tuple<bool, string>(result, "没有进程名称和要执行的命令");
                //执行路径 不存在 不执行
                if (!string.IsNullOrEmpty(cmdPath))
                    if (!Directory.Exists(cmdPath))
                        return new Tuple<bool, string>(result, "命令行跳转路径不存在");
                using (Process myPro = new Process())
                {
                    myPro.StartInfo.FileName = processName;       //执行进程名称
                    myPro.StartInfo.UseShellExecute = false;      //是否使用操作系统shell启动
                    myPro.StartInfo.RedirectStandardInput = true; //接受来自调用程序的输入信息
                    myPro.StartInfo.RedirectStandardOutput = true;//由调用程序获取输出信息
                    myPro.StartInfo.RedirectStandardError = true; //重定向标准错误输出
                    myPro.StartInfo.CreateNoWindow = true;        //不显示程序窗口

                    myPro.Start();

                    //不执行路径 跳转命令
                    if (!string.IsNullOrEmpty(cmdPath))
                    {
                        //如果调用程序路径中有空格时，cmd命令执行失败，可以用双引号括起来 
                        var CMDPath = "cd /d \"" + cmdPath + "\" ";
                        myPro.StandardInput.WriteLine(CMDPath);
                    }

                    //在前面的命令执行完成后，要加exit命令，否则后面调用ReadtoEnd()命令会假死。
                    var CMDStr = string.Format(@"{0} {1} ", cmdStr, "&exit");
                    myPro.StandardInput.WriteLine(CMDStr);

                    //p.StandardInput.WriteLine("exit");
                    //向标准输入写入要执行的命令。这里使用&是批处理命令的符号，表示前面一个命令不管是否执行成功都执行后面(exit)命令，如果不执行exit命令，后面调用ReadToEnd()方法会假死
                    //同类的符号还有&&和||前者表示必须前一个命令执行成功才会执行后面的命令，后者表示必须前一个命令执行失败才会执行后面的命令

                    myPro.StandardInput.AutoFlush = true;
                    var bo = myPro.WaitForExit(WaitForExit);//设定5分钟
                    if (!bo)
                    {
                        myPro.Kill();
                        return new Tuple<bool, string>(false, "执行命令 超时");
                    }
                    else
                    {
                        //获取cmd窗口的输出信息
                        string output = myPro.StandardOutput.ReadToEnd();
                        return new Tuple<bool, string>(true, output);
                    }
                }
            }
            catch (Exception ex)
            {
                var ErrMsg = Common.GetExceptionMsg(ex);
                return new Tuple<bool, string>(false, ErrMsg);
            }
        }

        /// <summary>
        /// 运行新的进程
        /// </summary>
        /// <param name="cmdPath">指定应用程序的完整路径</param>
        /// <param name="cmdStr">执行命令行参数</param>
        /// <param name="WaitForExit">执行命令行超时时间（毫秒）</param>
        public static bool RunCmd_(string cmdExePath, string cmdStr, int WaitForExit = 300000)
        {
            bool result = false;
            try
            {
                using (Process myPro = new Process())
                {
                    //指定启动进程是调用的应用程序和命令行参数
                    ProcessStartInfo psi = new ProcessStartInfo(cmdExePath, cmdStr);
                    //psi.FileName = processName;
                    psi.UseShellExecute = false;
                    psi.RedirectStandardInput = true;
                    psi.RedirectStandardOutput = true;
                    psi.RedirectStandardError = true;
                    psi.CreateNoWindow = true;

                    myPro.StartInfo = psi;
                    myPro.Start();
                    var po = myPro.WaitForExit(WaitForExit);
                    if (!po)
                        myPro.Kill();
                    result = true;
                }
            }
            catch
            {

            }
            return result;
        }

        #endregion

        #region Write Log information

        /// <summary>
        /// WriteLog
        /// </summary>
        /// <param name="nmm_strMessage">内容</param>
        /// <param name="LogFilePathName">日志目录名</param>
        /// <param name="DayFilePathName">是否生成 日目录</param>
        /// <param name="HourFileName">是否生成 小时文件名</param>
        /// <param name="FileName">文件名称（HourFileName为true时，设置无效）</param>
        public static void WriteLog(string nmm_strMessage, string LogFilePathName = "WriteLogHelper", bool DayFilePathName = false, bool HourFileName = false, string FileName = "")
        {
            string retErrMsg = "";
            StreamWriter nmm_StreamWriter = null;
            Stream nmm_Stream = null;
            try
            {
                string LogDirPath = "";
                //if (HttpContext.Current == null)
                //{
                if (System.Environment.CurrentDirectory == AppDomain.CurrentDomain.BaseDirectory)//Windows应用程序则相等   
                {
                    LogDirPath = AppDomain.CurrentDomain.BaseDirectory + "Log\\" +  LogFilePathName + "\\";
                }
                else
                {
                    LogDirPath = AppDomain.CurrentDomain.BaseDirectory + "Log\\" + LogFilePathName + "\\";
                }
                //}
                //else
                //    LogDirPath = HttpContext.Current.Server.MapPath("/Log");
                string reg = @"\:" + @"|\;" + @"|\/" + @"|\\" + @"|\|" + @"|\," + @"|\*" + @"|\?" + @"|\""" + @"|\<" + @"|\>";//特殊字符
                Regex r = new Regex(reg);
                string txtName = string.IsNullOrEmpty(FileName) ? DateTime.Now.ToString("yyyy-MM-dd") : FileName.Replace("\\", "-");
                txtName = r.Replace(txtName, "_");//将特殊字符替换为""
                LogDirPath += DateTime.Now.ToString("yyyy-MM");
                if (DayFilePathName)
                {
                    LogDirPath += "\\" + DateTime.Now.ToString("dd") + "号";
                }
                if (HourFileName)
                {
                    LogDirPath += "\\" + DateTime.Now.ToString("HH") + "点";
                    txtName = DateTime.Now.ToString("yyyy-MM-dd HH");
                }

                if (!Directory.Exists(LogDirPath))
                    Directory.CreateDirectory(LogDirPath);
                string nmm_strFileName = LogDirPath + "\\" + txtName + ".txt";
                //file not exists                        
                if (File.Exists(nmm_strFileName))
                    nmm_Stream = new FileStream(nmm_strFileName, System.IO.FileMode.Append);
                //file not exists  
                else
                    nmm_Stream = new FileStream(nmm_strFileName, System.IO.FileMode.Create);
                nmm_StreamWriter = new StreamWriter(nmm_Stream, Encoding.Unicode);
                nmm_strMessage = DateTime.Now.ToString() + ":  " + nmm_strMessage;
                //Write log
                nmm_StreamWriter.WriteLine(nmm_strMessage);
                nmm_StreamWriter.Flush();
            }
            catch (Exception ex)
            {
                retErrMsg = GetExceptionMsg(ex);
            }
            finally
            {
                if (nmm_StreamWriter != null)
                {
                    nmm_StreamWriter.Close();
                }
                if (nmm_Stream != null)
                    nmm_Stream.Close();
            }
            //return retErrMsg;
        }

        /// <summary>
        /// WriteLog
        /// </summary>
        /// <param name="nmm_strMessage">内容</param>
        /// <param name="LogFilePathName">日志目录名</param>
        /// <param name="NowTime">时间</param>
        /// <param name="DayFilePathName">是否生成 日目录</param>
        /// <param name="HourFileName">是否生成 小时文件名</param>
        /// <param name="FileName">文件名称（HourFileName为true时，设置无效）</param>
        public static void NewWriteLog(string nmm_strMessage, string LogFilePathName = "WriteLogHelper", DateTime? _NowTime = null, bool DayFilePathName = false, bool HourFileName = false, string FileName = "")
        {
            string retErrMsg = "";
            try
            {
                DateTime NowTime = _NowTime == null ? DateTime.Now : (Convert.ToDateTime(_NowTime));

                string LogDirPath = "";
                //if (HttpContext.Current == null)
                //{
                if (System.Environment.CurrentDirectory == AppDomain.CurrentDomain.BaseDirectory)//Windows应用程序则相等   
                {
                    LogDirPath = AppDomain.CurrentDomain.BaseDirectory + "Log\\" + LogFilePathName + "\\";
                }
                else
                {
                    LogDirPath = AppDomain.CurrentDomain.BaseDirectory + "Log\\" + LogFilePathName + "\\";
                }
                //}
                //else
                //    LogDirPath = HttpContext.Current.Server.MapPath("/Log");
                string reg = @"\:" + @"|\;" + @"|\/" + @"|\\" + @"|\|" + @"|\," + @"|\*" + @"|\?" + @"|\""" + @"|\<" + @"|\>";//特殊字符
                Regex r = new Regex(reg);
                string txtName = string.IsNullOrEmpty(FileName) ? DateTime.Now.ToString("yyyy-MM-dd") : FileName.Replace("\\", "-");
                txtName = r.Replace(txtName, "_");//将特殊字符替换为""
                LogDirPath += NowTime.ToString("yyyy-MM");
                if (DayFilePathName)
                {
                    LogDirPath += "\\" + NowTime.ToString("dd") + "号";
                }
                if (HourFileName)
                {
                    LogDirPath += "\\" + NowTime.ToString("HH") + "点";
                    txtName = NowTime.ToString("yyyy-MM-dd HH");
                }

                if (!Directory.Exists(LogDirPath))
                    Directory.CreateDirectory(LogDirPath);
                string nmm_strFileName = LogDirPath + "\\" + txtName + ".txt";
                Stream nmm_Stream;
                //file not exists                        
                if (File.Exists(nmm_strFileName))
                    nmm_Stream = new FileStream(nmm_strFileName, System.IO.FileMode.Append);
                //file not exists  
                else
                    nmm_Stream = new FileStream(nmm_strFileName, System.IO.FileMode.Create);
                StreamWriter nmm_StreamWriter = new StreamWriter(nmm_Stream, Encoding.Unicode);
                nmm_strMessage = NowTime.ToString() + ":  " + nmm_strMessage;
                //Write log
                nmm_StreamWriter.WriteLine(nmm_strMessage);
                nmm_StreamWriter.Flush();
                nmm_Stream.Close();
            }
            catch (Exception ex)
            {
                retErrMsg = GetExceptionMsg(ex);
            }
            //return retErrMsg;
        }

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
            else
            {
                string ErrMsg = ex.Message;
                Exception e_x = ex.InnerException;
                while (e_x != null)
                {
                    if (!string.IsNullOrEmpty(e_x.Message))
                    {
                        ErrMsg = e_x.Message;
                    }
                    e_x = e_x.InnerException;
                }
                return ErrMsg;
                //return ex.InnerException == null ? ex.Message : (ex.InnerException.InnerException == null ? ex.InnerException.Message : ex.InnerException.InnerException.Message);
            }
        }

        /// <summary>
        /// 添加服务错误日志
        /// </summary>
        /// <param name="subject">主题</param>
        /// <param name="key1">键1(Param_s不为空时，取 Param_s里的数据)</param>
        /// <param name="key2">键2</param>
        /// <param name="content">内容</param>
        /// <param name="Param_s">参数</param>
        /// <param name="messageType">Information,Message,Error,Alert,Warning</param>
        /// <param name="NotificationName">提醒类型</param>
        public static void WirteLog(string subject, string key1, string key2, string content, string methodname, OracleParameter[] Param_s, string messageType = "Error", string NotificationName = "TMService")
        {
            try
            {
                string InsertSQLStr = "";

                string Key1_Str = "";
                if (Param_s != null)
                {
                    if (Param_s.Any())
                    {
                        foreach (var item in Param_s)
                        {
                            if (string.IsNullOrEmpty(Key1_Str))
                                Key1_Str += item.ParameterName + ":" + item.Value.ToString();
                            else
                                Key1_Str += "," + item.ParameterName + ":" + item.Value.ToString();
                        }
                    }
                    else
                        Key1_Str = key1;
                }
                else
                    Key1_Str = key1;

                OracleParameter[] OrclParams = new OracleParameter[]{
                    new OracleParameter(":V_ID",OracleDbType.Int16),
                    new OracleParameter(":V_SUBJECT",OracleDbType.NVarchar2,100),
                    new OracleParameter(":V_KEY1",OracleDbType.NVarchar2,100),
                    new OracleParameter(":V_KEY2",OracleDbType.NVarchar2,100),
                    new OracleParameter(":V_CONTENT",OracleDbType.NVarchar2),
                    new OracleParameter(":V_TYPE",OracleDbType.NVarchar2,20),
                    new OracleParameter(":V_NEWDATE",OracleDbType.Date),
                    new OracleParameter(":V_ISSENDED",OracleDbType.Int16),
                    new OracleParameter(":V_SENDDATE",OracleDbType.Date),
                    new OracleParameter(":V_NOTIFICATIONName",OracleDbType.NVarchar2),
                    new OracleParameter(":V_CREATEDDATE",OracleDbType.Date),
                    new OracleParameter(":V_MODIFIEDDATE",OracleDbType.Date),
                    new OracleParameter(":V_CREATEDBY",OracleDbType.NVarchar2,20),
                    new OracleParameter(":V_MODIFIEDBY",OracleDbType.NVarchar2,20),
                    new OracleParameter(":V_OutID", OracleDbType.Int16, ParameterDirection.Output)
                };
                OrclParams.Where(x => x.ParameterName == ":V_ID").FirstOrDefault().Value = 0;
                OrclParams.Where(x => x.ParameterName == ":V_SUBJECT").FirstOrDefault().Value = subject;
                OrclParams.Where(x => x.ParameterName == ":V_KEY1").FirstOrDefault().Value = Key1_Str;
                OrclParams.Where(x => x.ParameterName == ":V_KEY2").FirstOrDefault().Value = key2;
                OrclParams.Where(x => x.ParameterName == ":V_CONTENT").FirstOrDefault().Value = content;
                OrclParams.Where(x => x.ParameterName == ":V_TYPE").FirstOrDefault().Value = messageType;
                OrclParams.Where(x => x.ParameterName == ":V_NEWDATE").FirstOrDefault().Value = DateTime.Now;
                OrclParams.Where(x => x.ParameterName == ":V_ISSENDED").FirstOrDefault().Value = 0;
                OrclParams.Where(x => x.ParameterName == ":V_SENDDATE").FirstOrDefault().Value = DateTime.Now;
                OrclParams.Where(x => x.ParameterName == ":V_NOTIFICATIONName").FirstOrDefault().Value = NotificationName;
                OrclParams.Where(x => x.ParameterName == ":V_CREATEDDATE").FirstOrDefault().Value = DateTime.Now;
                OrclParams.Where(x => x.ParameterName == ":V_MODIFIEDDATE").FirstOrDefault().Value = DBNull.Value;
                OrclParams.Where(x => x.ParameterName == ":V_CREATEDBY").FirstOrDefault().Value = methodname;
                OrclParams.Where(x => x.ParameterName == ":V_MODIFIEDBY").FirstOrDefault().Value = DBNull.Value;
                InsertSQLStr = @"begin INSERTMESSAGE(
                V_ID               => :V_ID                ,
                V_SUBJECT          => :V_SUBJECT           ,
                V_KEY1             => :V_KEY1              ,
                V_KEY2             => :V_KEY2              ,
                V_CONTENT          => :V_CONTENT           ,
                V_TYPE             => :V_TYPE              ,
                V_NEWDATE          => :V_NEWDATE           ,
                V_ISSENDED         => :V_ISSENDED          ,
                V_SENDDATE         => :V_SENDDATE          ,
                V_NOTIFICATIONName => :V_NOTIFICATIONName  ,
                V_CREATEDDATE      => :V_CREATEDDATE       ,
                V_MODIFIEDDATE     => :V_MODIFIEDDATE      ,
                V_CREATEDBY        => :V_CREATEDBY         ,
                V_MODIFIEDBY       => :V_MODIFIEDBY        ,
                V_OutID            => :V_OutID); 
                end;";

                SQLDALHelper.OracleHelper.ExecuteNonQuery(CommandType.Text, InsertSQLStr, OrclParams);

                object retObj = OrclParams.Where(x => x.ParameterName == ":V_OutID").FirstOrDefault().Value.ToString();
                int retNumber = 0;
                if (retObj != null)
                    retNumber = Convert.ToInt16(retObj);

                //string name = NotificationName;
                //if (notification != null)
                //{
                //    KSWECDS.Web.Models.Message message = new KSWECDS.Web.Models.Message();
                //    message.Content = content;
                //    message.Key1 = Key1_Str;
                //    message.Key2 = key2;
                //    message.CreatedDate = DateTime.Now;
                //    message.NewDate = DateTime.Now;
                //    message.NotificationId = notification.Id;
                //    message.Subject = subject;
                //    message.Type = MsgType.ToString();
                //    message.CreatedBy = methodname;
                //    message.CreatedDate = DateTime.Now;
                //    MessageRep.Insert(message);
                //}
            }
            catch (Exception)
            {

            }
        }

        #endregion
    }
}