// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Ivan Galkin
// Created:    2009.11.16

using System;
using Xtensive.Core.Tuples;

namespace Xtensive.Storage.Disconnected
{
  [Serializable]
  internal sealed class SerializedEntityState
  {
    public string Key { get; set; }
    
    public SerializedTuple Tuple { get; set; }
    
    public bool IsRemoved { get; set; }
    
    public SerializedReference[] References { get; set; }
    
    public SerializedEntitySet[] EntitySets { get; set; }
  }
}