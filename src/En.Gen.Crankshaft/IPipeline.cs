using System.Threading.Tasks;

namespace En.Gen.Crankshaft
{
    public interface IPipeline
    {
        Task<bool> Process(object payload);
    }
}