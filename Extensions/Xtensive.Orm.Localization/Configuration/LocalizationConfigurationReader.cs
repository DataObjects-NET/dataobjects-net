// Copyright (C) 2011-2024 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitri Maximov
// Created:    2012.07.06

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

    private const string DefaultCultureElementName = "DefaultCulture";
    private const string CultureNameAttributeName = "name";

    private LocalizationConfiguration ReadInternal(IConfigurationSection configurationSection)
    {
      var defaultCultureSection = configurationSection.GetSection(DefaultCultureElementName);
      if (defaultCultureSection == null)
        return new LocalizationConfiguration() { DefaultCulture = Thread.CurrentThread.CurrentCulture };

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

      if (cultureName.IsNullOrEmpty())
        return new LocalizationConfiguration() { DefaultCulture = Thread.CurrentThread.CurrentCulture };
      try {
        return new LocalizationConfiguration() { DefaultCulture = new CultureInfo(cultureName) };
      }
      catch (CultureNotFoundException) {
        return new LocalizationConfiguration() { DefaultCulture = Thread.CurrentThread.CurrentCulture };
      }
    }
  }
}