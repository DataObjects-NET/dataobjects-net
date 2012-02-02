Imports Xtensive.Orm

Namespace $safeprojectname$
    <HierarchyRoot>
    Public Class MyEntity 
        Inherits Entity
  
        Public Sub New()
            MyBase.New()
        End Sub

        Public Sub New(ByVal session As Session)
            MyBase.New(session)
        End Sub

        <Field, Key>
        Public Property Id As Integer

        <Field>
        Public Property FirstName As String

        <Field>
        Public Property LastName As String

        <Field>
        Public Property Age As Integer

    End Class
End NameSpace