// Copyright (C) 2019 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Kulakov
// Created:    2019.10.26

using System.Linq;
using NUnit.Framework;
using Xtensive.Core;
using Xtensive.Orm.Configuration;
using Xtensive.Orm.Tests.Storage.ClosureParametersCachingTestModel;

namespace Xtensive.Orm.Tests.Storage.ClosureParametersCachingTestModel
{
  #region Entityes

  [HierarchyRoot]
  public class PaymentSplit : Entity
  {
    [Field, Key]
    public long Id { get; set; }

    [Field]
    public bool Active { get; set; }

    [Field]
    public Payment Payment { get; set; }

    [Field]
    public Invoice Invoice { get; set; }

    [Field]
    public int Order { get;set; }

    [Field(Precision = 19, Scale = 6)]
    public decimal Amount { get; set; }
  }

  [HierarchyRoot]
  public class Payment : Entity
  {
    [Field, Key]
    public long Id { get; set; }

    [Field]
    public bool Active { get; set; }
  }

  [HierarchyRoot]
  public class Invoice : Entity
  {
    [Field, Key]
    public long Id { get; set; }

    [Field]
    public bool Active { get; set; }
  }

  #endregion

  public class BaseValueTypeTester
  {
    public class NestedValueTypeTester
    {
      private long privateField;
      public long PublicField;

      public int PublicProperty { get; set; }
      private int PrivateProperty { get; set; }

      public decimal TestPrivateField(Session session)
      {
        return session.Query.Execute(q => {
            var result = q.All<PaymentSplit>()
              .Where(split => split.Active && split.Payment.Active && split.Invoice.Id==privateField);
            return result;
          })
          .Sum(split => split.Amount);
      }

      public decimal TestPublicField(Session session)
      {
        return session.Query.Execute(q => {
            var result = q.All<PaymentSplit>()
              .Where(split => split.Active && split.Payment.Active && split.Invoice.Id==PublicField);
            return result;
          })
          .Sum(split => split.Amount);
      }

      public decimal TestPrivateProperty(Session session)
      {
        return session.Query.Execute(q => {
            var result = q.All<PaymentSplit>()
              .Where(split => split.Active && split.Payment.Active && split.Invoice.Id==PrivateProperty);
            return result;
          })
          .Sum(split => split.Amount);
      }

      public decimal TestPublicProperty(Session session)
      {
        return session.Query.Execute(q => {
            var result = q.All<PaymentSplit>()
              .Where(split => split.Active && split.Payment.Active && split.Invoice.Id==PublicProperty);
            return result;
          })
          .Sum(split => split.Amount);
      }

      public NestedValueTypeTester(long privateFieldValue)
      {
        privateField = privateFieldValue;
      }

      public NestedValueTypeTester(int privatePropertyValue)
      {
        PrivateProperty = privatePropertyValue;
      }
    }

    private long privateField;
    public long PublicField;

    public int PublicProperty { get; set; }
    private int PrivateProperty { get; set; }

    public decimal TestPrivateField(Session session)
    {
      return session.Query.Execute(q => {
          var result = q.All<PaymentSplit>()
            .Where(split => split.Active && split.Payment.Active && split.Invoice.Id==privateField);
          return result;
        })
        .Sum(split => split.Amount);
    }

    public decimal TestPublicField(Session session)
    {
      return session.Query.Execute(q => {
          var result = q.All<PaymentSplit>()
            .Where(split => split.Active && split.Payment.Active && split.Invoice.Id==PublicField);
          return result;
        })
        .Sum(split => split.Amount);
    }

    public decimal TestPrivateProperty(Session session)
    {
      return session.Query.Execute(q => {
          var result = q.All<PaymentSplit>()
            .Where(split => split.Active && split.Payment.Active && split.Invoice.Id==PrivateProperty);
          return result;
        })
        .Sum(split => split.Amount);
    }

    public decimal TestPublicProperty(Session session)
    {
      return session.Query.Execute(q => {
          var result = q.All<PaymentSplit>()
            .Where(split => split.Active && split.Payment.Active && split.Invoice.Id==PublicProperty);
          return result;
        })
        .Sum(split => split.Amount);
    }

    public BaseValueTypeTester(long privateFieldValue)
    {
      privateField = privateFieldValue;
    }

    public BaseValueTypeTester(int privatePropertyValue)
    {
      PrivateProperty = privatePropertyValue;
    }
  }

