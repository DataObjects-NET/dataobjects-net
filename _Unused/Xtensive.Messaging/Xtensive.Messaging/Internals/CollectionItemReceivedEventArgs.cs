// Copyright (C) 2007 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Gamzov
// Created:    2007.07.25

using System;

namespace Xtensive.Messaging
{
  internal class CollectionItemReceivedEventArgs : EventArgs
  {
    private readonly IMessageCollectionItem item;
    public IMessageCollectionItem Item
    {
      get { return item; }
    }


    public CollectionItemReceivedEventArgs(IMessageCollectionItem item)
    {
      this.item = item;
    }
  }
}