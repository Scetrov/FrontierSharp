using FluentAssertions;
using FrontierSharp.CommandLine;
using Microsoft.Extensions.DependencyInjection;
using NSubstitute;
using Spectre.Console.Cli;
using Xunit;

// ReSharper disable MemberCanBePrivate.Global

namespace FrontierSharp.Tests.CommandLine;

public class TypeRegistrarTests {
    private readonly TypeRegistrar _registrar;
    private readonly IServiceCollection _services;

    public TypeRegistrarTests() {
        _services = new ServiceCollection();
        _registrar = new TypeRegistrar(_services);
    }

    [Fact]
    public void Register_ShouldAddSingletonService() {
        _registrar.Register(typeof(IMyService), typeof(MyService));

        var provider = _services.BuildServiceProvider();
        var resolved = provider.GetService<IMyService>();

        resolved.Should().BeOfType<MyService>();
    }

    [Fact]
    public void RegisterInstance_ShouldAddSingletonInstance() {
        var instance = Substitute.For<IMyService>();

        _registrar.RegisterInstance(typeof(IMyService), instance);

        var provider = _services.BuildServiceProvider();
        var resolved = provider.GetService<IMyService>();

        resolved.Should().BeSameAs(instance);
    }

    [Fact]
    public void RegisterLazy_ShouldAddFactorySingleton() {
        var factoryCalled = false;

        _registrar.RegisterLazy(typeof(IMyService), Factory);

        var provider = _services.BuildServiceProvider();
        var resolved = provider.GetService<IMyService>();

        factoryCalled.Should().BeTrue();
        resolved.Should().BeOfType<MyService>();
        return;

        object Factory() {
            factoryCalled = true;
            return new MyService();
        }
    }

    [Fact]
    public void Build_ShouldReturnTypeResolverWithRegisteredServices() {
        _registrar.Register(typeof(IMyService), typeof(MyService));
        var resolver = _registrar.Build();

        var resolved = resolver.Resolve(typeof(IMyService));

        resolved.Should().BeOfType<MyService>();
    }

    public interface IMyService {
    }

    public class MyService : IMyService {
    }

    public class TypeResolver(IServiceProvider provider) : ITypeResolver {
        public object Resolve(Type? type) {
            ArgumentNullException.ThrowIfNull(type);
            return provider.GetService(type)!;
        }
    }
}