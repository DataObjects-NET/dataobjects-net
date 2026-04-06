Imports Xtensive.Orm.Tests.ObjectModel.ChinookDO
Imports Xtensive.Orm.Tests.ObjectModel
Imports Xtensive.Orm.Configuration
Imports NUnit.Framework

Namespace Linq
  <TestFixture()>
  Public Class StringTest
    Inherits ChinookDOModelTest

    Public Shadows ReadOnly Property Customers As IOrderedQueryable(Of Customer)
      Get
        Return Session.Query.All(Of Customer)().OrderBy(Function(c) c.CustomerId)
      End Get
    End Property

    <Test()>
    Public Sub TrimTest()
      Dim result = (From customer In Customers
                    Where Microsoft.VisualBasic.Strings.Trim("   prefix " + customer.CompanyName + " suffix  ") <> "test"
                    Select customer) _
              .ToList()
      Dim expected = (From customer In Customers.ToList()
                      Where customer.CompanyName <> Nothing AndAlso Microsoft.VisualBasic.Strings.Trim("   prefix " + customer.CompanyName + " suffix  ") <> "test"
                      Select customer) _
              .ToList()
      Assert.That(expected.SequenceEqual(result), [Is].True)
    End Sub

    <Test()>
    Public Sub LTrimTest()
      Dim result = (From customer In Customers
                    Where Microsoft.VisualBasic.Strings.LTrim("   prefix " + customer.CompanyName + " suffix  ") <> "test"
                    Select customer) _
              .ToList()
      Dim expected = (From customer In Customers.ToList()
                      Where customer.CompanyName <> Nothing AndAlso Microsoft.VisualBasic.Strings.LTrim("   prefix " + customer.CompanyName + " suffix  ") <> "test"
                      Select customer) _
              .ToList()
      Assert.That(expected.SequenceEqual(result), [Is].True)
    End Sub

    <Test()>
    Public Sub RTrimTest()
      Dim result = (From customer In Customers
                    Where Microsoft.VisualBasic.Strings.RTrim("   prefix " + customer.CompanyName + " suffix  ") <> "test"
                    Select customer) _
              .ToList()
      Dim expected = (From customer In Customers.ToList()
                      Where customer.CompanyName <> Nothing AndAlso Microsoft.VisualBasic.Strings.RTrim("   prefix " + customer.CompanyName + " suffix  ") <> "test"
                      Select customer) _
              .ToList()
      Assert.That(expected.SequenceEqual(result), [Is].True)
    End Sub

    <Test()>
    Public Sub LenTest()
      Dim result = (From customer In Customers
                    Where Microsoft.VisualBasic.Strings.Len("   prefix " + customer.CompanyName + " suffix  ") > 0
                    Select Microsoft.VisualBasic.Strings.Len("   prefix " + customer.CompanyName + " suffix  ")) _
              .ToList()
      Dim expected = (From customer In Customers.ToList()
                      Where customer.CompanyName <> Nothing AndAlso Microsoft.VisualBasic.Strings.Len("   prefix " + customer.CompanyName + " suffix  ") > 0
                      Select Microsoft.VisualBasic.Strings.Len("   prefix " + customer.CompanyName + " suffix  ")) _
              .ToList()
      Assert.That(expected.SequenceEqual(result), [Is].True)
    End Sub

    <Test()>
    Public Sub LeftTest()
      Dim result = (From customer In Customers
                    Where Microsoft.VisualBasic.Strings.Left("   prefix " + customer.CompanyName + " suffix  ", 2) <> "test"
                    Select customer) _
              .ToList()
      Dim expected = (From customer In Customers.ToList()
                      Where customer.CompanyName <> Nothing AndAlso Microsoft.VisualBasic.Strings.Left("   prefix " + customer.CompanyName + " suffix  ", 2) <> "test"
                      Select customer) _
              .ToList()
      Assert.That(expected.SequenceEqual(result), [Is].True)
    End Sub


    <Test()>
    Public Sub RightTest()
      Dim result = (From customer In Customers
                    Where Microsoft.VisualBasic.Strings.Right("   prefix " + customer.CompanyName + " suffix  ", 2) <> "test"
                    Select customer) _
              .ToList()
      Dim expected = (From customer In Customers.ToList()
                      Where customer.CompanyName <> Nothing AndAlso Microsoft.VisualBasic.Strings.Right("   prefix " + customer.CompanyName + " suffix  ", 2) <> "test"
                      Select customer) _
              .ToList()
      Assert.That(expected.SequenceEqual(result), [Is].True)
    End Sub

    <Test()>
    Public Sub Mid2Test()
      Dim result = (From customer In Customers
                    Where Microsoft.VisualBasic.Strings.Mid("   prefix " + customer.CompanyName + " suffix  ", 2) <> "test"
                    Select customer) _
              .ToList()
      Dim expected = (From customer In Customers.ToList()
                      Where customer.CompanyName <> Nothing AndAlso Microsoft.VisualBasic.Strings.Mid("   prefix " + customer.CompanyName + " suffix  ", 2) <> "test"
                      Select customer) _
              .ToList()
      Assert.That(expected.SequenceEqual(result), [Is].True)
    End Sub

    <Test()>
    Public Sub Mid3Test()
      Dim result = (From customer In Customers
                    Where Microsoft.VisualBasic.Strings.Mid("   prefix " + customer.CompanyName + " suffix  ", 2, 1) <> "test"
                    Select customer) _
              .ToList()
      Dim expected = (From customer In Customers.ToList()
                      Where customer.CompanyName <> Nothing AndAlso Microsoft.VisualBasic.Strings.Mid("   prefix " + customer.CompanyName + " suffix  ", 2, 1) <> "test"
                      Select customer) _
              .ToList()
      Assert.That(expected.SequenceEqual(result), [Is].True)
    End Sub

    <Test()>
    Public Sub UCaseStringTest()
      Dim result = (From customer In Customers
                    Where Microsoft.VisualBasic.Strings.UCase("   prefix " + customer.CompanyName + " suffix  ") <> "test"
                    Select customer) _
              .ToList()
      Dim expected = (From customer In Customers.ToList()
                      Where customer.CompanyName <> Nothing AndAlso Microsoft.VisualBasic.Strings.UCase("   prefix " + customer.CompanyName + " suffix  ") <> "test"
                      Select customer) _
              .ToList()
      Assert.That(expected.SequenceEqual(result), [Is].True)
    End Sub

    <Test()>
    Public Sub LCaseStringTest()
      Dim result = (From customer In Customers
                    Where Microsoft.VisualBasic.Strings.LCase("   prefix " + customer.CompanyName + " suffix  ") <> "test"
                    Select customer) _
              .ToList()
      Dim expected = (From customer In Customers.ToList()
                      Where customer.CompanyName <> Nothing AndAlso Microsoft.VisualBasic.Strings.LCase("   prefix " + customer.CompanyName + " suffix  ") <> "test"
                      Select customer) _
              .ToList()
      Assert.That(expected.SequenceEqual(result), [Is].True)
    End Sub

    <Test()>
    Public Sub UCaseCharTest()
      Dim result = (From customer In Customers
                    Where Microsoft.VisualBasic.Strings.UCase("A"c) <> "A"c
                    Select customer) _
              .ToList()
      Dim expected = (From customer In Customers.ToList()
                      Where Microsoft.VisualBasic.Strings.UCase("A"c) <> "A"c
                      Select customer) _
              .ToList()
      Assert.That(expected.SequenceEqual(result), [Is].True)
    End Sub

    <Test()>
    Public Sub LCaseCharTest()
      Dim result = (From customer In Customers
                    Where Microsoft.VisualBasic.Strings.LCase("A"c) <> "A"c
                    Select customer) _
              .ToList()
      Dim expected = (From customer In Customers.ToList()
                      Where Microsoft.VisualBasic.Strings.LCase("A"c) <> "A"c
                      Select customer) _
              .ToList()
      Assert.That(expected.SequenceEqual(result), [Is].True)
    End Sub
  End Class
End Namespace