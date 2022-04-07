using System;
using Serilog;
using SimpleInjector;
using SimpleInjector.Advanced;

namespace EVESharp.Node.SimpleInject;

/// <summary>
/// Custom behaviour for automatic serilog instantiation
///
/// Creates ILogger for specific contexts automatically for dependency injection
/// </summary>
public class SerilogContextualLoggerInjectionBehavior : IDependencyInjectionBehavior
{
    private readonly ILogger                      BaseLogger;
    private readonly Container                    Container;
    private readonly IDependencyInjectionBehavior Original;

    public SerilogContextualLoggerInjectionBehavior (ContainerOptions options, ILogger baseLogger)
    {
        this.Original   = options.DependencyInjectionBehavior;
        this.Container  = options.Container;
        this.BaseLogger = baseLogger;
    }

    public bool VerifyDependency (InjectionConsumerInfo dependency, out string msg)
    {
        return this.Original.VerifyDependency (dependency, out msg);
    }

    public InstanceProducer GetInstanceProducer (InjectionConsumerInfo i, bool t)
    {
        return i.Target.TargetType == typeof (ILogger)
            ? this.GetLoggerInstanceProducer (i.ImplementationType)
            : this.Original.GetInstanceProducer (i, t);
    }

    private InstanceProducer <ILogger> GetLoggerInstanceProducer (Type type)
    {
        return Lifestyle.Singleton.CreateProducer (() => this.BaseLogger.ForContext (type), this.Container);
    }
}