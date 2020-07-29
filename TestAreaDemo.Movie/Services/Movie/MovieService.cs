using BaseService;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;
using TestAreaDemo.Areas.Movie.Models;

namespace TestAreaDemo.Areas.Movie.Services
{
    public class MovieService : BaseService<Models.Movie>, IMovieService
    {
        public readonly MovieDbContext MyDbContext;
        public MovieService(MovieDbContext _MyDbContext)
            : base(_MyDbContext)
        {
            MyDbContext = _MyDbContext as MovieDbContext;
        }

        public void DeleteRange(IEnumerable<int> ArrKeyId)
        {
            var ArrMovie = ArrKeyId.Select(x => new Models.Movie { Id = x });
            foreach (var OMovie in ArrMovie)
            {
                MyDbContext.Entry(OMovie).State = EntityState.Deleted;
            }
        }

        public void DeleteRange(IQueryable<Models.Movie> ArrMovie)
        {
            foreach (var OMovie in ArrMovie)
            {
                MyDbContext.Entry(OMovie).State = EntityState.Deleted;
            }
        }

        public Actor GetActorByActorId(int ActorId)
        {
            var OActor = MyDbContext.Actor.Where(x => x.Id == ActorId).FirstOrDefault();
            if (OActor != null && OActor.Id > 0)
                return OActor;
            else
                return null;
        }

        public IEnumerable<Actor> GetActorList(int id)
        {
            var OMovie = this.Query(x => x.Id == id).Include("Movie.ActorList").FirstOrDefault();
            if (OMovie != null && OMovie.Id > 0)
                return OMovie.ActorList;
            else
                return null;
        }
    }
}