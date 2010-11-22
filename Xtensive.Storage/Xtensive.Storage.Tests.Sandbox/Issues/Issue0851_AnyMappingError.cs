// Copyright (C) 2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitri Maximov
// Created:    2010.10.14

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using NUnit.Framework;
using Xtensive.Storage.Configuration;
using Xtensive.Storage.Model;
using Xtensive.Storage.Tests.Issues.Issue0851_AnyMappingError_Model;
using Xtensive.Core;

namespace Xtensive.Storage.Tests.Issues.Issue0851_AnyMappingError_Model
{
  [HierarchyRoot]
  public class CustomNamed : Entity
  {
    [Field, Key]
    public int Id { get; private set; }

    [Field(Length = 100)]
    public string Name { get; set; }
  }

  public class Good : CustomNamed
  {
  }


  public interface IGoodDoc : IEntity
  {
    [Field]
    EntitySet<BladeItem> Items { get; }
  }


  public class Blade : CustomNamed, IGoodDoc
  {
    [Field]
    public EntitySet<BladeItem> Items { get; private set; }
  }


  [HierarchyRoot]
  public class BladeItem : Entity
  {
    [Field, Key]
    public int Id { get; private set; }

    [Field]
    public Blade Doc { get; set; }


    [Field]
    public Good Good { get; set; }
  }


}

namespace Xtensive.Storage.Tests.Issues
{
  public class Issue0851_AnyMappingError : AutoBuildTest
  {
    protected override DomainConfiguration BuildConfiguration()
    {
      var config = base.BuildConfiguration();
      config.Types.Register(typeof (IGoodDoc).Assembly, typeof (IGoodDoc).Namespace);
      return config;
    }

    [Test]
    public void TestBladeQuery()
    {
      // Создаем накладные
      using (Session.Open(Domain)) {
        using (var transactionScope = Transaction.Open()) {
            var g1 = new Good { Name = "Товар1" };
            var g2 = new Good { Name = "Товар2" };
            var g3 = new Good { Name = "Товар3" };

            var b1 = new Blade();
            b1.Items.Add(new BladeItem {Good = g1,});
            b1.Items.Add(new BladeItem { Good = g2, });

            var b2 = new Blade();
            b2.Items.Add(new BladeItem { Good = g1,  });
            b2.Items.Add(new BladeItem() { Good = g2,  });
            b2.Items.Add(new BladeItem() { Good = g3,  });

          Console.WriteLine("Созданы документы:");
          foreach (IGoodDoc doc in Query.All<Blade>().ToList()) {
            Console.WriteLine(doc.ToString());
            foreach (BladeItem item in doc.Items) {
              Console.WriteLine("     " + item.Good.ToString());
            }
          }
          Console.WriteLine();

          transactionScope.Complete();
        }
      }


      // Тестируем запрос к накладным
      using (Session.Open(Domain)) {
        using (var transactionScope = Transaction.Open()) {
          var Good = Query.All<Good>().Skip(2).FirstOrDefault();
          Console.WriteLine();
          Console.WriteLine("Good=" + Good);
          Console.WriteLine();
          var l = Query.All<IGoodDoc>()
            .Where(doc => (doc.Items.Any(item => item.Good==Good)))
            .ToList();

          Console.WriteLine("Результат запроса:");
          foreach (IGoodDoc doc in l) {
            Console.WriteLine(doc.ToString());
            foreach (BladeItem item in doc.Items) {
              Console.WriteLine("     " + item.Good.ToString());
            }
          }

          // В списке должен быть только один документ - содержащий Товар3.
          Assert.AreEqual(1, l.Count(), "Кол-во документов в списке");


          transactionScope.Complete();
        }
      }
    }
  }
}