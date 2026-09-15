Imports NUnit.Framework
Imports Xtensive.Orm.Tests.ObjectModel.ChinookDO
Imports Xtensive.Orm.Tests.ObjectModel

Namespace Linq

  Public Class DateTimeTest
    Inherits ChinookDOModelTest

    Public Shadows ReadOnly Property Invoices As IOrderedQueryable(Of Invoice)
      Get
        Return Query.All(Of Invoice)().OrderBy(Function(c) c.InvoiceId)
      End Get
    End Property

    <Test()>
    Public Sub DatePartNotSupported1Test()
      Assert.Throws(Of QueryTranslationException)(Function() (From invoice In Invoices
                                                              Where Microsoft.VisualBasic.DateAndTime.DatePart("Day", invoice.InvoiceDate) > 0
                                                              Select invoice) _
                                                         .ToList())
    End Sub

    <Test()>
    Public Sub DatePartNotSupported2Test()
      Assert.Throws(Of QueryTranslationException)(Function() (From invoice In Invoices
                                                              Where Microsoft.VisualBasic.DateAndTime.DatePart(DateInterval.Day, invoice.InvoiceDate) > 0
                                                              Select invoice) _
                                                         .ToList())
    End Sub

    <Test()>
    Public Sub NowTest()
      Dim result = (From invoice In Invoices
                    Where Microsoft.VisualBasic.DateAndTime.Now > invoice.InvoiceDate
                    Select invoice) _
                    .ToList()
      Dim expected = (From invoice In Invoices.ToList
                      Where Microsoft.VisualBasic.DateAndTime.Now > invoice.InvoiceDate
                      Select invoice) _
                    .ToList()
      Assert.That(expected.Count, [Is].GreaterThan(0))
      Assert.That(expected.SequenceEqual(result), [Is].True)
    End Sub

    <Test()>
    Public Sub YearTest()
      Dim result = (From invoice In Invoices
                    Where Microsoft.VisualBasic.DateAndTime.Year(invoice.InvoiceDate) > 0
                    Select invoice) _
                    .ToList()
      Dim expected = (From invoice In Invoices.ToList
                      Where Microsoft.VisualBasic.DateAndTime.Year(invoice.InvoiceDate) > 0
                      Select invoice) _
                    .ToList()
      Assert.That(expected.Count, [Is].GreaterThan(0))
      Assert.That(expected.SequenceEqual(result), [Is].True)
    End Sub

    <Test()>
    Public Sub MonthTest()
      Dim result = (From invoice In Invoices
                    Where Microsoft.VisualBasic.DateAndTime.Month(invoice.InvoiceDate) > 0
                    Select invoice) _
                    .ToList()
      Dim expected = (From invoice In Invoices.ToList
                      Where Microsoft.VisualBasic.DateAndTime.Month(invoice.InvoiceDate) > 0
                      Select invoice) _
                    .ToList()
      Assert.That(expected.Count, [Is].GreaterThan(0))
      Assert.That(expected.SequenceEqual(result), [Is].True)
    End Sub

    <Test()>
    Public Sub DayTest()
      Dim result = (From invoice In Invoices
                    Where Microsoft.VisualBasic.DateAndTime.Day(invoice.InvoiceDate) > 0
                    Select invoice) _
                    .ToList()
      Dim expected = (From invoice In Invoices.ToList
                      Where Microsoft.VisualBasic.DateAndTime.Day(invoice.InvoiceDate) > 0
                      Select invoice) _
                    .ToList()
      Assert.That(expected.Count, [Is].GreaterThan(0))
      Assert.That(expected.SequenceEqual(result), [Is].True)
    End Sub

    <Test()>
    Public Sub HourTest()
      Dim result = (From invoice In Invoices
                    Where invoice.PaymentDate.HasValue AndAlso Microsoft.VisualBasic.DateAndTime.Hour(invoice.PaymentDate.Value) > 0
                    Select invoice) _
                    .ToList()
      Dim expected = (From invoice In Invoices.ToList
                      Where invoice.PaymentDate.HasValue AndAlso Microsoft.VisualBasic.DateAndTime.Hour(invoice.PaymentDate.Value) > 0
                      Select invoice) _
                    .ToList()
      Assert.That(expected.Count, [Is].GreaterThan(0))
      Assert.That(expected.SequenceEqual(result), [Is].True)
    End Sub

    <Test()>
    Public Sub MinuteTest()
      Dim result = (From invoice In Invoices
                    Where invoice.PaymentDate.HasValue AndAlso Microsoft.VisualBasic.DateAndTime.Minute(invoice.PaymentDate.Value) > 0
                    Select invoice) _
                    .ToList()
      Dim expected = (From invoice In Invoices.ToList
                      Where invoice.PaymentDate.HasValue AndAlso Microsoft.VisualBasic.DateAndTime.Minute(invoice.PaymentDate.Value) > 0
                      Select invoice) _
                    .ToList()
      Assert.That(expected.Count, [Is].GreaterThan(0))
      Assert.That(expected.SequenceEqual(result), [Is].True)
    End Sub

    <Test()>
    <Ignore("Dataset has no dates with seconds")>
    Public Sub SecondTest()
      Dim result = (From invoice In Invoices
                    Where invoice.PaymentDate.HasValue AndAlso Microsoft.VisualBasic.DateAndTime.Second(invoice.PaymentDate.Value) > 0
                    Select invoice) _
                    .ToList()
      Dim expected = (From invoice In Invoices.ToList
                      Where invoice.PaymentDate.HasValue AndAlso Microsoft.VisualBasic.DateAndTime.Second(invoice.PaymentDate.Value) > 0
                      Select invoice) _
                    .ToList()
      Assert.That(expected.Count, [Is].GreaterThan(0))
      Assert.That(expected.SequenceEqual(result), [Is].True)
    End Sub

    <Test()>
    Public Sub MonthNameNotSupported1Test()
      Assert.Throws(Of QueryTranslationException)(Function() (From invoice In Invoices
                                                              Where Microsoft.VisualBasic.DateAndTime.MonthName(invoice.InvoiceDate.Month, True) <> "test"
                                                              Select invoice) _
                                                         .ToList())
    End Sub

    <Test()>
    Public Sub MonthNameNotSupported2Test()
      Assert.Throws(Of QueryTranslationException)(Function() (From invoice In Invoices
                                                              Where Microsoft.VisualBasic.DateAndTime.MonthName(invoice.InvoiceDate.Month, False) <> "test"
                                                              Select invoice) _
                                                         .ToList())
    End Sub

  End Class
End Namespace