module OtherOperationsCompilersTest

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
  member this.IsNullTest() =
    use session = base.Domain.OpenSession ()
    use ts = session.OpenTransaction ()
    X (BoolField = false) |> ignore
    let alll = session.Query.All<X> ()
    let query = 
      query {
        for x in alll do
          if isNull x.StringField then 
            yield x
      }
    let list = query |> Seq.toArray
    Assert.That(list.Length, Is.EqualTo(1))
    let fetched = list.[0]
    Assert.That(fetched.BoolField, Is.False)