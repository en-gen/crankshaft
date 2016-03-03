using System.Threading.Tasks;

namespace En.Gen.Crankshaft
{
    public interface IMiddleware
    {
        Task<bool> Process(object payload);
        Task PostProcess(object payload);
    }
}
