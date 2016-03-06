using System;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace En.Gen.Crankshaft
{
    public interface IPipeline<in TPayload>
    {
        Task<bool> Process(TPayload payload);
    }
}