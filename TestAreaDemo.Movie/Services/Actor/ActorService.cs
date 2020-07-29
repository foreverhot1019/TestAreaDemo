using BaseService;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;
using TestAreaDemo.Areas.Movie.Models;

namespace TestAreaDemo.Areas.Movie.Services
{
    public class ActorService : BaseService<Actor>, IActorService
    {
        public new readonly MovieDbContext MyDbContext;

        public ActorService(MovieDbContext _MyDbContext)
            : base(_MyDbContext)
        {
            MyDbContext = _MyDbContext as MovieDbContext;
        }

        public void DeleteRange(IEnumerable<int> ArrKeyId)
        {
            var ArrActor = ArrKeyId.Select(x => new Models.Actor { Id = x });
            foreach (var OActor in ArrActor)
            {
                MyDbContext.Entry(OActor).State = EntityState.Deleted;
            }
        }

        public void DeleteRange(IQueryable<Models.Actor> ArrActor)
        {
            foreach (var OActor in ArrActor)
            {
                MyDbContext.Entry(OActor).State = EntityState.Deleted;
            }
        }

        public Models.Movie GetMovieByMovieId(int MovieId)
        {
            var OMovie = MyDbContext.Movie.Where(x => x.Id == MovieId).FirstOrDefault();
            if (OMovie != null && OMovie.Id > 0)
                return OMovie;
            else
                return null;
        }

        public IEnumerable<Models.Movie> GetMovieList(int id)
        {
            var OActor = this.Query(x => x.Id == id).Include("Actor.MovieList").FirstOrDefault();
            if (OActor != null && OActor.Id > 0)
                return OActor.MovieList;
            else
                return null;
        }
    }
}