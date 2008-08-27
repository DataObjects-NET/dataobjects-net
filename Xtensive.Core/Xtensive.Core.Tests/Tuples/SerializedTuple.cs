// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Yakunin
// Created:    2008.08.27

using System;
using System.Diagnostics;
using System.Runtime.Serialization;
using Xtensive.Core.Comparison;
using Xtensive.Core.Internals.DocTemplates;
using Xtensive.Core.Tuples;
using Xtensive.Core.Collections;

namespace Xtensive.Core.Tests.Tuples
{
  /// <summary>
  /// A helper struct allowing to serialize <see cref="Tuple"/> objects.
  /// </summary>
  [Serializable]
  public struct SerializedTuple : ISerializable,
    IDeserializationCallback,
    IEquatable<SerializedTuple>
  {
    [Serializable] private sealed class NotAvailable { }
    private readonly static NotAvailable notAvailableInstance = new NotAvailable();

    private object value;

    /// <summary>
    /// Gets the wrapped <see cref="Tuple"/>.
    /// </summary>
    public Tuple Value {
      [DebuggerStepThrough]
      get { return value as Tuple; }
    }

    #region Equals, GetHashCode, ==, !=

    /// <inheritdoc/>
    public bool Equals(SerializedTuple other)
    {
      return AdvancedComparerStruct<Tuple>.System.Equals(Value, other.Value);
    }

    /// <inheritdoc/>
    public override bool Equals(object obj)
    {
      if (ReferenceEquals(null, obj))
        return false;
      if (obj.GetType()!=typeof (SerializedTuple))
        return false;
      return Equals((SerializedTuple) obj);
    }

    /// <inheritdoc/>
    public override int GetHashCode()
    {
      return Value!=null ? Value.GetHashCode() : 0;
    }

    /// <see cref="ClassDocTemplate.OperatorEq" copy="true" />
    public static bool operator ==(SerializedTuple left, SerializedTuple right)
    {
      return left.Equals(right);
    }

    /// <see cref="ClassDocTemplate.OperatorNeq" copy="true" />
    public static bool operator !=(SerializedTuple left, SerializedTuple right)
    {
      return !left.Equals(right);
    }

    #endregion

    /// <inheritdoc/>
    public override string ToString()
    {
      return Value!=null ? Value.ToString() : string.Empty;
    }


    // Constructors
    
    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true"/>
    /// </summary>
    /// <param name="value">The <see cref="Value"/> property value.</param>
    public SerializedTuple(Tuple value)
    {
      this.value = value;
    }

    // Serialization
    
    /// <see cref="SerializableDocTemplate.GetObjectData" copy="true" />
    public void GetObjectData(SerializationInfo info, StreamingContext context)
    {
      var tuple = Value;
      if (tuple==null) {
        info.AddValue("Value", null, typeof(object));
        return;
      }

      var count = tuple.Count;
      var p = new Pair<Type[], object[]>(tuple.Descriptor.ToArray(), new object[count]);
      for (int i = 0; i < count; i++) {
        if (tuple.IsAvailable(i))
          p.Second[i] = tuple[i];
        else
          p.Second[i] = notAvailableInstance;
      }
      info.AddValue("Value", p, typeof(object));
    }

    /// <see cref="SerializableDocTemplate.Ctor" copy="true" />
    private SerializedTuple(SerializationInfo info, StreamingContext context)
    {
      value = info.GetValue("Value", typeof(object));
    }

    /// <inheritdoc/>
    public void OnDeserialization(object sender)
    {
      if (value==null || value.GetType()!=typeof(Pair<Type[], object[]>))
        return;
      var p = (Pair<Type[], object[]>) value;
      var tuple = Tuple.Create(p.First);
      var count = tuple.Count;
      for (int i = 0; i < count; i++) {
        if (p.Second[i].GetType()==typeof(NotAvailable))
          continue;
        tuple[i] = p.Second[i];
      }
      value = tuple;
    }
  }
}