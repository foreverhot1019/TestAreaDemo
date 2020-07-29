using BaseService;
using System.Collections.Generic;
using System.Linq;

namespace TestAreaDemo.Services
{
    public interface IMessageService : IBaseService<Models.Message>
    {
        void DeleteRange(IEnumerable<int> ArrKeyId);

        void DeleteRange(IQueryable<Models.Message> ArrActor);
    }
}