  public class InheritorValueTypeTester : BaseValueTypeTester
  {
    public static long StaticField;

    public decimal TestInheritedPublicField(Session session)
    {
      return session.Query.Execute(q => {
          var result = q.All<PaymentSplit>()
            .Where(split => split.Active && split.Payment.Active && split.Invoice.Id==PublicField);
          return result;
        })
        .Sum(split => split.Amount);
    }

    public decimal TestInheritedPublicProperty(Session session)
    {
      return session.Query.Execute(q => {
          var result = q.All<PaymentSplit>()
            .Where(split => split.Active && split.Payment.Active && split.Invoice.Id==PublicProperty);
          return result;
        })
        .Sum(split => split.Amount);
    }

    public decimal TestStaticField(Session session)
    {
      return session.Query.Execute(q => {
          var result = q.All<PaymentSplit>()
            .Where(split => split.Active && split.Payment.Active && split.Invoice.Id==StaticField);
          return result;
        })
        .Sum(split => split.Amount);
    }

    public decimal[] TestForIndex(Session session)
    {
      decimal[] resultSums = new decimal[2];
      for (int i = 0; i < 2; i++) {
        resultSums[i] = session.Query.Execute(q => {
            var result = q.All<PaymentSplit>()
              .Where(split => split.Active && split.Payment.Active && split.Order==i);
            return result;
          })
          .Sum(split => split.Amount);
      }

      return resultSums;
    }

    public decimal[] TestForeachItem(Session session)
    {
      decimal[] resultSums = new decimal[2];
      foreach (var item in Enumerable.Range(0, 2)) {
        resultSums[item] = session.Query.Execute(q => {
            var result = q.All<PaymentSplit>()
              .Where(split => split.Active && split.Payment.Active && split.Order==item);
            return result;
          })
          .Sum(split => split.Amount);
      }

      return resultSums;
    }

    public decimal TestMethodParameter(Session session, long parameter)
    {
      return session.Query.Execute(q => {
          var result = q.All<PaymentSplit>()
            .Where(split => split.Active && split.Payment.Active && split.Invoice.Id==parameter);
          return result;
        })
        .Sum(split => split.Amount);
    }

    public decimal TestLocalVariable(Session session, long invoiceId)
    {
      var invoiceIdLocal = invoiceId;
      return session.Query.Execute(q => {
          var result = q.All<PaymentSplit>()
            .Where(split => split.Active && split.Payment.Active && split.Invoice.Id==invoiceIdLocal);
          return result;
        })
        .Sum(split => split.Amount);
    }

    public InheritorValueTypeTester(long privateFieldValue)
      : base(privateFieldValue)
    {
    }

    public InheritorValueTypeTester(int privatePropertyValue)
      : base(privatePropertyValue)
    {
    }
  }


  public class BaseReferenceTypeTester
  {
    public class NestedReferenceTypeTester
    {
      private Invoice privateField;
      public Invoice PublicField;

      public Invoice PublicProperty { get; set; }
      private Invoice PrivateProperty { get; set; }

      public decimal TestPrivateField(Session session)
      {
        return session.Query.Execute(q => {
            var result = q.All<PaymentSplit>()
              .Where(split => split.Active && split.Payment.Active && split.Invoice==privateField);
            return result;
          })
          .Sum(split => split.Amount);
      }

      public decimal TestPublicField(Session session)
      {
        return session.Query.Execute(q => {
            var result = q.All<PaymentSplit>()
              .Where(split => split.Active && split.Payment.Active && split.Invoice==PublicField);
            return result;
          })
          .Sum(split => split.Amount);
      }

      public decimal TestPrivateProperty(Session session)
      {
        return session.Query.Execute(q => {
            var result = q.All<PaymentSplit>()
              .Where(split => split.Active && split.Payment.Active && split.Invoice==PrivateProperty);
            return result;
          })
          .Sum(split => split.Amount);
      }

      public decimal TestPublicProperty(Session session)
      {
        return session.Query.Execute(q => {
            var result = q.All<PaymentSplit>()
              .Where(split => split.Active && split.Payment.Active && split.Invoice==PublicProperty);
            return result;
          })
          .Sum(split => split.Amount);
      }

      public NestedReferenceTypeTester(Invoice fieldAndPropertyValue)
      {
        privateField = fieldAndPropertyValue;
        PrivateProperty = fieldAndPropertyValue;
      }
    }

