using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace En.Gen.Crankshaft.Fork
{

    internal class ForkedPipelineBuilder : PipelineBuilder
    {
        public ForkedPipelineBuilder(IResolveMiddlewareFactory factoryResolver)
            : base(factoryResolver)
        {
        }

        public new IForkedPipeline Build<TPayload>()
        {
            return new ForkedPipeline(Middleware.ToList());
        }
    }
}
