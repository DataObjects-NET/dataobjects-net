// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Yakunin
// Created:    2009.03.23

using Xtensive.Core;
using Xtensive.Modelling.Comparison;

namespace Xtensive.Modelling.Actions
{
  /// <summary>
  /// Node action contract.
  /// </summary>
  public interface INodeAction : ILockable
  {
    /// <summary>
    /// Gets or sets the path of the node this action is applied to.
    /// </summary>
    string Path { get; set; }

    /// <summary>
    /// Gets or sets the difference this action is created for.
    /// </summary>
    Difference Difference { get; set; }

    /// <summary>
    /// Applies the action to the specified model.
    /// </summary>
    /// <param name="model">The model to apply the action to.</param>
    void Execute(IModel model);
  }
}