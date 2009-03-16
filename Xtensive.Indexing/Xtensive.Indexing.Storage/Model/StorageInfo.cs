// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Yakunin
// Created:    2009.03.16

using System;
using System.Diagnostics;
using Xtensive.Core.Helpers;
using Xtensive.Core.Internals.DocTemplates;

namespace Xtensive.Indexing.Storage.Model
{
  /// <summary>
  /// Describes index storage.
  /// </summary>
  [Serializable]
  public class StorageInfo : Node
  {
    /// <summary>
    /// Gets the indexes.
    /// </summary>
    public IndexInfoCollection Indexes
    { 
      [DebuggerStepThrough]
      get; 
      [DebuggerStepThrough]
      private set;
    }

    
    // Constructors

    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true"/>
    /// </summary>
    /// <param name="name"></param>
    public StorageInfo(string name)
      : base(name)
    {
      Indexes = new IndexInfoCollection(this);
    }
  }
}