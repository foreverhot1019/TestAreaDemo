using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
//using System.Data.SqlClient;
using Oracle.ManagedDataAccess.Client;
using System.Web;

namespace DataContext.Extensions
{
    public static class SequenceBuilder
    {
        //获取流水号 加锁
        static object LockGETSEQNO = new object();
        /// <summary>
        /// Redis帮助
        /// </summary>
        static RedisHelp.RedisHelper ORedisHelp = new RedisHelp.RedisHelper();
        //记录删除Key
        private static System.Collections.Concurrent.ConcurrentDictionary<string, int> DictDeltRedisIncrKey = new System.Collections.Concurrent.ConcurrentDictionary<string, int>();

        /// <summary>
        /// 更新数据库 sequencers表，委托
        /// </summary>
        private static Func<int, string, int> updateSQLSequenceFuc = new Func<int, string, int>((seed, v_prefix) =>
        {
            var SQLStr = " update sequencers t SET t.seed=" + seed + " WHERE  prefix='" + v_prefix + "'";
            int ret = SQLDALHelper.OracleHelper.ExecuteNonQuery(SQLStr);
            return ret;
        });

        /// <summary>
        /// 清除 Redis 自增Key
        /// 
        /// </summary>
        public static void DeltCurrDictRedisIncrKey()
        {
            try
            {
                var QCurrDict = DictDeltRedisIncrKey;//.Where(n => n.Value >= 1);
                foreach (var item in QCurrDict)
                {
                    int val = 0;
                    if (!DictDeltRedisIncrKey.TryRemove(item.Key, out val))
                    {
                        Extensions.WriteLogHelper.WriteLog_Local("删除-" + item.Key + "错误" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:dd"), "SequenceBuilder");
                    }
                    else
                    {
                        var DelKey = Task.Run(() =>
                        {
                            ORedisHelp.KeyDelete("Seqcer_" + item.Key);
                        });
                    }
                }
            }
            catch (Exception ex)
            {
                Console.Write(ex);
            }
        }

        /// <summary>
        /// 获取流水号
        /// </summary>
        /// <param name="ptag"></param>
        /// <param name="delSequenceKey">要删除的键</param>
        /// <returns></returns>
        public static int GETSEQNO(OracleParameter[] ptag, string delSequenceKey = "")
        {
            int result = -1;

            //lock (LockGETSEQNO)
            //{
            try
            {
                var v_Prefix = ptag.Where(x => x.ParameterName.ToLower() == ":v_prefix").FirstOrDefault().Value.ToString();
                var v_OutPutNum = ptag.Where(x => x.ParameterName.ToLower() == ":v_outputnum").FirstOrDefault();
                if (!string.IsNullOrWhiteSpace(delSequenceKey))
                {
                    var QDictDeltRedisIncrKey = DictDeltRedisIncrKey.Where(x => x.Key == delSequenceKey);
                    if (QDictDeltRedisIncrKey.Any())
                    {
                        //调整成异步删除 每天 23-3 点之间执行
                        //var DelKey = Task.Run(() =>
                        //{
                        //    ORedisHelp.KeyDelete("Seqcer_" + delSequenceKey);
                        //});
                        var ODictDeltRedisIncrKey = QDictDeltRedisIncrKey.FirstOrDefault();
                        var val = ODictDeltRedisIncrKey.Value;

                        int newval = 0;
                        if (DictDeltRedisIncrKey.TryRemove(delSequenceKey, out newval))
                        {
                            DictDeltRedisIncrKey.TryAdd(delSequenceKey, newval++);
                        }
                    }
                    else
                    {
                        DictDeltRedisIncrKey.GetOrAdd(delSequenceKey, 1);
                    }
                }
                double OutPutNum = 0;//流水号
                var GetNum = Task.Run(() =>
                {
                    OutPutNum = ORedisHelp.StringIncrement("Seqcer_" + v_Prefix);
                });
                GetNum.Wait();//等待 获取流水执行完毕
                if (OutPutNum > result)
                {
                    result = Convert.ToInt32(OutPutNum);
                    v_OutPutNum.Value = result;
                    Task.Run(new Func<int>(() =>
                    {
                        return updateSQLSequenceFuc(result, v_Prefix);
                    }));
                    //SQLDALHelper.OracleHelper.ExecuteNonQuery(" update sequencers t SET t.seed=" + OutPutNum + " WHERE  prefix='" + v_Prefix + "'");
                }
            }
            catch (Exception)
            {
                result = -1;
            }
            //}

            return result;
        }

        /// <summary>
        /// 获取成本管理中的记录号的流水号
        /// </summary>
        /// <returns></returns>
        public static string NextCostMoney_No()
        {
            string DelSeqName = "YF" + DateTime.Now.AddMonths(-3).ToString("yyMM");//删除3月前的键
            string SeqName = "YF" + DateTime.Now.ToString("yyMM");
            OracleParameter OutPut = new OracleParameter(":v_OutPutNum", OracleDbType.Int32);
            OutPut.Direction = System.Data.ParameterDirection.Output;
            OracleParameter[] ptag = new OracleParameter[] {
                new OracleParameter(":v_Prefix", SeqName),
                OutPut
            };
            var result = GETSEQNO(ptag, DelSeqName);
            string OStrVal = OutPut.Value.ToString();
            int Seed = 0;
            if (!int.TryParse(OStrVal, out Seed))
            {
                return SeqName + OStrVal;
            }
            else
                return SeqName + Seed.ToString("00000");
        }

        /// <summary>
        /// 获取应收/付 提交 流水号
        /// </summary>
        /// <returns>TJAE+yyMMdd+序号（三位不足补0）</returns>
        public static string NextSubmit_No(bool IsAr)
        {
            var deldate = DateTime.Now.AddDays(-2).ToString("yyMMdd");
            var date = DateTime.Now.ToString("yyMMdd");
            var profix = "Submit_No_:";// +"Submit_No_" + (IsAr ? "Ar" : "Ap") + ":";
            string SeqName = profix + date;
            string DelSeqName = profix + deldate;//删除2天前的 Sequence 键

            OracleParameter OutPut = new OracleParameter(":v_OutPutNum", OracleDbType.Int32);
            OutPut.Direction = System.Data.ParameterDirection.Output;
            OracleParameter[] ptag = new OracleParameter[] {
                new OracleParameter(":v_Prefix", SeqName),
                OutPut
            };
            var result = GETSEQNO(ptag, DelSeqName); //db.Database.SqlQuery<int>("begin GETSEQNO(v_prefix => :v_prefix,v_OutPutNum => :v_OutPutNum); end;", ptag).First();
            string OStrVal = OutPut.Value.ToString();
            int Seed = 0;
            if (!int.TryParse(OStrVal, out Seed))
            {
                return "TJAE" + date + OStrVal.PadLeft(3, '0');
            }
            else
                return "TJAE" + date + Seed.ToString("000");
        }
    }

