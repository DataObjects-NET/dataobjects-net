// Copyright (C) 2007 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Gamzov
// Created:    2007.07.25

using System;

namespace Xtensive.Messaging
{
  [Serializable]
  internal class MessageCollectionHeadItem : MessageCollectionItem, IMessageCollectionHeadItem
  {
    private readonly int count;

    public MessageCollectionHeadItem(object data, int count)
      : base(data, 0)
    {
      this.count = count;
    }

    /// <summary>
    /// Gets actual count of items in <see cref="IMessageCollection"/>.
    /// </summary>
    public int Count
    {
      get { return count; }
    }

  }
}