// Copyright (C) 2007 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Elena Vakhtina
// Created:    2008.07.14

using System;
using System.Collections.Generic;
using Xtensive.Core;
using Xtensive.Indexing.Implementation;
using Xtensive.Indexing.Measures;
using Xtensive.Indexing.Resources;

namespace Xtensive.Indexing
{
  partial class Index<TKey, TItem>
  {
        private IIndexPageProvider<TKey, TItem> provider;

    /// <summary>
    /// Serializes items from specified <see cref="IEnumerable{T}"/> into the index.
    /// </summary>
    /// <param name="source">Items to serialize into the index.</param>
    public void Serialize(IEnumerable<TItem> source)
    {
      provider.Serialize(source);
    }
  }
}