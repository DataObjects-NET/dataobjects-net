Imports Xtensive.Orm.Tests.ObjectModel
Imports Xtensive.Orm.Tests.ObjectModel.ChinookDO
Imports NUnit.Framework

Namespace Linq
  <TestFixture()>
  Public Class OperatorsTest
    Inherits ChinookDOModelTest

    Public Shadows ReadOnly Property Invoices As IOrderedQueryable(Of Invoice)
      Get
        Return Session.Query.All(Of Invoice)().OrderBy(Function(c) c.InvoiceId)
      End Get
    End Property

    Public Shadows ReadOnly Property Customers As IOrderedQueryable(Of Customer)
      Get
        Return Session.Query.All(Of Customer)().OrderBy(Function(c) c.CustomerId)
      End Get
    End Property

    <Test()>
    Public Sub CompareString1Test()
      Dim result = (From customer In Customers
                    Where customer.CompanyName > "test"
                    Select customer) _
                    .ToList()
      ' SQL compares CaseInsensitive by default
      Dim expected = (From customer In Customers.ToList()
                      Where Microsoft.VisualBasic.CompilerServices.Operators.CompareString(customer.CompanyName, "test", True) > 0
                      Select customer) _
                    .ToList()
      Assert.That(expected.Count, [Is].GreaterThan(0))
      Assert.That(expected.SequenceEqual(result), [Is].True)
    End Sub

    <Test()>
    <Ignore("")>
    Public Sub CompareString2Test()
      Dim result = (From customer In Customers
                    Select customer.CompanyName > "test") _
                    .Where(Function(i) i) _
                    .ToList()
      ' SQL compares CaseInsensitive by default
      Dim expected = (From customer In Customers.ToList()
                      Select customer.CompanyName > "test") _
                    .Where(Function(i) i) _
                    .ToList()
      Assert.That(expected.Count, [Is].GreaterThan(0))
      Assert.That(expected.SequenceEqual(result), [Is].True)
    End Sub

    <Test()>
    Public Sub BooleanTest()
      Dim result = (From invoice In Invoices
                    Select invoice.Total > 2.0) _
                    .Where(Function(i) i) _
                    .ToList()
      Dim expected = (From invoice In Invoices.ToList()
                      Select invoice.Total > 2.0) _
                    .Where(Function(i) i) _
                    .ToList()
      Assert.That(expected.Count, [Is].GreaterThan(0))
      Assert.That(expected.SequenceEqual(result), [Is].True)
    End Sub

  End Class
End Namespace