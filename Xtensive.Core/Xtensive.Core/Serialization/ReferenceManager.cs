// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitri Maximov
// Created:    2008.05.08

using System;
using System.Collections.Generic;

namespace Xtensive.Core.Serialization
{
  public class ReferenceManager
  {
    private Dictionary<IReference, object> referenceTable = new Dictionary<IReference, object>(64);
    private long regularCounter;
    private long surrogateCounter;

    private long GenerateId()
    {
      return ++regularCounter;
    }

    private long GenerateSurrogateId()
    {
      return --surrogateCounter;
    }

    public void Update(IReference reference, object newValue)
    {
      if (reference.IsEmpty)
        return;
      object currentValue = Resolve(reference);
      if (currentValue != null && !ReferenceEquals(currentValue, newValue))
        throw new InvalidOperationException("Duplicate values for the same reference.");
      referenceTable[reference] = newValue;
    }

    public object Resolve(IReference objRef)
    {
      if (objRef.IsEmpty)
        return null;
      object obj;
      referenceTable.TryGetValue(objRef, out obj);
      return obj;
    }

    public IReference CreateReference(long id)
    {
      Reference r = new Reference(id);
      if (!r.IsEmpty && !referenceTable.ContainsKey(r))
        referenceTable[r] = null;
      return r;
    }

    public IReference CreateSurrogate()
    {
      return new Reference(GenerateSurrogateId());
    }
  }
}