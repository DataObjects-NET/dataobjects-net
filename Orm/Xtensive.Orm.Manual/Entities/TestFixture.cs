// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Kofman
// Created:    2009.06.25

using System;
using System.Diagnostics;
using NUnit.Framework;
using Xtensive.Orm.Configuration;

namespace Xtensive.Orm.Manual.Entities
{
  [TestFixture]
  public class TestFixture
  {
    [Serializable]
    [HierarchyRoot]
    public abstract class Document : Entity 
    {
      [Key, Field]
      public int Id { get; private set; }

      [Field]
      public DateTime Date { get; set; }

      [Field]
      public int Number { get; set; }
    }

    [Serializable]
    public class Order : Document 
    {
      [Field]
      public Product Product { get; set; }

      [Field]
      public UnitOfMeasure UnitOfMeasure { get; set; }

      [Field]
      public double Amount { get; set; }
    }

    [Serializable]
    [HierarchyRoot]
    public class Product : Entity
    {
      [Key, Field(Length = 100)]
      public Guid Code { get; private set; }

      [Field(Length = 100)]
      public string Name { get; set; }

      public Product(string code) : base(code) { }
    }

    public enum UnitOfMeasure { Barrels, Tons, Liters }

    [Test]
    public void MainTest()
    {
      var config = new DomainConfiguration("sqlserver://localhost/DO40-Tests");
      config.UpgradeMode = DomainUpgradeMode.Recreate;
      config.Types.Register(typeof(Product));
//      config.Types.Register(typeof(Document).Assembly, typeof(Document).Namespace);
      Domain.Build(config);
    }
  }
}