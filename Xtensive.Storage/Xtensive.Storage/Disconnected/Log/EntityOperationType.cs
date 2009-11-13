// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexis Kochetov
// Created:    2009.10.21

using System;

namespace Xtensive.Storage.Disconnected.Log
{
  [Serializable]
  public enum EntityOperationType
  {
    Create,
    Update,
    Remove,
    AddItem,
    RemoveItem
  }
}