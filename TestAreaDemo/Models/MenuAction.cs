using EntityInfrastructure;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace TestAreaDemo.Models
{
    public partial class MenuAction : DbEntity
    {
        public MenuAction()
        {
        }

        [Key]
        public int Id { get; set; }

        [Display(Name = "菜单动作", Description = "菜单动作名称")]
        [StringLength(20)]
        [Required(ErrorMessage = "Please enter : 菜单动作")]
        [Index("IX_ActionName", 1, IsUnique = true)]
        public string Name { get; set; }

        [Display(Name = "动作代码", Description = "菜单动作代码")]
        [Required(ErrorMessage = "Please enter : 菜单动作")]
        [Index("IX_ActionCode", 1, IsUnique = true)]
        [StringLength(20)]
        public string Code { get; set; }

        [Display(Name = "排序代码", Description = "菜单排序代码（0100开始）")]
        [StringLength(20)]
        [Required(ErrorMessage = "Please enter : 排序代码")]
        public string Sort { get; set; }

        [Display(Name = "启用", Description = "启用")]
        public bool IsEnabled { get; set; }

        [Display(Name = "菜单描述", Description = "菜单描述")]
        [StringLength(100)]
        public string Description { get; set; }

        #region ScaffoldColumn

        [ScaffoldColumn(false)]
        [Display(Name = "新增用户", Description = "新增用户")]
        [StringLength(20)]
        public string CreatedUserId { get; set; }

        [ScaffoldColumn(false)]
        [Display(Name = "新增时间", Description = "新增时间")]
        public DateTime? CreatedDateTime { get; set; }

        [ScaffoldColumn(false)]
        [Display(Name = "最后修改用户", Description = "最后修改用户")]
        [StringLength(20)]
        public string LastEditUserId { get; set; }

        [ScaffoldColumn(false)]
        [Display(Name = "最后修改时间", Description = "最后修改时间")]
        public DateTime? LastEditDateTime { get; set; }

        #endregion
    }
}