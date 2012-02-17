// Copyright (C) 2011 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Denis Krjuchkov
// Created:    2011.11.15

using System;
using System.Linq;
using NUnit.Framework;
using Xtensive.Storage;
using Xtensive.Storage.Configuration;
using Xtensive.Storage.Tests.Storage.AdvancedDefaultAndTypeDiscriminatorTestModel;

namespace Xtensive.Storage.Tests.Storage.AdvancedDefaultAndTypeDiscriminatorTestModel
{
  public static class CodeRegistry
  {
    public const string DefaultCode = "{9F011719-DD4D-4B45-84F8-56B05A47952B}";
  }

  public interface IDiscriminatedByValue : IEntity
  {
    [Field, TypeDiscriminator]
    Guid Code { get; }
  }

  [HierarchyRoot, TypeDiscriminatorValue(CodeRegistry.DefaultCode)]
  public class DiscriminatedByValue : Entity, IDiscriminatedByValue
  {
    [Field, Key]
    public int Id { get; private set; }

    [Field]
    public Guid Code { get; set; }
  }

  [HierarchyRoot]
  public class Ref : Entity
  {
    [Field, Key]
    public Guid Id { get; private set; }

    public Ref(Guid id)
      : base(id)
    {}
  }

  public interface IDiscriminatedByRef : IEntity
  {
    [Field, TypeDiscriminator]
    Ref Ref { get; }
  }

  [HierarchyRoot, TypeDiscriminatorValue(CodeRegistry.DefaultCode)]
  public class DiscriminatedByRef : Entity, IDiscriminatedByRef
  {
    [Field, Key]
    public int Id { get; private set; }

    [Field]
    public Ref Ref { get; set; }
  }
}

namespace Xtensive.Storage.Tests.Storage
{
  public class TypeDiscriminatorFromInterfaceTest : AutoBuildTest
  {
    protected override DomainConfiguration BuildConfiguration()
    {
      var config = base.BuildConfiguration();
      config.Types.Register(typeof (DiscriminatedByValue).Assembly, typeof (DiscriminatedByValue).Namespace);
      return config;
    }

    [Test]
    public void DiscriminateByValueTest()
    {
      using (Session.Open(Domain))
      using (var t = Transaction.Open()) {
        var d = new DiscriminatedByValue();
        var items = Query.All<DiscriminatedByValue>().ToList();
        Assert.AreEqual(new Guid(CodeRegistry.DefaultCode), d.Code);
      }
    }

    [Test]
    public void DiscriminateByRefTest()
    {
      using (Session.Open(Domain))
      using (var t = Transaction.Open()) {
        var r = new Ref(new Guid(CodeRegistry.DefaultCode));
        var d = new DiscriminatedByRef()
                  {
                    Ref = r
                  };
        t.Complete();
      }

      using (Session.Open(Domain))
      using (var t = Transaction.Open()) {
        var items = Query.All<DiscriminatedByRef>().ToList();
        var d = items[0];
        Assert.IsNotNull(d.Ref);
        Assert.AreEqual(new Guid(CodeRegistry.DefaultCode), d.Ref.Id);
      }
    }
  }
}