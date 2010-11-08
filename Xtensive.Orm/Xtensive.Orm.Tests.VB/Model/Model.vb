Imports Xtensive.Aspects
Imports PostSharp.Extensibility
Imports PostSharp.Aspects
Imports Xtensive.Orm


Namespace Model
  Public Class NonPersistent
    Public Property Id As Integer
  End Class

  <HierarchyRoot>
  Public Class Author
    Inherits Entity
  
    Sub New()
      MyBase.New()
    End Sub


    <Field, Key>
    Public Property Id As Integer

    <Field>
    Public Property Name As String

    <Field>
    <Association(PairTo: = "Author", OnOwnerRemove: = Xtensive.Orm.OnRemoveAction.Cascade, OnTargetRemove:= Xtensive.Orm.OnRemoveAction.Deny)>
    Public Property Books As EntitySet(Of Book)

  End Class

  <HierarchyRoot>
  Public Class Book
    Inherits Entity

    Sub New()
      MyBase.New()
    End Sub

    <Field, Key>
    Public Property Id As Integer

    <Field>
    Public Property Name As String

    <Field>
    Public Property Author As Author

  End Class

  <Serializable>
  <MulticastAttributeUsage(MulticastTargets.Method, AllowMultiple: = false, Inheritance: = MulticastInheritance.Multicast)>
  <AttributeUsage(AttributeTargets.Method, AllowMultiple: = false, Inherited: = true)>
  Public Class SomeAspect
    Inherits PostSharp.Aspects.OnMethodBoundaryAspect

    Public Overrides Sub OnEntry(ByVal args As MethodExecutionArgs)
      Console.WriteLine("Entry...")
    End Sub
  End Class


  Public Class Test
    <SomeAspect>
    Public Sub Method()
      Console.WriteLine("Hello!!!")
    End Sub
  End Class
End Namespace
