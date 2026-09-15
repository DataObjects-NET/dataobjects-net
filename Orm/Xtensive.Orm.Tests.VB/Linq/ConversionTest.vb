Option Strict On

Imports Xtensive.Orm.Tests.ObjectModel
Imports Xtensive.Orm.Tests.ObjectModel.ChinookDO
Imports NUnit.Framework

Namespace Linq
  <TestFixture()>
  <Ignore("DataSet does not contain suited data")>
  Public Class ConversionTest
    Inherits ChinookDOModelTest
    Public Shadows ReadOnly Property Customers As IOrderedQueryable(Of Customer)
      Get
        Return Query.All(Of Customer)().OrderBy(Function(c) c.CustomerId)
      End Get
    End Property

    <Test()>
    Public Sub StringToBooleanTest()
      Dim result = (From customer In Customers
                    Where Microsoft.VisualBasic.CompilerServices.Conversions.ToBoolean(customer.CustomerId)
                    Select customer) _
                    .ToList()

      Dim expected = (From customer In Customers.ToList()
                      Where Microsoft.VisualBasic.CompilerServices.Conversions.ToBoolean(customer.CustomerId)
                      Select customer) _
                    .ToList()
      Assert.That(expected.SequenceEqual(result), [Is].True)
    End Sub

    <Test()>
    Public Sub StringToByteTest()
      Dim result = (From customer In Customers
                    Where Microsoft.VisualBasic.CompilerServices.Conversions.ToByte(customer.CustomerId) > 1
                    Select customer) _
                    .ToList()

      Dim expected = (From customer In Customers.ToList()
                      Where Microsoft.VisualBasic.CompilerServices.Conversions.ToByte(customer.CustomerId) > 1
                      Select customer) _
                    .ToList()
      Assert.That(expected.SequenceEqual(result), [Is].True)
    End Sub

    <Test()>
    Public Sub StringToCharTest()
      Dim result = (From customer In Customers
                    Where Microsoft.VisualBasic.CompilerServices.Conversions.ToChar(customer.CustomerId) <> "a"c
                    Select customer) _
                    .ToList()

      Dim expected = (From customer In Customers.ToList()
                      Where Microsoft.VisualBasic.CompilerServices.Conversions.ToChar(customer.CustomerId) <> "a"c
                      Select customer) _
                    .ToList()
      Assert.That(expected.SequenceEqual(result), [Is].True)
    End Sub

    <Test()>
    Public Sub StringToDateTest()
      Dim result = (From customer In Customers
                    Where Microsoft.VisualBasic.CompilerServices.Conversions.ToDate(customer.CustomerId) < DateTime.Now
                    Select customer) _
                    .ToList()

      Dim expected = (From customer In Customers.ToList()
                      Where Microsoft.VisualBasic.CompilerServices.Conversions.ToDate(customer.CustomerId) < DateTime.Now
                      Select customer) _
                    .ToList()
      Assert.That(expected.SequenceEqual(result), [Is].True)
    End Sub


    <Test()>
    Public Sub StringToDecimalTest()
      Dim result = (From customer In Customers
                    Where Microsoft.VisualBasic.CompilerServices.Conversions.ToDecimal(customer.CustomerId) > 1
                    Select customer) _
                    .ToList()

      Dim expected = (From customer In Customers.ToList()
                      Where Microsoft.VisualBasic.CompilerServices.Conversions.ToDecimal(customer.CustomerId) > 1
                      Select customer) _
                    .ToList()
      Assert.That(expected.SequenceEqual(result), [Is].True)
    End Sub

    <Test()>
    Public Sub BooleanToDecimalTest()
      Dim result = (From customer In Customers
                    Where Microsoft.VisualBasic.CompilerServices.Conversions.ToDecimal(customer.CustomerId <> 0) > 1
                    Select customer) _
                    .ToList()

      Dim expected = (From customer In Customers.ToList()
                      Where Microsoft.VisualBasic.CompilerServices.Conversions.ToDecimal(customer.CustomerId <> 0) > 1
                      Select customer) _
                    .ToList()
      Assert.That(expected.SequenceEqual(result), [Is].True)
    End Sub

    <Test()>
    Public Sub StringToDoubleTest()
      Dim result = (From customer In Customers
                    Where Microsoft.VisualBasic.CompilerServices.Conversions.ToDouble(customer.CustomerId) > 1
                    Select customer) _
                    .ToList()

      Dim expected = (From customer In Customers.ToList()
                      Where Microsoft.VisualBasic.CompilerServices.Conversions.ToDouble(customer.CustomerId) > 1
                      Select customer) _
                    .ToList()
      Assert.That(expected.SequenceEqual(result), [Is].True)
    End Sub

    <Test()>
    Public Sub StringToIntegerTest()
      Dim result = (From customer In Customers
                    Where Microsoft.VisualBasic.CompilerServices.Conversions.ToInteger(customer.CustomerId) > 1
                    Select customer) _
                    .ToList()

      Dim expected = (From customer In Customers.ToList()
                      Where Microsoft.VisualBasic.CompilerServices.Conversions.ToInteger(customer.CustomerId) > 1
                      Select customer) _
                    .ToList()
      Assert.That(expected.SequenceEqual(result), [Is].True)
    End Sub

    <Test()>
    Public Sub StringToLongTest()
      Dim result = (From customer In Customers
                    Where Microsoft.VisualBasic.CompilerServices.Conversions.ToLong(customer.CustomerId) > 1L
                    Select customer) _
                    .ToList()

      Dim expected = (From customer In Customers.ToList()
                      Where Microsoft.VisualBasic.CompilerServices.Conversions.ToLong(customer.CustomerId) > 1L
                      Select customer) _
                    .ToList()
      Assert.That(expected.SequenceEqual(result), [Is].True)
    End Sub

    <Test()>
    Public Sub StringToSByteTest()
      Dim result = (From customer In Customers
                    Where Microsoft.VisualBasic.CompilerServices.Conversions.ToSByte(customer.CustomerId) > 1
                    Select customer) _
                    .ToList()

      Dim expected = (From customer In Customers.ToList()
                      Where Microsoft.VisualBasic.CompilerServices.Conversions.ToSByte(customer.CustomerId) > 1
                      Select customer) _
                    .ToList()
      Assert.That(expected.SequenceEqual(result), [Is].True)
    End Sub

    <Test()>
    Public Sub StringToShortTest()
      Dim result = (From customer In Customers
                    Where Microsoft.VisualBasic.CompilerServices.Conversions.ToShort(customer.CustomerId) > 1S
                    Select customer) _
                    .ToList()

      Dim expected = (From customer In Customers.ToList()
                      Where Microsoft.VisualBasic.CompilerServices.Conversions.ToShort(customer.CustomerId) > 1S
                      Select customer) _
                    .ToList()
      Assert.That(expected.SequenceEqual(result), [Is].True)
    End Sub

    <Test()>
    Public Sub StringToSingleTest()
      Dim result = (From customer In Customers
                    Where Microsoft.VisualBasic.CompilerServices.Conversions.ToSingle(customer.CustomerId) > 1S
                    Select customer) _
                    .ToList()

      Dim expected = (From customer In Customers.ToList()
                      Where Microsoft.VisualBasic.CompilerServices.Conversions.ToSingle(customer.CustomerId) > 1S
                      Select customer) _
                    .ToList()
      Assert.That(expected.SequenceEqual(result), [Is].True)
    End Sub

    <Test()>
    Public Sub StringToUIntegerTest()
      Dim result = (From customer In Customers
                    Where Microsoft.VisualBasic.CompilerServices.Conversions.ToUInteger(customer.CustomerId) > 1
                    Select customer) _
                    .ToList()

      Dim expected = (From customer In Customers.ToList()
                      Where Microsoft.VisualBasic.CompilerServices.Conversions.ToUInteger(customer.CustomerId) > 1
                      Select customer) _
                    .ToList()
      Assert.That(expected.SequenceEqual(result), [Is].True)
    End Sub

    <Test()>
    Public Sub StringToULongTest()
      Dim result = (From customer In Customers
                    Where Microsoft.VisualBasic.CompilerServices.Conversions.ToULong(customer.CustomerId) > 1
                    Select customer) _
                    .ToList()

      Dim expected = (From customer In Customers.ToList()
                      Where Microsoft.VisualBasic.CompilerServices.Conversions.ToULong(customer.CustomerId) > 1
                      Select customer) _
                    .ToList()
      Assert.That(expected.SequenceEqual(result), [Is].True)
    End Sub

    <Test()>
    Public Sub StringToUShortTest()
      Dim result = (From customer In Customers
                    Where Microsoft.VisualBasic.CompilerServices.Conversions.ToUShort(customer.CustomerId) > 1
                    Select customer) _
                    .ToList()

      Dim expected = (From customer In Customers.ToList()
                      Where Microsoft.VisualBasic.CompilerServices.Conversions.ToUShort(customer.CustomerId) > 1
                      Select customer) _
                    .ToList()
      Assert.That(expected.SequenceEqual(result), [Is].True)
    End Sub

    <Test()>
    Public Sub ToStringTest()
      ToStringGenericTest(Of Boolean)(True)
      ToStringGenericTest(Of Byte)(1)
      ToStringGenericTest(Of Char)("a"c)
      ToStringGenericTest(Of Date)(Date.Now)
      ToStringGenericTest(Of Decimal)(1)
      ToStringGenericTest(Of Double)(1)
      ToStringGenericTest(Of Short)(1)
      ToStringGenericTest(Of Integer)(1)
      ToStringGenericTest(Of Long)(1)
      ToStringGenericTest(Of Single)(1)
      ToStringGenericTest(Of UInt32)(1)
      ToStringGenericTest(Of UInt64)(1)
    End Sub

    Public Sub ToStringGenericTest(Of TType)(ByVal ParamArray parameter() As TType)
      Dim store = Session.Query.Store(Of TType)(parameter)
      Dim result = (From customer In store
                    Where Microsoft.VisualBasic.CompilerServices.Conversions.ToString(customer) <> "test"
                    Select customer) _
                    .ToList()

      Dim expected = (From customer In store.ToList()
                      Where Microsoft.VisualBasic.CompilerServices.Conversions.ToString(parameter) <> "test"
                      Select customer) _
                    .ToList()
      Assert.That(expected.SequenceEqual(result), [Is].True)
    End Sub
  End Class
End Namespace