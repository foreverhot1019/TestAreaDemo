using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using TestAreaDemo.Models;

namespace TestAreaDemo.Services
{
    public interface  IA_Service
    {
        Guid GetGuid();
    }

    public class A_Service: IA_Service
    {
        private Guid Insta;
        private WebDbContext wb;

        public A_Service()
        {
            Insta = Guid.NewGuid();
            //wb = IWebDbContext as WebDbContext;
        }

        public Guid GetGuid()
        {
            return Insta;
        }
    }
}