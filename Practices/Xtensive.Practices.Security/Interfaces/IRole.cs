// Copyright (C) 2011 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitri Maximov
// Created:    2011.06.26

using System.Collections.Generic;
using Xtensive.Orm;

namespace Xtensive.Practices.Security
{
  public interface IRole : IEntity
  {
    [Field]
    string Name { get; }

    [Field]
    [Association(PairTo = "Roles", OnOwnerRemove = OnRemoveAction.Clear, OnTargetRemove = OnRemoveAction.Clear)]
    EntitySet<IPrincipal> Principals { get; }

    IList<Permission> Permissions { get; }
  }
}