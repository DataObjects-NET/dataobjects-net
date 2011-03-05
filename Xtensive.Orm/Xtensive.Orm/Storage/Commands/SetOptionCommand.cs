// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Yakunin
// Created:    2009.03.31

using System;
using Xtensive.Core;
using Xtensive.Internals.DocTemplates;
using Xtensive.Storage.Commands.Resources;

namespace Xtensive.Storage.Commands
{
  /// <summary>
  /// Set option command.
  /// </summary>
  [Serializable]
  public class SetOptionCommand<T> : Command<T>
  {
    private string name;

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
    public T Value { get; set; }

    /// <inheritdoc/>
    public override string ToString()
    {
      return string.Format(Strings.OptionCommandFormat, base.ToString(), Name, Value);
    }


    // Constructors

    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true"/>
    /// </summary>
    protected SetOptionCommand()
      : base(CommandType.SetOption)
    {
    }

    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true"/>
    /// </summary>
    /// <param name="name">The name.</param>
    /// <param name="value">The value.</param>
    public SetOptionCommand(string name, T value)
      : base(CommandType.SetOption)
    {
      this.name = name;
      Value = value;
    }
  }
}