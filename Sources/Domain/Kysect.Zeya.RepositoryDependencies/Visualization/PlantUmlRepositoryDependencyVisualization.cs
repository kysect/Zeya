using Kysect.CommonLib.BaseTypes.Extensions;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Kysect.Zeya.RepositoryDependencies.Visualization;

// TODO: I hope this class will be replaced with library for PlantUML
public class PlantUmlRepositoryDependencyVisualization
{
    public string ConvertToString(IReadOnlyCollection<RepositoryDependencyLink> graphLinks, IReadOnlyCollection<string> repositoriesWithDiagnostics)
    {
        const string orangeColorCode = "%23F80";
        const string redColorCode = "%23F22";
        graphLinks.ThrowIfNull();

        var builder = new StringBuilder();
        // TODO: Investigate what to do with ';'. gravizo don't work without it.

        builder.AppendLine("@startuml;");
        builder.AppendLine("skinparam monochrome false;");

        List<string> components = graphLinks
            .SelectMany(g => new[] { g.From, g.To })
            .Distinct()
            .ToList();

        foreach (string component in components)
        {
            if (!repositoriesWithDiagnostics.Contains(component))
            {
                builder.AppendLine($"component {component};");
            }
            else
            {
                builder.AppendLine($"component {component} {orangeColorCode};");
            }
        }

        foreach ((string? from, string? to, bool isActual) in graphLinks)
        {
            if (isActual)
            {
                builder.AppendLine($"{from} --> {to};");
            }
            else
            {
                builder.AppendLine($"{from} --> {to} {redColorCode};");
            }
        }

        builder.AppendLine("@enduml;");
        // TODO: Add other symbols replacing and use display name
        return builder.ToString().Replace("/", ".");
    }
}