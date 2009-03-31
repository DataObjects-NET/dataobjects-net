// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Yakunin
// Created:    2009.03.31

using System;
using Xtensive.Core;
using Xtensive.Core.Internals.DocTemplates;
using Xtensive.Indexing.Storage.Resources;

namespace Xtensive.Indexing.Storage
{
  /// <summary>
  /// Query command.
  /// </summary>
  [Serializable]
  public class QueryCommand : Command
  {
    private object definition;

    /// <summary>
    /// Gets or sets the definition of the query.
    /// </summary>
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
  }
}