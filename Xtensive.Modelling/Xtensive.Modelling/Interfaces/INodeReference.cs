// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Yakunin
// Created:    2009.03.24

namespace Xtensive.Modelling
{
  /// <summary>
  /// Node reference contract.
  /// </summary>
  public interface INodeReference : INode
  {
    /// <summary>
    /// Gets the target node the reference refers to.
    /// </summary>
    Node Value { get; }
  }
}