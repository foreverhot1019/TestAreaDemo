using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DataContext.Models
{
    class WordExcleModel
    {
    }

    public class TotalColumn
    {
        [Display(Name = "统计列名", Description = "统计列名")]
        public string ColumnName { get; set; }

        [Display(Name = "统计列 序列", Description = "统计列 序列")]
        public int ColumnIndex { get; set; }

        [Display(Name = "统计列汇总", Description = "统计列汇总")]
        public string ColumnTotal { get; set; }
    }
}
