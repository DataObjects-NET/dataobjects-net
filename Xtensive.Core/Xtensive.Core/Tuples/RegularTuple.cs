// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Yakunin
// Created:    2008.01.24

using System;
using System.Runtime.Serialization;
using Xtensive.Core.Internals.DocTemplates;

namespace Xtensive.Core.Tuples
{
  /// <summary>
  /// Base class for any regular tuple.
  /// </summary>
  [Serializable]
  public abstract class RegularTuple: Tuple
  {
    [NonSerialized]
    [IgnoreDataMember] 
    protected TupleDescriptor descriptor;

    public override TupleDescriptor Descriptor
    {
      get { return descriptor; }
    }
    /*    /// <summary>
        /// Sets the field state associated with the field.
        /// </summary>
        /// <param name="fieldIndex">Index of the field to set the state for.</param>
        /// <param name="state">The state to set.</param>
        protected abstract void SetFieldState(int fieldIndex, TupleFieldState state);*/

    protected sealed override Delegate GetGetValueDelegate(int fieldIndex)
    {
      return descriptor.GetValueDelegates[fieldIndex];
    }

    protected sealed override Delegate GetGetNullableValueDelegate(int fieldIndex)
    {
      return descriptor.GetNullableValueDelegates[fieldIndex];
    }

    protected sealed override Delegate GetSetValueDelegate(int fieldIndex)
    {
      return descriptor.SetValueDelegates[fieldIndex];
    }

    protected sealed override Delegate GetSetNullableValueDelegate(int fieldIndex)
    {
      return descriptor.SetNullableValueDelegates[fieldIndex];
    }

    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true" />
    /// </summary>
    protected RegularTuple(TupleDescriptor descriptor)
    {
      this.descriptor = descriptor;
    }
  }
}