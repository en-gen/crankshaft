# crankshaft

[![Slack](https://img.shields.io/badge/Slack-Channel-blue.svg)](https://en-gen.slack.com/messages/crankshaft/) [![GitHub license](https://img.shields.io/github/license/en-gen/crankshaft.svg)](https://raw.githubusercontent.com/en-gen/crankshaft/master/LICENSE)

| Branch | Nuget | Build | Test Coverage | Static Analysis |
| ------ | ----- | ----- | ------------- | --------------- |
| master | | [![Build status](https://ci.appveyor.com/api/projects/status/y7wu6ll9no2twhhp/branch/master?svg=true)](https://ci.appveyor.com/project/en-gen/crankshaft/branch/master) | [![Coverage Status](https://coveralls.io/repos/github/en-gen/crankshaft/badge.svg?branch=master)](https://coveralls.io/github/en-gen/crankshaft?branch=master) | [![Coverity](https://scan.coverity.com/projects/8159/badge.svg)](https://scan.coverity.com/projects/en-gen-crankshaft) |
| development | [![NuGet Pre Release](https://img.shields.io/nuget/vpre/Crankshaft.svg)](https://www.nuget.org/packages/Crankshaft) | [![Build status](https://ci.appveyor.com/api/projects/status/y7wu6ll9no2twhhp/branch/development?svg=true)](https://ci.appveyor.com/project/en-gen/crankshaft/branch/development) | [![Coverage Status](https://coveralls.io/repos/github/en-gen/crankshaft/badge.svg?branch=development)](https://coveralls.io/github/en-gen/crankshaft?branch=master) | [![Coverity](https://scan.coverity.com/projects/8159/badge.svg)](https://scan.coverity.com/projects/en-gen-crankshaft) |

## What is it?
A crankshaft converts the reciprocating motion of several pistons into the driveshaft's rotational motion.  Such is the purpose of this project.  It combines the efforts of many independent and modular components of business logic or infrastructure into a single pipeline to act on a given payload.

## Just show me the code.
### IMiddlware
Middleware are injected into the pipeline to act on the payload as it moves through the pipeline.  Order matters.
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
*environment*: Transitent dictionary for anything middleware might need to store or query.  For example, middleware could store an ORM session, system configuration, or some computed value performed early in the pipeline for a later middleware to consume.  Middleware should provide extensions to define their API for getting and setting environment variables.

*payload*: The object being processed by the pipeline.  It is the middleware's responsibility to determine if it's interested in the payload based on the ```environment```, the ```payload```, or both.  It's perfectly fine (and possibly common) for a middleware to ignore the payload if it's not interested.  For example, we may have a pipeline of various middleware for processing exceptions.  One middleware may be interested specifically in IO-related exceptions to send email alerts, whereas another middleware in the same pipeline may process all exceptions by logging them to a file.

### IBuildPipeline
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
We can now depend on our pipeline and use it to process ICommands.  Here is an over-simplification of what the call stack might look like:

1. ```pipeline.Process(new CreateSomethingCommand())```
2. ```NHibernateMiddleware.BeforeNext: ISession.BeginTransaction() //store session in environment for other middleware to use```
3. ```ExecuteCommandMiddleware.BeforeNext: IExecuteCommand.Execute(payload)```
4. ```ExecuteCommandMiddleware.AfterNext: environment["session"].SaveOrUpdate()```
5. ```NHibernateMiddleware.Afternext: ITransaction.Commit()```

### Fork It
You may add forks into the pipeline.  A ```ForkedMiddleware``` is a special middlware that makes a decision to continue down one of two possible execution paths.
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
Feel free to drop us a line via [issue](https://github.com/en-gen/crankshaft/issues), [pull request](https://github.com/en-gen/crankshaft/pulls), or on our [![Slack](https://img.shields.io/badge/Slack-Channel-blue.svg)](https://en-gen.slack.com/messages/crankshaft/)
