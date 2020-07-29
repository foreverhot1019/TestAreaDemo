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
using DataContext.Extensions;

namespace TestAreaDemo.Areas.Movie.Controllers
{
    public class MoviesController : Controller
    {
        //[Unity.Dependency("IMovieDbContext")]//报错（Unity 5.x 方式）
        private readonly MovieDbContext db;// = new WebDbContext();
        private IMovieService _MovieService;
        private IActorService _ActorService;
        private IA_Service _A_Service;

        public MoviesController()
        {
            db = UnityConfig.Container.Resolve(typeof(IMovieDbContext), "IMovieDbContext") as MovieDbContext;
            _MovieService = UnityConfig.Container.Resolve(typeof(IMovieService), "IMovieService") as MovieService;
            _ActorService = UnityConfig.Container.Resolve(typeof(IActorService), "IActorService") as ActorService;
            _A_Service = UnityConfig.Container.Resolve(typeof(IA_Service), "IA_Service") as A_Service;
            var A_Service1 = UnityConfig.Container.Resolve(typeof(IA_Service), "IA_Service");
        }

        // GET: Movies
        public ActionResult Index()
        {
            int totalcount = 0;
            return View(_MovieService.Query(x => x.Id > 0).SelectPage(1, 10, out totalcount));
            //var ArrMovie = db.Database.SqlQuery<TestAreaDemo.Areas.Movie.Models.Movie>("select * from Movies where id>:id", new { id = 1 }).ToList();
        }

        // GET: Movies/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Movie.Models.Movie movie = db.Movie.Find(id);
            if (movie == null)
            {
                return HttpNotFound();
            }
            return View(movie);
        }

        // GET: Movies/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: Movies/Create
        // 为了防止“过多发布”攻击，请启用要绑定到的特定属性，有关 
        // 详细信息，请参阅 http://go.microsoft.com/fwlink/?LinkId=317598。
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(Models.Movie movie)
        {
            if (ModelState.IsValid)
            {
                _MovieService.Insert(movie);
                movie.AddUser = "admin";
                movie.AddDate = DateTime.Now;
                //var a = Mydb.ChangeTracker.Entries();
                var b = db.ChangeTracker.Entries();
                //db.Movie.Add(movie);
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            else
            {
                //SocketTestHttp();
            }

            return View(movie);
        }

        public void SocketTestHttp()
        {
            var Encode = System.Text.Encoding.UTF8;
            byte[] ResBytes = new byte[10000];
            System.Net.Sockets.Socket OClient = new System.Net.Sockets.Socket(System.Net.Sockets.SocketType.Stream, System.Net.Sockets.ProtocolType.Tcp);
            OClient.Connect("172.20.60.181", 8004);
            //OClient.Connect("172.20.36.81", 62194);
            var headTokenData = @"POST /Token HTTP/1.1
Host: 172.20.60.181:8004
Connection: keep-alive
Content-Length: 111
User-Agent: Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/73.0.3683.103 Safari/537.36
Content-Type: application/x-www-form-urlencoded
Accept: */*
Accept-Encoding: gzip, deflate
Accept-Language: zh-CN,zh;q=0.9

client_id=fldkdjhs&client_secret=fld_47HDu8s&UserName=Michael&grant_type=client_credentials&response_type=token";

            OClient.Send(Encode.GetBytes(headTokenData));
            OClient.Receive(ResBytes);
            var resTokendata = Encode.GetString(ResBytes);
            var access_token = "\"access_token\":\"";
            var idx = resTokendata.IndexOf(access_token) + access_token.Length;
            var edx = resTokendata.IndexOf("\",", idx);
            access_token = resTokendata.Substring(idx, edx - idx);
            //OClient.Close();
            //System.Threading.Thread.Sleep(100);
            //System.Net.Sockets.Socket OClient1 = new System.Net.Sockets.Socket(System.Net.Sockets.SocketType.Stream, System.Net.Sockets.ProtocolType.Tcp);
            //OClient1.Connect("172.20.60.181", 8004);
            var headData = @"POST /Loan/getinfoD HTTP/1.1
Host: 172.20.60.181:8004
Connection: keep-alive
Content-Length: {0}
Authorization: bearer {1}
User-Agent: Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/73.0.3683.103 Safari/537.36
Content-Type: application/json
Accept: */*
Accept-Encoding: gzip, deflate
Accept-Language: zh-CN,zh;q=0.9

";

            var bodyData = "{\"ENTITY_XML\":\"<?xml version=\\\"1.0\\\" encoding=\\\"UTF-8\\\"?><ENTITY>  <Head>   <Trans_Id>20190412FLD1001000001</Trans_Id>   <Trans_Code>FLD1001</Trans_Code>  </Head>  <Data>    <Unn_Soc_Cr_Cd>1234567890123</Unn_Soc_Cr_Cd>    <CoPlf_ID>1234567</CoPlf_ID>    <KeyCode>KeyCode123</KeyCode>    <remark1></remark1>    <remark2></remark2>  </Data></ENTITY>\"}";
            var bodyBytes = Encode.GetBytes(bodyData);
            var headBytes = Encode.GetBytes(string.Format(headData, bodyBytes.Length, access_token));
            var sendData = headBytes.Concat(bodyBytes).ToArray();
            ResBytes = new byte[10000];
            OClient.Send(sendData);
            OClient.Receive(ResBytes);
            var resdata = Encode.GetString(ResBytes);

            ViewData["resdata"] = resdata;
        }

        // GET: Movies/Edit/5
        public ActionResult Edit(int? id)
        {
            if (!id.HasValue)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            //Movie.Models.Movie movie = db.Movie.Find(id);
            Movie.Models.Movie movie = _MovieService.Find((int)id);
            if (movie == null)
            {
                return HttpNotFound();
            }
            return View(movie);
        }

        // POST: Movies/Edit/5
        // 为了防止“过多发布”攻击，请启用要绑定到的特定属性，有关 
        // 详细信息，请参阅 http://go.microsoft.com/fwlink/?LinkId=317598。
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "Id,Name,Rate,InDate,Description,AddUser,AddDate")] Movie.Models.Movie movie)
        {
            if (ModelState.IsValid)
            {
                _MovieService.Update(movie);
                //db.Entry(movie).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(movie);
        }

        // GET: Movies/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Movie.Models.Movie movie = db.Movie.Find(id);
            if (movie == null)
            {
                return HttpNotFound();
            }
            return View(movie);
        }

        // POST: Movies/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            Movie.Models.Movie movie = db.Movie.Find(id);
            db.Movie.Remove(movie);
            db.SaveChanges();
            return RedirectToAction("Index");
        }

        /// <summary>
        /// 获取Actor
        /// </summary>
        /// <param name="ActorId"></param>
        /// <returns></returns>
        public ActionResult GetActor(int ActorId)
        {
            var Actor = _MovieService.GetActorByActorId(ActorId);
            return Json(Actor);
        }

        /// <summary>
        /// 获取Movie
        /// </summary>
        /// <param name="ActorId"></param>
        /// <returns></returns>
        public ActionResult GetActorList(int Id)
        {
            var ActorList = _MovieService.GetActorList(Id);
            return Json(ActorList);
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
