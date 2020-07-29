using DataContext.Models;
using EntityInfrastructure;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using TestAreaDemo.Views.Messages.Lang;

namespace TestAreaDemo.Models
{
    public class Message: DbEntity
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required]
        [Display(Name = "MsgType", Description = "信息类型")]
        public EnumType.Log4NetMsgType MsgType { get; set; }

        [Required]
        [Display(Name = "KeyNo", Description = "关联字段值")]
        [MaxLength(200)]
        public string KeyNo { get; set; }

        [Required]
        [Display(Name = "TargetPath", Description = "目标路径")]
        [MaxLength(200)]
        public string TargetPath { get; set; }

        [Required, Display(Name = "Content", Description = "内容")]
        public string Content { get; set; }

        #region ScaffoldColumn

        [ScaffoldColumn(false)]
        [Display(Name = "新增用户", Description = "新增用户")]
        [StringLength(20)]
        public string CreatedUserId { get; set; }

        [ScaffoldColumn(false)]
        [Display(Name = "新增用户", Description = "新增用户")]
        [StringLength(20)]
        public string CreatedUserName { get; set; }

        [ScaffoldColumn(false)]
        [Display(Name = "新增时间", Description = "新增时间")]
        public DateTime CreatedDateTime { get; set; }

        [ScaffoldColumn(false)]
        [Display(Name = "最后修改用户", Description = "最后修改用户")]
        [StringLength(20)]
        public string LastEditUserId { get; set; }

        [ScaffoldColumn(false)]
        [Display(Name = "最后修改用户", Description = "最后修改用户")]
        [StringLength(20)]
        public string LastEditUserName { get; set; }

        [ScaffoldColumn(false)]
        [Display(Name = "最后修改时间", Description = "最后修改时间")]
        public DateTime? LastEditDateTime { get; set; }

        #endregion
    }
}