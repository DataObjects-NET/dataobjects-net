// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Yakunin
// Created:    2009.03.18

using System;
using System.Diagnostics;

namespace Xtensive.Orm.Tests.Core.Modelling.DatabaseModel
{
  [Serializable]
  public sealed class UserCollection : NodeCollectionBase<User, Security>
  {
    internal UserCollection(Security parent)
      : base(parent, "Users")
    {
    }
  }
}