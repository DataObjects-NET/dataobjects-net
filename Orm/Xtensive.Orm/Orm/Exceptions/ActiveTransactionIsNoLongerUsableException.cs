// Copyright (C) 2003-2021 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.

using System;
using System.Runtime.Serialization;


namespace Xtensive.Orm
{
  [Serializable]
  public class ActiveTransactionIsNoLongerUsableException : InvalidOperationException
  {
    public ActiveTransactionIsNoLongerUsableException()
      : base(Strings.ExActiveTransactionIsNoLongerUsable)
    {
    }
  }
}
