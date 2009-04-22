// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Denis Krjuchkov
// Created:    2009.04.22

using System.Collections.Generic;
using Xtensive.Core.Collections;

namespace Xtensive.Sql.Dom.Compiler.Internals
{
  internal class VariantKeys
  {
    private readonly HashSet<object> keys;

    public bool IsActive(object key)
    {
      return keys!=null && keys.Contains(key);
    }
    
    public VariantKeys()
    {
      keys = null;
    }

    public VariantKeys(IEnumerable<object> keys)
    {
      this.keys = keys.ToHashSet();
    }
  }
}