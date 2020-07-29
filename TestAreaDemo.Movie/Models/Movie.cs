using DataContext.Models;
using EntityInfrastructure;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;
using TestAreaDemo.Areas.Movie.Views.Movies.Lang;

namespace TestAreaDemo.Areas.Movie.Models
{
    public class Movie : DbEntity
    {
        public Movie()
        {
            ActorList = new List<Actor>();
        }

        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Display(Name = "Id", Description = "主键")]//, ResourceType = typeof(Language)
        public int Id { get; set; }

        //[Required(ErrorMessageResourceName = "RequiredAttribute_ValidationError", ErrorMessageResourceType = typeof(CommonLanguage.Language))]
        //[MaxLength(50, ErrorMessageResourceName = "MaxLengthAttribute_ValidationError", ErrorMessageResourceType = typeof(CommonLanguage.Language))]
        [Required,MaxLength(50)]
        [Display(Name = "Name", Description = "电影名称")]
        public string Name { get; set; }

        [NotMapped]
        //[Required(ErrorMessageResourceName = "RequiredAttribute_ValidationError", ErrorMessageResourceType = typeof(CommonLanguage.Language))]
        [Required]
        [Compare("Name")]
        [Display(Name = "CompareName", Description = "确认电影名称")]
        public string CompareName{ get; set; }

        //[Required(ErrorMessageResourceName = "RequiredAttribute_ValidationError", ErrorMessageResourceType = typeof(CommonLanguage.Language))]
        //[MaxLength(100, ErrorMessageResourceName = "MaxLengthAttribute_ValidationError", ErrorMessageResourceType = typeof(CommonLanguage.Language))]
        //[EmailAddress(ErrorMessageResourceName = "EmailAddressAttribute_Invalid", ErrorMessageResourceType = typeof(CommonLanguage.Language))]
        [Required,MaxLength(100),EmailAddress]
        [Display(Name = "Email", Description = "邮箱")]
        public string Email { get; set; }

        //[Range(0.1, 10, ErrorMessageResourceName = "RangeAttribute_ValidationError", ErrorMessageResourceType = typeof(CommonLanguage.Language))]
        [Range(0.1, 10)]
        [Display(Name = "Rate", Description = "评分")]
        public decimal Rate { get; set; }

        [Display(Name = "InDate", Description = "上映日期")]
        public DateTime InDate { get; set; }

        //[Required(ErrorMessageResourceName = "RequiredAttribute_ValidationError", ErrorMessageResourceType = typeof(CommonLanguage.Language))]
        [Required]
        [Display(Name = "Description", Description = "描述")]
        public string Description { get; set; }

        [Display(Name = "ActorList", Description = "演员列表")]//, ResourceType = typeof(Language)
        public virtual ICollection<Actor> ActorList { get; set; }

        //[Required(ErrorMessageResourceName = "RequiredAttribute_ValidationError", ErrorMessageResourceType = typeof(CommonLanguage.Language))]
        //[EnumDataType(typeof(EnumType.StatusEnum), ErrorMessageResourceName = "EnumDataTypeAttribute_TypeNeedsToBeAnEnum", ErrorMessageResourceType = typeof(CommonLanguage.Language))]
        [Required]
        [Display(Name = "Status", Description = "状态")]
        public EnumType.StatusEnum Status { get; set; }

        [Display(Name = "AddUser", Description = "新增人")]
        //[MaxLength(20, ErrorMessageResourceName = "MaxLengthAttribute_ValidationError", ErrorMessageResourceType = typeof(CommonLanguage.Language))]
        [MaxLength(20)]
        //[ScaffoldColumn(false)]
        public string AddUser { get; set; }

        [Display(Name = "AddDate", Description = "新增时间")]
        //[ScaffoldColumn(false)]
        public DateTime AddDate { get; set; }

        //[MaxLength(20, ErrorMessageResourceName = "MaxLengthAttribute_ValidationError", ErrorMessageResourceType = typeof(CommonLanguage.Language))]
        [Display(Name = "EditUser", Description = "新增人")]
        [MaxLength(20)]
        //[ScaffoldColumn(false)]
        public string EditUser { get; set; }

        [Display(Name = "EditDate", Description = "新增时间")]
        //[ScaffoldColumn(false)]
        public DateTime? EditDate { get; set; }
    }
}