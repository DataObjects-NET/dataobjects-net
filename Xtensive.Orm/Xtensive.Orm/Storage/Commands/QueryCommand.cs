// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Yakunin
// Created:    2009.03.31

using System;
using System.Collections.Generic;
using Xtensive.Core;
using Xtensive.Internals.DocTemplates;
using Xtensive.Tuples;
using Tuple = Xtensive.Tuples.Tuple;
using Xtensive.Storage.Commands.Resources;

namespace Xtensive.Storage.Commands
{
  /// <summary>
  /// Query command.
  /// </summary>
  [Serializable]
  public class QueryCommand<T> : Command<T>,
    IQueryCommand
  {
    private object definition;

    /// <inheritdoc/>
    public object Definition {
      get { return definition; }
      set {
        ArgumentValidator.EnsureArgumentNotNull(value, "value");
        definition = value;
      }
    }

    /// <inheritdoc/>
    public override string ToString()
    {
      return string.Format(Strings.QueryCommandFormat, base.ToString(), Definition);
    }

    
    // Constructors

    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true"/>
    /// </summary>
    public QueryCommand()
      : base(CommandType.Query)
    {
    }

    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true"/>
    /// </summary>
    /// <param name="definition">Query definition.</param>
    public QueryCommand(object definition)
      : base(CommandType.Query)
    {
      this.definition = definition;
    }
  }
}