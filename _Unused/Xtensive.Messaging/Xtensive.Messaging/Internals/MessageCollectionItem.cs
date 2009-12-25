// Copyright (C) 2007 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Gamzov
// Created:    2007.07.25

using System;

namespace Xtensive.Messaging
{
  [Serializable]
  internal class MessageCollectionItem : IMessageCollectionItem
  {
    private readonly int sequence;
    private readonly object data;

    public int Sequence
    {
      get { return sequence; }
    }

    public object Data
    {
      get { return data; }
    }

    /// <summary>
    /// Gets or sets unique query identifier.
    /// </summary>
    public long QueryId { get; set; }

    public MessageCollectionItem(object data, int sequence)
    {
      this.sequence = sequence;
      this.data = data;
    }

  }
}