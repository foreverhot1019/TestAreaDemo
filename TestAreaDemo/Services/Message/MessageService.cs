using BaseService;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using TestAreaDemo.Models;

namespace TestAreaDemo.Services
{
    public class MessageService : BaseService<Message>, IMessageService
    {
        public new readonly WebDbContext MyDbContext;

        public MessageService(WebDbContext _MyDbContext)
            : base(_MyDbContext)
        {
            MyDbContext = _MyDbContext;
        }

        public void DeleteRange(IEnumerable<int> ArrKeyId)
        {
            var ArrMessage = ArrKeyId.Select(x => new Models.Message { Id = x });
            foreach (var OMessage in ArrMessage)
            {
                MyDbContext.Entry(OMessage).State = EntityState.Deleted;
            }
        }

        public void DeleteRange(IQueryable<Models.Message> ArrMessage)
        {
            foreach (var OMessage in ArrMessage)
            {
                MyDbContext.Entry(OMessage).State = EntityState.Deleted;
            }
        }
    }
}