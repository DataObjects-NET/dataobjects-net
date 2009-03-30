// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Yakunin
// Created:    2009.03.23

using Xtensive.Core;

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
    /// Applies the action to the specified model.
    /// </summary>
    /// <param name="model">The model to apply the action to.</param>
    void Apply(IModel model);

    /// <summary>
    /// Gets dependencies this action produces.
    /// </summary>
    /// <returns>The array of dependencies this action produces.</returns>
    string[] GetDependencies();


    /// <summary>
    /// Gets required dependencies.
    /// </summary>
    /// <returns>The array of dependencies this action requires to be built first.</returns>
    string[] GetRequiredDependencies();
  }
}