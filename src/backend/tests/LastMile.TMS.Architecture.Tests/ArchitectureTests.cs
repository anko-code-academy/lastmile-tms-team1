using FluentAssertions;
using NetArchTest.Rules;

namespace LastMile.TMS.Architecture.Tests;

public class ArchitectureTests
{
    private const string DomainNamespace = "LastMile.TMS.Domain";
    private const string ApplicationNamespace = "LastMile.TMS.Application";
    private const string InfrastructureNamespace = "LastMile.TMS.Infrastructure";
    private const string PersistenceNamespace = "LastMile.TMS.Persistence";
    private const string ApiNamespace = "LastMile.TMS.Api";

    [Fact]
    public void Domain_Should_Not_Depend_On_Other_Projects()
    {
        var assembly = typeof(Domain.Common.BaseEntity).Assembly;

        var result = Types.InAssembly(assembly)
            .ShouldNot()
            .HaveDependencyOnAny(
                ApplicationNamespace,
                InfrastructureNamespace,
                PersistenceNamespace,
                ApiNamespace)
            .GetResult();

        result.IsSuccessful.Should().BeTrue();
    }

    [Fact]
    public void Application_Should_Not_Depend_On_Infrastructure_Or_Persistence()
    {
        var assembly = typeof(Application.DependencyInjection).Assembly;

        var result = Types.InAssembly(assembly)
            .ShouldNot()
            .HaveDependencyOnAny(
                InfrastructureNamespace,
                PersistenceNamespace,
                ApiNamespace)
            .GetResult();

        result.IsSuccessful.Should().BeTrue();
    }

    [Fact]
    public void Infrastructure_Should_Not_Depend_On_Persistence_Or_Api()
    {
        var assembly = typeof(Infrastructure.DependencyInjection).Assembly;

        var result = Types.InAssembly(assembly)
            .ShouldNot()
            .HaveDependencyOnAny(
                PersistenceNamespace,
                ApiNamespace)
            .GetResult();

        result.IsSuccessful.Should().BeTrue();
    }

    [Fact]
    public void Persistence_Should_Not_Depend_On_Infrastructure_Or_Api()
    {
        var assembly = typeof(Persistence.AppDbContext).Assembly;

        var result = Types.InAssembly(assembly)
            .ShouldNot()
            .HaveDependencyOnAny(
                InfrastructureNamespace,
                ApiNamespace)
            .GetResult();

        result.IsSuccessful.Should().BeTrue();
    }
}
