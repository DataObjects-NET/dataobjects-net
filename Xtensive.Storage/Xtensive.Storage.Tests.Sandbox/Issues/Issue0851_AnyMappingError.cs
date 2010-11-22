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
  // Мы можем создать свой класс бизнес-объектов,
  // чтобы проверить какие-то нюансы работы модели.

  [Serializable]
  [HierarchyRoot]
  public class CustomNamed : Entity
  {
    [Field, Key]
    public int ID { get; private set; }

    [Field(Length = 100)]
    public string Name { get; set; }

    public override string ToString()
    {
      return Name;
    }
  }

  public class Good : CustomNamed
  {
  }

  public class Contractor : CustomNamed
  {
  }


  public interface IDoc : IEntity
  {
    [Field]
    //[Key]
      int ID { get; }

    //[Field]
    string TypeName { get; }

    [Field]
    string DocNo { get; }

    [Field]
    DateTime Date { get; }
  }


  public interface IGoodDoc : IDoc
  {
    [Field]
    EntitySet<BladeItem> Items { get; }
  }


  public class Blade : CustomNamed, IGoodDoc
  {
    [Field]
    public DateTime Date { get; set; }


    public string TypeName
    {
      get { return this.GetType().ToString(); }
    }

    [Field]
    public string DocNo { get; set; }

    [Field]
    public Contractor Contractor { get; set; }


    [Field]
    public EntitySet<BladeItem> Items { get; private set; }


    public override string ToString()
    {
      return TypeName + " " + DocNo + "  - " + Date;
    }
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

    [Field]
    public decimal Number { get; set; }

    [Field(Scale = 2)]
    public decimal Price { get; set; }

    [Field(Scale = 2)]
    public decimal Cost { get; set; }
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
            var contr = new Contractor() { Name = "Сидоров" };
            var g1 = new Good() { Name = "Товар1" };
            var g2 = new Good() { Name = "Товар2" };
            var g3 = new Good() { Name = "Товар3" };

            var b1 = new Blade() { Date = DateTime.Now, Contractor = contr };
            b1.Items.Add(new BladeItem(){Good = g1, Number= 1, Price = 10, Cost = 10});
            b1.Items.Add(new BladeItem() { Good = g2, Number = 2, Price = 20, Cost = 40 });

            var b2 = new Blade() { Date = DateTime.Now, Contractor = contr };
            b2.Items.Add(new BladeItem() { Good = g1, Number = 1, Price = 10, Cost = 10 });
            b2.Items.Add(new BladeItem() { Good = g2, Number = 2, Price = 20, Cost = 40 });
            b2.Items.Add(new BladeItem() { Good = g3, Number = 3, Price = 30, Cost = 90 });

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