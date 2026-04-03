module ArithmeticOperationsCompilersTest

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
  member this.AdditionStringTest() =
    use session = base.Domain.OpenSession ()
    use ts = session.OpenTransaction ()
    X (StringField = "John") |> ignore
    let alll = session.Query.All<X> ()
    let query = 
      query {
        for x in alll do
          if (x.StringField + x.StringField) = "JohnJohn" then 
            yield x
      }
    let list = query |> Seq.toArray
    Assert.That(list.Length, Is.EqualTo(1))
    let fetched = list.[0]
    Assert.That(fetched.StringField, Is.EqualTo("John"))

  [<Test>]
  member this.AdditionDateTimeTest() =
    use session = base.Domain.OpenSession ()
    use ts = session.OpenTransaction ()
    X (DateTimeField = new System.DateTime(System.DateTime.Now.Year, 2, 15)) |> ignore
    let alll = session.Query.All<X> ()
    let query = 
      query {
        for x in alll do
          if (x.DateTimeField + System.TimeSpan.FromDays(2)) > new System.DateTime(System.DateTime.Now.Year, 2, 15) then 
            yield x
      }
    let list = query |> Seq.toArray
    Assert.That(list.Length, Is.EqualTo(1))
    let fetched = list.[0]
    Assert.That(fetched.DateTimeField, Is.EqualTo(new System.DateTime(System.DateTime.Now.Year, 2, 15)))

  [<Test>]
  member this.AdditionTimeSpanTest() =
    use session = base.Domain.OpenSession ()
    use ts = session.OpenTransaction ()
    X (TimeSpanField = System.TimeSpan.FromTicks(111222333)) |> ignore
    let alll = session.Query.All<X> ()
    let query = 
      query {
        for x in alll do
          if (x.TimeSpanField + System.TimeSpan.FromDays(2)) > System.TimeSpan.FromTicks(111222333) + System.TimeSpan.FromDays(1) then 
            yield x
      }
    let list = query |> Seq.toArray
    Assert.That(list.Length, Is.EqualTo(1))
    let fetched = list.[0]
    Assert.That(fetched.TimeSpanField, Is.EqualTo(System.TimeSpan.FromTicks(111222333)))

  [<Test>]
  member this.AdditionDecimalTest() =
    use session = base.Domain.OpenSession ()
    use ts = session.OpenTransaction ()
    X (DecimalField = decimal 16.0) |> ignore
    let alll = session.Query.All<X> ()
    let query = 
      query {
        for x in alll do
          if x.DecimalField + x.DecimalField <= decimal 35 then 
            yield x
      }
    let list = query |> Seq.toArray
    Assert.That(list.Length, Is.EqualTo(1))
    let fetched = list.[0]
    Assert.That(fetched.DecimalField, Is.EqualTo(decimal 16))

  [<Test>]
  member this.SubtractionDateTimeTest1() =
    use session = base.Domain.OpenSession ()
    use ts = session.OpenTransaction ()
    X (DateTimeField = new System.DateTime(System.DateTime.Now.Year, 2, 15)) |> ignore
    let alll = session.Query.All<X> ()
    let query = 
      query {
        for x in alll do
          if (x.DateTimeField - System.TimeSpan.FromDays(2)) > new System.DateTime(System.DateTime.Now.Year, 2, 10) then 
            yield x
      }
    let list = query |> Seq.toArray
    Assert.That(list.Length, Is.EqualTo(1))
    let fetched = list.[0]
    Assert.That(fetched.DateTimeField, Is.EqualTo(new System.DateTime(System.DateTime.Now.Year, 2, 15)))

  [<Test>]
  member this.SubtractionDateTimeTest2() =
    use session = base.Domain.OpenSession ()
    use ts = session.OpenTransaction ()
    X (DateTimeField = new System.DateTime(System.DateTime.Now.Year, 2, 15)) |> ignore
    let alll = session.Query.All<X> ()
    let query = 
      query {
        for x in alll do
          if (x.DateTimeField - new System.DateTime(System.DateTime.Now.Year, 2, 10)) > System.TimeSpan.FromDays(2) then 
            yield x
      }
    let list = query |> Seq.toArray
    Assert.That(list.Length, Is.EqualTo(1))
    let fetched = list.[0]
    Assert.That(fetched.DateTimeField, Is.EqualTo(new System.DateTime(System.DateTime.Now.Year, 2, 15)))

  [<Test>]
  member this.SubtractionTimeSpanTest() =
    use session = base.Domain.OpenSession ()
    use ts = session.OpenTransaction ()
    X (TimeSpanField = System.TimeSpan.FromTicks(111222333)) |> ignore
    let alll = session.Query.All<X> ()
    let query = 
      query {
        for x in alll do
          if (x.TimeSpanField - System.TimeSpan.FromTicks(111222)) > System.TimeSpan.FromTicks(111222) then 
            yield x
      }
    let list = query |> Seq.toArray
    Assert.That(list.Length, Is.EqualTo(1))
    let fetched = list.[0]
    Assert.That(fetched.TimeSpanField, Is.EqualTo(System.TimeSpan.FromTicks(111222333)))

  [<Test>]
  member this.SubtractionDecimalTest() =
    use session = base.Domain.OpenSession ()
    use ts = session.OpenTransaction ()
    X (DecimalField = decimal 16.0) |> ignore
    let alll = session.Query.All<X> ()
    let query = 
      query {
        for x in alll do
          if x.DecimalField - x.DecimalField + decimal 3 <= decimal 5 then 
            yield x
      }
    let list = query |> Seq.toArray
    Assert.That(list.Length, Is.EqualTo(1))
    let fetched = list.[0]
    Assert.That(fetched.DecimalField, Is.EqualTo(decimal 16))

  [<Test>]
  member this.MultiplyDecimalTest() =
    use session = base.Domain.OpenSession ()
    use ts = session.OpenTransaction ()
    X (DecimalField = decimal 16.0) |> ignore
    let alll = session.Query.All<X> ()
    let query = 
      query {
        for x in alll do
          if x.DecimalField * x.DecimalField <= decimal 350 then 
            yield x
      }
    let list = query |> Seq.toArray
    Assert.That(list.Length, Is.EqualTo(1))
    let fetched = list.[0]
    Assert.That(fetched.DecimalField, Is.EqualTo(decimal 16))

  [<Test>]
  member this.DivideDecimalTest() =
    use session = base.Domain.OpenSession ()
    use ts = session.OpenTransaction ()
    X (DecimalField = decimal 16.0) |> ignore
    let alll = session.Query.All<X> ()
    let query = 
      query {
        for x in alll do
          if x.DecimalField / decimal 2 <= decimal 16 then 
            yield x
      }
    let list = query |> Seq.toArray
    Assert.That(list.Length, Is.EqualTo(1))
    let fetched = list.[0]
    Assert.That(fetched.DecimalField, Is.EqualTo(decimal 16))
