// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitri Maximov
// Created:  2009.05.13

using NUnit.Framework;
using Xtensive.Storage.Configuration;
using Xtensive.Storage.Tests.Issues.Issue0036_Model;

namespace Xtensive.Storage.Tests.Issues.Issue0036_Model
{
  public interface ISecurable : IEntity
  {
  }

  [HierarchyRoot]
  public class Person : Entity
  {
    [Field, KeyField]
    public int Id { get; private set; }
  }

  [HierarchyRoot]
  public class User : Entity, ISecurable
  {
    [Field, KeyField]
    public int Id { get; private set; }
  }

  [KeyGenerator(null)]
  [HierarchyRoot]
  public class SyncInfo<TEntity> : Entity where TEntity : Entity
  {
    [Field, KeyField]
    public TEntity Target { get; private set; }

    public SyncInfo (TEntity target)
      : base(target.Key.Value)
    {
    }
  }

  [KeyGenerator(null)]
  [HierarchyRoot]
  public class SecurityInfo<TEntity> : Entity where TEntity : Entity, ISecurable
  {
    [Field, KeyField]
    public TEntity Target { get; private set; }

    public SecurityInfo (TEntity target)
      : base(target.Key.Value)
    {
    }
  }
}

namespace Xtensive.Storage.Tests.Issues
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
      Domain.Model.Dump();

      using (Domain.OpenSession()) {
        using (var t = Transaction.Open()) {
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