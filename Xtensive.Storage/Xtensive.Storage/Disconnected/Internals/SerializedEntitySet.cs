// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Ivan Galkin
// Created:    2009.11.16

using System;
using Xtensive.Storage.Model;

namespace Xtensive.Storage.Disconnected
{
  [Serializable]
  internal sealed class SerializedEntitySet
  {
    public FieldInfoRef Field { get; set; }

    public bool IsFullyLoaded { get; set; }

    public string[] Items { get; set; }

    public string[] AddedItems { get; set; }

    public string[] RemovedItems { get; set; }
  }
}