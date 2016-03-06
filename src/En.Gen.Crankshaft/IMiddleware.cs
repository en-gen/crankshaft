using System.Collections.Generic;
using System.Threading.Tasks;

namespace En.Gen.Crankshaft
{
    public interface IMiddleware
    {
        Task<bool> BeforeNext(IDictionary<string, object> environment, object payload);
        Task AfterNext(IDictionary<string, object> environment, object payload);
    }
}
