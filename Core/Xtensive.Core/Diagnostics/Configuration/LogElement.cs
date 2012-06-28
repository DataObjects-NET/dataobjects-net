// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Yakunin
// Created:    2009.10.14

using System;
using System.Configuration;
using Xtensive.Core;

namespace Xtensive.Diagnostics.Configuration
{
  /// <summary>
  /// Log configuration element.
  /// </summary>
  public class LogElement : ConfigurationCollectionElementBase
  {
    private const string NameElementName = "name";
    private const string EventsElementName = "events";
    private const string ProviderElementName = "provider";
    private const string FormatElementName = "format";
    private const string FileNameElementName = "fileName";
    private const string FormatStringElementName = "formatString";

    /// <summary>
    /// Gets or sets the name of the log.
    /// </summary>
    [ConfigurationProperty(NameElementName, IsKey = true, IsRequired = false, DefaultValue = "")]
    public string Name
    {
      get { return (string) this[NameElementName]; }
      set { this[NameElementName] = value; }
    }

    /// <inheritdoc/>
    public override object Identifier {
      get { return Name; }
    }

    /// <summary>
    /// Gets or sets log provider name.
    /// </summary>
    [ConfigurationProperty(ProviderElementName, DefaultValue = LogProviderType.Console, IsRequired = false)]
    public LogProviderType Provider
    {
      get { return (LogProviderType) this[ProviderElementName]; }
      set { this[ProviderElementName] = value; }
    }

    /// <summary>
    /// Gets or sets log format kind.
    /// </summary>
    [ConfigurationProperty(FormatElementName, DefaultValue = LogFormat.Default, IsRequired = false)]
    public LogFormat Format
    {
      get { return (LogFormat) this[FormatElementName]; }
      set { this[FormatElementName] = value; }
    }

    /// <summary>
    /// Gets or sets logged event types.
    /// </summary>
    [ConfigurationProperty(EventsElementName, DefaultValue = LogEventTypes.All, IsRequired = false)]
    public LogEventTypes Events
    {
      get { return (LogEventTypes) this[EventsElementName]; }
      set { this[EventsElementName] = value; }
    }

    /// <summary>
    /// Gets or sets the name of the log file, if underlying <see cref="Provider"/> requires this.
    /// </summary>
    [ConfigurationProperty(FileNameElementName, DefaultValue = null, IsRequired = false)]
    public string FileName
    {
      get { return (string) this[FileNameElementName]; }
      set { this[FileNameElementName] = value; }
    }

    /// <summary>
    /// Gets or sets the format string to use.
    /// </summary>
    [ConfigurationProperty(FormatStringElementName, DefaultValue = null, IsRequired = false)]
    public string FormatString
    {
      get { return (string) this[FormatStringElementName]; }
      set { this[FormatStringElementName] = value; }
    }
  }
}