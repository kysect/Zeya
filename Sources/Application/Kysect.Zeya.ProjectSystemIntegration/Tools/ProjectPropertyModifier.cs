using System.Linq;
using Kysect.DotnetSlnParser.Parsers;
using Kysect.Zeya.ProjectSystemIntegration.XmlDocumentModificationStrategies;
using Kysect.Zeya.ProjectSystemIntegration.XmlProjectFileModifyStrategies;
using Microsoft.Extensions.Logging;

namespace Kysect.Zeya.ProjectSystemIntegration.Tools;

public class ProjectPropertyModifier
{
    private readonly XmlProjectFileAccessor _projectFileAccessor;
    private readonly ILogger _logger;

    public ProjectPropertyModifier(XmlProjectFileAccessor projectFileAccessor, ILogger logger)
    {
        _projectFileAccessor = projectFileAccessor;
        _logger = logger;

        projectFileAccessor.UpdateDocument(CreateProjectDocumentIfEmptyModificationStrategy.Instance);
    }

    public void AddOrUpdateProperty(string key, string value)
    {
        var propertyNodes = _projectFileAccessor.GetNodesByName(key);
        if (propertyNodes.Any())
        {
            if (propertyNodes.Count > 1)
                _logger.LogWarning("Found multiple nodes with name {Name}", key);

            _projectFileAccessor.UpdateDocument(new AddProjectPropertyValueModificationStrategy(key, value));
            return;
        }
        
        _projectFileAccessor.UpdateDocument(AddProjectGroupNodeIfNotExistsModificationStrategy.PropertyGroup);
        _projectFileAccessor.UpdateDocument(new UpdateProjectPropertyValueModificationStrategy(key, value));
    }
}