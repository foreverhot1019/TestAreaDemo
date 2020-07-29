using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using System.Net;
using System.Web;
using System.Web.Mvc;
using TestAreaDemo.Models;
using System.Threading;
using TestAreaDemo.Services;

namespace TestAreaDemo.Controllers
{
    public class MessagesController : Controller
    {
        //protected override IAsyncResult BeginExecuteCore(AsyncCallback callback, object state)
        //{
        //    string cultureName = RouteData.Values["lang"] as string;

        //    if (string.IsNullOrEmpty(cultureName))
        //        cultureName = Request.UserLanguages != null && Request.UserLanguages.Length > 0 ? Request.UserLanguages[0] : "zh-cn"; // obtain it from HTTP header AcceptLanguages

        //    if (RouteData.Values["lang"] as string != cultureName)
        //    {
        //        RouteData.Values["lang"] = cultureName.ToLowerInvariant();

        //        Response.RedirectToRoute(RouteData.Values);
        //    }

        //    Thread.CurrentThread.CurrentCulture = new System.Globalization.CultureInfo(cultureName);
        //    Thread.CurrentThread.CurrentUICulture = Thread.CurrentThread.CurrentCulture;

        //    return base.BeginExecuteCore(callback, state);
        //}

        //[Unity.Dependency("IMovieDbContext")]//报错（Unity 5.x 方式）
        private readonly WebDbContext db;
        public MessageService MessageService;
        private IA_Service A_Service { get; set; }

        /// <summary>
        /// Unity 4.x 方式
        /// 构造函数直接注入
        /// </summary>
        /// <param name="_db"></param>
        /// <param name="_MessageService"></param>
        /// <param name="_A_Service"></param>
        //public MessagesController(IWebDbContext _db, IMessageService _MessageService, A_Service _A_Service)
        //{
        //    //var OC = System.Threading.Thread.CurrentThread.CurrentUICulture;
        //    db = _db as WebDbContext;
        //    MessageService = _MessageService;
        //    var MessageService1 = UnityConfig.Container.Resolve(typeof(MessageService), "MessageService") as MessageService;
        //    A_Service = _A_Service;
        //    var A_Service1 = UnityConfig.Container.Resolve(typeof(A_Service), "A_Service");
        //}

        /// <summary>
        /// Unity 5.x 方式
        /// </summary>
        public MessagesController()
        {
            var OC = System.Threading.Thread.CurrentThread.CurrentUICulture;
            db = UnityConfig.Container.Resolve(typeof(IWebDbContext), "IWebDbContext") as WebDbContext;//必须是IWebDbContext，不然会重新创建实例
            MessageService = UnityConfig.Container.Resolve(typeof(IMessageService), "IMessageService") as MessageService;
            var MessageService1 = UnityConfig.Container.Resolve(typeof(IMessageService), "IMessageService") as MessageService;
            A_Service = UnityConfig.Container.Resolve(typeof(IA_Service), "IA_Service") as A_Service;
            var A_Service1 = UnityConfig.Container.Resolve(typeof(IA_Service), "IA_Service");
        }

        // GET: Messages
        public async Task<ActionResult> Index()
        {
            return View(await db.Messages.ToListAsync());
        }

        // GET: Messages/Details/5
        public async Task<ActionResult> Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Message message = await db.Messages.FindAsync(id);
            if (message == null)
            {
                return HttpNotFound();
            }
            return View(message);
        }

        // GET: Messages/Create
        public ActionResult Create()
        {
            var OC = System.Threading.Thread.CurrentThread.CurrentUICulture;
            return View();
        }

        // POST: Messages/Create
        // 为了防止“过多发布”攻击，请启用要绑定到的特定属性，有关 
        // 详细信息，请参阅 http://go.microsoft.com/fwlink/?LinkId=317598。
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create(Message message)
        {
            if (ModelState.IsValid)
            {
                message.CreatedUserId = "CreatedUserId";
                message.CreatedUserName = "CreatedUserName";
                message.CreatedDateTime = DateTime.Now;
                MessageService.Insert(message);
                await db.SaveChangesAsync();
                return RedirectToAction("Index");
            }

            return View(message);
        }

        // GET: Messages/Edit/5
        public async Task<ActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Message message = await db.Messages.FindAsync(id);
            if (message == null)
            {
                return HttpNotFound();
            }
            return View(message);
        }

        // POST: Messages/Edit/5
        // 为了防止“过多发布”攻击，请启用要绑定到的特定属性，有关 
        // 详细信息，请参阅 http://go.microsoft.com/fwlink/?LinkId=317598。
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit([Bind(Include = "Id,MsgType,TargetPath,Content,AddUser,AddDate")] Message message)
        {
            if (ModelState.IsValid)
            {
                db.Entry(message).State = EntityState.Modified;
                await db.SaveChangesAsync();
                return RedirectToAction("Index");
            }
            return View(message);
        }

        // GET: Messages/Delete/5
        public async Task<ActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Message message = await db.Messages.FindAsync(id);
            if (message == null)
            {
                return HttpNotFound();
            }
            return View(message);
        }

        // POST: Messages/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> DeleteConfirmed(int id)
        {
            Message message = await db.Messages.FindAsync(id);
            db.Messages.Remove(message);
            await db.SaveChangesAsync();
            return RedirectToAction("Index");
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
