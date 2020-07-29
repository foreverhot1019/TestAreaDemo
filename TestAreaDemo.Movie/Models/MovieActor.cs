using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;
using EntityInfrastructure;

namespace TestAreaDemo.Areas.Movie.Models
{
    public class MovieActor : DbEntity
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        public int MovieId { get; set; }

        public int ActorId { get; set; }

        public virtual Movie Movie { set; get; }

        public virtual Actor Actor { set; get; }
    }
}