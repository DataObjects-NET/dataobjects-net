// Copyright (C) 2014 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Denis Krjuchkov
// Created:    2014.04.11

using System;
using System.Linq;
using NUnit.Framework;
using Xtensive.Orm.Tests.Issues.IssueJira0528_LongEnumBitOperationsModel;

namespace Xtensive.Orm.Tests.Issues
{
  namespace IssueJira0528_LongEnumBitOperationsModel
  {
    [Flags]
    public enum LongEnum : long
    {
      None = 0,
      Flag0 = 1L << 32,
      Flag1 = 1L << 33,
      Flag2 = 1L << 34,
    }

    [HierarchyRoot]
    public class EnumContainer : Entity
    {
      [Key, Field]
      public long Id { get; private set; }

      [Field]
      public LongEnum Value { get; set; }
    }
  }

  [TestFixture]
  public class IssueJira0528_LongEnumBitOperations : AutoBuildTest
  {
    protected override Orm.Configuration.DomainConfiguration BuildConfiguration()
    {
      var configuration = base.BuildConfiguration();
      configuration.Types.Register(typeof (EnumContainer));
      return configuration;
    }

    protected override void PopulateData()
    {
      using (var session = Domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        new EnumContainer {Value = LongEnum.Flag0 | LongEnum.Flag1};
        new EnumContainer {Value = LongEnum.Flag1 | LongEnum.Flag2};
        new EnumContainer {Value = LongEnum.Flag2 | LongEnum.Flag0};
        tx.Complete();
      }
    }

    [Test]
    public void BitAndTest()
    {
      using (var session = Domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        var result = session.Query.All<EnumContainer>()
          .Count(item => (item.Value & LongEnum.Flag1)==LongEnum.Flag1);
        Assert.That(result, Is.EqualTo(2));
        result = session.Query.All<EnumContainer>()
          .Count(item => (item.Value & LongEnum.Flag1)==0);
        Assert.That(result, Is.EqualTo(1));
        tx.Complete();
      }
    }

    [Test]
    public void BitOrTest()
    {
      using (var session = Domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        var result = session.Query.All<EnumContainer>()
          .Count(item => (item.Value | LongEnum.Flag1)==(LongEnum.Flag0 | LongEnum.Flag1));
        Assert.That(result, Is.EqualTo(1));
        tx.Complete();
      }
    }

    [Test]
    public void BitNotTest()
    {
      using (var session = Domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        var result = session.Query.All<EnumContainer>()
          .Count(item => ~item.Value==~(LongEnum.Flag1 | LongEnum.Flag2));
        Assert.That(result, Is.EqualTo(1));
        tx.Complete();
      }
    }

    [Test]
    public void BitXorTest()
    {
      using (var session = Domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        var result = session.Query.All<EnumContainer>()
          .Count(item => (item.Value ^ LongEnum.Flag0)==(LongEnum.Flag0 | LongEnum.Flag1 | LongEnum.Flag2));
        Assert.That(result, Is.EqualTo(1));
        tx.Complete();
      }
    }
  }
}