using NetArchTest.Rules;

using DomainAssembly =
    Domain.Aggregates.MatchAggregate.Match;

using ApplicationAssembly =
    Application.Matches.Commands.MatchCommands.ClaimScorerCommand;

using InfrastructureAssembly =
    Infrastructure.Persistence.AppDbContext;

namespace tests.Test;

public sealed class ArchitectureTests
{
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

        AssertArchitectureRule(
            result,
            "Domain must not depend on Application.");
    }

    [Fact]
    public void Domain_ShouldNotDependOn_Infrastructure()
    {
        var result = Types
            .InAssembly(typeof(DomainAssembly).Assembly)
            .ShouldNot()
            .HaveDependencyOn(InfrastructureNamespace)
            .GetResult();

        AssertArchitectureRule(
            result,
            "Domain must not depend on Infrastructure.");
    }

    [Fact]
    public void Domain_ShouldNotDependOn_WebApi()
    {
        var result = Types
            .InAssembly(typeof(DomainAssembly).Assembly)
            .ShouldNot()
            .HaveDependencyOn(WebApiNamespace)
            .GetResult();

        AssertArchitectureRule(
            result,
            "Domain must not depend on WebApi.");
    }

    [Fact]
    public void Application_ShouldNotDependOn_Infrastructure()
    {
        var result = Types
            .InAssembly(typeof(ApplicationAssembly).Assembly)
            .ShouldNot()
            .HaveDependencyOn(InfrastructureNamespace)
            .GetResult();

        AssertArchitectureRule(
            result,
            "Application must not depend on Infrastructure.");
    }

    [Fact]
    public void Application_ShouldNotDependOn_WebApi()
    {
        var result = Types
            .InAssembly(typeof(ApplicationAssembly).Assembly)
            .ShouldNot()
            .HaveDependencyOn(WebApiNamespace)
            .GetResult();

        AssertArchitectureRule(
            result,
            "Application must not depend on WebApi.");
    }

    [Fact]
    public void Infrastructure_ShouldNotDependOn_WebApi()
    {
        var result = Types
            .InAssembly(typeof(InfrastructureAssembly).Assembly)
            .ShouldNot()
            .HaveDependencyOn(WebApiNamespace)
            .GetResult();

        AssertArchitectureRule(
            result,
            "Infrastructure must not depend on WebApi.");
    }

    private static void AssertArchitectureRule(
        TestResult result,
        string message)
    {
        Assert.True(
            result.IsSuccessful,
            $"{message} " +
            $"Failing types: {string.Join(", ", result.FailingTypes ?? [])}");
    }
}