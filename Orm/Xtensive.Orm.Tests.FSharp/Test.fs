module Test

open FsUnit
open NUnit.Framework
open Xtensive.Orm
open Xtensive.Orm.Tests
open Model
open Microsoft.FSharp.Linq
open Microsoft.FSharp.Linq.Query

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
      query <@ seq {
        for p in persons do
          if p.Name = "John" then 
            yield p
      } @>
    let list = query |> Seq.toArray
    Assert.AreEqual(1, list.Length)
    let fetched = list.[0]
    Assert.AreEqual ("John", fetched.Name)

