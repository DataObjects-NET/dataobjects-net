
Imports Xtensive.Orm.Configuration
Imports NUnit.Framework

Namespace Model
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
      Using session As Session = Domain.OpenSession()
        Using scope As TransactionScope = session.OpenTransaction()
          Dim author = New Author With {.Name = "Vasya"}
          Dim book = New Book With {.Name = "The Book", .Author = author}
          Assert.AreEqual(1, author.Books.Count)
          Dim result = From c In Query.All(Of Author)()
                Select c
          Dim list = result.ToList()
          Dim loaded = result.Single()
          Assert.AreEqual("Vasya", loaded.Name)
        End Using
      End Using
    End Sub
  End Class
End Namespace
