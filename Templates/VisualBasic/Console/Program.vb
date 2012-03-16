Imports $safeprojectname$.Model
Imports Xtensive.Orm
Imports Xtensive.Orm.Configuration

Module Program

    Sub Main(args As String())
      ' Loading configuration section for local SQL Server database.
      ' See other cases in App.config file.
      Dim config = DomainConfiguration.Load("Default")
      Dim myDomain = Domain.Build(config)

      Using session = myDomain.OpenSession()
         Using transactionScope = session.OpenTransaction()
            ' Creating new persistent object
             Dim helloWorld = New MyEntity(session) With { _
                .Text = "Hello World!" _
             }
             ' Committing transaction
             transactionScope.Complete()
         End Using
      End Using
      
      ' Reading all persisted objects from another Session
      Using session = myDomain.OpenSession()
          Using transactionScope = session.OpenTransaction()
              If session.Query.All(Of MyEntity)().Count() <> 1
                  Throw New InvalidOperationException()
              End If
              If session.Query.All(Of MyEntity)().First().Text <> "Hello World!"
                  Throw New InvalidOperationException()
              End If
              For Each myEntity In session.Query.All(Of MyEntity)()
                  System.Console.WriteLine(myEntity.Text)
              Next
              transactionScope.Complete()
          End Using
      End Using
      System.Console.ReadKey()
    End Sub
End Module
