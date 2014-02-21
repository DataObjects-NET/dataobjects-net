Namespace Model
  Public Class NonPersistent
    Public Property Id As Integer
  End Class

  <HierarchyRoot()>
  Public Class Author
    Inherits Entity

    Sub New()
      MyBase.New()
    End Sub

    <Field(), Key()>
    Public Property Id As Integer

    <Field()>
    Public Property Name As String

    <Field()>
    <Association(PairTo:="Author", OnOwnerRemove:=Xtensive.Orm.OnRemoveAction.Cascade, OnTargetRemove:=Xtensive.Orm.OnRemoveAction.Deny)>
    Public Property Books As EntitySet(Of Book)
  End Class

  <HierarchyRoot()>
  Public Class Book
    Inherits Entity

    Sub New()
      MyBase.New()
    End Sub

    <Field(), Key()>
    Public Property Id As Integer

    <Field()>
    Public Property Name As String

    <Field()>
    Public Property Author As Author

  End Class
End Namespace
