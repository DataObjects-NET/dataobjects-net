// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Gamzov
// Created:    2008.01.29

using System;
using Xtensive.Collections;
using Xtensive.Core;
using Xtensive.IoC;
using Xtensive.Reflection;

using Xtensive.Tuples;
using Tuple = Xtensive.Tuples.Tuple;

namespace Xtensive.Indexing.Hashing
{
  internal class TupleHasher : WrappingHasher<Tuples.Tuple, object>,
    IFinalAssociate
  {
    #region Nested types: SingleHashData, ArrayHashData

    internal struct SingleHashData
    {
      public Tuples.Tuple X;
      public long Result;

      public SingleHashData(Tuples.Tuple x)
      {
        X = x;
        Result = 0;
      }
    }

    internal struct ArrayHashData
    {
      public Tuples.Tuple X;
      public int HashCount;
      public long[] Result;

      public ArrayHashData(Tuples.Tuple x, int hashCount)
      {
        X = x;
        HashCount = hashCount;
        Result = new long[hashCount];
      }
    }

    internal class SingleHashingHandler
    {
      public ExecutionSequenceHandler<SingleHashData>[] Handlers;

      public SingleHashingHandler(TupleDescriptor descriptor)
      {
        Handlers = new ExecutionSequenceHandler<SingleHashData>[descriptor.Count];
      }
    }

    internal class ArrayHashingHandler
    {
      public ExecutionSequenceHandler<ArrayHashData>[] Handlers;

      public ArrayHashingHandler(TupleDescriptor descriptor)
      {
        Handlers = new ExecutionSequenceHandler<ArrayHashData>[descriptor.Count];
      }
    }

    #endregion

    [NonSerialized]
    private ThreadSafeList<SingleHashingHandler> singleHashingHandlers;

    [NonSerialized]
    private ThreadSafeList<ArrayHashingHandler> arrayHashingHandlers;


    /// <inheritdoc/>
    public override long GetHash(Tuples.Tuple value)
    {
      if (value==null)
        return BaseHasher.GetHash(null);
      var data = new SingleHashData(value);
      SingleHashingHandler h = GetSingleHashingHandler(value.Descriptor);
      DelegateHelper.ExecuteDelegates(h.Handlers, ref data, Direction.Positive);
      return data.Result;
    }

    /// <inheritdoc/>
    public override long[] GetHashes(Tuples.Tuple value, int count)
    {
      if (value==null)
        return BaseHasher.GetHashes(null, count);
      var data = new ArrayHashData(value, count);
      ArrayHashingHandler h = GetArrayHashingHandler(value.Descriptor);
      DelegateHelper.ExecuteDelegates(h.Handlers, ref data, Direction.Positive);
      return data.Result;
    }

    private SingleHashingHandler GetSingleHashingHandler(TupleDescriptor descriptor)
    {
      return singleHashingHandlers.GetValue(descriptor.Identifier,
        (indentifier, _this, _descriptor) => {
          var box = new Box<SingleHashingHandler>(new SingleHashingHandler(descriptor));
          ExecutionSequenceHandler<Box<SingleHashingHandler>>[] initializers =
            DelegateHelper.CreateDelegates<ExecutionSequenceHandler<Box<SingleHashingHandler>>>(
              _this, _this.GetType(), "SingleInitializeStep", _descriptor);
          DelegateHelper.ExecuteDelegates(initializers, ref box, Direction.Positive);
          return box.Value;
        },
        this, descriptor);
    }

    private ArrayHashingHandler GetArrayHashingHandler(TupleDescriptor descriptor)
    {
      return arrayHashingHandlers.GetValue(descriptor.Identifier,
        (indentifier, _this, _descriptor) => {
          var box = new Box<ArrayHashingHandler>(new ArrayHashingHandler(descriptor));
          ExecutionSequenceHandler<Box<ArrayHashingHandler>>[] initializers =
            DelegateHelper.CreateDelegates<ExecutionSequenceHandler<Box<ArrayHashingHandler>>>(
              _this, _this.GetType(), "ArrayInitializeStep", _descriptor);
          DelegateHelper.ExecuteDelegates(initializers, ref box, Direction.Positive);
          return box.Value;
        },
        this, descriptor);
    }

    private bool SingleInitializeStep<TFieldType>(ref Box<SingleHashingHandler> data, int fieldIndex)
    {
      data.Value.Handlers[fieldIndex] = ExecuteSingle<TFieldType>;
      return false;
    }

    private bool ArrayInitializeStep<TFieldType>(ref Box<ArrayHashingHandler> data, int fieldIndex)
    {
      data.Value.Handlers[fieldIndex] = ExecuteArray<TFieldType>;
      return false;
    }

    private bool ExecuteSingle<TFieldType>(ref SingleHashData actionData, int fieldIndex)
    {
      TupleFieldState fieldState;
      var value = actionData.X.GetValue<TFieldType>(fieldIndex, out fieldState);
      if (fieldState.HasValue())
        actionData.Result ^= Provider.GetHasher<TFieldType>().GetHash(value);
      else
        actionData.Result ^= BaseHasher.GetHash(null);
      return false;
    }

    private bool ExecuteArray<TFieldType>(ref ArrayHashData actionData, int fieldIndex)
    {
      TupleFieldState fieldState;
      var value = actionData.X.GetValue<TFieldType>(fieldIndex, out fieldState);
      var hashes = fieldState.HasValue() 
        ? Provider.GetHasher<TFieldType>().GetHashes(value, actionData.HashCount) 
        : BaseHasher.GetHashes(null, actionData.HashCount);
      for (int i = 0; i < actionData.HashCount; i++)
        actionData.Result[i] ^= hashes[i];
      return false;
    }

    private void Initialize()
    {
      singleHashingHandlers.Initialize(new object());
      arrayHashingHandlers.Initialize(new object());
    }


    // Constructors

    public TupleHasher(IHasherProvider provider)
      : base(provider)
    {
      Initialize();
    }
  }
}
