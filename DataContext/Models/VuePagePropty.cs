using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace DataContext.Models
{
    /// <summary>
    /// Vue 前端页面 配置
    /// </summary>
    public class VuePagePropty
    {
        /// <summary>
        /// 名称
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// 显示名称
        /// </summary>
        public string DisplayName { get; set; }

        /// <summary>
        /// 列表-列宽度
        /// </summary>
        public string Width_List { get; set; }

        /// <summary>
        /// Form-input宽度
        /// </summary>
        public string Width_input { get; set; }

        /// <summary>
        /// 类型 
        /// datetime/number/string/boolean
        /// </summary>
        public string Type { get; set; }

        /// <summary>
        /// Type为number时，可设置小数位 
        /// </summary>
        public int? Precision { get; set; }

        /// <summary>
        /// form中的input类型
        /// password/datetime/text
        /// </summary>
        public string inputType { get; set; }

        /// <summary>
        /// 主键
        /// </summary>
        public bool IsKey { get; set; }

        /// <summary>
        /// 必填
        /// </summary>
        public bool Required { get; set; }

        /// <summary>
        /// 是否可排序 
        /// </summary>
        public bool Sortable { get; set; }

        /// <summary>
        /// 可编辑
        /// </summary>
        public bool Editable { get; set; }

        /// <summary>
        /// 搜索中展示
        /// </summary>
        public bool SearchShow { get; set; }

        /// <summary>
        /// Form中展示
        /// </summary>
        public bool FormShow { get; set; }

        /// <summary>
        /// 列表展示
        /// </summary>
        public bool ListShow { get; set; }

        /// <summary>
        /// 最大长度
        /// </summary>
        public int MaxLength { get; set; }

        /// <summary>
        /// 最小长度
        /// </summary>
        public int MinLength { get; set; }

        /// <summary>
        /// 列表排序
        /// </summary>
        public int ListOrder { get; set; }

        /// <summary>
        /// 搜索排序
        /// </summary>
        public int SearchOrder { get; set; }

        /// <summary>
        /// Form排序
        /// </summary>
        public int FormOrder { get; set; }

        /// <summary>
        /// 外键关联
        /// </summary>
        public bool IsForeignKey { get; set; }

        /// <summary>
        /// 外键关联 获取数据Url
        /// </summary>
        public string ForeignKeyGetListUrl { get; set; }

    }
}