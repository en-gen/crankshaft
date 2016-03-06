using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace En.Gen.Crankshaft.Fork
{
    internal interface IForkedPipeline : IPipeline<object>
    {
        Task<bool> Process(IDictionary<string, object> environment, object payload);
    }
}
