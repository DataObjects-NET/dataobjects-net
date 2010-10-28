// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Ivan Galkin
// Created:    2009.11.16

using System;
using Xtensive.Tuples;
using Tuple = Xtensive.Tuples.Tuple;

namespace Xtensive.Orm.Disconnected
{
  [Serializable]
  internal sealed class SerializableEntityState
  {
    public string Key { get; set; }

    public string Type { get; set; }
    
    public Tuple Tuple { get; set; }
    
    public bool IsRemoved { get; set; }
    
    public SerializableReference[] References { get; set; }
    
    public SerializableEntitySet[] EntitySets { get; set; }
  }
}