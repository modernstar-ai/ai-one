using Agile.Chat.Domain.Assistants.ValueObjects;
using Agile.Framework.Common.Attributes;
using Agile.Framework.Common.EnvironmentVariables;
using Microsoft.Extensions.DependencyInjection;
using static Agile.Framework.Common.EnvironmentVariables.Constants;

namespace Agile.Chat.Application.Assistants.Services;

public interface IAssistantModelConfigService
{
    public AssistantModelOptions GetDefaultModelOptions();

    public bool AllowModelSelectionDefaultValue { get; }

    public bool ModelSelectionFeatureEnabled { get; }
}

[Export(typeof(IAssistantModelConfigService), ServiceLifetime.Singleton)]
public class AssistantModelConfigService : IAssistantModelConfigService
{
    public bool AllowModelSelectionDefaultValue => Configs.AppSettings.ModelSelectionFeatureEnabled &&
        Configs.AppSettings.AllowModelSelectionDefaultValue;

    public bool ModelSelectionFeatureEnabled => Configs.AppSettings.ModelSelectionFeatureEnabled;


    public AssistantModelOptions GetDefaultModelOptions()
    {
        var defaultModelOptions = new AssistantModelOptions
        {
            AllowModelSelection = AllowModelSelectionDefaultValue
        };

        defaultModelOptions.DefaultModelId = Configs.AppSettings.DefaultTextModelId;
        defaultModelOptions.Models.Add(new AssistantTextModelSelectionDto
        {
            ModelId = defaultModelOptions.DefaultModelId,
            IsSelected = true
        });

        if (ModelSelectionFeatureEnabled)
        {
            defaultModelOptions.Models.Add(new AssistantTextModelSelectionDto
            {
                ModelId = TextModels.O3Mini,
                IsSelected = false
            });
        }

        return defaultModelOptions;
    }
}
