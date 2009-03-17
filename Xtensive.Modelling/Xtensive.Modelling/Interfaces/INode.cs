// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Yakunin
// Created:    2009.03.17

using Xtensive.Core.Notifications;

namespace Xtensive.Modelling
{
  /// <summary>
  /// Node interface.
  /// </summary>
  public interface INode : IPathNode,
    IChangeNotifier
  {
    /// <summary>
    /// Gets the parent node collection this node belongs to.
    /// </summary>
    /// <returns>Parent node collection;
    /// <see langword="null" />, if none.</returns>
    INodeCollection Collection { get; }
  }
}