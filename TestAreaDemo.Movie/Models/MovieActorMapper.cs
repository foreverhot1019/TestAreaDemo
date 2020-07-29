//using System;
//using System.Collections.Generic;
//using System.Data.Entity.ModelConfiguration;
//using System.Linq;
//using System.Web;

//namespace TestAreaDemo.Areas.Movie.Models
//{
//    public class MovieActorMapper
//    {


//    }
//    public class MovieMap : EntityTypeConfiguration<Movie>
//    {
//        public MovieMap()
//        {
//            ToTable("Movie");
//            HasKey(u => u.Id);
//        }
//    }

//    public class ActorMap : EntityTypeConfiguration<Actor>
//    {
//        public ActorMap()
//        {
//            ToTable("Actor");
//            HasKey(r => r.Id);
//        }
//    }

//    public class MovieActorMap : EntityTypeConfiguration<MovieActor>
//    {
//        public MovieActorMap()
//        {
//            ToTable("MovieActor");
//            HasKey(ur => ur.Id);

//            //HasRequired(pt => pt.Movie).WithMany(p => p.ActorList).HasForeignKey(pt => pt.ActorId).WillCascadeOnDelete(false);
//            //HasRequired(pt => pt.Actor).WithMany(t => t.MovieList).HasForeignKey(pt => pt.MovieId).WillCascadeOnDelete(false);
//        }
//    }
//}