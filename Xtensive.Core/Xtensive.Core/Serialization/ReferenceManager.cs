// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitri Maximov
// Created:    2008.05.08

using System;
using System.Collections.Generic;
using Xtensive.Core.Internals.DocTemplates;
using Xtensive.Core.Resources;

namespace Xtensive.Core.Serialization
{
  /// <summary>
  /// Manages <see cref="IReference"/> objects during 
  /// serialization and deserialization.
  /// </summary>
  public class ReferenceManager
  {
    private readonly SerializationContext context;
    internal readonly HashSet<Type> resolvedTypes = new HashSet<Type>();
    private readonly Dictionary<IReference, object> refToObj;
    private readonly Dictionary<object, IReference> objToRef;
    private int nextReferenceValue;

    /// <summary>
    /// Gets the next reference <see cref="IReference.Value"/> property value.
    /// </summary>
    /// <returns>Next reference <see cref="IReference.Value"/> property value.</returns>
    public string GetNextReferenceValue()
    {
      return (nextReferenceValue++).ToString();
    }

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
      if (TryResolve(reference, out currentTarget) && !ReferenceEquals(target, currentTarget))
        throw new InvalidOperationException(string.Format(
          Strings.ExReferenceIsAlreadyDefined,
          reference));
      refToObj[reference] = target;
      if (reference.IsUnique)
        // TODO: Add a check for uniqueness here
        objToRef[target] = reference;
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
      IReference reference;
      if (target == null) {
        isNew = false;
        return Reference.Null;
      }
      if (objToRef.ContainsKey(target)) {
        isNew = false;
        return objToRef[target];
      }
      else {
        var type = target.GetType();
        if (!resolvedTypes.Contains(type)) {
          var formatter = context.Formatter;
          formatter.RegisterDescriptor(formatter.CreateDescriptor(type));
          resolvedTypes.Add(type);
        }
        reference = CreateReference(target);
        isNew = true;
      }

      if (isNew) {
      }
      return reference;
    }

    public IReference CreateReference(object obj)
    {
      return new Reference(obj);
    }

    /// <exception cref="InvalidOperationException">Current formatter process type 
    /// differs from the <paramref name="expectedProcessType"/>.</exception>
    private void EnsureFormatterProcessTypeIs(FormatterProcessType expectedProcessType) 
    {
      if (context.ProcessType != expectedProcessType)
        throw new InvalidOperationException(string.Format(
          Strings.ExInvalidFormatterProcessType,
          context.ProcessType));
    }

    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true"/>
    /// </summary>
    /// <param name="context"><see cref="SerializationContext"/> to operate in.</param>
    public ReferenceManager(SerializationContext context) 
    {
      this.context = context;
      if (context.ProcessType == FormatterProcessType.Serialization) {
        refToObj = null;
        objToRef = new Dictionary<object, IReference>(64);
      }
      else {
        refToObj = new Dictionary<IReference, object>(64);
        objToRef = null;
      }
    }
  }
}