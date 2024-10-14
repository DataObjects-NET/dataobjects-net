// Copyright (C) 2024 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.


using System;
using System.Linq;
using Microsoft.Extensions.Configuration;
using Xtensive.Core;

namespace Xtensive.Orm.Configuration
{
  internal sealed class LoggingConfigurationReader : IConfigurationSectionReader<LoggingConfiguration>
  {
    private const string LoggingSectionName = "Logging";
    private const string LogsSectionName = "Logs";

    private const string ProviderElementName = "Provider";
    private const string LogElementName = "Log";
    private const string LogSourceElementName = "Source";
    private const string LogTargerElementName = "Target";

    /// <inheritdoc/>
    public LoggingConfiguration Read(IConfigurationRoot configurationRoot, string sectionName)
    {
      ArgumentValidator.EnsureArgumentNotNull(configurationRoot, nameof(configurationRoot));
      ArgumentValidator.EnsureArgumentNotNullOrEmpty(sectionName, nameof(sectionName));

      return Read(configurationRoot.GetSection(sectionName));
    }

    /// <inheritdoc/>
    public LoggingConfiguration Read(IConfigurationRoot configurationRoot)
    {
      ArgumentValidator.EnsureArgumentNotNull(configurationRoot, nameof(configurationRoot));

      return Read(configurationRoot.GetSection(WellKnown.DefaultConfigurationSection));
    }

    /// <inheritdoc/>
    public LoggingConfiguration Read(IConfigurationRoot configurationRoot, string sectionName, string nameOfConfiguration)
    {
      ArgumentValidator.EnsureArgumentNotNull(configurationRoot, nameof(configurationRoot));
      ArgumentValidator.EnsureArgumentNotNullOrEmpty(sectionName, nameof(sectionName));

      return Read(configurationRoot.GetSection(sectionName));
    }

    /// <inheritdoc/>
    public LoggingConfiguration Read(IConfigurationSection configurationSection)
    {
      ArgumentValidator.EnsureArgumentNotNull(configurationSection, nameof(configurationSection));

      var ormConfigurationSection = configurationSection;

      var loggingSection = ormConfigurationSection.GetSection(LoggingSectionName);

      if (loggingSection != null && loggingSection.GetChildren().Any()) {
        var provider = loggingSection.GetSection(ProviderElementName)?.Value;
        var logsSection = loggingSection.GetSection(LogsSectionName);
        IConfigurationSection logElement;

        if (logsSection != null && logsSection.GetChildren().Any()) {
          logElement = logsSection.GetSection(LogElementName);
          if (logElement == null || !logElement.GetChildren().Any()) {
            logElement = logsSection;
          }
        }
        else {
          logElement = loggingSection.GetSection(LogElementName);
        }

        var configuration = new LoggingConfiguration(provider);
        foreach (var logItem in logElement.GetSelfOrChildren()) {
          var source = logItem.GetSection(LogSourceElementName).Value;
          var target = logItem.GetSection(LogTargerElementName).Value;
          if (source.IsNullOrEmpty() || target.IsNullOrEmpty())
            throw new InvalidOperationException();
          configuration.Logs.Add(new LogConfiguration(source, target));
        }

        if (configuration.Provider ==  null && configuration.Logs.Count==0)
          throw new InvalidOperationException();

        return configuration;
      }

      throw new InvalidOperationException(
        string.Format(Strings.ExSectionIsNotFoundInApplicationConfigurationFile, WellKnown.DefaultConfigurationSection));
    }

    /// <inheritdoc/>
    public LoggingConfiguration Read(IConfigurationSection configurationSection, string nameOfConfiguration)
    {
      throw new NotSupportedException();
    }
  }
}