    private Invoice privateField;
    public Invoice PublicField;

    public Invoice PublicProperty { get; set; }
    private Invoice PrivateProperty { get; set; }

    public decimal TestPrivateField(Session session)
    {
      return session.Query.Execute(q => {
          var result = q.All<PaymentSplit>()
            .Where(split => split.Active && split.Payment.Active && split.Invoice==privateField);
          return result;
        })
        .Sum(split => split.Amount);
    }

    public decimal TestPublicField(Session session)
    {
      return session.Query.Execute(q => {
          var result = q.All<PaymentSplit>()
            .Where(split => split.Active && split.Payment.Active && split.Invoice==PublicField);
          return result;
        })
        .Sum(split => split.Amount);
    }

    public decimal TestPrivateProperty(Session session)
    {
      return session.Query.Execute(q => {
          var result = q.All<PaymentSplit>()
            .Where(split => split.Active && split.Payment.Active && split.Invoice==PrivateProperty);
          return result;
        })
        .Sum(split => split.Amount);
    }

    public decimal TestPublicProperty(Session session)
    {
      return session.Query.Execute(q => {
          var result = q.All<PaymentSplit>()
            .Where(split => split.Active && split.Payment.Active && split.Invoice==PublicProperty);
          return result;
        })
        .Sum(split => split.Amount);
    }

    public BaseReferenceTypeTester(Invoice fieldAndPropertyValue)
    {
      privateField = fieldAndPropertyValue;
      PrivateProperty = fieldAndPropertyValue;
    }
  }

  public class InheritorReferenceTypeTester : BaseReferenceTypeTester
  {
    public decimal TestInheritedPublicField(Session session)
    {
      return session.Query.Execute(q =>
        {
          var result = q.All<PaymentSplit>()
            .Where(split => split.Active && split.Payment.Active && split.Invoice==PublicField);
          return result;
        })
        .Sum(split => split.Amount);
    }

    public decimal TestInheritedPublicProperty(Session session)
    {
      return session.Query.Execute(q => {
          var result = q.All<PaymentSplit>()
            .Where(split => split.Active && split.Payment.Active && split.Invoice==PublicProperty);
          return result;
        })
        .Sum(split => split.Amount);
    }

    public decimal TestMethodParameter(Session session, Invoice parameter)
    {
      return session.Query.Execute(q => {
          var result = q.All<PaymentSplit>()
            .Where(split => split.Active && split.Payment.Active && split.Invoice==parameter);
          return result;
        })
        .Sum(split => split.Amount);
    }

    public InheritorReferenceTypeTester(Invoice fieldAndPropertyValue)
      : base(fieldAndPropertyValue)
    {
    }
  }
}

namespace Xtensive.Orm.Tests.Storage
{
  public class ClosureParametersCachingTest : AutoBuildTest
  {
    private Pair<int> idsPair;

    protected override DomainConfiguration BuildConfiguration()
    {
      var configuration = base.BuildConfiguration();
      configuration.Types.Register(typeof (Invoice).Assembly, typeof (Invoice).Namespace);
      configuration.UpgradeMode = DomainUpgradeMode.Recreate;
      return configuration;
    }

    protected override void PopulateData()
    {
      using (var session = Domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        var invoice1 = new Invoice();
        var invoice2 = new Invoice();
        new PaymentSplit {
          Active = true,
          Payment = new Payment { Active = true },
          Invoice = invoice1,
          Order = 1,
          Amount = 6.0m
        };
        new PaymentSplit {
          Active = true,
          Payment = new Payment { Active = true },
          Invoice = invoice2,
          Order = 0,
          Amount = 16.0m
        };

        idsPair = new Pair<int>((int)invoice1.Id, (int)invoice2.Id);
        tx.Complete();
      }
    }

    [Test]
    public void LocalVariableClosureTest()
    {
      using (var session = Domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        var tester = new InheritorValueTypeTester(-1);

        Domain.QueryCache.Clear();
        Assert.That(tester.TestLocalVariable(session, idsPair.Second), Is.EqualTo(16.0m));
        Assert.That(tester.TestLocalVariable(session, idsPair.First), Is.EqualTo(6.0m));

        Assert.That(Domain.QueryCache.Count, Is.EqualTo(1));
      }
    }

