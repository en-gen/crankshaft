# crankshaft

[![Join the chat at https://gitter.im/en-gen/crankshaft](https://badges.gitter.im/en-gen/crankshaft.svg)](https://gitter.im/en-gen/crankshaft?utm_source=badge&utm_medium=badge&utm_campaign=pr-badge&utm_content=badge) [![GitHub license](https://img.shields.io/github/license/en-gen/crankshaft.svg)](https://raw.githubusercontent.com/en-gen/crankshaft/master/LICENSE)

| Branch | Nuget | Build | Test Coverage | Static Analysis |
| ------ | ----- | ----- | ------------- | --------------- |
| master | | [![Build status](https://ci.appveyor.com/api/projects/status/y7wu6ll9no2twhhp/branch/master?svg=true)](https://ci.appveyor.com/project/en-gen/crankshaft/branch/master) | [![Coverage Status](https://coveralls.io/repos/github/en-gen/crankshaft/badge.svg?branch=master)](https://coveralls.io/github/en-gen/crankshaft?branch=master) | [![Coverity](https://scan.coverity.com/projects/8159/badge.svg)](https://scan.coverity.com/projects/en-gen-crankshaft) |
| development | [![NuGet Pre Release](https://img.shields.io/nuget/vpre/Crankshaft.svg)](https://www.nuget.org/packages/Crankshaft) | [![Build status](https://ci.appveyor.com/api/projects/status/y7wu6ll9no2twhhp/branch/development?svg=true)](https://ci.appveyor.com/project/en-gen/crankshaft/branch/development) | [![Coverage Status](https://coveralls.io/repos/github/en-gen/crankshaft/badge.svg?branch=development)](https://coveralls.io/github/en-gen/crankshaft?branch=master) | [![Coverity](https://scan.coverity.com/projects/8159/badge.svg)](https://scan.coverity.com/projects/en-gen-crankshaft) |

### What is it?
A crankshaft converts the reciprocating motion of several pistons into the driveshaft's rotational motion.  Such is the purpose of this project.  It combines the efforts of many independent and modular components of business logic or infrastructure into a single pipeline to act on a given payload.

## Under the hood
### PipelineBuilder
It all starts by building a pipeline.  We've chosen Autofac for dependency injection, but an interface is provided if you need to plug into some other strategy for resolving middleware.
```csharp
protected override void Load(ContainerBuilder builder)
{
    builder.Register(SuperCoolPipeline);
}

private IPipeline<ICommand> SuperCoolPipeline(IComponentContext context)
{
    var factory = new AutofacMiddlewareFactoryResolver(context);
    IPipeline<ICommand> pipeline = new PipelineBuilder(factory)
        .Use<NHibernateMiddleware>()
        .Use<ExecuteCommandMiddleware>()
        .Build<ICommand>();
    return pipeline;
}
```
We can now depend on our pipeline and use it to process ICommands.
### Pipeline
Let's take a quick look at how the pipeline actually executes.  You'll notice each middleware has ```BeforeNext``` and ```AfterNext``` methods that wrap the next middleware execution in the pipeline.  Additionally, any middlware has the ability to halt execution by returning ```false```.
```csharp
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
```
### Middlware
Middleware are injected into the pipeline at runtime to act on the payload as it moves through the pipeline.
```csharp
public class ConsoleLogMiddleware : IMiddleware
{
  public Task<bool> BeforeNext(IDictionary<string, object> environment, object payload)
  {
    Console.WriteLine("Set up some stuff before the next middleware in the pipeline runs.");
  }
  
  public Task AfterNext(IDictionary<string, object> environment, object payload)
  {
    Console.WriteLine("Clean up after the next middleware in the pipeline ran.");
  }
}
```
### Fork It
You may add forks into the pipeline.  A ```ForkedMiddleware``` is a special middlware that makes a decision to continue down one of two possible execution paths.
```csharp
public virtual async Task<bool> BeforeNext(IDictionary<string, object> environment, object payload)
{
	var pipeline = ChoosePipeline(payload) as IForkedPipeline;
	if (pipeline != null)
	{
		return await pipeline.Process(environment, payload);
	}
	return true;
}

protected abstract IPipeline<object> ChoosePipeline(object payload);
```
Forks can be used to make more discrete, decision-based pipelines.
```csharp
private IPipeline<ICommand> EvenCoolerPipeline(IComponentContext context)
{
    var factory = new AutofacMiddlewareFactoryResolver(context);
    return new PipelineBuilder(factory)
        .Use<NHibernateMiddleware>()
        .Fork<ValidateCommandMiddleware>(
            errorsPipeline => errorsPipeline
                .Use<LogErrorMiddleware>()
                .Use<SendEmailAlertMiddleware>(),
            validPipeline => validPipeline
                .Use<ExecuteCommandMiddleware>())
        .Build<ICommand>();
}
```


### Contact
Feel free to drop us a line via [issue](https://github.com/en-gen/crankshaft/issues), [pull request](https://github.com/en-gen/crankshaft/pulls), or [![Join the chat at https://gitter.im/en-gen/crankshaft](https://badges.gitter.im/en-gen/crankshaft.svg)](https://gitter.im/en-gen/crankshaft?utm_source=badge&utm_medium=badge&utm_campaign=pr-badge&utm_content=badge)
