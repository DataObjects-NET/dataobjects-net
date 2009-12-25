// Copyright (C) 2007 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Gamzov
// Created:    2007.07.24

namespace Xtensive.Messaging
{
  /// <summary>
  /// Describes item of <see cref="IMessageCollection"/>.
  /// </summary>
  public interface IMessageCollectionItem : IMessage
  {
    /// <summary>
    /// Gets sequence of item in collection.
    /// </summary>
    int Sequence{ get;}

    /// <summary>
    /// Gets data of item.
    /// </summary>
    object Data{ get;}
  }
}