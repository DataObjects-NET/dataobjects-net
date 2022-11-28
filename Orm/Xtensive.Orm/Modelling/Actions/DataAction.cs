// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Ivan Galkin
// Created:    2009.05.14

using System;
using Xtensive.Core;
using Xtensive.Modelling.Comparison.Hints;

namespace Xtensive.Modelling.Actions
{
  /// <summary>
  /// Change data action.
  /// </summary>
  [Serializable]
  public class DataAction : NodeAction
  {
    private DataHint dataHint;

    /// <summary>
    /// Gets or sets the data hint described data operation.
    /// </summary>
    public DataHint DataHint
    {
      get { return dataHint; }
      set
      {
        EnsureNotLocked();
        dataHint = value;
        Path = dataHint.SourceTablePath;
      }
    }

    /// <inheritdoc/>
    protected override void PerformExecute(IModel model, IPathNode item)
    {
    }

    /// <inheritdoc/>
    public override string ToString()
    {
      return dataHint.ToString();
    }
  }
}