    [Test]
    public void MethodValueParameterClosureTest()
    {
      using (var session = Domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        var tester = new InheritorValueTypeTester(-1);

        Domain.QueryCache.Clear();
        Assert.That(tester.TestMethodParameter(session, idsPair.Second), Is.EqualTo(16.0m));
        Assert.That(tester.TestMethodParameter(session, idsPair.First), Is.EqualTo(6.0m));

        Assert.That(Domain.QueryCache.Count, Is.EqualTo(1));
      }
    }

    [Test]
    public void MethodRefParameterClosureTest()
    {
      using (var session = Domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        var invoices = session.Query.All<Invoice>().ToArray();
        var tester = new InheritorReferenceTypeTester(null);

        Domain.QueryCache.Clear();
        Assert.That(tester.TestMethodParameter(session, invoices[1]), Is.EqualTo(16.0m));
        Assert.That(tester.TestMethodParameter(session, invoices[0]), Is.EqualTo(6.0m));

        Assert.That(Domain.QueryCache.Count, Is.EqualTo(1));
      }
    }

    [Test]
    public void ForeachItemClosureTest()
    {
      using (var session = Domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        var tester = new InheritorValueTypeTester(-1);

        Domain.QueryCache.Clear();
        var results = tester.TestForeachItem(session);
        Assert.That(results.Length, Is.EqualTo(2));
        Assert.That(results[0], Is.EqualTo(16.0m));
        Assert.That(results[1], Is.EqualTo(6.0m));

        Assert.That(Domain.QueryCache.Count, Is.EqualTo(1));
      }
    }

    [Test]
    public void ForIndexClosureTest()
    {
      using (var session = Domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        var tester = new InheritorValueTypeTester(-1);

        Domain.QueryCache.Clear();
        var results = tester.TestForIndex(session);
        Assert.That(results.Length, Is.EqualTo(2));
        Assert.That(results[0], Is.EqualTo(16.0m));
        Assert.That(results[1], Is.EqualTo(6.0m));

        Assert.That(Domain.QueryCache.Count, Is.EqualTo(1));
      }
    }

    [Test]
    public void StaticFieldClosureTest()
    {
      using (var session = Domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        var tester = new InheritorValueTypeTester(-1);
        InheritorValueTypeTester.StaticField = idsPair.First;

        Domain.QueryCache.Clear();
        Assert.That(tester.TestStaticField(session), Is.EqualTo(6.0m));

        InheritorValueTypeTester.StaticField = idsPair.Second;
        Assert.That(tester.TestStaticField(session), Is.EqualTo(16.0m));

        Assert.That(Domain.QueryCache.Count, Is.EqualTo(1));
      }
    }

    [Test]
    public void PrivateValueFieldClosureTest()
    {
      using (var session = Domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        var first = new BaseValueTypeTester((long) idsPair.First) {PublicField = idsPair.First};
        var second = new BaseValueTypeTester((long) idsPair.Second) {PublicField = idsPair.Second};

        Domain.QueryCache.Clear();
        var firstResult = first.TestPrivateField(session);
        var secondResult = second.TestPrivateField(session);
        Assert.That(firstResult, Is.EqualTo(6.0m));
        Assert.That(secondResult, Is.EqualTo(16.0m));
        Assert.That(Domain.QueryCache.Count, Is.EqualTo(1));
      }
    }

    [Test]
    public void PublicValueFieldClosureTest()
    {
      using (var session = Domain.OpenSession())
      using (var tx = session.OpenTransaction())
      {
        var first = new BaseValueTypeTester((long)idsPair.First) { PublicField = idsPair.First };
        var second = new BaseValueTypeTester((long)idsPair.Second) { PublicField = idsPair.Second };

        Domain.QueryCache.Clear();
        var firstResult = first.TestPublicField(session);
        var secondResult = second.TestPublicField(session);
        Assert.That(firstResult, Is.EqualTo(6.0m));
        Assert.That(secondResult, Is.EqualTo(16.0m));
        Assert.That(Domain.QueryCache.Count, Is.EqualTo(1));
      }
    }

