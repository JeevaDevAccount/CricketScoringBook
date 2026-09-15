using NetArchTest.Rules;
using Xunit;

// Type pointers to resolve targeted assemblies cleanly
using DomainAssembly = Domain.Aggregates.MatchAggregate.Match;
using ApplicationAssembly = Application.Matches.Commands.MatchCommands.ClaimScorerCommand;
using InfrastructureAssembly = Infrastructure.Persistence.AppDbContext;

namespace tests.Architecture;

public sealed class ArchitectureTests
{
    // Define clean constants matching your actual project namespaces to prevent string typos
    private const string DomainNamespace = "Domain";
    private const string ApplicationNamespace = "Application";
    private const string InfrastructureNamespace = "Infrastructure";
    private const string WebApiNamespace = "WebApi";

    [Fact]
    public void Domain_ShouldNotDependOn_Application()
    {
        var result = Types
            .InAssembly(typeof(DomainAssembly).Assembly)
            .ShouldNot()
            .HaveDependencyOn(ApplicationNamespace)
            .GetResult();

        Assert.True(result.IsSuccessful, $"Domain types leak into Application: {string.Join(", ", result.FailingTypes ?? [])}");
    }

    [Fact]
    public void Domain_ShouldNotDependOn_Infrastructure()
    {
        var result = Types
            .InAssembly(typeof(DomainAssembly).Assembly)
            .ShouldNot()
            .HaveDependencyOn(InfrastructureNamespace)
            .GetResult();

        Assert.True(result.IsSuccessful, $"Domain types leak into Infrastructure: {string.Join(", ", result.FailingTypes ?? [])}");
    }

    [Fact]
    public void Domain_ShouldNotDependOn_Presentation()
    {
        var result = Types
            .InAssembly(typeof(DomainAssembly).Assembly)
            .ShouldNot()
            .HaveDependencyOn(WebApiNamespace)
            .GetResult();

        Assert.True(result.IsSuccessful, $"Domain types leak into Presentation/WebApi: {string.Join(", ", result.FailingTypes ?? [])}");
    }

    [Fact]
    public void Application_ShouldNotDependOn_Infrastructure()
    {
        var result = Types
            .InAssembly(typeof(ApplicationAssembly).Assembly)
            .ShouldNot()
            .HaveDependencyOn(InfrastructureNamespace)
            .GetResult();

        Assert.True(result.IsSuccessful, $"Application layer depends on Infrastructure: {string.Join(", ", result.FailingTypes ?? [])}");
    }

    [Fact]
    public void Application_ShouldNotDependOn_Presentation()
    {
        var result = Types
            .InAssembly(typeof(ApplicationAssembly).Assembly)
            .ShouldNot()
            .HaveDependencyOn(WebApiNamespace)
            .GetResult();

        Assert.True(result.IsSuccessful, $"Application layer depends on Presentation/WebApi: {string.Join(", ", result.FailingTypes ?? [])}");
    }

    [Fact]
    public void Infrastructure_ShouldNotDependOn_Presentation()
    {
        var result = Types
            .InAssembly(typeof(InfrastructureAssembly).Assembly)
            .ShouldNot()
            .HaveDependencyOn(WebApiNamespace)
            .GetResult();

        Assert.True(result.IsSuccessful, $"Infrastructure layer depends on Presentation/WebApi: {string.Join(", ", result.FailingTypes ?? [])}");
    }
}