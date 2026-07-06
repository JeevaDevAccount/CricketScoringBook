using Xunit;
using NetArchTest.Rules;
using Domain.Aggregates.MatchAggregate;
using Application.Common.Interfaces;
using Infrastructure.Persistence;

namespace Tests.Architecture;

public sealed class ArchitectureTests
{
    private const string DomainNamespace = "Domain";
    private const string ApplicationNamespace = "Application";
    private const string InfrastructureNamespace = "Infrastructure";
    private const string WebApiNamespace = "WebApi";

    [Fact]
    public void Domain_Should_Not_Have_Dependencies_On_Other_Layers()
    {
        // Target your real BallEvent record as the structural anchor for the Domain assembly
        var types = Types.InAssembly(typeof(BallEvent).Assembly);

        var result = types
            .ShouldNot()
            .HaveDependencyOnAll(ApplicationNamespace, InfrastructureNamespace, WebApiNamespace)
            .GetResult();

        Assert.True(result.IsSuccessful, 
            "Architectural Violation: The Domain layer must remain pure and free from outside cross-contamination.");
    }

    [Fact]
    public void Application_Should_Not_Have_Dependencies_On_Infrastructure_Or_WebApi()
    {
        // Target your real, concrete application interface repository contract as the assembly anchor
        var types = Types.InAssembly(typeof(IBallEventRepository).Assembly);

        var result = types
            .ShouldNot()
            .HaveDependencyOnAll(InfrastructureNamespace, WebApiNamespace)
            .GetResult();

        Assert.True(result.IsSuccessful, 
            "Architectural Violation: The Application layer must never statically depend on infrastructure databases or web layers.");
    }

    [Fact]
    public void Infrastructure_Should_Not_Have_Dependencies_On_WebApi()
    {
        // Target your database persistence Anchor class as the assembly anchor
        var types = Types.InAssembly(typeof(Anchor).Assembly);

        var result = types
            .ShouldNot()
            .HaveDependencyOn(WebApiNamespace)
            .GetResult();

        Assert.True(result.IsSuccessful, 
            "Architectural Violation: The Infrastructure layer cannot reference presentation components or API models.");
    }
}