    [Test]
    public void PrivateValuePropertyClosureTest()
    {
      using (var session = Domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        var first = new BaseValueTypeTester(idsPair.First) { PublicProperty = idsPair.First };
        var second = new BaseValueTypeTester(idsPair.Second) { PublicProperty = idsPair.Second };

        Domain.QueryCache.Clear();
        var firstResult = first.TestPrivateProperty(session);
        var secondResult = second.TestPrivateProperty(session);
        Assert.That(firstResult, Is.EqualTo(6.0m));
        Assert.That(secondResult, Is.EqualTo(16.0m));
        Assert.That(Domain.QueryCache.Count, Is.EqualTo(1));
      }
    }

    [Test]
    public void PublicValuePropertyClosureTest()
    {
      using (var session = Domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        var first = new BaseValueTypeTester(idsPair.First) { PublicProperty = idsPair.First };
        var second = new BaseValueTypeTester(idsPair.Second) { PublicProperty = idsPair.Second };

        Domain.QueryCache.Clear();
        var firstResult = first.TestPublicProperty(session);
        var secondResult = second.TestPublicProperty(session);
        Assert.That(firstResult, Is.EqualTo(6.0m));
        Assert.That(secondResult, Is.EqualTo(16.0m));
        Assert.That(Domain.QueryCache.Count, Is.EqualTo(1));
      }
    }

    [Test]
    public void InhertitedValueFieldClosureTest()
    {
      using (var session = Domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        var first = new InheritorValueTypeTester((long) idsPair.First) {PublicField = idsPair.First};
        var second = new InheritorValueTypeTester((long) idsPair.Second) {PublicField = idsPair.Second};

        Domain.QueryCache.Clear();
        var firstResult = first.TestInheritedPublicField(session);
        var secondResult = second.TestInheritedPublicField(session);
        Assert.That(firstResult, Is.EqualTo(6.0m));
        Assert.That(secondResult, Is.EqualTo(16.0m));
        Assert.That(Domain.QueryCache.Count, Is.EqualTo(1));
      }
    }

    [Test]
    public void InheritedValuePropertyClosureTest()
    {
      using (var session = Domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        var first = new InheritorValueTypeTester(idsPair.First) {PublicProperty = idsPair.First};
        var second = new InheritorValueTypeTester(idsPair.Second) {PublicProperty = idsPair.Second};

        Domain.QueryCache.Clear();
        var firstResult = first.TestInheritedPublicProperty(session);
        var secondResult = second.TestInheritedPublicProperty(session);
        Assert.That(firstResult, Is.EqualTo(6.0m));
        Assert.That(secondResult, Is.EqualTo(16.0m));
        Assert.That(Domain.QueryCache.Count, Is.EqualTo(1));
      }
    }

    [Test]
    public void NestedClassPrivateValueFieldClosureTest()
    {
      using (var session = Domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        var first = new BaseValueTypeTester.NestedValueTypeTester((long) idsPair.First) {PublicField = idsPair.First};
        var second = new BaseValueTypeTester.NestedValueTypeTester((long) idsPair.Second) {PublicField = idsPair.Second};

        Domain.QueryCache.Clear();
        var firstResult = first.TestPrivateField(session);
        var secondResult = second.TestPrivateField(session);
        Assert.That(firstResult, Is.EqualTo(6.0m));
        Assert.That(secondResult, Is.EqualTo(16.0m));
        Assert.That(Domain.QueryCache.Count, Is.EqualTo(1));
      }
    }

    [Test]
    public void NestedClassPublicValueFieldClosureTest()
    {
      using (var session = Domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        var first = new BaseValueTypeTester.NestedValueTypeTester((long) idsPair.First) {PublicField = idsPair.First};
        var second = new BaseValueTypeTester.NestedValueTypeTester((long) idsPair.Second) {PublicField = idsPair.Second};

        Domain.QueryCache.Clear();
        var firstResult = first.TestPublicField(session);
        var secondResult = second.TestPublicField(session);
        Assert.That(firstResult, Is.EqualTo(6.0m));
        Assert.That(secondResult, Is.EqualTo(16.0m));
        Assert.That(Domain.QueryCache.Count, Is.EqualTo(1));
      }
    }

    [Test]
    public void NestedClassPrivateValuePropertyClosureTest()
    {
      using (var session = Domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        var first = new BaseValueTypeTester.NestedValueTypeTester(idsPair.First) {PublicProperty = idsPair.First};
        var second = new BaseValueTypeTester.NestedValueTypeTester(idsPair.Second) {PublicProperty = idsPair.Second};

        Domain.QueryCache.Clear();
        var firstResult = first.TestPrivateProperty(session);
        var secondResult = second.TestPrivateProperty(session);
        Assert.That(firstResult, Is.EqualTo(6.0m));
        Assert.That(secondResult, Is.EqualTo(16.0m));
        Assert.That(Domain.QueryCache.Count, Is.EqualTo(1));
      }
    }

