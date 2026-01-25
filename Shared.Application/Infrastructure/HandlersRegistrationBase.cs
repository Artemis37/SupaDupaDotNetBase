using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using Shared.Application.Interfaces;

namespace Shared.Application.Infrastructure;

public abstract class HandlersRegistrationBase
{
    public abstract string AssemblyName { get; }
    protected abstract Type ToDecorator(object attribute);

    protected HandlersRegistrationBase(IServiceCollection services)
    {
        AddHandlers(services);
    }

    private void AddHandlers(IServiceCollection services)
    {
        var assembly = GetAssemblyByName(AssemblyName);
        List<Type> handlerTypes = assembly.GetTypes()
            .Where(x => x.GetInterfaces().Any(IsHandlerInterface))
            .Where(x => x.Name.EndsWith("Handler"))
            .Where(x => !x.IsAbstract)
            .ToList();

        foreach (Type type in handlerTypes)
        {
            AddHandler(services, type);
        }
    }

    private Assembly GetAssemblyByName(string name)
    {
        return AppDomain.CurrentDomain.GetAssemblies()
            .SingleOrDefault(assembly => assembly.GetName().Name == name);
    }

    private void AddHandler(IServiceCollection services, Type type)
    {
        object[] attributes = type.GetCustomAttributes(false)
            .Where(attr => attr is IDecoratorAttribute)
            .ToArray();

        Type interfaceType = type.GetInterfaces().Single(IsHandlerInterface);
        
        List<Type> pipeline = attributes
            .Select(ToDecorator)
            .Concat(new[] { type })
            .Reverse()
            .ToList();

        Func<IServiceProvider, object> factory = BuildPipeline(pipeline, interfaceType);

        services.AddTransient(interfaceType, factory);
    }

    private Func<IServiceProvider, object> BuildPipeline(List<Type> pipeline, Type interfaceType)
    {
        List<ConstructorInfo> constructorInfos = pipeline
            .Select(x =>
            {
                Type type = x.IsGenericType ? x.MakeGenericType(interfaceType.GenericTypeArguments) : x;
                return type.GetConstructors().Single();
            })
            .ToList();

        Func<IServiceProvider, object> func = provider =>
        {
            object current = null;

            foreach (ConstructorInfo ctor in constructorInfos)
            {
                List<ParameterInfo> parameterInfos = ctor.GetParameters().ToList();
                object[] parameters = GetParameters(parameterInfos, current, provider);
                current = ctor.Invoke(parameters);
            }

            return current;
        };

        return func;
    }

    private object[] GetParameters(List<ParameterInfo> parameterInfos, object current, IServiceProvider provider)
    {
        var result = new object[parameterInfos.Count];

        for (int i = 0; i < parameterInfos.Count; i++)
        {
            result[i] = GetParameter(parameterInfos[i], current, provider);
        }

        return result;
    }

    private object GetParameter(ParameterInfo parameterInfo, object current, IServiceProvider provider)
    {
        Type parameterType = parameterInfo.ParameterType;

        if (IsHandlerInterface(parameterType))
            return current;

        object service = provider.GetService(parameterType);
        if (service != null)
            return service;

        throw new ArgumentException($"Type {parameterType} not found in DI container");
    }

    private bool IsHandlerInterface(Type type)
    {
        if (!type.IsGenericType)
            return false;

        Type typeDefinition = type.GetGenericTypeDefinition();

        return typeDefinition == typeof(ICommandHandler<>) || 
               typeDefinition == typeof(IQueryHandler<,>);
    }
}
