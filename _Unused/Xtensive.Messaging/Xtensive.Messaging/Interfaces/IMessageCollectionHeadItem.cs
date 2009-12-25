// Copyright (C) 2007 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Gamzov
// Created:    2007.07.24

namespace Xtensive.Messaging
{
  /// <summary>
  /// Defines head message in <see cref="IMessageCollection"/>.
  /// </summary>
  public interface IMessageCollectionHeadItem: IMessageCollectionItem
  {
    /// <summary>
    /// Gets actual count of items in <see cref="IMessageCollection"/>.
    /// </summary>
    int Count { get; }
  }
}