module MathCompilersTest

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
  member this.MinIntegersTest() =
    use session = base.Domain.OpenSession ()
    use ts = session.OpenTransaction ()
    X (Int16Field = -5s, Int32Field = -6, Int64Field = -7L) |> ignore
    let alll = session.Query.All<X> ()
    let query = 
      query {
        for x in alll do
          if ((min x.Int16Field -7s) = -7s) && ((min x.Int32Field  -7) = -7) && ((min x.Int64Field -8L) = -8L) then 
            yield x
      }
    let list = query |> Seq.toArray
    Assert.That(list.Length, Is.EqualTo(1))
    let fetched = list.[0]
    Assert.That(fetched.Int16Field, Is.EqualTo(-5s))
    Assert.That(fetched.Int32Field, Is.EqualTo(-6))
    Assert.That(fetched.Int64Field, Is.EqualTo(-7L))

  [<Test>]
  member this.MinDoubleTest() =
    use session = base.Domain.OpenSession ()
    use ts = session.OpenTransaction ()
    X (DoubleField = -6.00) |> ignore
    let alll = session.Query.All<X> ()
    let query = 
      query {
        for x in alll do
          if (min x.DoubleField -7.00) = -7.00 then 
            yield x
      }
    let list = query |> Seq.toArray
    Assert.That(list.Length, Is.EqualTo(1))
    let fetched = list.[0]
    Assert.That(fetched.DoubleField, Is.EqualTo(-6.00))

  [<Test>]
  member this.MaxIntegersTest() =
    use session = base.Domain.OpenSession ()
    use ts = session.OpenTransaction ()
    X (Int16Field = -5s, Int32Field = -6, Int64Field = -7L) |> ignore
    let alll = session.Query.All<X> ()
    let query = 
      query {
        for x in alll do
          if ((max x.Int16Field -7s) = -5s) && ((max x.Int32Field  -7) = -6) && ((max x.Int64Field -8L) = -7L) then 
            yield x
      }
    let list = query |> Seq.toArray
    Assert.That(list.Length, Is.EqualTo(1))
    let fetched = list.[0]
    Assert.That(fetched.Int16Field, Is.EqualTo(-5s))
    Assert.That(fetched.Int32Field, Is.EqualTo(-6))
    Assert.That(fetched.Int64Field, Is.EqualTo(-7L))

  [<Test>]
  member this.MaxDoubleTest() =
    use session = base.Domain.OpenSession ()
    use ts = session.OpenTransaction ()
    X (DoubleField = -6.00) |> ignore
    let alll = session.Query.All<X> ()
    let query = 
      query {
        for x in alll do
          if (max x.DoubleField -7.00) = -6.00 then 
            yield x
      }
    let list = query |> Seq.toArray
    Assert.That(list.Length, Is.EqualTo(1))
    let fetched = list.[0]
    Assert.That(fetched.DoubleField, Is.EqualTo(-6.00))