    [Test]
    public void NestedClassPublicValuePropertyClosureTest()
    {
      using (var session = Domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        var first = new BaseValueTypeTester.NestedValueTypeTester(idsPair.First) {PublicProperty = idsPair.First};
        var second = new BaseValueTypeTester.NestedValueTypeTester(idsPair.Second) {PublicProperty = idsPair.Second};

        Domain.QueryCache.Clear();
        var firstResult = first.TestPublicProperty(session);
        var secondResult = second.TestPublicProperty(session);
        Assert.That(firstResult, Is.EqualTo(6.0m));
        Assert.That(secondResult, Is.EqualTo(16.0m));
        Assert.That(Domain.QueryCache.Count, Is.EqualTo(1));
      }
    }

    [Test]
    public void PrivateRefFieldClosureTest()
    {
      using (var session = Domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        var invoices = session.Query.All<Invoice>().ToArray();

        var first = new BaseReferenceTypeTester(invoices[0]) {PublicField = invoices[0]};
        var second = new BaseReferenceTypeTester(invoices[1]) {PublicField = invoices[1]};

        Domain.QueryCache.Clear();
        var firstResult = first.TestPrivateField(session);
        var secondResult = second.TestPrivateField(session);
        Assert.That(firstResult, Is.EqualTo(6.0m));
        Assert.That(secondResult, Is.EqualTo(16.0m));
        Assert.That(Domain.QueryCache.Count, Is.EqualTo(1));
      }
    }

    [Test]
    public void PublicRefFieldClosureTest()
    {
      using (var session = Domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        var invoices = session.Query.All<Invoice>().ToArray();

        var first = new BaseReferenceTypeTester(invoices[0]) {PublicField = invoices[0]};
        var second = new BaseReferenceTypeTester(invoices[1]) {PublicField = invoices[1]};

        Domain.QueryCache.Clear();
        var firstResult = first.TestPublicField(session);
        var secondResult = second.TestPublicField(session);
        Assert.That(firstResult, Is.EqualTo(6.0m));
        Assert.That(secondResult, Is.EqualTo(16.0m));
        Assert.That(Domain.QueryCache.Count, Is.EqualTo(1));
      }
    }

    [Test]
    public void PrivateRefPropertyClosureTest()
    {
      using (var session = Domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        var invoices = session.Query.All<Invoice>().ToArray();

        var first = new BaseReferenceTypeTester(invoices[0]) {PublicProperty = invoices[0]};
        var second = new BaseReferenceTypeTester(invoices[1]) {PublicProperty = invoices[1]};

        Domain.QueryCache.Clear();
        var firstResult = first.TestPrivateProperty(session);
        var secondResult = second.TestPrivateProperty(session);
        Assert.That(firstResult, Is.EqualTo(6.0m));
        Assert.That(secondResult, Is.EqualTo(16.0m));
        Assert.That(Domain.QueryCache.Count, Is.EqualTo(1));
      }
    }

    [Test]
    public void PublicRefPropertyClosureTest()
    {
      using (var session = Domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        var invoices = session.Query.All<Invoice>().ToArray();

        var first = new BaseReferenceTypeTester(invoices[0]) {PublicProperty = invoices[0]};
        var second = new BaseReferenceTypeTester(invoices[1]) {PublicProperty = invoices[1]};

        Domain.QueryCache.Clear();
        var firstResult = first.TestPublicProperty(session);
        var secondResult = second.TestPublicProperty(session);
        Assert.That(firstResult, Is.EqualTo(6.0m));
        Assert.That(secondResult, Is.EqualTo(16.0m));
        Assert.That(Domain.QueryCache.Count, Is.EqualTo(1));
      }
    }

    [Test]
    public void InhertitedRefFieldClosureTest()
    {
      using (var session = Domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        var invoices = session.Query.All<Invoice>().ToArray();

        var first = new InheritorReferenceTypeTester(invoices[0]) {PublicField = invoices[0]};
        var second = new InheritorReferenceTypeTester(invoices[1]) {PublicField = invoices[1]};

        Domain.QueryCache.Clear();
        var firstResult = first.TestInheritedPublicField(session);
        var secondResult = second.TestInheritedPublicField(session);
        Assert.That(firstResult, Is.EqualTo(6.0m));
        Assert.That(secondResult, Is.EqualTo(16.0m));
        Assert.That(Domain.QueryCache.Count, Is.EqualTo(1));
      }
    }

