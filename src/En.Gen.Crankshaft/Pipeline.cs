using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace En.Gen.Crankshaft
{
    public class Pipeline : IPipeline
    {
        protected IList<Func<IMiddleware>> Middleware { get; }

        public Pipeline(IList<Func<IMiddleware>> middleware)
        {
            Middleware = middleware;
        }

        public async Task<bool> Process(object payload)
        {
            var enumerator = Middleware.GetEnumerator();
            return await InvokeMiddleware(enumerator, payload);
        }

        private static async Task<bool> InvokeMiddleware(IEnumerator<Func<IMiddleware>> enumerator, object payload)
        {
            var success = true;
            if (enumerator.MoveNext())
            {
                var createMiddleware = enumerator.Current;
                var middleware = createMiddleware();
                success = await middleware.Process(payload);
                if (success)
                {
                    success = await InvokeMiddleware(enumerator, payload);
                    await middleware.PostProcess(payload);
                }
            }
            return success;
        }
    }
}
