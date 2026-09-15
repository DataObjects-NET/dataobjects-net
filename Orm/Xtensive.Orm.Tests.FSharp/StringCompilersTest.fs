module StringCompilersTest

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
  member this.EqualityTest() =
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

  [<Test>]
  member this.InequalityTest() =
    use session = base.Domain.OpenSession ()
    use ts = session.OpenTransaction ()
    Person (Name = "Not John") |> ignore
    let persons = session.Query.All<Person> ()
    let query = 
      query {
        for p in persons do
          if p.Name <> "John" then 
            yield p
      }
    let list = query |> Seq.toArray
    Assert.That(list.Length, Is.EqualTo(1))
    let fetched = list.[0]
    Assert.That(fetched.Name, Is.EqualTo("Not John"))

  [<Test>]
  member this.StartsWithTest() =
    use session = base.Domain.OpenSession ()
    use ts = session.OpenTransaction ()
    Person (Name = "Not John") |> ignore
    let persons = session.Query.All<Person> ()
    let query = 
      query {
        for p in persons do
          if p.Name.StartsWith("Not") then 
            yield p
      }
    let list = query |> Seq.toArray
    Assert.That(list.Length, Is.EqualTo(1))
    let fetched = list.[0]
    Assert.That(fetched.Name, Is.EqualTo("Not John"))

  [<Test>]
  member this.EndsWithTest() =
    use session = base.Domain.OpenSession ()
    use ts = session.OpenTransaction ()
    Person (Name = "Not John") |> ignore
    let persons = session.Query.All<Person> ()
    let query = 
      query {
        for p in persons do
          if p.Name.EndsWith("John") then 
            yield p
      }
    let list = query |> Seq.toArray
    Assert.That(list.Length, Is.EqualTo(1))
    let fetched = list.[0]
    Assert.That(fetched.Name, Is.EqualTo("Not John"))

  [<Test>]
  member this.ContainsStrTest() =
    use session = base.Domain.OpenSession ()
    use ts = session.OpenTransaction ()
    Person (Name = "Not John") |> ignore
    let persons = session.Query.All<Person> ()
    let query = 
      query {
        for p in persons do
          if p.Name.Contains("oh") then 
            yield p
      }
    let list = query |> Seq.toArray
    Assert.That(list.Length, Is.EqualTo(1))
    let fetched = list.[0]
    Assert.That(fetched.Name, Is.EqualTo("Not John"))

  [<Test>]
  member this.ContainsCharTest() =
    use session = base.Domain.OpenSession ()
    use ts = session.OpenTransaction ()
    Person (Name = "Not John") |> ignore
    let persons = session.Query.All<Person> ()
    let query = 
      query {
        for p in persons do
          if p.Name.Contains('o') then 
            yield p
      }
    let list = query |> Seq.toArray
    Assert.That(list.Length, Is.EqualTo(1))
    let fetched = list.[0]
    Assert.That(fetched.Name, Is.EqualTo("Not John"))

  [<Test>]
  member this.SubstringTest1() =
    use session = base.Domain.OpenSession ()
    use ts = session.OpenTransaction ()
    Person (Name = "Not John") |> ignore
    let persons = session.Query.All<Person> ()
    let query = 
      query {
        for p in persons do
          if p.Name.Substring(1) = "ot John" then 
            yield p
      }
    let list = query |> Seq.toArray
    Assert.That(list.Length, Is.EqualTo(1))
    let fetched = list.[0]
    Assert.That(fetched.Name, Is.EqualTo("Not John"))

  [<Test>]
  member this.SubstringTest2() =
    use session = base.Domain.OpenSession ()
    use ts = session.OpenTransaction ()
    Person (Name = "Not John") |> ignore
    let persons = session.Query.All<Person> ()
    let query = 
      query {
        for p in persons do
          if p.Name.Substring(1, 4) = "ot J" then 
            yield p
      }
    let list = query |> Seq.toArray
    Assert.That(list.Length, Is.EqualTo(1))
    let fetched = list.[0]
    Assert.That(fetched.Name, Is.EqualTo("Not John"))

  [<Test>]
  member this.ToUpperTest() =
    use session = base.Domain.OpenSession ()
    use ts = session.OpenTransaction ()
    Person (Name = "Not John") |> ignore
    let persons = session.Query.All<Person> ()
    let query = 
      query {
        for p in persons do
          if p.Name.ToUpper() = "NOT JOHN" then 
            yield p
      }
    let list = query |> Seq.toArray
    Assert.That(list.Length, Is.EqualTo(1))
    let fetched = list.[0]
    Assert.That(fetched.Name, Is.EqualTo("Not John"))

  [<Test>]
  member this.ToLowerTest() =
    use session = base.Domain.OpenSession ()
    use ts = session.OpenTransaction ()
    Person (Name = "Not John") |> ignore
    let persons = session.Query.All<Person> ()
    let query = 
      query {
        for p in persons do
          if p.Name.ToLower() = "not john" then 
            yield p
      }
    let list = query |> Seq.toArray
    Assert.That(list.Length, Is.EqualTo(1))
    let fetched = list.[0]
    Assert.That(fetched.Name, Is.EqualTo("Not John"))

  [<Test>]
  member this.TrimTest() =
    Require.ProviderIsNot(StorageProvider.Firebird)
    use session = base.Domain.OpenSession ()
    use ts = session.OpenTransaction ()
    Person (Name = " John ") |> ignore
    let persons = session.Query.All<Person> ()
    let query = 
      query {
        for p in persons do
          if p.Name.Trim() = "John" then 
            yield p
      }
    let list = query |> Seq.toArray
    Assert.That(list.Length, Is.EqualTo(1))
    let fetched = list.[0]
    Assert.That(fetched.Name, Is.EqualTo(" John "))

  [<Test>]
  member this.TrimStartTest() =
    Require.ProviderIsNot(StorageProvider.Firebird)
    use session = base.Domain.OpenSession ()
    use ts = session.OpenTransaction ()
    Person (Name = " John ") |> ignore
    let persons = session.Query.All<Person> ()
    let query = 
      query {
        for p in persons do
          if p.Name.TrimStart() = "John " then 
            yield p
      }
    let list = query |> Seq.toArray
    Assert.That(list.Length, Is.EqualTo(1))
    let fetched = list.[0]
    Assert.That(fetched.Name, Is.EqualTo(" John "))

  [<Test>]
  member this.TrimEndTest() =
    Require.ProviderIsNot(StorageProvider.Firebird)
    use session = base.Domain.OpenSession ()
    use ts = session.OpenTransaction ()
    Person (Name = " John ") |> ignore
    let persons = session.Query.All<Person> ()
    let query = 
      query {
        for p in persons do
          if p.Name.TrimEnd() = " John" then 
            yield p
      }
    let list = query |> Seq.toArray
    Assert.That(list.Length, Is.EqualTo(1))
    let fetched = list.[0]
    Assert.That(fetched.Name, Is.EqualTo(" John "))

  [<Test>]
  member this.TrimCharsTest() =
    Require.ProviderIsNot(StorageProvider.SqlServer ||| StorageProvider.Firebird)
    use session = base.Domain.OpenSession ()
    use ts = session.OpenTransaction ()
    Person (Name = "!?John!?") |> ignore
    let persons = session.Query.All<Person> ()
    let query = 
      query {
        for p in persons do
          if p.Name.Trim('!', '?') = "John" then 
            yield p
      }
    let list = query |> Seq.toArray
    Assert.That(list.Length, Is.EqualTo(1))
    let fetched = list.[0]
    Assert.That(fetched.Name, Is.EqualTo("!?John!?"))

  [<Test>]
  member this.TrimStartCharsTest() =
    Require.ProviderIsNot(StorageProvider.SqlServer ||| StorageProvider.Firebird)
    use session = base.Domain.OpenSession ()
    use ts = session.OpenTransaction ()
    Person (Name = "!?John!?") |> ignore
    let persons = session.Query.All<Person> ()
    let query = 
      query {
        for p in persons do
          if p.Name.TrimStart('!', '?') = "John!?" then 
            yield p
      }
    let list = query |> Seq.toArray
    Assert.That(list.Length, Is.EqualTo(1))
    let fetched = list.[0]
    Assert.That(fetched.Name, Is.EqualTo("!?John!?"))

  [<Test>]
  member this.TrimEndCharsTest() =
    Require.ProviderIsNot(StorageProvider.SqlServer ||| StorageProvider.Firebird)
    use session = base.Domain.OpenSession ()
    use ts = session.OpenTransaction ()
    Person (Name = "!?John!?") |> ignore
    let persons = session.Query.All<Person> ()
    let query = 
      query {
        for p in persons do
          if p.Name.TrimEnd('!', '?') = "!?John" then 
            yield p
      }
    let list = query |> Seq.toArray
    Assert.That(list.Length, Is.EqualTo(1))
    let fetched = list.[0]
    Assert.That(fetched.Name, Is.EqualTo("!?John!?"))

  [<Test>]
  member this.ReplaceCharTest() =
    use session = base.Domain.OpenSession ()
    use ts = session.OpenTransaction ()
    Person (Name = "!?John!?") |> ignore
    let persons = session.Query.All<Person> ()
    let query = 
      query {
        for p in persons do
          if p.Name.Replace('o', 'b') = "!?Jbhn!?" then 
            yield p
      }
    let list = query |> Seq.toArray
    Assert.That(list.Length, Is.EqualTo(1))
    let fetched = list.[0]
    Assert.That(fetched.Name, Is.EqualTo("!?John!?"))

  [<Test>]
  member this.ReplaceStrTest() =
    use session = base.Domain.OpenSession ()
    use ts = session.OpenTransaction ()
    Person (Name = "!?John!?") |> ignore
    let persons = session.Query.All<Person> ()
    let query = 
      query {
        for p in persons do
          if p.Name.Replace("?J", "!J") = "!!John!?" then 
            yield p
      }
    let list = query |> Seq.toArray
    Assert.That(list.Length, Is.EqualTo(1))
    let fetched = list.[0]
    Assert.That(fetched.Name, Is.EqualTo("!?John!?"))

  [<Test>]
  member this.RemoveTest1() =
    use session = base.Domain.OpenSession ()
    use ts = session.OpenTransaction ()
    Person (Name = "!?John!?") |> ignore
    let persons = session.Query.All<Person> ()
    let query = 
      query {
        for p in persons do
          if p.Name.Remove(0, 2) = "John!?" then 
            yield p
      }
    let list = query |> Seq.toArray
    Assert.That(list.Length, Is.EqualTo(1))
    let fetched = list.[0]
    Assert.That(fetched.Name, Is.EqualTo("!?John!?"))

  [<Test>]
  member this.RemoveTest2() =
    use session = base.Domain.OpenSession ()
    use ts = session.OpenTransaction ()
    Person (Name = "!?John!?") |> ignore
    let persons = session.Query.All<Person> ()
    let query = 
      query {
        for p in persons do
          if p.Name.Remove(6) = "!?John" then 
            yield p
      }
    let list = query |> Seq.toArray
    Assert.That(list.Length, Is.EqualTo(1))
    let fetched = list.[0]
    Assert.That(fetched.Name, Is.EqualTo("!?John!?"))

  [<Test>]
  member this.PadLeftTest1() =
    use session = base.Domain.OpenSession ()
    use ts = session.OpenTransaction ()
    Person (Name = "!?John!?") |> ignore
    let persons = session.Query.All<Person> ()
    let query = 
      query {
        for p in persons do
          if p.Name.PadLeft(10) = "  !?John!?" then 
            yield p
      }
    let list = query |> Seq.toArray
    Assert.That(list.Length, Is.EqualTo(1))
    let fetched = list.[0]
    Assert.That(fetched.Name, Is.EqualTo("!?John!?"))

  [<Test>]
  member this.PadLeftTest2() =
    use session = base.Domain.OpenSession ()
    use ts = session.OpenTransaction ()
    Person (Name = "!?John!?") |> ignore
    let persons = session.Query.All<Person> ()
    let query = 
      query {
        for p in persons do
          if p.Name.PadLeft(10, '_') = "__!?John!?" then 
            yield p
      }
    let list = query |> Seq.toArray
    Assert.That(list.Length, Is.EqualTo(1))
    let fetched = list.[0]
    Assert.That(fetched.Name, Is.EqualTo("!?John!?"))

  [<Test>]
  member this.PadRightTest1() =
    use session = base.Domain.OpenSession ()
    use ts = session.OpenTransaction ()
    Person (Name = "!?John!?") |> ignore
    let persons = session.Query.All<Person> ()
    let query = 
      query {
        for p in persons do
          if p.Name.PadRight(10) = "!?John!?  " then 
            yield p
      }
    let list = query |> Seq.toArray
    Assert.That(list.Length, Is.EqualTo(1))
    let fetched = list.[0]
    Assert.That(fetched.Name, Is.EqualTo("!?John!?"))

  [<Test>]
  member this.PadRightTest2() =
    use session = base.Domain.OpenSession ()
    use ts = session.OpenTransaction ()
    Person (Name = "!?John!?") |> ignore
    let persons = session.Query.All<Person> ()
    let query = 
      query {
        for p in persons do
          if p.Name.PadRight(10, '_') = "!?John!?__" then 
            yield p
      }
    let list = query |> Seq.toArray
    Assert.That(list.Length, Is.EqualTo(1))
    let fetched = list.[0]
    Assert.That(fetched.Name, Is.EqualTo("!?John!?"))

  [<Test>]
  member this.IndexOfTest1() =
    Require.ProviderIsNot StorageProvider.Sqlite
    use session = base.Domain.OpenSession ()
    use ts = session.OpenTransaction ()
    Person (Name = "!?John!?") |> ignore
    let persons = session.Query.All<Person> ()
    let query = 
      query {
        for p in persons do
          if p.Name.IndexOf("!?") = 0 then 
            yield p
      }
    let list = query |> Seq.toArray
    Assert.That(list.Length, Is.EqualTo(1))
    let fetched = list.[0]
    Assert.That(fetched.Name, Is.EqualTo("!?John!?"))

  [<Test>]
  member this.IndexOfTest2() =
    Require.ProviderIsNot StorageProvider.Sqlite
    use session = base.Domain.OpenSession ()
    use ts = session.OpenTransaction ()
    Person (Name = "!?John!?") |> ignore
    let persons = session.Query.All<Person> ()
    let query = 
      query {
        for p in persons do
          if p.Name.IndexOf("!?", 3) = 6 then 
            yield p
      }
    let list = query |> Seq.toArray
    Assert.That(list.Length, Is.EqualTo(1))
    let fetched = list.[0]
    Assert.That(fetched.Name, Is.EqualTo("!?John!?"))

  [<Test>]
  member this.IndexOfTest3() =
    Require.ProviderIsNot StorageProvider.Sqlite
    use session = base.Domain.OpenSession ()
    use ts = session.OpenTransaction ()
    Person (Name = "!?John!?") |> ignore
    let persons = session.Query.All<Person> ()
    let query = 
      query {
        for p in persons do
          if p.Name.IndexOf('?') = 1 then 
            yield p
      }
    let list = query |> Seq.toArray
    Assert.That(list.Length, Is.EqualTo(1))
    let fetched = list.[0]
    Assert.That(fetched.Name, Is.EqualTo("!?John!?"))

  [<Test>]
  member this.IndexOfTest4() =
    Require.ProviderIsNot StorageProvider.Sqlite
    use session = base.Domain.OpenSession ()
    use ts = session.OpenTransaction ()
    Person (Name = "!?John!?") |> ignore
    let persons = session.Query.All<Person> ()
    let query = 
      query {
        for p in persons do
          if p.Name.IndexOf('?', 3) = 7 then 
            yield p
      }
    let list = query |> Seq.toArray
    Assert.That(list.Length, Is.EqualTo(1))
    let fetched = list.[0]
    Assert.That(fetched.Name, Is.EqualTo("!?John!?"))

  [<Test>]
  member this.LengthTest() =
    use session = base.Domain.OpenSession ()
    use ts = session.OpenTransaction ()
    Person (Name = "!?John!?") |> ignore
    let persons = session.Query.All<Person> ()
    let query = 
      query {
        for p in persons do
          if p.Name.Length = 8 then 
            yield p
      }
    let list = query |> Seq.toArray
    Assert.That(list.Length, Is.EqualTo(1))
    let fetched = list.[0]
    Assert.That(fetched.Name, Is.EqualTo("!?John!?"))