    /// <summary>
    /// 清除 Redis序列键
    /// </summary>
    public class TimerDeltSequenceRedisKey : System.Web.Hosting.IRegisteredObject
    {
        System.Threading.Timer OTime;
        private int period = 1000 * 60 * 60;//间隔事件 毫秒

        public TimerDeltSequenceRedisKey()
        {
            OTime = new System.Threading.Timer(new System.Threading.TimerCallback((x) =>
            {
                var day = DateTime.Now.ToString("yyyy/MM/dd");
                var date = DateTime.Now;
                var MinDate = DateTime.MinValue;
                var MaxDate = DateTime.MinValue;
                //string Msg = "OTime:" + date.ToString("yyyy-MM-dd HH:mm:ss");
                //Console.WriteLine("----------------"+Msg);
                if (!DateTime.TryParse(day + " 23:00:00", out MinDate)) MinDate = DateTime.MinValue;
                if (!DateTime.TryParse(day + " 03:00:00", out MaxDate)) MaxDate = DateTime.MinValue;
                var a = date < MaxDate;
                var b = date > MinDate;
                if (MinDate != MaxDate && (a || b))
                {
                    SequenceBuilder.DeltCurrDictRedisIncrKey();
                }
            }), null, 0, period);
        }

        public void Stop(bool immediate)
        {
            OTime.Dispose();
        }
    }
}
