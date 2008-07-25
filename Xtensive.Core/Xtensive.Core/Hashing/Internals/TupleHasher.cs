// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Gamzov
// Created:    2008.01.29

using Xtensive.Core.Tuples;

namespace Xtensive.Core.Hashing
{
  internal class TupleHasher : WrappingHasher<Tuple, object>,
    IFinalAssociate,
    ITupleFunctionHandler<TupleHasher.SingleHashData, long>,
    ITupleFunctionHandler<TupleHasher.ArrayHashData, long[]>
  {
    #region Nested types: SingleHashData, ArrayHashData

    internal struct SingleHashData : ITupleFunctionData<long>
    {
      public Tuple X;      
      public long Result;      

      public SingleHashData(Tuple x)
      {
        X = x;
        Result = 0;        
      }

      long ITupleFunctionData<long>.Result
      {
        get { return Result; }
      }
    }

    internal struct ArrayHashData : ITupleFunctionData<long[]>
    {
      public Tuple  X;
      public int    HashCount;      
      public long[] Result;

      public ArrayHashData(Tuple x, int hashCount)
      {
        X = x;                
        HashCount = hashCount;        
        Result = new long[hashCount];        
      }      

      long[] ITupleFunctionData<long[]>.Result
      {
        get { return Result; }
      }
    }

    #endregion

    /// <inheritdoc/>
    public override long GetHash(Tuple value)
    {
      if (value==null)
        return BaseHasher.GetHash(null);
      var data = new SingleHashData(value);
      return value.Descriptor.Execute((ITupleFunctionHandler<SingleHashData, long>) this, ref data, Direction.Positive);
    }

    /// <inheritdoc/>
    public override long[] GetHashes(Tuple value, int count)
    {
      if (value==null)
        return BaseHasher.GetHashes(null, count);
      var data = new ArrayHashData(value, count);
      return value.Descriptor.Execute((ITupleFunctionHandler<ArrayHashData, long[]>) this, ref data, Direction.Positive);
    }

    bool ITupleActionHandler<SingleHashData>.Execute<TFieldType>(ref SingleHashData actionData, int fieldIndex)
    {
      if (actionData.X.IsAvailable(fieldIndex) && actionData.X.HasValue(fieldIndex))
        actionData.Result ^= Provider.GetHasher<TFieldType>().GetHash((TFieldType)actionData.X.GetValueOrDefault(fieldIndex));        
      else
        actionData.Result ^= BaseHasher.GetHash(null);
      return false;
    }

    bool ITupleActionHandler<ArrayHashData>.Execute<TFieldType>(ref ArrayHashData actionData, int fieldIndex)
    {
      long[] hashes;            
      if (!actionData.X.IsAvailable(fieldIndex) || !actionData.X.HasValue(fieldIndex)) 
        hashes = BaseHasher.GetHashes(null, actionData.HashCount);   
      else {
        var value = actionData.X.GetValueOrDefault<TFieldType>(fieldIndex);
        hashes = Provider.GetHasher<TFieldType>().GetHashes(value, actionData.HashCount);
      }
      for (int i = 0; i < actionData.HashCount; i++) 
        actionData.Result[i] ^= hashes[i];      
      return false;
    }
    

    // Constructors

    public TupleHasher(IHasherProvider provider)
      : base(provider)
    {
    }
  }
}