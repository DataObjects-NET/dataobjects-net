// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitri Maximov
// Created:  2009.05.13

using System;
using NUnit.Framework;
using Xtensive.Orm.Configuration;
using Xtensive.Orm.Tests.Issues.Issue0036_Model;

namespace Xtensive.Orm.Tests.Issues.Issue0036_Model
{
  public interface ISecurable : IEntity
  {
  }

  [Serializable]
  [HierarchyRoot]
  public class Person : Entity
  {
    [Field, Key]
    public int Id { get; private set; }
  }

  [Serializable]
  [HierarchyRoot]
  public class User : Entity, ISecurable
  {
    [Field, Key]
    public int Id { get; private set; }
  }

  [Serializable]
  [KeyGenerator(KeyGeneratorKind.None)]
  [HierarchyRoot]
  public class SyncInfo<TEntity> : Entity 
    where TEntity : Entity, new()
  {
    [Field, Key]
    public TEntity Target { get; private set; }

    public SyncInfo (TEntity target)
      : base(target)
    {
    }
  }

  [Serializable]
  [KeyGenerator(KeyGeneratorKind.None)]
  [HierarchyRoot]
  public class SecurityInfo<TEntity> : Entity 
    where TEntity : Entity, ISecurable
  {
    [Field, Key]
    public TEntity Target { get; private set; }

    public SecurityInfo (TEntity target)
      : base(target)
    {
    }
  }
}

namespace Xtensive.Orm.Tests.Issues
{
  public class Issue0036_AutomaticGenericTypesRegistration : AutoBuildTest
  {
    protected override DomainConfiguration BuildConfiguration()
    {
      var config = base.BuildConfiguration();
      config.Types.Register(typeof (Person).Assembly, typeof (Person).Namespace);
      return config;
    }

    [Test]
    public void MainTest()
    {
//      Domain.Model.Dump();

      using (var session = Domain.OpenSession()) {
        using (var t = session.OpenTransaction()) {
          var person = new Person();
          var personSyncInfo = new SyncInfo<Person>(person);
          var user = new User();
          var userSyncInfo = new SyncInfo<User>(user);
          var userSecurityInfo = new SecurityInfo<User>(user);

          t.Complete();
        }
      }
    }
  }
}