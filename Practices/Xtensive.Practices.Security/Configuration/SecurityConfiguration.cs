// Copyright (C) 2011 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitri Maximov
// Created:    2011.06.10

using System;
using System.Configuration;
using Xtensive.Configuration;

namespace Xtensive.Practices.Security.Configuration
{
  public class SecurityConfiguration
  {
    /// <summary>
    /// Default <see cref="SectionName"/> value:
    /// "<see langword="Xtensive.Security" />".
    /// </summary>
    public const string DefaultSectionName = "Xtensive.Security";

    public string HashingServiceName { get; private set; }

    public string ValidationServiceName { get; private set; }

    public static SecurityConfiguration Load()
    {
      return Load(DefaultSectionName);
    }

    public static SecurityConfiguration Load(string sectionName)
    {
      var section = (ConfigurationSection) ConfigurationManager.GetSection(sectionName);
      var result = new SecurityConfiguration();

      string hashingService = section == null ? string.Empty : section.HashingService.Name;
      if (string.IsNullOrEmpty(hashingService))
        hashingService = "plain";
      result.HashingServiceName = hashingService.ToLowerInvariant();

      string validationService = section == null ? string.Empty : section.ValidationService.Name;
      if (string.IsNullOrEmpty(validationService))
        validationService = "default";
      result.ValidationServiceName = validationService.ToLowerInvariant();

      return result;
    }
  }
}