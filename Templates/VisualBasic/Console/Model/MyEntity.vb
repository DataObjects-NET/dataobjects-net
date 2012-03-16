Imports Xtensive.Orm

Namespace Model
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

        <Field(Length := 100)>
        Public Property Text As String

    End Class
End NameSpace