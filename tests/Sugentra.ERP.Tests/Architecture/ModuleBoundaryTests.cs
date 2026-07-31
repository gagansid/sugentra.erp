using NetArchTest.Rules;
using Sugentra.ERP.Api.Shared.Persistence;

namespace Sugentra.ERP.Tests.Architecture;

/// <summary>
/// Enforces module isolation without separate assemblies: a type under Modules.X
/// must never reference another module's namespace directly (Shared.* is always allowed).
/// </summary>
public class ModuleBoundaryTests
{
    private static readonly System.Reflection.Assembly ApiAssembly = typeof(IDbConnectionFactory).Assembly;

    [Fact]
    public void Modules_Should_Not_Reference_Other_Modules_Directly()
    {
        var moduleNamespaces = Types.InAssembly(ApiAssembly)
            .That().ResideInNamespace("Sugentra.ERP.Api.Modules")
            .GetTypes()
            .Select(t => t.Namespace!)
            .Where(ns => ns is not null)
            .Select(ns => ns.Split('.').Take(4).Aggregate((a, b) => $"{a}.{b}")) // Sugentra.ERP.Api.Modules.<ModuleName>
            .Distinct()
            .ToList();

        var failures = new List<string>();

        foreach (var moduleNamespace in moduleNamespaces)
        {
            var otherModuleNamespaces = moduleNamespaces.Where(ns => ns != moduleNamespace);

            foreach (var otherNamespace in otherModuleNamespaces)
            {
                var result = Types.InAssembly(ApiAssembly)
                    .That().ResideInNamespace(moduleNamespace)
                    .ShouldNot().HaveDependencyOn(otherNamespace)
                    .GetResult();

                if (!result.IsSuccessful)
                {
                    failures.Add($"{moduleNamespace} depends on {otherNamespace} (violates module isolation): " +
                        string.Join(", ", result.FailingTypeNames ?? Array.Empty<string>()));
                }
            }
        }

        Assert.True(failures.Count == 0, string.Join("\n", failures));
    }
}
