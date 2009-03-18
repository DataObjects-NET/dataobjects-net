// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Yakunin
// Created:    2009.03.17

using System;
using System.Diagnostics;

namespace Xtensive.Modelling
{
  /// <summary>
  /// Abstract base class for any model.
  /// </summary>
  [Serializable]
  public abstract class Model : Node
  {
    /// <inheritdoc/>
    protected sealed override INodeNesting CreateNesting()
    {
      return new NodeNesting<Model, Model, Model>(this);
    }


    // Constructors

    /// <inheritdoc/>
    protected Model(string name)
      : base(name)
    {
    }
  }
}