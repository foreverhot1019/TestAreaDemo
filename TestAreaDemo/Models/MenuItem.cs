using EntityInfrastructure;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace TestAreaDemo.Models
{
    /// <summary>
    /// 菜单
    /// </summary>
    public partial class MenuItem : DbEntity
    {
        public MenuItem()
        {
            SubMenus = new HashSet<MenuItem>();
        }

        [Key]
        public int Id { get; set; }

        [Display(Name = "菜单名称", Description = "菜单名称")]
        [StringLength(20)]
        [Required(ErrorMessage = "Please enter : 菜单名称")]
        [Index("IX_MenuTitle", 1, IsUnique = false)]
        public string Title { get; set; }

        [Display(Name = "菜单描述", Description = "菜单描述")]
        [StringLength(100)]
        public string Description { get; set; }

        [Display(Name = "排序代码", Description = "菜单排序代码（0100开始）")]
        [StringLength(20)]
        [Required(ErrorMessage = "Please enter : 代码")]
        [Index("IX_MenuCode", 1, IsUnique = true)]
        public string Code { get; set; }

        [Display(Name = "菜单Url", Description = "菜单Url")]
        [StringLength(100)]
        [Required(ErrorMessage = "Please enter : Url")]
        [Index("IX_MenuUrl", 1, IsUnique = false)]
        public string Url { get; set; }

        [Display(Name = "是否启用", Description = "是否启用")]
        public bool IsEnabled { get; set; }

        [Display(Name = "子菜单", Description = "子菜单")]
        public ICollection<MenuItem> SubMenus { get; set; }

        [Display(Name = "上级菜单", Description = "上级菜单")]
        public int? ParentId { get; set; }

        [Display(Name = "上级菜单", Description = "上级菜单")]
        [ForeignKey("ParentId")]
        public MenuItem Parent { get; set; }

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