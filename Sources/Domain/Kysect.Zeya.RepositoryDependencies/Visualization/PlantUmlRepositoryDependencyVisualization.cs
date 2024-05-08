using Kysect.CommonLib.BaseTypes.Extensions;
using Kysect.CommonLib.Graphs;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Kysect.Zeya.RepositoryDependencies.Visualization;

public class PlantUmlRepositoryDependencyVisualization
{
    public string ConvertToString(IReadOnlyCollection<GraphLink<string>> graphLinks)
    {
        graphLinks.ThrowIfNull();

        var builder = new StringBuilder();
        // TODO: Investigate what to do with ';'. gravizo don't work without it.

        builder.AppendLine("@startuml;");

        List<string> components = graphLinks
            .SelectMany(g => new[] { g.From, g.To })
            .Distinct()
            .ToList();

        foreach (string component in components)
            builder.AppendLine($"component {component};");

        foreach ((string? from, string? to) in graphLinks)
            builder.AppendLine($"{from} --> {to};");

        builder.AppendLine("@enduml;");
        // TODO: Add other symbols replacing and use display name
        return builder.ToString().Replace("/", ".");
    }
}