using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using TestAreaDemo.Areas.Movie.Models;

namespace TestAreaDemo.Areas.Movie.Services
{
    public interface  IA_Service
    {
        Guid GetGuid();
    }

    public class A_Service: IA_Service
    {
        private Guid Insta;
        private MovieDbContext wb;

        public A_Service(IMovieDbContext IWebDbContext)
        {
            Insta = Guid.NewGuid();
            wb = IWebDbContext as MovieDbContext;
        }

        public Guid GetGuid()
        {
            return Insta;
        }

        public Guid GetNovieGuid()
        {
            return Insta;
        }
    }
}