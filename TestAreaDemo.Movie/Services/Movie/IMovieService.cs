using BaseService;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TestAreaDemo.Areas.Movie.Models;

namespace TestAreaDemo.Areas.Movie.Services
{
    public interface IMovieService : IBaseService<Models.Movie>
    {
        void DeleteRange(IEnumerable<int> ArrKeyId);

        void DeleteRange(IQueryable<Models.Movie> ArrMovie);

        Actor GetActorByActorId(int ActorId);

        IEnumerable<Actor> GetActorList(int id);
    }
}
