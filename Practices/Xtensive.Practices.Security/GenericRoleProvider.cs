// Copyright (C) 2011 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitri Maximov
// Created:    2011.05.30

using System;
using System.Collections.Generic;
using System.Linq;
using Xtensive.IoC;
using Xtensive.Orm;
using Xtensive.Reflection;

namespace Xtensive.Practices.Security
{
  [Service(typeof(IRoleProvider), Singleton = true)]
  public class GenericRoleProvider : IRoleProvider
  {
    public Domain Domain { get; private set; }

    public IEnumerable<Role> GetAllRoles()
    {
      var root = typeof(Role);
      var types = root.Assembly.FindDependentAssemblies()
        .SelectMany(a => a.GetTypes())
        .Where(t => t.IsClass && !t.IsAbstract && t.IsSubclassOf(root));

      var result = new List<Role>(types.Count());
      foreach (var type in types) {
        var ctor = type.GetConstructor(new Type[]{});
        if (ctor == null)
          throw new InvalidOperationException(string.Format("Unable to initialize {0} role, the type doesn't have a constructor without parameters", type.GetShortName()));
        result.Add((Role) Activator.CreateInstance(type));
      }
      return result;
    }

    [ServiceConstructor]
    public GenericRoleProvider(Domain domain)
    {
      Domain = domain;
    }
  }
}