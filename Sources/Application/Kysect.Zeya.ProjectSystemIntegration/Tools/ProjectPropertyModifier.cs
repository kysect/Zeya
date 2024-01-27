using Kysect.DotnetProjectSystem.Projects;
using Kysect.Zeya.ProjectSystemIntegration.XmlDocumentModificationStrategies;
using Kysect.Zeya.ProjectSystemIntegration.XmlProjectFileModifyStrategies;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Linq;

namespace Kysect.Zeya.ProjectSystemIntegration.Tools;

public class ProjectPropertyModifier
{
    private readonly DotnetProjectFile _projectFileAccessor;
    private readonly ILogger _logger;

    public ProjectPropertyModifier(DotnetProjectFile projectFileAccessor, ILogger logger)
    {
        _projectFileAccessor = projectFileAccessor;
        _logger = logger;

        projectFileAccessor.UpdateDocument(CreateProjectDocumentIfEmptyModificationStrategy.Instance);
    }

    public void AddOrUpdateProperty(string key, string value)
    {
        IReadOnlyCollection<DotnetProjectProperty> properties = _projectFileAccessor.GetProperties(key);
        if (properties.Any())
        {
            if (properties.Count > 1)
                _logger.LogWarning("Found multiple nodes with name {Name}", key);

            _projectFileAccessor.UpdateDocument(new AddProjectPropertyValueModificationStrategy(key, value));
            return;
        }

        _projectFileAccessor.UpdateDocument(AddProjectGroupNodeIfNotExistsModificationStrategy.PropertyGroup);
        _projectFileAccessor.UpdateDocument(new UpdateProjectPropertyValueModificationStrategy(key, value));
    }
}