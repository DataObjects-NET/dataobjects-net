// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Kofman
// Created:    2008.08.07

using System.Collections.Generic;
using System.Linq;
using Xtensive.Core.Collections;
using Xtensive.Core.Internals.DocTemplates;

namespace Xtensive.Storage.Rse
{
  /// <summary>
  /// Read only collection of <see cref="RecordColumnGroupMapping"/>.
  /// </summary>
  public class RecordColumnGroupMappingCollection : ReadOnlyCollection<RecordColumnGroupMapping>
  {
    private static RecordColumnGroupMappingCollection emptyCollection;

    /// <summary>
    /// Gets the empty <see cref="RecordColumnGroupMappingCollection"/>.
    /// </summary>    
    public static RecordColumnGroupMappingCollection Empty
    {
      get
      {
        if (emptyCollection==null)
          emptyCollection = new RecordColumnGroupMappingCollection(Enumerable.Empty<RecordColumnGroupMapping>());

        return emptyCollection;
      }
    }

    /// <summary>
    ///   <see cref="ClassDocTemplate.Ctor" copy="true"/>
    /// </summary>
    /// <param name="items">The collection items.</param>
    public RecordColumnGroupMappingCollection(IEnumerable<RecordColumnGroupMapping> items)
      : base(items.ToList())
    {      
    }
  }
}