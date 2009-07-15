// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Ivan Galkin
// Created:    2009.04.15

using System;
using System.Diagnostics;
using System.Transactions;
using Xtensive.Core;
using Xtensive.Integrity.Transactions;

namespace Xtensive.Storage.Providers.Index
{
  [Serializable]
  public class ClientTransaction:ITransaction
  {
    protected ITransaction realTransaction;

    object IIdentified.Identifier
    {
      get { return Identifier; }
    }

    public Guid Identifier
    {
      get { throw new System.NotImplementedException(); }
    }

    public void Commit()
    {
      throw new System.NotImplementedException();
    }

    public void Rollback()
    {
      throw new System.NotImplementedException();
    }

    public TransactionState State
    {
      get { throw new System.NotImplementedException(); }
    }

    public IsolationLevel IsolationLevel
    {
      get { throw new System.NotImplementedException(); }
    }

    public DateTime TimeStamp
    {
      get { throw new NotImplementedException(); }
    }
  }
}