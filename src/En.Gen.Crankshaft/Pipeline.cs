using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace En.Gen.Crankshaft
{
    public class Pipeline<TPayload> : IPipeline<TPayload>
    {
        protected IList<Func<IMiddleware>> Middleware { get; }

        public Pipeline(IList<Func<IMiddleware>> middleware)
        {
            Middleware = middleware;
        }

        public async Task<bool> Process(TPayload payload)
        {
            var enumerator = Middleware.GetEnumerator();
            return await InvokeMiddleware(enumerator, new Dictionary<string, object>(), payload);
        }

        protected static async Task<bool> InvokeMiddleware(
            IEnumerator<Func<IMiddleware>> enumerator,
            IDictionary<string, object> environment,
            object payload)
        {
            var success = true;
            if (enumerator.MoveNext())
            {
                var createMiddleware = enumerator.Current;
                var middleware = createMiddleware();
                success = await middleware.BeforeNext(environment, payload);
                if (success)
                {
                    success = await InvokeMiddleware(enumerator, environment, payload);
                    await middleware.AfterNext(environment, payload);
                }
            }
            return success;
        }
    }
}
