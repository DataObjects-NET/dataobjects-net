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
  [DataContract]
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

    protected sealed override Delegate GetGetValueDelegate(int fieldIndex)
    {
      return Descriptor.GetValueDelegates[fieldIndex];
    }

    protected sealed override Delegate GetGetNullableValueDelegate(int fieldIndex)
    {
      return Descriptor.GetNullableValueDelegates[fieldIndex];
    }

    protected sealed override Delegate GetSetValueDelegate(int fieldIndex)
    {
      return Descriptor.SetValueDelegates[fieldIndex];
    }

    protected sealed override Delegate GetSetNullableValueDelegate(int fieldIndex)
    {
      return Descriptor.SetNullableValueDelegates[fieldIndex];
    }

    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true"/>
    /// </summary>
    /// <param name="descriptor">The tuple descriptor.</param>
    protected RegularTuple(TupleDescriptor descriptor)
    {
      this.descriptor = descriptor;
    }
  }
}