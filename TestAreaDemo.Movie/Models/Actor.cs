using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;
using EntityInfrastructure;
using TestAreaDemo.Areas.Movie.Views.Actors.Lang;

namespace TestAreaDemo.Areas.Movie.Models
{
    public class Actor : DbEntity
    {
        public Actor()
        {
            MovieList = new List<Movie>();
        }

        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        //[Required(ErrorMessageResourceName = "RequiredAttribute_ValidationError", ErrorMessageResourceType = typeof(CommonLanguage.Language))]
        //[StringLength(50, MinimumLength = 10, ErrorMessageResourceName = "StringLengthAttribute_ValidationErrorIncludingMinimum", ErrorMessageResourceType = typeof(CommonLanguage.Language))]
        //[Display(Name = "Name", ResourceType = typeof(Language), Description = "姓名")]
        [Required,StringLength(50,MinimumLength=10)]
        [Display(Name = "Name", Description = "姓名")]
        public string Name { get; set; }

        //[Display(Name = "Birthday", ResourceType = typeof(Language), Description = "出生年月")]
        [Display(Name = "Birthday", Description = "出生年月")]
        public DateTime Birthday { get; set; }

        //[Display(Name = "Age", ResourceType = typeof(Language), Description = "年龄")]
        //[Range(1, 123, ErrorMessageResourceName = "RangeAttribute_ValidationError", ErrorMessageResourceType = typeof(CommonLanguage.Language))]
        [Display(Name = "Age", Description = "年龄")]
        [Range(1, 123)]
        public int Age { get; set; }

        [Display(Name = "Sex", Description = "性别")]
        public bool? Sex { get; set; }

        //[Display(Name = "Country", ResourceType = typeof(Language), Description = "国家")]
        //[MinLength(3, ErrorMessageResourceName = "MinLengthAttribute_ValidationError", ErrorMessageResourceType = typeof(CommonLanguage.Language))]
        //[MaxLength(50, ErrorMessageResourceName = "MaxLengthAttribute_ValidationError", ErrorMessageResourceType = typeof(CommonLanguage.Language))]
        [Display(Name = "Country", Description = "国家")]
        [StringLength(50,MinimumLength=3)]
        public string Country { get; set; }

        [Display(Name = "MovieList", Description = "代表作")]
        public virtual ICollection<Movie> MovieList { get; set; }

        [Display(Name = "AddUser", Description = "新增人")]
        [MaxLength(20, ErrorMessageResourceName = "MaxLengthAttribute_ValidationError", ErrorMessageResourceType = typeof(CommonLanguage.Language))]
        [ScaffoldColumn(false)]
        public string AddUser { get; set; }

        [Display(Name = "AddDate", Description = "新增时间")]
        [ScaffoldColumn(false)]
        public DateTime AddDate { get; set; }

        [Display(Name = "EditUser", Description = "新增人")]
        [MaxLength(20, ErrorMessageResourceName = "MaxLengthAttribute_ValidationError", ErrorMessageResourceType = typeof(CommonLanguage.Language))]
        [ScaffoldColumn(false)]
        public string EditUser { get; set; }

        [Display(Name = "EditDate", Description = "新增时间")]
        [ScaffoldColumn(false)]
        public DateTime EditDate { get; set; }
    }
}