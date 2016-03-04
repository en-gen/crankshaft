using System;
using System.Threading.Tasks;

namespace En.Gen.Crankshaft
{
    public interface IPipeline<in TPayload>
    {
        Task<bool> Process(TPayload payload);
    }
    public interface IPipeline : IPipeline<object>
    {
    }
}