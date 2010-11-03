Imports Xtensive.Storage.Tests.VB.Model
Imports Xtensive.Storage.Tests.ObjectModel
Imports Xtensive.Storage.Configuration
Imports NUnit.Framework

Namespace Linq
  <TestFixture()>
  Public Class PersistenceTest
    Inherits AutoBuildTest

    Protected Overrides Function BuildConfiguration() As DomainConfiguration
      Dim config = MyBase.BuildConfiguration()
      config.Types.Register(GetType(Author).Assembly, GetType(Author).Namespace)

      Return config
    End Function


    <Test()>
    Public Sub MainTest()
      Using Session as Session = Session.Open(Domain)
        Using Scope as TransactionScope = Transaction.Open (Session)
          Dim Author = New Author With {.Name = "Vasya"}
          Dim Book = New Book With {.Name = "The Book", .Author = Author}
          Assert.AreEqual(1, Author.Books.Count)
          Dim result = From c In Query.All(Of Author)
                       Select c
          Dim list = result.ToList()
          Dim loaded = result.Single()
          Assert.AreEqual("Vasya", loaded.Name)
        End Using
      End Using
    End Sub

  End Class
End Namespace
