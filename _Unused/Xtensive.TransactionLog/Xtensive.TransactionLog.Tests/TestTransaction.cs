// Copyright (C) 2007 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Gamzov
// Created:    2007.10.31

using System;
using Xtensive.Core;
using Xtensive.Integrity.Transactions;

namespace Xtensive.TransactionLog.Tests
{
  [Serializable]
  public class TestTransaction : ITransactionInfo<long>
  {
    private readonly long identifier;
    private TransactionState state = TransactionState.Active;
    private byte[] data = new byte[16*1024]; // 16Kb of data. Just for serialization
    private Guid internalData;


    public long Identifier
    {
      get { return identifier; }
    }

    object IIdentified.Identifier
    {
      get { return identifier; }
    }

    public TransactionState State
    {
      get { return state; }
    }

    public void SetState(TransactionState state)
    {
      this.state = state;
    }

    public object Data
    {
      get { return data; }
    }

    public Guid InternalData
    {
      get { return internalData; }
      set { internalData = value; }
    }


    // Constructors

    public TestTransaction(long identifier)
    {
      this.identifier = identifier;
      for (int i = 0; i<data.Length;i++) {
        data[i] = 123;
      }
    }
  }
}