// Copyright (C) 2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexis Kochetov
// Created:    2010.09.30

using System;
using System.Configuration;
using System.Diagnostics;
using System.Web;
using System.Web.Configuration;
using Xtensive.Orm;
using Xtensive.Orm.Configuration;
using ConfigurationSection = Xtensive.Orm.Configuration.Elements.ConfigurationSection;

namespace Xtensive.Orm.Web
{
  /// <summary>
  /// <see cref="DomainConfiguration"/> for web projects.
  /// </summary>
  [Serializable]
  public class WebDomainConfiguration : DomainConfiguration
  {
    /// <summary>
    /// Loads the <see cref="DomainConfiguration"/> for <see cref="Domain"/>
    /// with the specified <paramref name="name"/>
    /// from application configuration file (section with <paramref name="sectionName"/>).
    /// </summary>
    /// <param name="sectionName">Name of the section.</param>
    /// <param name="name">Name of the <see cref="Domain"/>.</param>
    /// <returns>
    /// The <see cref="DomainConfiguration"/> for the specified domain.
    /// </returns>
    /// <exception cref="InvalidOperationException">Section <paramref name="sectionName"/>
    /// is not found in application configuration file, or there is no configuration for
    /// the <see cref="Domain"/> with specified <paramref name="name"/>.</exception>
    public static new DomainConfiguration Load(string sectionName, string name)
    {
      ConfigurationSection section;
      if (HttpContext.Current != null) {
        // (workaround for IIS 7 @ 64 bit Windows Server 2008)
        var config = WebConfigurationManager.OpenWebConfiguration("~");
        section = (ConfigurationSection)config.GetSection(sectionName);
      }
      else
        section = (ConfigurationSection)ConfigurationManager.GetSection(sectionName);
      if (section == null)
        throw new InvalidOperationException(string.Format(
          "Section {0} is not found in application configuration file", sectionName));
      var domainElement = section.Domains[name];
      if (domainElement == null)
        throw new InvalidOperationException(string.Format(
          "Configuration for domain {0} is not found in application configuration file", name));
      return domainElement.ToNative();
    }
  }
}