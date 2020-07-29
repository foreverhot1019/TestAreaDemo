using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Web;

namespace DataContext
{
    /// <summary>
    /// 添加时需要设定的字段名
    /// 英文单词 单复数转换
    /// </summary>
    public static class StringUtil
    {
        /// <summary>
        /// 获取表名
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public static string GetTableName(Type type)
        {
            try
            {
                TableAttribute[] tableAttributes = (TableAttribute[])type.GetCustomAttributes(typeof(TableAttribute), false);

                if (!tableAttributes.Any())
                {
                    //var pluralizationService = DbConfiguration.DependencyResolver.GetService<IPluralizationService>();
                    //var result = pluralizationService.Pluralize(type.Name);
                    //var result = Regex.Replace(type.Name, ".[A-Z]", m => m.Value[0] + "_" + m.Value[1]);
                    var result = StringUtil.ToPlural(type.Name);

                    return result.ToUpper();
                }
                else
                {
                    var tableattr = tableAttributes.FirstOrDefault();
                    if (tableattr != null)
                    {
                        if (!string.IsNullOrEmpty(tableattr.Name))
                            return tableattr.Name;
                        else
                            return StringUtil.ToPlural(type.Name).ToUpper();
                    }
                    else
                        return StringUtil.ToPlural(type.Name).ToUpper();
                }
            }
            catch (Exception)
            {
                return StringUtil.ToPlural(type.Name).ToUpper();
            }
        }

        /// <summary>
        /// 获取表名
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public static string GetColumnName(PropertyInfo pri)
        {
            try
            {
                ColumnAttribute[] columnAttribute = (ColumnAttribute[])pri.GetCustomAttributes(typeof(ColumnAttribute), false);

                if (columnAttribute != null)
                {
                    if (!columnAttribute.Any())
                    {
                        //var pluralizationService = DbConfiguration.DependencyResolver.GetService<IPluralizationService>();
                        //var result = pluralizationService.Pluralize(type.Name);
                        //var result = Regex.Replace(type.Name, ".[A-Z]", m => m.Value[0] + "_" + m.Value[1]);
                        var result = pri.Name;

                        return result.ToUpper();
                    }
                    else
                    {
                        var columnattr = columnAttribute.FirstOrDefault();
                        if (columnattr != null)
                        {
                            if (!string.IsNullOrEmpty(columnattr.Name))
                                return columnattr.Name;
                            else
                                return pri.Name.ToUpper();
                        }
                        else
                            return pri.Name.ToUpper();
                    }
                }
                else
                    return pri.Name.ToUpper();
            }
            catch (Exception)
            {
                return StringUtil.ToPlural(pri.Name).ToUpper();
            }
        }

        /// <summary>
        /// 单词变成单数形式
        /// </summary>
        /// <param name="word"></param>
        /// <returns></returns>
        public static string ToSingular(string word)
        {
            Regex plural1 = new Regex("(?<keep>[^aeiou])ies$");
            Regex plural2 = new Regex("(?<keep>[aeiou]y)s$");
            Regex plural3 = new Regex("(?<keep>[sxzh])es$");
            Regex plural4 = new Regex("(?<keep>[^sxzhyu])s$");

            if (plural1.IsMatch(word))
                return plural1.Replace(word, "${keep}y");
            else if (plural2.IsMatch(word))
                return plural2.Replace(word, "${keep}");
            else if (plural3.IsMatch(word))
                return plural3.Replace(word, "${keep}");
            else if (plural4.IsMatch(word))
                return plural4.Replace(word, "${keep}");

            return word;
        }

        /// <summary>
        /// 单词变成复数形式
        /// </summary>
        /// <param name="word"></param>
        /// <returns></returns>
        public static string ToPlural(string word)
        {
            Regex plural1 = new Regex("(?<keep>[^aeiou])y$");
            Regex plural2 = new Regex("(?<keep>[aeiou]y)$");
            Regex plural3 = new Regex("(?<keep>[sxzh])$");
            Regex plural4 = new Regex("(?<keep>[^sxzhy])$");

            if (plural1.IsMatch(word))
                return plural1.Replace(word, "${keep}ies");
            else if (plural2.IsMatch(word))
                return plural2.Replace(word, "${keep}s");
            else if (plural3.IsMatch(word))
                return plural3.Replace(word, "${keep}es");
            else if (plural4.IsMatch(word))
                return plural4.Replace(word, "${keep}s");

            return word;
        }
    }
}