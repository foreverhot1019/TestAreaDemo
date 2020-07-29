using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using TestAreaDemo.Areas.Movie.Models;
using TestAreaDemo.Areas.Movie.Services;

namespace TestAreaDemo.Areas.Movie.Controllers
{
    public class ActorsController : Controller
    {
        //[Unity.Dependency("IMovieDbContext")]//报错（Unity 5.x 方式）
        private readonly MovieDbContext db;// = new MovieDbContext();
        public readonly IMovieService MovieService;
        public readonly IActorService ActorService;

        public ActorsController()
        {
            db = UnityConfig.Container.Resolve(typeof(IMovieDbContext), "IMovieDbContext") as MovieDbContext;
            MovieService = UnityConfig.Container.Resolve(typeof(IMovieService), "IMovieService") as MovieService;
            ActorService = UnityConfig.Container.Resolve(typeof(IActorService), "IActorService") as ActorService;
        }

        // GET: Actors
        public ActionResult Index()
        {
            return View(ActorService.QueryList(x => x.Id > 0).ToList());
            //return View(db.Actor.ToList());
        }

        // GET: Actors/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Actor actor = db.Actor.Find(id);
            if (actor == null)
            {
                return HttpNotFound();
            }
            return View(actor);
        }

        // GET: Actors/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: Actors/Create
        // 为了防止“过多发布”攻击，请启用要绑定到的特定属性，有关 
        // 详细信息，请参阅 http://go.microsoft.com/fwlink/?LinkId=317598。
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "Id,Name,Birthday,Age,Sex,Country,AddUser,AddDate,EditUser,EditDate")] Actor actor)
        {
            if (ModelState.IsValid)
            {
                ActorService.Insert(actor);
                actor.AddUser = "admin";
                actor.AddDate= actor.EditDate = DateTime.Now;
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            return View(actor);
        }

        // GET: Actors/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Actor actor = ActorService.Find(x => x.Id == id);//db.Actor.Find(id);
            if (actor == null)
            {
                return HttpNotFound();
            }
            return View(actor);
        }

        // POST: Actors/Edit/5
        // 为了防止“过多发布”攻击，请启用要绑定到的特定属性，有关 
        // 详细信息，请参阅 http://go.microsoft.com/fwlink/?LinkId=317598。
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "Id,Name,Birthday,Age,Sex,Country,AddUser,AddDate,EditUser,EditDate")] Actor actor)
        {
            if (ModelState.IsValid)
            {
                ActorService.Update(actor);
                //db.Entry(actor).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(actor);
        }

        // GET: Actors/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Actor actor = db.Actor.Find(id);
            if (actor == null)
            {
                return HttpNotFound();
            }
            return View(actor);
        }

        // POST: Actors/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            Actor actor = db.Actor.Find(id);
            db.Actor.Remove(actor);
            db.SaveChanges();
            return RedirectToAction("Index");
        }

        /// <summary>
        /// 获取Movie
        /// </summary>
        /// <param name="ActorId"></param>
        /// <returns></returns>
        public ActionResult GetMovie(int MovieId)
        {
            var Movie = MovieService.Query(x => x.Id == MovieId);
            return Json(Movie);
        }

        /// <summary>
        /// 获取Movie
        /// </summary>
        /// <param name="ActorId"></param>
        /// <returns></returns>
        public ActionResult GetMovieList(int Id)
        {
            var MovieList = ActorService.GetMovieList(Id);
            return Json(MovieList);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
