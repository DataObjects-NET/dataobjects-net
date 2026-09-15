module DateTimeCompilersTest

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
  member this.EqualityTest() =
    use session = base.Domain.OpenSession ()
    use ts = session.OpenTransaction ()
    X (DateTimeField = new System.DateTime(2025, 10, 9)) |> ignore
    let alll = session.Query.All<X> ()
    let query = 
      query {
        for x in alll do
          if (x.DateTimeField = new System.DateTime(2025, 10, 9)) then 
            yield x
      }
    let list = query |> Seq.toArray
    Assert.That(list.Length, Is.EqualTo(1))
    let fetched = list.[0]
    Assert.That(fetched.DateTimeField, Is.EqualTo(new System.DateTime(2025, 10, 9)))

  [<Test>]
  member this.InequalityTest() =
    use session = base.Domain.OpenSession ()
    use ts = session.OpenTransaction ()
    X (DateTimeField = new System.DateTime(2025, 10, 9)) |> ignore
    let alll = session.Query.All<X> ()
    let query = 
      query {
        for x in alll do
          if (x.DateTimeField <> new System.DateTime(2025, 10, 8)) then 
            yield x
      }
    let list = query |> Seq.toArray
    Assert.That(list.Length, Is.EqualTo(1))
    let fetched = list.[0]
    Assert.That(fetched.DateTimeField, Is.EqualTo(new System.DateTime(2025, 10, 9)))

  [<Test>]
  member this.YearExtractorTest() =
    use session = base.Domain.OpenSession ()
    use ts = session.OpenTransaction ()
    X (DateTimeField = new System.DateTime(2025, 10, 9)) |> ignore
    let alll = session.Query.All<X> ()
    let query = 
      query {
        for x in alll do
          if (x.DateTimeField.Year = 2025) then 
            yield x
      }
    let list = query |> Seq.toArray
    Assert.That(list.Length, Is.EqualTo(1))
    let fetched = list.[0]
    Assert.That(fetched.DateTimeField, Is.EqualTo(new System.DateTime(2025, 10, 9)))

  [<Test>]
  member this.MonthExtractorTest() =
    use session = base.Domain.OpenSession ()
    use ts = session.OpenTransaction ()
    X (DateTimeField = new System.DateTime(2025, 10, 9)) |> ignore
    let alll = session.Query.All<X> ()
    let query = 
      query {
        for x in alll do
          if (x.DateTimeField.Month = 10) then 
            yield x
      }
    let list = query |> Seq.toArray
    Assert.That(list.Length, Is.EqualTo(1))
    let fetched = list.[0]
    Assert.That(fetched.DateTimeField, Is.EqualTo(new System.DateTime(2025, 10, 9)))

  [<Test>]
  member this.DayExtractorTest() =
    use session = base.Domain.OpenSession ()
    use ts = session.OpenTransaction ()
    X (DateTimeField = new System.DateTime(2025, 10, 9)) |> ignore
    let alll = session.Query.All<X> ()
    let query = 
      query {
        for x in alll do
          if (x.DateTimeField.Day = 9) then 
            yield x
      }
    let list = query |> Seq.toArray
    Assert.That(list.Length, Is.EqualTo(1))
    let fetched = list.[0]
    Assert.That(fetched.DateTimeField, Is.EqualTo(new System.DateTime(2025, 10, 9)))

  [<Test>]
  member this.AddYearTest() =
    use session = base.Domain.OpenSession ()
    use ts = session.OpenTransaction ()
    X (DateTimeField = new System.DateTime(2025, 10, 9)) |> ignore
    let alll = session.Query.All<X> ()
    let query = 
      query {
        for x in alll do
          if (x.DateTimeField.AddYears(2) = new System.DateTime(2027, 10, 9)) then 
            yield x
      }
    let list = query |> Seq.toArray
    Assert.That(list.Length, Is.EqualTo(1))
    let fetched = list.[0]
    Assert.That(fetched.DateTimeField, Is.EqualTo(new System.DateTime(2025, 10, 9)))

  [<Test>]
  member this.AddMonthTest() =
    use session = base.Domain.OpenSession ()
    use ts = session.OpenTransaction ()
    X (DateTimeField = new System.DateTime(2025, 10, 9)) |> ignore
    let alll = session.Query.All<X> ()
    let query = 
      query {
        for x in alll do
          if (x.DateTimeField.AddMonths(1) = new System.DateTime(2025, 11, 9)) then 
            yield x
      }
    let list = query |> Seq.toArray
    Assert.That(list.Length, Is.EqualTo(1))
    let fetched = list.[0]
    Assert.That(fetched.DateTimeField, Is.EqualTo(new System.DateTime(2025, 10, 9)))

  [<Test>]
  member this.AddDayTest() =
    use session = base.Domain.OpenSession ()
    use ts = session.OpenTransaction ()
    X (DateTimeField = new System.DateTime(2025, 10, 9)) |> ignore
    let alll = session.Query.All<X> ()
    let query = 
      query {
        for x in alll do
          if (x.DateTimeField.AddDays(3) = new System.DateTime(2025, 10, 12)) then 
            yield x
      }
    let list = query |> Seq.toArray
    Assert.That(list.Length, Is.EqualTo(1))
    let fetched = list.[0]
    Assert.That(fetched.DateTimeField, Is.EqualTo(new System.DateTime(2025, 10, 9)))