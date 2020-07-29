using BaseService;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TestAreaDemo.Areas.Movie;

namespace TestAreaDemo.Areas.Movie.Services
{
    public interface IActorService : IBaseService<Models.Actor>
    {
        void DeleteRange(IEnumerable<int> ArrKeyId);

        void DeleteRange(IQueryable<Models.Actor> ArrActor);

        Models.Movie GetMovieByMovieId(int MovieId);

        IEnumerable<Models.Movie> GetMovieList(int id);
    }
}
