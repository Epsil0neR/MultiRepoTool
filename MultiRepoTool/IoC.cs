using System;
using System.Collections.Generic;
using System.Linq;
using MultiRepoTool.Extensions;
using Unity;
using Unity.Lifetime;

namespace MultiRepoTool;

public static class IoC
{
    private static IUnityContainer _container = _container ??= InitializeUnityContainer();

    public static IUnityContainer Container => _container;

    private static IUnityContainer InitializeUnityContainer()
    {
        var container = new UnityContainer();
        container.RegisterInstance(container);
        container.AddNewExtension<DisposeDisposablesExtension>();
        return container;
    }

    public static void Register<TFrom, TTo>() where TTo : TFrom
    {
        _container.RegisterType<TFrom, TTo>();
    }

    public static T Resolve<T>()
    {
        return _container.Resolve<T>();
    }

    public static object Resolve(Type type)
    {
        return _container.Resolve(type);
    }

    public static TCast Resolve<TCast>(Type t)
    {
        return (TCast) _container.Resolve(t);
    }

    public static T Resolve<T>(string name)
    {
        return _container.Resolve<T>(name);
    }

    public static void Register<T>()
    {
        _container.RegisterType<T>();
    }

    public static void Register<T>(Func<IUnityContainer, T> factory)
    {
        _container.RegisterFactory<T>(c => factory(c));
    }

    public static void RegisterSingleton<T>()
    {
        _container.RegisterSingleton<T>();
    }

    public static void RegisterSingleton<TFrom, TTo>() where TTo : TFrom
    {
        _container.RegisterSingleton<TFrom, TTo>();
    }

    public static void RegisterSingleton<T>(Func<IUnityContainer, T> factory)
    {
        _container.RegisterFactory<T>(c => factory(c), new ContainerControlledLifetimeManager());
    }

    public static void RegisterInstance<T>(T instance)
    {
        _container.RegisterInstance(typeof(T), instance);
    }

    public static void RegisterInstance(Type type, object instance)
    {
        _container.RegisterInstance(type, instance);
    }

    public static void RegisterInstance<T>(string name, T instance)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentNullException(nameof(name));

        _container.RegisterInstance(typeof(T), name, instance);
    }

    public static IEnumerable<T> ResolveAll<T>() where T : class
    {
        var t = typeof(T);
        var types = AppDomain.CurrentDomain.GetAssemblies()
            .SelectMany(x => x.GetTypes())
            .Where(x => t.IsAssignableFrom(x));

        foreach (var type in types)
        {
            if (ReferenceEquals(t, type))
                continue;

            if (type.IsInterface)
                continue;

            if (type.IsAbstract)
                continue;

            if (Resolve(type) is T rv)
                yield return rv;
        }
    }
}