// Copyright (C) 2024 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.

using System;
using Microsoft.Extensions.Configuration;
using Xtensive.Orm.Configuration;

namespace Xtensive.Orm.Reprocessing.Configuration
{
  internal sealed class ReprocessingConfigurationReader : IConfigurationSectionReader<ReprocessingConfiguration>
  {
    // intermediate class for reading section
    private class ReprocessingOptions
    {
      public TransactionOpenMode? DefaultTransactionOpenMode { get; set; }

      public string DefaultExecuteStrategy { get; set; }
    }

    public ReprocessingConfiguration Read(IConfigurationSection configurationSection) => ReadInternal(configurationSection);

    public ReprocessingConfiguration Read(IConfigurationSection configurationSection, string nameOfConfiguration) =>
      throw new NotSupportedException();

    public ReprocessingConfiguration Read(IConfigurationRoot configurationRoot) =>
      Read(configurationRoot, ReprocessingConfiguration.DefaultSectionName);

    public ReprocessingConfiguration Read(IConfigurationRoot configurationRoot, string sectionName)
    {
      var section = configurationRoot.GetSection(sectionName);
      return ReadInternal(section);
    }

    public ReprocessingConfiguration Read(IConfigurationRoot configurationRoot, string sectionName, string nameOfConfiguration) =>
      throw new NotSupportedException();

    private ReprocessingConfiguration ReadInternal(IConfigurationSection section)
    {
      var reprocessingOptions = section.Get<ReprocessingOptions>();

      if (reprocessingOptions == default) {
        return new ReprocessingConfiguration();
      }

      if (reprocessingOptions.DefaultTransactionOpenMode == default
        && reprocessingOptions.DefaultExecuteStrategy == default) {
        // that means instance is default. probably invalid
        return new ReprocessingConfiguration();
      }

      var result = new ReprocessingConfiguration();
      if (reprocessingOptions.DefaultTransactionOpenMode != default) {
        result.DefaultTransactionOpenMode = reprocessingOptions.DefaultTransactionOpenMode.Value;
      }
      if (!string.IsNullOrEmpty(reprocessingOptions.DefaultExecuteStrategy)) {
        var type = Type.GetType(reprocessingOptions.DefaultExecuteStrategy, false);
        if (type == null)
          throw new InvalidOperationException($"Can't resolve type '{reprocessingOptions.DefaultExecuteStrategy}'. Note that DefaultExecuteStrategy value should be in form of Assembly Qualified Name");
        result.DefaultExecuteStrategy = type;
      }
      return result;
    }
  }
}