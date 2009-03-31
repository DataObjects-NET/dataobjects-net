// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Yakunin
// Created:    2009.03.31

using System;
using System.Diagnostics;
using Xtensive.Core;
using Xtensive.Core.Internals.DocTemplates;
using Xtensive.Core.Tuples;
using Xtensive.Indexing.Storage.Resources;

namespace Xtensive.Indexing.Storage
{
  /// <summary>
  /// Option command.
  /// </summary>
  [Serializable]
  public class OptionCommand : Command
  {
    private string name;
    private object value;
    
    /// <summary>
    /// Gets or sets the name of the option.
    /// </summary>
    public string Name {
      get { return name; }
      set {
        ArgumentValidator.EnsureArgumentNotNull(value, "value");
        name = value;
      }
    }

    /// <summary>
    /// Gets or sets the new value.
    /// </summary>
    public object Value {
      get { return value; }
      set { this.value = value; }
    }

    /// <inheritdoc/>
    public override string ToString()
    {
      return string.Format(Strings.OptionCommandFormat, base.ToString(), Name, Value);
    }


    // Constructors

    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true"/>
    /// </summary>
    protected OptionCommand()
      : base(CommandType.Option)
    {
    }
  }
}