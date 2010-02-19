// Copyright (C) 2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexander Nikolaev
// Created:    2010.02.19

using System;
using System.Runtime.Serialization;
using Xtensive.Core;

namespace Xtensive.Storage.Operations
{
  [Serializable]
  internal abstract class UniqueOperationBase : Operation,
    IUniqueOperation,
    IEquatable<UniqueOperationBase>
  {
    private const string keyName = "key";
    private readonly Pair<Type, Key> identifier;
    private readonly int hashCode;

    public object Identifier {get { return identifier;}}

    public abstract bool IgnoreDuplicate { get; }

    public Key Key { get; private set; }

    public bool Equals(UniqueOperationBase other)
    {
      if (ReferenceEquals(null, other))
        return false;
      if (ReferenceEquals(this, other))
        return true;
      return other.identifier.Equals(identifier);
    }

    public override bool Equals(object obj)
    {
      if (ReferenceEquals(null, obj))
        return false;
      if (ReferenceEquals(this, obj))
        return true;
      if (obj.GetType()!=typeof (UniqueOperationBase))
        return false;
      return Equals((UniqueOperationBase) obj);
    }

    public override int GetHashCode()
    {
      return hashCode;
    }


    // Constructors

    protected UniqueOperationBase(Key key, OperationType operationType)
      : base(operationType)
    {
      ArgumentValidator.EnsureArgumentNotNull(key, "key");

      Key = key;
      identifier = new Pair<Type, Key>(GetType(), key);
      hashCode = identifier.GetHashCode();
    }

    // Serialization

    protected override void GetObjectData(SerializationInfo info, StreamingContext context)
    {
      base.GetObjectData(info, context);
      info.AddValue(keyName, (SerializableKey) Key, typeof (SerializableKey));
    }

    protected UniqueOperationBase(SerializationInfo info, StreamingContext context)
      : base(info, context)
    {
      Key = ((SerializableKey) info.GetValue(keyName, typeof (SerializableKey))).Key;
      identifier = new Pair<Type, Key>(GetType(), Key);
    }
  }
}