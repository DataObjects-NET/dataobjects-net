' Copyright (C) 2013 Xtensive LLC.
' All rights reserved.
' For conditions of distribution and use, see license.
' Created by: Alena Mikshina
' Created:    2013.11.05
Imports System
Imports System.Linq
Imports NUnit.Framework
Imports Xtensive.Orm.Configuration
Imports Xtensive.Orm.Tests.Model.PersistentPropertyWeavingTestModel

Namespace Model
  Namespace PersistentPropertyWeavingTestModel
    <HierarchyRoot()> _
    Public Class Base
      Inherits Entity
      <Key(), Field()> _
      Public Property Id() As Long
      Public Sub New()
        MyBase.New()
      End Sub
    End Class

    ' Override virtual property

    Public Class OverrideFieldBase
      Inherits Base
      <Field()> _
      Public Overridable Property Value() As String
    End Class

    Public Class OverrideField
      Inherits OverrideFieldBase
      Public Overrides Property Value() As String
    End Class

    Public Class GenericOverrideFieldBase(Of T)
      Inherits Base
      <Field()> _
      Public Overridable Property Value() As T
    End Class

    Public Class GenericOverrideField
      Inherits GenericOverrideFieldBase(Of String)
      Public Overrides Property Value() As String
    End Class

    Public Class GenericOverrideField(Of T)
      Inherits GenericOverrideFieldBase(Of T)
      Public Overrides Property Value() As T
    End Class

    ' Implement persistent interface property

    Public Interface IImplementField
      Inherits IEntity
      <Field()> _
      Property Value() As String
    End Interface

    Public Class ImplementField
      Inherits Base
      Implements IImplementField
      Public Property Value() As String Implements IImplementField.Value
    End Class

    Public Interface IGenericImplementField(Of T)
      Inherits IEntity
      <Field()> _
      Property Value() As T
    End Interface

    Public Class GenericImplementField
      Inherits Base
      Implements IGenericImplementField(Of String)
      Public Property Value() As String Implements IGenericImplementField(Of String).Value
    End Class

    Public Class GenericImplementField(Of T)
      Inherits Base
      Implements IGenericImplementField(Of T)
      Public Property Value() As T Implements IGenericImplementField(Of T).Value
    End Class

    ' Implement non-persistent interface property

    Public Interface INonPersistentImplementField
      <Field()> _
      Property Value() As String
    End Interface

    Public Class NonPersistentImplementField
      Inherits Base
      Implements INonPersistentImplementField
      Public Property Value() As String Implements INonPersistentImplementField.Value
    End Class

    Public Interface INonPersistentGenericImplementField(Of T)
      <Field()> _
      Property Value() As T
    End Interface

    Public Class NonPersistentGenericImplementField
      Inherits Base
      Implements INonPersistentGenericImplementField(Of String)
      Public Property Value() As String Implements INonPersistentGenericImplementField(Of String).Value
    End Class

    Public Class NonPersistentGenericImplementField(Of T)
      Inherits Base
      Implements INonPersistentGenericImplementField(Of T)
      Public Property Value() As T Implements INonPersistentGenericImplementField(Of T).Value
    End Class

    ' New property

    Public Class NewFieldBase
      Inherits Base
      <Field()> _
      Public Property PersistentValue() As String

      <Field()> _
      Public Overridable Property PersistentVirtualValue() As String

      Public Property NonPersistentValue() As String

      Public Shared Property NonPersistentStaticValue() As String
    End Class

    Public Class PersistentNewField
      Inherits NewFieldBase
      <Field()> _
      Public Shadows Property PersistentValue() As String

      <Field()> _
      Public Shadows Property PersistentVirtualValue() As String

      <Field()> _
      Public Shadows Property NonPersistentValue() As String

      <Field()> _
      Public Shadows Property NonPersistentStaticValue() As String

    End Class

    Public Class NonPersistentNewField
      Inherits NewFieldBase
      Public Shadows Property PersistentValue() As String

      Public Shadows Property PersistentVirtualValue() As String

      Public Shadows Property NonPersistentValue() As String

      Public Shadows Property NonPersistentStaticValue() As String

    End Class
  End Namespace

  <TestFixture()> _
  Public Class PersistentPropertyWeavingTest
    Inherits AutoBuildTest
    Private Const Hello As String = "Hello"
    Private Const Hello1 As String = "Hello1"
    Private Const Hello2 As String = "Hello2"
    Private Const Hello3 As String = "Hello3"
    Private Const Hello4 As String = "Hello4"
    Private Const Hello5 As String = "Hello5"
    Private Const Hello6 As String = "Hello6"

    Protected Overrides Function BuildConfiguration() As DomainConfiguration
      Dim configuration = MyBase.BuildConfiguration()
      configuration.Types.Register(GetType(Base).Assembly, GetType(Base).Namespace)
      configuration.Types.Register(GetType(GenericOverrideField(Of String)))
      configuration.Types.Register(GetType(GenericImplementField(Of String)))
      configuration.Types.Register(GetType(NonPersistentGenericImplementField(Of String)))
      Return configuration
    End Function

    <Test()> _
    Public Sub OverrideFieldTest()
      ExecuteTest(Of OverrideField)(Sub(e) e.Value = Hello, Sub(e) Assert.That(e.Value, [Is].EqualTo((Hello))))
    End Sub

    <Test()> _
    Public Sub GenericOverrideFieldTest()
      ExecuteTest(Of GenericOverrideField)(Sub(e) e.Value = Hello, Sub(e) Assert.That(e.Value, [Is].EqualTo(Hello)))
    End Sub

    <Test()> _
    Public Sub GenericOverrideFieldTest2()
      ExecuteTest(Of GenericOverrideField(Of String))(Sub(e) e.Value = Hello, Sub(e) Assert.That(e.Value, [Is].EqualTo(Hello)))
    End Sub

    <Test()> _
    Public Sub ImplementFieldTest()
      ExecuteTest(Of ImplementField)(Sub(e) e.Value = Hello, Sub(e) Assert.That(e.Value, [Is].EqualTo(Hello)))
    End Sub

    <Test()> _
    Public Sub GenericImplementFieldTest()
      ExecuteTest(Of GenericImplementField)(Sub(e) e.Value = Hello, Sub(e) Assert.That(e.Value, [Is].EqualTo(Hello)))
    End Sub

    <Test()> _
    Public Sub GenericImplementFieldTest2()
      ExecuteTest(Of GenericImplementField(Of String))(Sub(e) e.Value = Hello, Sub(e) Assert.That(e.Value, [Is].EqualTo(Hello)))
    End Sub

    <Test()> _
    Public Sub NonPersistentImplementFieldTest()
      ExecuteTest(Of NonPersistentImplementField)(Sub(e) e.Value = Hello, Sub(e) Assert.That(e.Value, [Is].EqualTo(Hello)))
    End Sub

    <Test()> _
    Public Sub NonPersistentGenericImplementFieldTest()
      ExecuteTest(Of NonPersistentGenericImplementField)(Sub(e) e.Value = Hello, Sub(e) Assert.That(e.Value, [Is].EqualTo(Hello)))
    End Sub

    <Test()> _
    Public Sub NonPersistentGenericImplementFieldTest2()
      ExecuteTest(Of NonPersistentGenericImplementField(Of String))(Sub(e) e.Value = Hello, Sub(e) Assert.That(e.Value, [Is].EqualTo(Hello)))
    End Sub

    <Test()> _
    Public Sub PersistentNewFieldTest()
      ExecuteTest(Of PersistentNewField)(
        Sub(entity)
          Dim baseEntity = DirectCast(entity, NewFieldBase)
          baseEntity.PersistentValue = Hello1
          baseEntity.PersistentVirtualValue = Hello2
          entity.PersistentValue = Hello3
          entity.PersistentVirtualValue = Hello4
          entity.NonPersistentValue = Hello5
          entity.NonPersistentStaticValue = Hello6

        End Sub,
        Sub(entity)
          Dim baseEntity = DirectCast(entity, NewFieldBase)
          Assert.That(baseEntity.PersistentValue, [Is].EqualTo(Hello1))
          Assert.That(baseEntity.PersistentVirtualValue, [Is].EqualTo(Hello2))
          Assert.That(entity.PersistentValue, [Is].EqualTo(Hello3))
          Assert.That(entity.PersistentVirtualValue, [Is].EqualTo(Hello4))
          Assert.That(entity.NonPersistentValue, [Is].EqualTo(Hello5))
          Assert.That(entity.NonPersistentStaticValue, [Is].EqualTo(Hello6))

        End Sub)
    End Sub

    <Test()> _
    Public Sub NonPersistentNewFieldTest()
      ExecuteTest(Of NonPersistentNewField)(
        Sub(entity)
          Dim baseEntity = DirectCast(entity, NewFieldBase)
          baseEntity.PersistentValue = Hello1
          baseEntity.PersistentVirtualValue = Hello2
          entity.PersistentValue = Hello3
          entity.PersistentVirtualValue = Hello4
          entity.NonPersistentValue = Hello5
          entity.NonPersistentStaticValue = Hello6

        End Sub,
        Sub(entity)
          Dim baseEntity = DirectCast(entity, NewFieldBase)
          Assert.That(baseEntity.PersistentValue, [Is].EqualTo(Hello1))
          Assert.That(baseEntity.PersistentVirtualValue, [Is].EqualTo(Hello2))
          Assert.That(entity.PersistentValue, [Is].EqualTo(Nothing))
          Assert.That(entity.PersistentVirtualValue, [Is].EqualTo(Nothing))
          Assert.That(entity.NonPersistentValue, [Is].EqualTo(Nothing))
          Assert.That(entity.NonPersistentStaticValue, [Is].EqualTo(Nothing))

        End Sub)
    End Sub

    Private Sub ExecuteTest(Of T As {Entity, New})(ByVal initializer As Action(Of T), ByVal checker As Action(Of T))
      Using session = Domain.OpenSession()
        Using tx = session.OpenTransaction()
          Dim entity = New T()
          initializer.Invoke(entity)
          tx.Complete()
        End Using
      End Using

      Using session = Domain.OpenSession()
        Using tx = session.OpenTransaction()
          Dim entity = session.Query.All(Of T)().Single()
          checker.Invoke(entity)
          tx.Complete()
        End Using
      End Using
    End Sub
  End Class
End Namespace
