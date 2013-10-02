// Copyright (C) 2013 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Denis Krjuchkov
// Created:    2013.02.11

using System.Linq;
using NUnit.Framework;
using Xtensive.Orm.Configuration;
using Xtensive.Orm.Tests.Issues.IssueJira0432_EnumTypeDiscriminatorModel;

namespace Xtensive.Orm.Tests.Issues
{
  namespace IssueJira0432_EnumTypeDiscriminatorModel
  {
    public enum TypeIdentifier
    {
      Base = 0,
      Child = 1,
    }

    [HierarchyRoot, TypeDiscriminatorValue(TypeIdentifier.Base, Default = true)]
    public class Base : Entity
    {
      [Key, Field]
      public int Id { get; private set; }

      [Field, TypeDiscriminator]
      public TypeIdentifier TypeIdentifier { get; private set; }
    }

    [TypeDiscriminatorValue(TypeIdentifier.Child)]
    public class Child : Base
    {
    }
  }

  [TestFixture]
  public class IssueJira0432_EnumTypeDiscriminator : AutoBuildTest
  {
    protected override DomainConfiguration BuildConfiguration()
    {
      var configuration = base.BuildConfiguration();
      configuration.Types.Register(typeof (TypeIdentifier).Assembly, typeof (TypeIdentifier).Namespace);
      return configuration;
    }

     [Test]
     public void MainTest()
     {
       using (var session = Domain.OpenSession())
       using (var tx = session.OpenTransaction()) {
         var baseQuery = session.Query.All<Base>().ToList();
         var childQuery = session.Query.All<Child>().ToList();
       }
     }
  }
}