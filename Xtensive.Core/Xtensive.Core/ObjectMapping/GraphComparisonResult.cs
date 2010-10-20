// Copyright (C) 2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexander Nikolaev
// Created:    2010.01.19

using System;
using System.Collections.Generic;
using Xtensive.Collections;
using Xtensive.Core;
using Xtensive.Internals.DocTemplates;

namespace Xtensive.ObjectMapping
{
  /// <summary>
  /// Result of comparison the original graph of target objects with the modified one.
  /// </summary>
  [Serializable]
  public class GraphComparisonResult
  {
    /// <summary>
    /// The set of operations describing found changes.
    /// </summary>
    public readonly IOperationSequence Operations;

    /// <summary>
    /// The mapping from surrogate keys to real keys for new objects.
    /// </summary>
    public readonly ReadOnlyDictionary<object, object> KeyMapping;


    // Constructors

    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true"/>
    /// </summary>
    /// <param name="operations">The set of operations describing found changes.</param>
    /// <param name="keyMapping">The mapping from surrogate keys to real keys for new objects.</param>
    public GraphComparisonResult(IOperationSequence operations, ReadOnlyDictionary<object, object> keyMapping)
    {
      ArgumentValidator.EnsureArgumentNotNull(operations, "operations");

      Operations = operations;
      KeyMapping = keyMapping
        ?? new ReadOnlyDictionary<object, object>(new Dictionary<object, object>(), false);
    }
  }
}