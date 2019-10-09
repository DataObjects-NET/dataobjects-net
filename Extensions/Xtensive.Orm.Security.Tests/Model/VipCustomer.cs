// Copyright (C) 2011 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitri Maximov
// Created:    2011.05.30

using System;
using Xtensive.Orm;

namespace Xtensive.Orm.Security.Tests.Model
{
  public class VipCustomer : Customer
  {
    [Field]
    public string Reason { get; set; }

    public VipCustomer(Session session)
      : base(session)
    {
      IsVip = true;
    }
  }
}