    [Test]
    public void InheritedRefPropertyClosureTest()
    {
      using (var session = Domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        var invoices = session.Query.All<Invoice>().ToArray();

        var first = new InheritorReferenceTypeTester(invoices[0]) {PublicProperty = invoices[0]};
        var second = new InheritorReferenceTypeTester(invoices[1]) {PublicProperty = invoices[1]};

        Domain.QueryCache.Clear();
        var firstResult = first.TestInheritedPublicProperty(session);
        var secondResult = second.TestInheritedPublicProperty(session);
        Assert.That(firstResult, Is.EqualTo(6.0m));
        Assert.That(secondResult, Is.EqualTo(16.0m));
        Assert.That(Domain.QueryCache.Count, Is.EqualTo(1));
      }
    }

    [Test]
    public void NestedClassPrivateRefFieldClosureTest()
    {
      using (var session = Domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        var invoices = session.Query.All<Invoice>().ToArray();

        var first = new BaseReferenceTypeTester.NestedReferenceTypeTester(invoices[0]) {PublicField = invoices[0]};
        var second = new BaseReferenceTypeTester.NestedReferenceTypeTester(invoices[1]) {PublicField = invoices[1]};

        Domain.QueryCache.Clear();
        var firstResult = first.TestPrivateField(session);
        var secondResult = second.TestPrivateField(session);
        Assert.That(firstResult, Is.EqualTo(6.0m));
        Assert.That(secondResult, Is.EqualTo(16.0m));
        Assert.That(Domain.QueryCache.Count, Is.EqualTo(1));
      }
    }

    [Test]
    public void NestedClassPublicRefFieldClosureTest()
    {
      using (var session = Domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        var invoices = session.Query.All<Invoice>().ToArray();

        var first = new BaseReferenceTypeTester.NestedReferenceTypeTester(invoices[0]) {PublicField = invoices[0]};
        var second = new BaseReferenceTypeTester.NestedReferenceTypeTester(invoices[1]) {PublicField = invoices[1]};

        Domain.QueryCache.Clear();
        var firstResult = first.TestPublicField(session);
        var secondResult = second.TestPublicField(session);
        Assert.That(firstResult, Is.EqualTo(6.0m));
        Assert.That(secondResult, Is.EqualTo(16.0m));
        Assert.That(Domain.QueryCache.Count, Is.EqualTo(1));
      }
    }

    [Test]
    public void NestedClassPrivateRefPropertyClosureTest()
    {
      using (var session = Domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        var invoices = session.Query.All<Invoice>().ToArray();

        var first = new BaseReferenceTypeTester.NestedReferenceTypeTester(invoices[0]) {PublicProperty = invoices[0]};
        var second = new BaseReferenceTypeTester.NestedReferenceTypeTester(invoices[1]) {PublicProperty = invoices[1]};

        Domain.QueryCache.Clear();
        var firstResult = first.TestPrivateProperty(session);
        var secondResult = second.TestPrivateProperty(session);
        Assert.That(firstResult, Is.EqualTo(6.0m));
        Assert.That(secondResult, Is.EqualTo(16.0m));
        Assert.That(Domain.QueryCache.Count, Is.EqualTo(1));
      }
    }

    [Test]
    public void NestedClassPublicRefPropertyClosureTest()
    {
      using (var session = Domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        var invoices = session.Query.All<Invoice>().ToArray();

        var first = new BaseReferenceTypeTester.NestedReferenceTypeTester(invoices[0]) {PublicProperty = invoices[0]};
        var second = new BaseReferenceTypeTester.NestedReferenceTypeTester(invoices[1]) {PublicProperty = invoices[1]};

        Domain.QueryCache.Clear();
        var firstResult = first.TestPublicProperty(session);
        var secondResult = second.TestPublicProperty(session);
        Assert.That(firstResult, Is.EqualTo(6.0m));
        Assert.That(secondResult, Is.EqualTo(16.0m));
        Assert.That(Domain.QueryCache.Count, Is.EqualTo(1));
      }
    }
  }
}
