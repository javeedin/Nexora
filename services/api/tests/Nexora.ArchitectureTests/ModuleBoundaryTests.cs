using System.Reflection;
using NetArchTest.Rules;
using Nexora.BuildingBlocks.Modules;

namespace Nexora.ArchitectureTests;

/// <summary>
/// ADR 0003: one module = one folder + own schema; modules talk to each other only through <c>*.Contracts</c>.
/// Module assemblies are discovered by name, so a new module is covered without editing this file. Cross-module rules
/// check assembly references (exact; the compiler drops unused references) — namespace prefixes would confuse
/// <c>Nexora.Modules.X</c> with <c>Nexora.Modules.X.Contracts</c>.
/// </summary>
public sealed class ModuleBoundaryTests
{
    private const string ModulePrefix = "Nexora.Modules.";

    private static readonly Assembly[] All = LoadNexoraAssemblies();

    private static Assembly[] ModuleAssemblies =>
        [.. All.Where(a => a.GetName().Name!.StartsWith(ModulePrefix, StringComparison.Ordinal) && !IsContracts(a))];

    private static Assembly[] ContractAssemblies => [.. All.Where(IsContracts)];

    public static TheoryData<string> Modules => [.. ModuleAssemblies.Select(a => a.GetName().Name!)];

    [Fact]
    public void Modules_are_discovered()
    {
        Assert.Contains("Nexora.Modules.Platform", ModuleAssemblies.Select(a => a.GetName().Name));
        Assert.Contains("Nexora.Modules.Platform.Contracts", ContractAssemblies.Select(a => a.GetName().Name));
    }

    [Theory]
    [MemberData(nameof(Modules))]
    public void A_module_uses_other_modules_only_through_their_contracts(string module)
    {
        var otherInternals = ModuleAssemblies.Select(a => a.GetName().Name!).Where(n => n != module).ToHashSet();
        var violations = References(Load(module)).Where(otherInternals.Contains).ToList();
        Assert.True(violations.Count == 0, $"{module} references other modules' internals: {string.Join(", ", violations)}");
    }

    [Theory]
    [MemberData(nameof(Modules))]
    public void A_module_does_not_depend_on_the_host(string module)
    {
        var result = Types.InAssembly(Load(module)).ShouldNot().HaveDependencyOn("Nexora.Api").GetResult();
        Assert.True(result.IsSuccessful, Describe(result));
    }

    [Theory]
    [MemberData(nameof(Modules))]
    public void A_module_exposes_exactly_one_sealed_IModule(string module)
    {
        var entryPoints = Load(module).GetTypes().Where(t => typeof(IModule).IsAssignableFrom(t) && !t.IsAbstract).ToList();
        var entry = Assert.Single(entryPoints);
        Assert.True(entry.IsSealed, $"{entry.FullName} must be sealed");
    }

    [Fact]
    public void Contracts_depend_on_nothing_but_the_base_library_and_other_contracts()
    {
        foreach (var contracts in ContractAssemblies)
        {
            var violations = References(contracts)
                .Where(r => !r.StartsWith("System", StringComparison.Ordinal) && r != "netstandard" && !r.EndsWith(".Contracts", StringComparison.Ordinal))
                .ToList();
            Assert.True(violations.Count == 0, $"{contracts.GetName().Name} references {string.Join(", ", violations)}");
        }
    }

    [Fact]
    public void Building_blocks_know_nothing_about_modules_or_the_host()
    {
        var result = Types.InAssembly(typeof(IModule).Assembly)
            .ShouldNot().HaveDependencyOnAny([.. All.Select(a => a.GetName().Name!).Where(n => n.StartsWith(ModulePrefix, StringComparison.Ordinal)), "Nexora.Api"])
            .GetResult();
        Assert.True(result.IsSuccessful, Describe(result));
    }

    private static IEnumerable<string> References(Assembly assembly) => assembly.GetReferencedAssemblies().Select(r => r.Name!);

    private static bool IsContracts(Assembly assembly) => assembly.GetName().Name!.EndsWith(".Contracts", StringComparison.Ordinal);

    private static Assembly Load(string name) => All.Single(a => a.GetName().Name == name);

    private static string Describe(NetArchTest.Rules.TestResult result) =>
        "Violations: " + string.Join(", ", result.FailingTypeNames ?? []);

    private static Assembly[] LoadNexoraAssemblies()
    {
        var directory = AppContext.BaseDirectory;
        return [.. Directory.GetFiles(directory, "Nexora.*.dll")
            .Where(f => !Path.GetFileName(f).Contains("Tests", StringComparison.Ordinal))
            .Select(Assembly.LoadFrom)];
    }
}
