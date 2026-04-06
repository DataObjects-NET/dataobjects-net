module ComparisonCompilersTest

open FsUnit
open NUnit.Framework
open Xtensive.Orm
open Xtensive.Orm.Tests
open Model
open Microsoft.FSharp.Linq

[<TestFixture>]
type Fixture() =
  inherit AutoBuildTest()

  override this.BuildConfiguration() =
    let config = base.BuildConfiguration ()
    config.Types.Register typeof<X>
    config

  [<Test>]
  member this.EqualityStringTest() =
    use session = base.Domain.OpenSession ()
    use ts = session.OpenTransaction ()
    X (StringField = "John") |> ignore
    let alll = session.Query.All<X> ()
    let query = 
      query {
        for x in alll do
          if x.StringField = "John" then 
            yield x
      }
    let list = query |> Seq.toArray
    Assert.That(list.Length, Is.EqualTo(1))
    let fetched = list.[0]
    Assert.That(fetched.StringField, Is.EqualTo("John"))
  
  [<Test>]
  member this.EqualityBooleanTest() =
    use session = base.Domain.OpenSession ()
    use ts = session.OpenTransaction ()
    X (BoolField = true) |> ignore
    let alll = session.Query.All<X> ()
    let query = 
      query {
        for x in alll do
          if x.BoolField = true then 
            yield x
      }
    let list = query |> Seq.toArray
    Assert.That(list.Length, Is.EqualTo(1))
    let fetched = list.[0]
    Assert.That(fetched.BoolField, Is.True)

  [<Test>]
  member this.EqualityInt16Test() =
    use session = base.Domain.OpenSession ()
    use ts = session.OpenTransaction ()
    X (Int16Field = 16s) |> ignore
    let alll = session.Query.All<X> ()
    let query = 
      query {
        for x in alll do
          if x.Int16Field = 16s then 
            yield x
      }
    let list = query |> Seq.toArray
    Assert.That(list.Length, Is.EqualTo(1))
    let fetched = list.[0]
    Assert.That(fetched.Int16Field, Is.EqualTo(16s))

  [<Test>]
  member this.EqualityUInt16Test() =
    use session = base.Domain.OpenSession ()
    use ts = session.OpenTransaction ()
    X ( UInt16Field = 16us) |> ignore
    let alll = session.Query.All<X> ()
    let query = 
      query {
        for x in alll do
          if x.UInt16Field = 16us then 
            yield x
      }
    let list = query |> Seq.toArray
    Assert.That(list.Length, Is.EqualTo(1))
    let fetched = list.[0]
    Assert.That(fetched.UInt16Field, Is.EqualTo(16us))

  [<Test>]
  member this.EqualityInt32Test() =
    use session = base.Domain.OpenSession ()
    use ts = session.OpenTransaction ()
    X (Int32Field = 32) |> ignore
    let alll = session.Query.All<X> ()
    let query = 
      query {
        for x in alll do
          if x.Int32Field = 32 then 
            yield x
      }
    let list = query |> Seq.toArray
    Assert.That(list.Length, Is.EqualTo(1))
    let fetched = list.[0]
    Assert.That(fetched.Int32Field, Is.EqualTo(32))

  [<Test>]
  member this.EqualityUInt32Test() =
    use session = base.Domain.OpenSession ()
    use ts = session.OpenTransaction ()
    X (UInt32Field = 32u) |> ignore
    let alll = session.Query.All<X> ()
    let query = 
      query {
        for x in alll do
          if x.UInt32Field = 32u then 
            yield x
      }
    let list = query |> Seq.toArray
    Assert.That(list.Length, Is.EqualTo(1))
    let fetched = list.[0]
    Assert.That(fetched.UInt32Field, Is.EqualTo(32u))

  [<Test>]
  member this.EqualityInt64Test() =
    use session = base.Domain.OpenSession ()
    use ts = session.OpenTransaction ()
    X (Int64Field = 64L) |> ignore
    let alll = session.Query.All<X> ()
    let query = 
      query {
        for x in alll do
          if x.Int64Field = 64L then 
            yield x
      }
    let list = query |> Seq.toArray
    Assert.That(list.Length, Is.EqualTo(1))
    let fetched = list.[0]
    Assert.That(fetched.Int64Field, Is.EqualTo(64L))

  [<Test>]
  member this.EqualityUInt64Test() =
    use session = base.Domain.OpenSession ()
    use ts = session.OpenTransaction ()
    X (UInt64Field = 64UL) |> ignore
    let alll = session.Query.All<X> ()
    let query = 
      query {
        for x in alll do
          if x.UInt64Field = 64UL then 
            yield x
      }
    let list = query |> Seq.toArray
    Assert.That(list.Length, Is.EqualTo(1))
    let fetched = list.[0]
    Assert.That(fetched.UInt64Field, Is.EqualTo(64UL))

  [<Test>]
  member this.EqualitySingleTest() =
    Require.ProviderIsNot (StorageProvider.Sqlite ||| StorageProvider.Firebird ||| StorageProvider.MySql)
    use session = base.Domain.OpenSession ()
    use ts = session.OpenTransaction ()
    X (SingleField = 1.14F) |> ignore
    let alll = session.Query.All<X> ()
    let query = 
      query {
        for x in alll do
          if x.SingleField = 1.14F then 
            yield x
      }
    let list = query |> Seq.toArray
    Assert.That(list.Length, Is.EqualTo(1))
    let fetched = list.[0]
    Assert.That(fetched.SingleField, Is.EqualTo(1.14F))

  [<Test>]
  member this.EqualityDoubleTest() =
    use session = base.Domain.OpenSession ()
    use ts = session.OpenTransaction ()
    X (DoubleField = 64.0) |> ignore
    let alll = session.Query.All<X> ()
    let query = 
      query {
        for x in alll do
          if x.DoubleField = 64.0 then 
            yield x
      }
    let list = query |> Seq.toArray
    Assert.That(list.Length, Is.EqualTo(1))
    let fetched = list.[0]
    Assert.That(fetched.DoubleField, Is.EqualTo(64.0))

  [<Test>]
  member this.EqualityDecimalTest() =
    use session = base.Domain.OpenSession ()
    use ts = session.OpenTransaction ()
    X (DecimalField = decimal 64.0) |> ignore
    let alll = session.Query.All<X> ()
    let query = 
      query {
        for x in alll do
          if x.DecimalField = decimal 64.0 then 
            yield x
      }
    let list = query |> Seq.toArray
    Assert.That(list.Length, Is.EqualTo(1))
    let fetched = list.[0]
    Assert.That(fetched.DecimalField, Is.EqualTo(decimal 64.0))


  [<Test>]
  member this.InequalityStringTest() =
    use session = base.Domain.OpenSession ()
    use ts = session.OpenTransaction ()
    X (StringField = "Not John") |> ignore
    let alll = session.Query.All<X> ()
    let query = 
      query {
        for x in alll do
          if x.StringField <> "John" then 
            yield x
      }
    let list = query |> Seq.toArray
    Assert.That(list.Length, Is.EqualTo(1))
    let fetched = list.[0]
    Assert.That(fetched.StringField, Is.EqualTo("Not John"))

  [<Test>]
  member this.InequalityDateTimeTest() =
    use session = base.Domain.OpenSession ()
    use ts = session.OpenTransaction ()
    X (DateTimeField = System.DateTime.Now.Date.AddDays(-1)) |> ignore
    let alll = session.Query.All<X> ()
    let query = 
      query {
        for x in alll do
          if x.DateTimeField <> System.DateTime.Now then 
            yield x
      }
    let list = query |> Seq.toArray
    Assert.That(list.Length, Is.EqualTo(1))
    let fetched = list.[0]
    Assert.That(fetched.DateTimeField, Is.EqualTo(System.DateTime.Now.Date.AddDays(-1)))

  [<Test>]
  member this.InequalityBooleanTest() =
    use session = base.Domain.OpenSession ()
    use ts = session.OpenTransaction ()
    X (BoolField = true) |> ignore
    let alll = session.Query.All<X> ()
    let query = 
      query {
        for x in alll do
          if x.BoolField <> false then 
            yield x
      }
    let list = query |> Seq.toArray
    Assert.That(list.Length, Is.EqualTo(1))
    let fetched = list.[0]
    Assert.That(fetched.BoolField, Is.True)

  [<Test>]
  member this.InequalityInt16Test() =
    use session = base.Domain.OpenSession ()
    use ts = session.OpenTransaction ()
    X (Int16Field = 16s) |> ignore
    let alll = session.Query.All<X> ()
    let query = 
      query {
        for x in alll do
          if x.Int16Field <> 15s then 
            yield x
      }
    let list = query |> Seq.toArray
    Assert.That(list.Length, Is.EqualTo(1))
    let fetched = list.[0]
    Assert.That(fetched.Int16Field, Is.EqualTo(16s))

  [<Test>]
  member this.InequalityUInt16Test() =
    use session = base.Domain.OpenSession ()
    use ts = session.OpenTransaction ()
    X ( UInt16Field = 16us) |> ignore
    let alll = session.Query.All<X> ()
    let query = 
      query {
        for x in alll do
          if x.UInt16Field <> 15us then 
            yield x
      }
    let list = query |> Seq.toArray
    Assert.That(list.Length, Is.EqualTo(1))
    let fetched = list.[0]
    Assert.That(fetched.UInt16Field, Is.EqualTo(16us))

  [<Test>]
  member this.InequalityInt32Test() =
    use session = base.Domain.OpenSession ()
    use ts = session.OpenTransaction ()
    X (Int32Field = 32) |> ignore
    let alll = session.Query.All<X> ()
    let query = 
      query {
        for x in alll do
          if x.Int32Field <> 31 then 
            yield x
      }
    let list = query |> Seq.toArray
    Assert.That(list.Length, Is.EqualTo(1))
    let fetched = list.[0]
    Assert.That(fetched.Int32Field, Is.EqualTo(32))

  [<Test>]
  member this.InequalityUInt32Test() =
    use session = base.Domain.OpenSession ()
    use ts = session.OpenTransaction ()
    X (UInt32Field = 32u) |> ignore
    let alll = session.Query.All<X> ()
    let query = 
      query {
        for x in alll do
          if x.UInt32Field <> 31u then 
            yield x
      }
    let list = query |> Seq.toArray
    Assert.That(list.Length, Is.EqualTo(1))
    let fetched = list.[0]
    Assert.That(fetched.UInt32Field, Is.EqualTo(32u))

  [<Test>]
  member this.InequalityInt64Test() =
    use session = base.Domain.OpenSession ()
    use ts = session.OpenTransaction ()
    X (Int64Field = 64L) |> ignore
    let alll = session.Query.All<X> ()
    let query = 
      query {
        for x in alll do
          if x.Int64Field <> 63L then 
            yield x
      }
    let list = query |> Seq.toArray
    Assert.That(list.Length, Is.EqualTo(1))
    let fetched = list.[0]
    Assert.That(fetched.Int64Field, Is.EqualTo(64L))

  [<Test>]
  member this.InequalityUInt64Test() =
    use session = base.Domain.OpenSession ()
    use ts = session.OpenTransaction ()
    X (UInt64Field = 64UL) |> ignore
    let alll = session.Query.All<X> ()
    let query = 
      query {
        for x in alll do
          if x.UInt64Field <> 63UL then 
            yield x
      }
    let list = query |> Seq.toArray
    Assert.That(list.Length, Is.EqualTo(1))
    let fetched = list.[0]
    Assert.That(fetched.UInt64Field, Is.EqualTo(64UL))

  [<Test>]
  member this.InequalitySingleTest() =
    use session = base.Domain.OpenSession ()
    use ts = session.OpenTransaction ()
    X (SingleField = 1.14F) |> ignore
    let alll = session.Query.All<X> ()
    let query = 
      query {
        for x in alll do
          if x.SingleField <> 1.18F then 
            yield x
      }
    let list = query |> Seq.toArray
    Assert.That(list.Length, Is.EqualTo(1))
    let fetched = list.[0]
    Assert.That(fetched.SingleField, Is.EqualTo(1.14F))

  [<Test>]
  member this.InequalityDoubleTest() =
    use session = base.Domain.OpenSession ()
    use ts = session.OpenTransaction ()
    X (DoubleField = 64.0) |> ignore
    let alll = session.Query.All<X> ()
    let query = 
      query {
        for x in alll do
          if x.DoubleField <> 63.0 then 
            yield x
      }
    let list = query |> Seq.toArray
    Assert.That(list.Length, Is.EqualTo(1))
    let fetched = list.[0]
    Assert.That(fetched.DoubleField, Is.EqualTo(64.0))

  [<Test>]
  member this.InequalityDecimalTest() =
    use session = base.Domain.OpenSession ()
    use ts = session.OpenTransaction ()
    X (DecimalField = decimal 64.0) |> ignore
    let alll = session.Query.All<X> ()
    let query = 
      query {
        for x in alll do
          if x.DecimalField <> decimal 63.0 then 
            yield x
      }
    let list = query |> Seq.toArray
    Assert.That(list.Length, Is.EqualTo(1))
    let fetched = list.[0]
    Assert.That(fetched.DecimalField, Is.EqualTo(decimal 64.0))


  [<Test>]
  member this.GreaterThanDateTimeTest() =
    use session = base.Domain.OpenSession ()
    use ts = session.OpenTransaction ()
    X (DateTimeField = System.DateTime.Now.Date.AddDays(-1)) |> ignore
    let alll = session.Query.All<X> ()
    let query = 
      query {
        for x in alll do
          if x.DateTimeField > System.DateTime.Now.Date.AddDays(-2) then 
            yield x
      }
    let list = query |> Seq.toArray
    Assert.That(list.Length, Is.EqualTo(1))
    let fetched = list.[0]
    Assert.That(fetched.DateTimeField, Is.EqualTo(System.DateTime.Now.Date.AddDays(-1)))

  [<Test>]
  member this.GreaterThanInt16Test() =
    use session = base.Domain.OpenSession ()
    use ts = session.OpenTransaction ()
    X (Int16Field = 16s) |> ignore
    let alll = session.Query.All<X> ()
    let query = 
      query {
        for x in alll do
          if x.Int16Field > 15s then 
            yield x
      }
    let list = query |> Seq.toArray
    Assert.That(list.Length, Is.EqualTo(1))
    let fetched = list.[0]
    Assert.That(fetched.Int16Field, Is.EqualTo(16s))

  [<Test>]
  member this.GreaterThanUInt16Test() =
    use session = base.Domain.OpenSession ()
    use ts = session.OpenTransaction ()
    X ( UInt16Field = 16us) |> ignore
    let alll = session.Query.All<X> ()
    let query = 
      query {
        for x in alll do
          if x.UInt16Field > 15us then 
            yield x
      }
    let list = query |> Seq.toArray
    Assert.That(list.Length, Is.EqualTo(1))
    let fetched = list.[0]
    Assert.That(fetched.UInt16Field, Is.EqualTo(16us))

  [<Test>]
  member this.GreaterThanInt32Test() =
    use session = base.Domain.OpenSession ()
    use ts = session.OpenTransaction ()
    X (Int32Field = 32) |> ignore
    let alll = session.Query.All<X> ()
    let query = 
      query {
        for x in alll do
          if x.Int32Field > 31 then 
            yield x
      }
    let list = query |> Seq.toArray
    Assert.That(list.Length, Is.EqualTo(1))
    let fetched = list.[0]
    Assert.That(fetched.Int32Field, Is.EqualTo(32))

  [<Test>]
  member this.GreaterThanUInt32Test() =
    use session = base.Domain.OpenSession ()
    use ts = session.OpenTransaction ()
    X (UInt32Field = 32u) |> ignore
    let alll = session.Query.All<X> ()
    let query = 
      query {
        for x in alll do
          if x.UInt32Field > 31u then 
            yield x
      }
    let list = query |> Seq.toArray
    Assert.That(list.Length, Is.EqualTo(1))
    let fetched = list.[0]
    Assert.That(fetched.UInt32Field, Is.EqualTo(32u))

  [<Test>]
  member this.GreaterThanInt64Test() =
    use session = base.Domain.OpenSession ()
    use ts = session.OpenTransaction ()
    X (Int64Field = 64L) |> ignore
    let alll = session.Query.All<X> ()
    let query = 
      query {
        for x in alll do
          if x.Int64Field > 63L then 
            yield x
      }
    let list = query |> Seq.toArray
    Assert.That(list.Length, Is.EqualTo(1))
    let fetched = list.[0]
    Assert.That(fetched.Int64Field, Is.EqualTo(64L))

  [<Test>]
  member this.GreaterThanUInt64Test() =
    use session = base.Domain.OpenSession ()
    use ts = session.OpenTransaction ()
    X (UInt64Field = 64UL) |> ignore
    let alll = session.Query.All<X> ()
    let query = 
      query {
        for x in alll do
          if x.UInt64Field > 63UL then 
            yield x
      }
    let list = query |> Seq.toArray
    Assert.That(list.Length, Is.EqualTo(1))
    let fetched = list.[0]
    Assert.That(fetched.UInt64Field, Is.EqualTo(64UL))

  [<Test>]
  member this.GreaterThanSingleTest() =
    use session = base.Domain.OpenSession ()
    use ts = session.OpenTransaction ()
    X (SingleField = 1.14F) |> ignore
    let alll = session.Query.All<X> ()
    let query = 
      query {
        for x in alll do
          if x.SingleField > 1.11F then 
            yield x
      }
    let list = query |> Seq.toArray
    Assert.That(list.Length, Is.EqualTo(1))
    let fetched = list.[0]
    Assert.That(fetched.SingleField, Is.EqualTo(1.14F))

  [<Test>]
  member this.GreaterThanDoubleTest() =
    use session = base.Domain.OpenSession ()
    use ts = session.OpenTransaction ()
    X (DoubleField = 64.0) |> ignore
    let alll = session.Query.All<X> ()
    let query = 
      query {
        for x in alll do
          if x.DoubleField > 63.0 then 
            yield x
      }
    let list = query |> Seq.toArray
    Assert.That(list.Length, Is.EqualTo(1))
    let fetched = list.[0]
    Assert.That(fetched.DoubleField, Is.EqualTo(64.0))

  [<Test>]
  member this.GreaterThanDecimalTest() =
    use session = base.Domain.OpenSession ()
    use ts = session.OpenTransaction ()
    X (DecimalField = decimal 64.0) |> ignore
    let alll = session.Query.All<X> ()
    let query = 
      query {
        for x in alll do
          if x.DecimalField > decimal 63.0 then 
            yield x
      }
    let list = query |> Seq.toArray
    Assert.That(list.Length, Is.EqualTo(1))
    let fetched = list.[0]
    Assert.That(fetched.DecimalField, Is.EqualTo(decimal 64.0))


  [<Test>]
  member this.GreaterThanOrEqualDateTimeTest() =
    use session = base.Domain.OpenSession ()
    use ts = session.OpenTransaction ()
    X (DateTimeField = System.DateTime.Now.Date.AddDays(-1)) |> ignore
    let alll = session.Query.All<X> ()
    let query = 
      query {
        for x in alll do
          if x.DateTimeField >= System.DateTime.Now.Date.AddDays(-1) then 
            yield x
      }
    let list = query |> Seq.toArray
    Assert.That(list.Length, Is.EqualTo(1))
    let fetched = list.[0]
    Assert.That(fetched.DateTimeField, Is.EqualTo(System.DateTime.Now.Date.AddDays(-1)))

  [<Test>]
  member this.GreaterThanOrEqualInt16Test() =
    use session = base.Domain.OpenSession ()
    use ts = session.OpenTransaction ()
    X (Int16Field = 16s) |> ignore
    let alll = session.Query.All<X> ()
    let query = 
      query {
        for x in alll do
          if x.Int16Field >= 16s then 
            yield x
      }
    let list = query |> Seq.toArray
    Assert.That(list.Length, Is.EqualTo(1))
    let fetched = list.[0]
    Assert.That(fetched.Int16Field, Is.EqualTo(16s))

  [<Test>]
  member this.GreaterThanOrEqualUInt16Test() =
    use session = base.Domain.OpenSession ()
    use ts = session.OpenTransaction ()
    X ( UInt16Field = 16us) |> ignore
    let alll = session.Query.All<X> ()
    let query = 
      query {
        for x in alll do
          if x.UInt16Field >= 16us then 
            yield x
      }
    let list = query |> Seq.toArray
    Assert.That(list.Length, Is.EqualTo(1))
    let fetched = list.[0]
    Assert.That(fetched.UInt16Field, Is.EqualTo(16us))

  [<Test>]
  member this.GreaterThanOrEqualInt32Test() =
    use session = base.Domain.OpenSession ()
    use ts = session.OpenTransaction ()
    X (Int32Field = 32) |> ignore
    let alll = session.Query.All<X> ()
    let query = 
      query {
        for x in alll do
          if x.Int32Field >= 32 then 
            yield x
      }
    let list = query |> Seq.toArray
    Assert.That(list.Length, Is.EqualTo(1))
    let fetched = list.[0]
    Assert.That(fetched.Int32Field, Is.EqualTo(32))

  [<Test>]
  member this.GreaterThanOrEqualUInt32Test() =
    use session = base.Domain.OpenSession ()
    use ts = session.OpenTransaction ()
    X (UInt32Field = 32u) |> ignore
    let alll = session.Query.All<X> ()
    let query = 
      query {
        for x in alll do
          if x.UInt32Field >= 32u then 
            yield x
      }
    let list = query |> Seq.toArray
    Assert.That(list.Length, Is.EqualTo(1))
    let fetched = list.[0]
    Assert.That(fetched.UInt32Field, Is.EqualTo(32u))

  [<Test>]
  member this.GreaterThanOrEqualInt64Test() =
    use session = base.Domain.OpenSession ()
    use ts = session.OpenTransaction ()
    X (Int64Field = 64L) |> ignore
    let alll = session.Query.All<X> ()
    let query = 
      query {
        for x in alll do
          if x.Int64Field >= 64L then 
            yield x
      }
    let list = query |> Seq.toArray
    Assert.That(list.Length, Is.EqualTo(1))
    let fetched = list.[0]
    Assert.That(fetched.Int64Field, Is.EqualTo(64L))

  [<Test>]
  member this.GreaterThanOrEqualUInt64Test() =
    use session = base.Domain.OpenSession ()
    use ts = session.OpenTransaction ()
    X (UInt64Field = 64UL) |> ignore
    let alll = session.Query.All<X> ()
    let query = 
      query {
        for x in alll do
          if x.UInt64Field >= 64UL then 
            yield x
      }
    let list = query |> Seq.toArray
    Assert.That(list.Length, Is.EqualTo(1))
    let fetched = list.[0]
    Assert.That(fetched.UInt64Field, Is.EqualTo(64UL))

  [<Test>]
  member this.GreaterThanOrEqualSingleTest() =
    use session = base.Domain.OpenSession ()
    use ts = session.OpenTransaction ()
    X (SingleField = 1.14F) |> ignore
    let alll = session.Query.All<X> ()
    let query = 
      query {
        for x in alll do
          if x.SingleField >= 1.11F then 
            yield x
      }
    let list = query |> Seq.toArray
    Assert.That(list.Length, Is.EqualTo(1))
    let fetched = list.[0]
    Assert.That(fetched.SingleField, Is.EqualTo(1.14F))

  [<Test>]
  member this.GreaterThanOrEqualDoubleTest() =
    use session = base.Domain.OpenSession ()
    use ts = session.OpenTransaction ()
    X (DoubleField = 64.0) |> ignore
    let alll = session.Query.All<X> ()
    let query = 
      query {
        for x in alll do
          if x.DoubleField >= 64.0 then 
            yield x
      }
    let list = query |> Seq.toArray
    Assert.That(list.Length, Is.EqualTo(1))
    let fetched = list.[0]
    Assert.That(fetched.DoubleField, Is.EqualTo(64.0))

  [<Test>]
  member this.GreaterThanOrEqualDecimalTest() =
    use session = base.Domain.OpenSession ()
    use ts = session.OpenTransaction ()
    X (DecimalField = decimal 64.0) |> ignore
    let alll = session.Query.All<X> ()
    let query = 
      query {
        for x in alll do
          if x.DecimalField >= decimal 64.0 then 
            yield x
      }
    let list = query |> Seq.toArray
    Assert.That(list.Length, Is.EqualTo(1))
    let fetched = list.[0]
    Assert.That(fetched.DecimalField, Is.EqualTo(decimal 64.0))


  [<Test>]
  member this.LessThanDateTimeTest() =
    use session = base.Domain.OpenSession ()
    use ts = session.OpenTransaction ()
    X (DateTimeField = System.DateTime.Now.Date.AddDays(-1)) |> ignore
    let alll = session.Query.All<X> ()
    let query = 
      query {
        for x in alll do
          if x.DateTimeField < System.DateTime.Now.Date.AddDays(1) then 
            yield x
      }
    let list = query |> Seq.toArray
    Assert.That(list.Length, Is.EqualTo(1))
    let fetched = list.[0]
    Assert.That(fetched.DateTimeField, Is.EqualTo(System.DateTime.Now.Date.AddDays(-1)))

  [<Test>]
  member this.LessThanInt16Test() =
    use session = base.Domain.OpenSession ()
    use ts = session.OpenTransaction ()
    X (Int16Field = 16s) |> ignore
    let alll = session.Query.All<X> ()
    let query = 
      query {
        for x in alll do
          if x.Int16Field < 17s then 
            yield x
      }
    let list = query |> Seq.toArray
    Assert.That(list.Length, Is.EqualTo(1))
    let fetched = list.[0]
    Assert.That(fetched.Int16Field, Is.EqualTo(16s))

  [<Test>]
  member this.LessThanUInt16Test() =
    use session = base.Domain.OpenSession ()
    use ts = session.OpenTransaction ()
    X ( UInt16Field = 16us) |> ignore
    let alll = session.Query.All<X> ()
    let query = 
      query {
        for x in alll do
          if x.UInt16Field < 17us then 
            yield x
      }
    let list = query |> Seq.toArray
    Assert.That(list.Length, Is.EqualTo(1))
    let fetched = list.[0]
    Assert.That(fetched.UInt16Field, Is.EqualTo(16us))

  [<Test>]
  member this.LessThanInt32Test() =
    use session = base.Domain.OpenSession ()
    use ts = session.OpenTransaction ()
    X (Int32Field = 32) |> ignore
    let alll = session.Query.All<X> ()
    let query = 
      query {
        for x in alll do
          if x.Int32Field < 33 then 
            yield x
      }
    let list = query |> Seq.toArray
    Assert.That(list.Length, Is.EqualTo(1))
    let fetched = list.[0]
    Assert.That(fetched.Int32Field, Is.EqualTo(32))

  [<Test>]
  member this.LessThanUInt32Test() =
    use session = base.Domain.OpenSession ()
    use ts = session.OpenTransaction ()
    X (UInt32Field = 32u) |> ignore
    let alll = session.Query.All<X> ()
    let query = 
      query {
        for x in alll do
          if x.UInt32Field < 33u then 
            yield x
      }
    let list = query |> Seq.toArray
    Assert.That(list.Length, Is.EqualTo(1))
    let fetched = list.[0]
    Assert.That(fetched.UInt32Field, Is.EqualTo(32u))

  [<Test>]
  member this.LessThanInt64Test() =
    use session = base.Domain.OpenSession ()
    use ts = session.OpenTransaction ()
    X (Int64Field = 64L) |> ignore
    let alll = session.Query.All<X> ()
    let query = 
      query {
        for x in alll do
          if x.Int64Field < 65L then 
            yield x
      }
    let list = query |> Seq.toArray
    Assert.That(list.Length, Is.EqualTo(1))
    let fetched = list.[0]
    Assert.That(fetched.Int64Field, Is.EqualTo(64L))

  [<Test>]
  member this.LessThanUInt64Test() =
    use session = base.Domain.OpenSession ()
    use ts = session.OpenTransaction ()
    X (UInt64Field = 64UL) |> ignore
    let alll = session.Query.All<X> ()
    let query = 
      query {
        for x in alll do
          if x.UInt64Field < 65UL then 
            yield x
      }
    let list = query |> Seq.toArray
    Assert.That(list.Length, Is.EqualTo(1))
    let fetched = list.[0]
    Assert.That(fetched.UInt64Field, Is.EqualTo(64UL))

  [<Test>]
  member this.LessThanSingleTest() =
    use session = base.Domain.OpenSession ()
    use ts = session.OpenTransaction ()
    X (SingleField = 1.14F) |> ignore
    let alll = session.Query.All<X> ()
    let query = 
      query {
        for x in alll do
          if x.SingleField < 1.18F then 
            yield x
      }
    let list = query |> Seq.toArray
    Assert.That(list.Length, Is.EqualTo(1))
    let fetched = list.[0]
    Assert.That(fetched.SingleField, Is.EqualTo(1.14F))

  [<Test>]
  member this.LessThanDoubleTest() =
    use session = base.Domain.OpenSession ()
    use ts = session.OpenTransaction ()
    X (DoubleField = 64.0) |> ignore
    let alll = session.Query.All<X> ()
    let query = 
      query {
        for x in alll do
          if x.DoubleField < 65.0 then 
            yield x
      }
    let list = query |> Seq.toArray
    Assert.That(list.Length, Is.EqualTo(1))
    let fetched = list.[0]
    Assert.That(fetched.DoubleField, Is.EqualTo(64.0))

  [<Test>]
  member this.LessThanDecimalTest() =
    use session = base.Domain.OpenSession ()
    use ts = session.OpenTransaction ()
    X (DecimalField = decimal 64.0) |> ignore
    let alll = session.Query.All<X> ()
    let query = 
      query {
        for x in alll do
          if x.DecimalField < decimal 65.0 then 
            yield x
      }
    let list = query |> Seq.toArray
    Assert.That(list.Length, Is.EqualTo(1))
    let fetched = list.[0]
    Assert.That(fetched.DecimalField, Is.EqualTo(decimal 64.0))


  [<Test>]
  member this.LessThanOrDateTimeTest() =
    use session = base.Domain.OpenSession ()
    use ts = session.OpenTransaction ()
    X (DateTimeField = System.DateTime.Now.Date.AddDays(-1)) |> ignore
    let alll = session.Query.All<X> ()
    let query = 
      query {
        for x in alll do
          if x.DateTimeField <= System.DateTime.Now.Date.AddDays(1) then 
            yield x
      }
    let list = query |> Seq.toArray
    Assert.That(list.Length, Is.EqualTo(1))
    let fetched = list.[0]
    Assert.That(fetched.DateTimeField, Is.EqualTo(System.DateTime.Now.Date.AddDays(-1)))

  [<Test>]
  member this.LessThanOrEqualInt16Test() =
    use session = base.Domain.OpenSession ()
    use ts = session.OpenTransaction ()
    X (Int16Field = 16s) |> ignore
    let alll = session.Query.All<X> ()
    let query = 
      query {
        for x in alll do
          if x.Int16Field <= 16s then 
            yield x
      }
    let list = query |> Seq.toArray
    Assert.That(list.Length, Is.EqualTo(1))
    let fetched = list.[0]
    Assert.That(fetched.Int16Field, Is.EqualTo(16s))

  [<Test>]
  member this.LessThanOrEqualUInt16Test() =
    use session = base.Domain.OpenSession ()
    use ts = session.OpenTransaction ()
    X ( UInt16Field = 16us) |> ignore
    let alll = session.Query.All<X> ()
    let query = 
      query {
        for x in alll do
          if x.UInt16Field <= 16us then 
            yield x
      }
    let list = query |> Seq.toArray
    Assert.That(list.Length, Is.EqualTo(1))
    let fetched = list.[0]
    Assert.That(fetched.UInt16Field, Is.EqualTo(16us))

  [<Test>]
  member this.LessThanOrEqualInt32Test() =
    use session = base.Domain.OpenSession ()
    use ts = session.OpenTransaction ()
    X (Int32Field = 32) |> ignore
    let alll = session.Query.All<X> ()
    let query = 
      query {
        for x in alll do
          if x.Int32Field <= 32 then 
            yield x
      }
    let list = query |> Seq.toArray
    Assert.That(list.Length, Is.EqualTo(1))
    let fetched = list.[0]
    Assert.That(fetched.Int32Field, Is.EqualTo(32))

  [<Test>]
  member this.LessThanOrEqualUInt32Test() =
    use session = base.Domain.OpenSession ()
    use ts = session.OpenTransaction ()
    X (UInt32Field = 32u) |> ignore
    let alll = session.Query.All<X> ()
    let query = 
      query {
        for x in alll do
          if x.UInt32Field <= 32u then 
            yield x
      }
    let list = query |> Seq.toArray
    Assert.That(list.Length, Is.EqualTo(1))
    let fetched = list.[0]
    Assert.That(fetched.UInt32Field, Is.EqualTo(32u))

  [<Test>]
  member this.LessThanOrEqualInt64Test() =
    use session = base.Domain.OpenSession ()
    use ts = session.OpenTransaction ()
    X (Int64Field = 64L) |> ignore
    let alll = session.Query.All<X> ()
    let query = 
      query {
        for x in alll do
          if x.Int64Field <= 64L then 
            yield x
      }
    let list = query |> Seq.toArray
    Assert.That(list.Length, Is.EqualTo(1))
    let fetched = list.[0]
    Assert.That(fetched.Int64Field, Is.EqualTo(64L))

  [<Test>]
  member this.LessThanOrEqualUInt64Test() =
    use session = base.Domain.OpenSession ()
    use ts = session.OpenTransaction ()
    X (UInt64Field = 64UL) |> ignore
    let alll = session.Query.All<X> ()
    let query = 
      query {
        for x in alll do
          if x.UInt64Field <= 64UL then 
            yield x
      }
    let list = query |> Seq.toArray
    Assert.That(list.Length, Is.EqualTo(1))
    let fetched = list.[0]
    Assert.That(fetched.UInt64Field, Is.EqualTo(64UL))

  [<Test>]
  member this.LessThanOrEqualSingleTest() =
    use session = base.Domain.OpenSession ()
    use ts = session.OpenTransaction ()
    X (SingleField = 1.14F) |> ignore
    let alll = session.Query.All<X> ()
    let query = 
      query {
        for x in alll do
          if x.SingleField <= 1.18F then 
            yield x
      }
    let list = query |> Seq.toArray
    Assert.That(list.Length, Is.EqualTo(1))
    let fetched = list.[0]
    Assert.That(fetched.SingleField, Is.EqualTo(1.14F))

  [<Test>]
  member this.LessThanOrEqualDoubleTest() =
    use session = base.Domain.OpenSession ()
    use ts = session.OpenTransaction ()
    X (DoubleField = 64.0) |> ignore
    let alll = session.Query.All<X> ()
    let query = 
      query {
        for x in alll do
          if x.DoubleField <= 64.0 then 
            yield x
      }
    let list = query |> Seq.toArray
    Assert.That(list.Length, Is.EqualTo(1))
    let fetched = list.[0]
    Assert.That(fetched.DoubleField, Is.EqualTo(64.0))

  [<Test>]
  member this.LessThanOrEqualDecimalTest() =
    use session = base.Domain.OpenSession ()
    use ts = session.OpenTransaction ()
    X (DecimalField = decimal 64.0) |> ignore
    let alll = session.Query.All<X> ()
    let query = 
      query {
        for x in alll do
          if x.DecimalField <= decimal 64.0 then 
            yield x
      }
    let list = query |> Seq.toArray
    Assert.That(list.Length, Is.EqualTo(1))
    let fetched = list.[0]
    Assert.That(fetched.DecimalField, Is.EqualTo(decimal 64.0))
