// Copyright (C) 2024 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.

using System;
using System.Globalization;
using System.Linq;
using System.Threading;
using Microsoft.Extensions.Configuration;
using Xtensive.Core;
using Xtensive.Orm.Configuration;

namespace Xtensive.Orm.Localization.Configuration
{
  internal sealed class LocalizationConfigurationReader : IConfigurationSectionReader<LocalizationConfiguration>
  {
    private const string DefaultCultureElementName = "DefaultCulture";
    private const string CultureNameAttributeName = "name";

    public LocalizationConfiguration Read(IConfigurationSection configurationSection) => ReadInternal(configurationSection);

    public LocalizationConfiguration Read(IConfigurationSection configurationSection, string nameOfConfiguration) =>
      throw new NotSupportedException();

    public LocalizationConfiguration Read(IConfigurationRoot configurationRoot) =>
      Read(configurationRoot, LocalizationConfiguration.DefaultSectionName);

    public LocalizationConfiguration Read(IConfigurationRoot configurationRoot, string sectionName)
    {
      var section = configurationRoot.GetSection(sectionName);
      return ReadInternal(section);
    }

    public LocalizationConfiguration Read(IConfigurationRoot configurationRoot, string sectionName, string nameOfConfiguration) =>
      throw new NotSupportedException();

    private LocalizationConfiguration ReadInternal(IConfigurationSection configurationSection)
    {
      var defaultCultureSection = configurationSection.GetSection(DefaultCultureElementName);
      var defaultCulture = Thread.CurrentThread.CurrentCulture;
      if (defaultCultureSection == null) {
        return new LocalizationConfiguration() { DefaultCulture = defaultCulture };
      }

      var cultureName = defaultCultureSection.Value;
      if (cultureName == null) {
        cultureName = defaultCultureSection.GetSection(CultureNameAttributeName)?.Value;
        if (cultureName == null) {
          var children = defaultCultureSection.GetChildren().ToList();
          if (children.Count > 0) {
            cultureName = children[0].GetSection(CultureNameAttributeName).Value;
          }
        }
      }
      if(!cultureName.IsNullOrEmpty()) {
        try {
          defaultCulture = CultureInfo.GetCultureInfo(cultureName);
        }
        catch(CultureNotFoundException) {
          // swallow it, this is mark wrong culture name;
        }
      }

      return new LocalizationConfiguration() { DefaultCulture = defaultCulture };
    }
  }
}