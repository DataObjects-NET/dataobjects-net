// Copyright (C) 2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexis Kochetov
// Created:    2010.07.26

using System;
using System.Diagnostics;
using System.Linq.Expressions;
using NUnit.Framework;
using System.Linq;
using Xtensive.Orm.Configuration;
using Xtensive.Orm.Model;
using Xtensive.Orm.Tests.Issues.Issue0771_AbstractTypeRegistration_Model;

namespace Xtensive.Orm.Tests.Issues
{
  namespace Issue0771_AbstractTypeRegistration_Model
  {
    [HierarchyRoot]
    public abstract class A : Entity
    {
      [Field,Key]
      public int Id { get; private set; }
    }

    public abstract class B : A
    {}

    public abstract class C : B
    {}

    public class D : B
    {}

    public class E : C
    {}
  }

  [Serializable]
  public class Issue0771_AbstractTypeRegistration 
  {

    [Test]
//    [ExpectedException(typeof(DomainBuilderException))]
    public void AbstractClassPerHierarchyTest()
    {
      var config = DomainConfigurationFactory.Create();
      config.Types.Register(typeof (A));
      var domain = Domain.Build(config);
      using (var session = domain.OpenSession())
      using (var t = session.OpenTransaction()) {
        var list = session.Query.All<A>().ToList();
      }
    }

    [Test]
//    [ExpectedException(typeof(DomainBuilderException))]
    public void AbstractHierarchyTest()
    {
      var config = DomainConfigurationFactory.Create();
      config.Types.Register(typeof(A));
      config.Types.Register(typeof(B));
      var domain = Domain.Build(config);
      using (var session = domain.OpenSession())
      using (var t = session.OpenTransaction()) {
        var listA = session.Query.All<A>().ToList();
        var listB = session.Query.All<B>().ToList();
      }
    }

    [Test]
    public void NonAbstractLeafTest()
    {
      var config = DomainConfigurationFactory.Create();
      config.Types.Register(typeof(A));
      config.Types.Register(typeof(B));
      config.Types.Register(typeof(C));
      config.Types.Register(typeof(D));
      config.Types.Register(typeof(E));
      var domain = Domain.Build(config);
      using (var session = domain.OpenSession())
      using (var t = session.OpenTransaction()) {
        var listA = session.Query.All<A>().ToList();
        var listB = session.Query.All<B>().ToList();
        var listC = session.Query.All<C>().ToList();
        var listD = session.Query.All<D>().ToList();
        var listE = session.Query.All<E>().ToList();
      }
    }
  }
}