// Copyright (C) 2011 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitri Maximov
// Created:    2011.05.23

using System;
using System.Collections.Generic;
using Xtensive.Orm;

namespace Xtensive.Practices.Security
{
  public interface IRoleProvider : IDomainService
  {
    IEnumerable<Role> GetAllRoles();
  }
}