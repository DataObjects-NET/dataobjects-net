// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitri Maximov
// Created:    2008.05.08

using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Xtensive.Comparison;
using Xtensive.Internals.DocTemplates;
using Xtensive.Indexing.Resources;

namespace Xtensive.Indexing.Serialization.Implementation
{
  /// <summary>
  /// Manages <see cref="IReference"/> objects during 
  /// serialization and deserialization.
  /// </summary>
  public class ReferenceManager
  {
    private readonly Dictionary<IReference, object> refToObj = 
      new Dictionary<IReference, object>();
    private readonly Dictionary<object, IReference> objToRef = 
      new Dictionary<object, IReference>(ReferenceEqualityComparer<object>.Instance);
    private int nextReferenceValue;

    /// <summary>
    /// Sets the object corresponding to the reference.
    /// </summary>
    /// <param name="reference">Reference to the object.</param>
    /// <param name="target">Object.</param>
    /// <exception cref="InvalidOperationException">Reference points to <see langword="null" />, 
    /// or is already defined.</exception>
    public void Define(IReference reference, object target) 
    {
      reference.EnsureNotNull();
      
      object currentTarget;
      // Allows self-resolving references not wasting the space in refToObj and objToRef
      if (reference.TryResolve(out currentTarget) && 
          !ReferenceEquals(target, currentTarget))
        throw new InvalidOperationException(string.Format(
          Strings.ExReferenceIsAlreadyDefined,
          reference));
      refToObj[reference] = target;

      if (reference.IsCacheable) {
        IReference cachedReference;
        if (objToRef.TryGetValue(target, out cachedReference) && 
            !ReferenceEquals(reference, cachedReference))
          throw new InvalidOperationException(string.Format(
            Strings.ExReferenceIsAlreadyDefined,
            reference));
        objToRef[target] = reference;
      }
    }

    /// <summary>
    /// Tries to resolve the reference.
    /// </summary>
    /// <param name="reference">The reference to resolve.</param>
    /// <param name="target">The object reference is pointing to.</param>
    /// <returns>
    /// <see langword="True"/> if reference was resolved successfully;
    /// otherwise, <see langword="false"/>.
    /// </returns>
    public bool TryResolve(IReference reference, out object target) 
    {
      reference.EnsureNotNull();
      return refToObj.TryGetValue(reference, out target);
    }

    /// <summary>
    /// Gets an existing reference to the specified object,
    /// or creates a new one.
    /// </summary>
    /// <param name="target">Object to get the reference to.</param>
    /// <returns>An existing or a new reference to the specified object.</returns>
    public IReference GetReference(object target) 
    {
      bool isNew;
      return GetReference(target, out isNew);
    }

    /// <summary>
    /// Gets an existing reference to the specified object,
    /// or creates a new one.
    /// </summary>
    /// <param name="target">Object to get the reference to.</param>
    /// <param name="isNew">Indicates whether returned reference is a new one.</param>
    /// <returns>An existing or a new reference to the specified object.</returns>
    public IReference GetReference(object target, out bool isNew) 
    {
      isNew = false;
      IReference reference;
      
      if (target==null)
        return Reference.Null;
      if (objToRef.TryGetValue(target, out reference))
        return reference;
      var context = SerializationContext.Current;
      if (context.ProcessType==SerializerProcessType.Serialization) {
        // Any object queued for serialization must be resolved to the same reference
        var queue = context.SerializationQueue;
        if (queue.Contains(target))
          return queue[target].First;
      }

      isNew = true;
      reference = CreateReference(target);
      return reference;
    }

    /// <summary>
    /// Creates the reference to the specified <paramref name="target"/>.
    /// </summary>
    /// <param name="target">The reference target.</param>
    /// <returns>Newly created reference to the <paramref name="target"/>.</returns>
    public virtual IReference CreateReference(object target)
    {
      return new Reference(target);
    }

    /// <summary>
    /// Gets the next reference <see cref="IReference.Value"/> property value.
    /// </summary>
    /// <returns>Next reference <see cref="IReference.Value"/> property value.</returns>
    public virtual string GetNextReferenceValue()
    {
      return (nextReferenceValue++).ToString();
    }


    // Constructors

    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true"/>
    /// </summary>
    public ReferenceManager() 
    {
    }
  }
}