// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Ivan Galkin
// Created:    2009.04.14

using System;
using System.Transactions;
using Xtensive.Storage.Indexing;

namespace Xtensive.Storage.Providers.Index
{
  [Serializable]
  public class Storage : MarshalByRefObject, IStorage
  {
    #region Implementation of IStorage

    public IStorageView CreateView(IsolationLevel isolationLevel)
    {
      throw new System.NotImplementedException();
    }

    public IStorageView GetView(Guid transactionId)
    {
      throw new System.NotImplementedException();
    }

    #endregion
  }
}