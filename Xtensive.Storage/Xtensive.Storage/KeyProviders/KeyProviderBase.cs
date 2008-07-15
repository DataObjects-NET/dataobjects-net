// Copyright (C) 2007 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Gamzov
// Created:    2007.12.20

using Xtensive.Core;
using Xtensive.Core.Tuples;

namespace Xtensive.Storage
{
  public abstract class KeyProviderBase : IKeyProvider
  {
    /// <summary>
    /// Fills the key tuple with the unique values.
    /// </summary>
    /// <param name="keyTuple">The target key tuple to fill with values.</param>
    public abstract void GetNext(Tuple keyTuple);

    /// <summary>
    /// Fills the key tuple with specified set of key data.
    /// </summary>
    /// <param name="keyTuple">The target key tuple to fill with key data.</param>
    /// <param name="keyData">Key field values.</param>
    public virtual void Build(Tuple keyTuple, params object[] keyData)
    {
      ArgumentValidator.EnsureArgumentNotNull(keyTuple, "keyTuple");
      ArgumentValidator.EnsureArgumentNotNull(keyData, "keyData");
      ArgumentValidator.EnsureArgumentIsInRange(keyData.Length, 1, keyTuple.Count, "keyData.Length");
      for (int i = 0; i < keyData.Length; i++)
        keyTuple.SetValue(i, keyData[i]);
    }
  }
}