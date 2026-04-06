module QueryEntityBackTest

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
    config.Types.Register typeof<Person>
    config

  [<Test>]
  member this.
    ``Should persist an entity and query it back via Linq``() =
    use session = base.Domain.OpenSession ()
    use ts = session.OpenTransaction ()
    Person (Name = "John") |> ignore
    let persons = session.Query.All<Person> ()
    let query = 
      query {
        for p in persons do
          if p.Name = "John" then 
            yield p
      }
    let list = query |> Seq.toArray
    Assert.That(list.Length, Is.EqualTo(1))
    let fetched = list.[0]
    Assert.That(fetched.Name, Is.EqualTo("John"))