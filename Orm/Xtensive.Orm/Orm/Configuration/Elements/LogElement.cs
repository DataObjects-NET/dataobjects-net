// Copyright (C) 2013 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Kulakov
// Created:    2013.08.17

using System.Configuration;
using Xtensive.Core;

namespace Xtensive.Orm.Configuration.Elements
{
  /// <summary>
  /// Log configuration element within a configuration file.
  /// </summary>
  public class LogElement : ConfigurationCollectionElementBase
  {
    private const string SourceElementName = "source";
    private const string TargetElementName = "target";

    /// <inheritdoc/>
    public override object Identifier
    {
      get { return Source; }
    }

    /// <summary>
    /// Gets or sets source or sources of log separated by comma.
    /// </summary>
    [ConfigurationProperty(SourceElementName, IsRequired = true)]
    public string Source
    {
      get { return (string)this[SourceElementName]; }
      set { this[SourceElementName] = value; }
    }

    /// <summary>
    /// Gets or sets target of log (Console, DebufgOnlyConsole or file path).
    /// </summary>
    [ConfigurationProperty(TargetElementName, IsRequired = true)]
    public string Target
    {
      get { return (string)this[TargetElementName]; }
      set { this[TargetElementName] = value; }
    }

    /// <summary>
    /// Converts this instance to corresponding <see cref="LogConfiguration"/>.
    /// </summary>
    /// <returns>Result of conversion.</returns>
    public LogConfiguration ToNative()
    {
      return new LogConfiguration(Source, Target);
    }
  }
}
