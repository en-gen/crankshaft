using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

[assembly: InternalsVisibleTo("En.Gen.Crankshaft.Tests")]

namespace En.Gen.Crankshaft.Fork
{
    internal class ForkedPipeline : Pipeline<object>, IForkedPipeline
    {
        public ForkedPipeline(IList<Func<IMiddleware>> middleware)
            : base(middleware)
        {
        }

        public async Task<bool> Process(IDictionary<string, object> environment, object payload)
        {
            var enumerator = Middleware.GetEnumerator();
            return await InvokeMiddleware(enumerator, environment, payload);
        }
    }
}
