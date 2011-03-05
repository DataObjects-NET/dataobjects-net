Option Strict On

Imports Xtensive.Orm.Tests.ObjectModel
Imports Xtensive.Orm.Tests.ObjectModel.NorthwindDO
Imports NUnit.Framework

Namespace Linq
    <TestFixture()>
    Public Class ConversionTest
        Inherits NorthwindDOModelTest
        Public Shadows ReadOnly Property Customers As IOrderedQueryable(Of Customer)
            Get
                Return Query.All(Of Customer)().OrderBy(Function(c) c.Id)
            End Get
        End Property

        <Test()>
        Public Sub StringToBooleanTest()
            Dim result = (From customer In Customers _
                    Where Microsoft.VisualBasic.CompilerServices.Conversions.ToBoolean(customer.Id) _
                    Select customer) _
                    .ToList()

            Dim expected = (From customer In Customers.ToList() _
                    Where Microsoft.VisualBasic.CompilerServices.Conversions.ToBoolean(customer.Id) _
                    Select customer) _
                    .ToList()
            Assert.IsTrue(expected.SequenceEqual(result))
        End Sub

        <Test()>
        Public Sub StringToByteTest()
            Dim result = (From customer In Customers _
                    Where Microsoft.VisualBasic.CompilerServices.Conversions.ToByte(customer.Id) > 1 _
                    Select customer) _
                    .ToList()

            Dim expected = (From customer In Customers.ToList() _
                    Where Microsoft.VisualBasic.CompilerServices.Conversions.ToByte(customer.Id) > 1 _
                    Select customer) _
                    .ToList()
            Assert.IsTrue(expected.SequenceEqual(result))
        End Sub

        <Test()>
        Public Sub StringToCharTest()
            Dim result = (From customer In Customers _
                    Where Microsoft.VisualBasic.CompilerServices.Conversions.ToChar(customer.Id) <> "a"c _
                    Select customer) _
                    .ToList()

            Dim expected = (From customer In Customers.ToList() _
                    Where Microsoft.VisualBasic.CompilerServices.Conversions.ToChar(customer.Id) <> "a"c _
                    Select customer) _
                    .ToList()
            Assert.IsTrue(expected.SequenceEqual(result))
        End Sub

        <Test()>
        Public Sub StringToDateTest()
            Dim result = (From customer In Customers _
                    Where Microsoft.VisualBasic.CompilerServices.Conversions.ToDate(customer.Id) < DateTime.Now _
                    Select customer) _
                    .ToList()

            Dim expected = (From customer In Customers.ToList() _
                    Where Microsoft.VisualBasic.CompilerServices.Conversions.ToDate(customer.Id) < DateTime.Now _
                    Select customer) _
                    .ToList()
            Assert.IsTrue(expected.SequenceEqual(result))
        End Sub


        <Test()>
        Public Sub StringToDecimalTest()
            Dim result = (From customer In Customers _
                    Where Microsoft.VisualBasic.CompilerServices.Conversions.ToDecimal(customer.Id) > 1 _
                    Select customer) _
                    .ToList()

            Dim expected = (From customer In Customers.ToList() _
                    Where Microsoft.VisualBasic.CompilerServices.Conversions.ToDecimal(customer.Id) > 1 _
                    Select customer) _
                    .ToList()
            Assert.IsTrue(expected.SequenceEqual(result))
        End Sub

        <Test()>
        Public Sub BooleanToDecimalTest()
            Dim result = (From customer In Customers _
                    Where Microsoft.VisualBasic.CompilerServices.Conversions.ToDecimal(customer.Id <> "test") > 1 _
                    Select customer) _
                    .ToList()

            Dim expected = (From customer In Customers.ToList() _
                    Where Microsoft.VisualBasic.CompilerServices.Conversions.ToDecimal(customer.Id <> "test") > 1 _
                    Select customer) _
                    .ToList()
            Assert.IsTrue(expected.SequenceEqual(result))
        End Sub

        <Test()>
        Public Sub StringToDoubleTest()
            Dim result = (From customer In Customers _
                    Where Microsoft.VisualBasic.CompilerServices.Conversions.ToDouble(customer.Id) > 1 _
                    Select customer) _
                    .ToList()

            Dim expected = (From customer In Customers.ToList() _
                    Where Microsoft.VisualBasic.CompilerServices.Conversions.ToDouble(customer.Id) > 1 _
                    Select customer) _
                    .ToList()
            Assert.IsTrue(expected.SequenceEqual(result))
        End Sub

        <Test()>
        Public Sub StringToIntegerTest()
            Dim result = (From customer In Customers _
                    Where Microsoft.VisualBasic.CompilerServices.Conversions.ToInteger(customer.Id) > 1 _
                    Select customer) _
                    .ToList()

            Dim expected = (From customer In Customers.ToList() _
                    Where Microsoft.VisualBasic.CompilerServices.Conversions.ToInteger(customer.Id) > 1 _
                    Select customer) _
                    .ToList()
            Assert.IsTrue(expected.SequenceEqual(result))
        End Sub

        <Test()>
        Public Sub StringToLongTest()
            Dim result = (From customer In Customers _
                    Where Microsoft.VisualBasic.CompilerServices.Conversions.ToLong(customer.Id) > 1L _
                    Select customer) _
                    .ToList()

            Dim expected = (From customer In Customers.ToList() _
                    Where Microsoft.VisualBasic.CompilerServices.Conversions.ToLong(customer.Id) > 1L _
                    Select customer) _
                    .ToList()
            Assert.IsTrue(expected.SequenceEqual(result))
        End Sub

        <Test()>
        Public Sub StringToSByteTest()
            Dim result = (From customer In Customers _
                    Where Microsoft.VisualBasic.CompilerServices.Conversions.ToSByte(customer.Id) > 1 _
                    Select customer) _
                    .ToList()

            Dim expected = (From customer In Customers.ToList() _
                    Where Microsoft.VisualBasic.CompilerServices.Conversions.ToSByte(customer.Id) > 1 _
                    Select customer) _
                    .ToList()
            Assert.IsTrue(expected.SequenceEqual(result))
        End Sub

        <Test()>
        Public Sub StringToShortTest()
            Dim result = (From customer In Customers _
                    Where Microsoft.VisualBasic.CompilerServices.Conversions.ToShort(customer.Id) > 1S _
                    Select customer) _
                    .ToList()

            Dim expected = (From customer In Customers.ToList() _
                    Where Microsoft.VisualBasic.CompilerServices.Conversions.ToShort(customer.Id) > 1S _
                    Select customer) _
                    .ToList()
            Assert.IsTrue(expected.SequenceEqual(result))
        End Sub

        <Test()>
        Public Sub StringToSingleTest()
            Dim result = (From customer In Customers _
                    Where Microsoft.VisualBasic.CompilerServices.Conversions.ToSingle(customer.Id) > 1S _
                    Select customer) _
                    .ToList()

            Dim expected = (From customer In Customers.ToList() _
                    Where Microsoft.VisualBasic.CompilerServices.Conversions.ToSingle(customer.Id) > 1S _
                    Select customer) _
                    .ToList()
            Assert.IsTrue(expected.SequenceEqual(result))
        End Sub

        <Test()>
        Public Sub StringToUIntegerTest()
            Dim result = (From customer In Customers _
                    Where Microsoft.VisualBasic.CompilerServices.Conversions.ToUInteger(customer.Id) > 1 _
                    Select customer) _
                    .ToList()

            Dim expected = (From customer In Customers.ToList() _
                    Where Microsoft.VisualBasic.CompilerServices.Conversions.ToUInteger(customer.Id) > 1 _
                    Select customer) _
                    .ToList()
            Assert.IsTrue(expected.SequenceEqual(result))
        End Sub

        <Test()>
        Public Sub StringToULongTest()
            Dim result = (From customer In Customers _
                    Where Microsoft.VisualBasic.CompilerServices.Conversions.ToULong(customer.Id) > 1 _
                    Select customer) _
                    .ToList()

            Dim expected = (From customer In Customers.ToList() _
                    Where Microsoft.VisualBasic.CompilerServices.Conversions.ToULong(customer.Id) > 1 _
                    Select customer) _
                    .ToList()
            Assert.IsTrue(expected.SequenceEqual(result))
        End Sub

        <Test()>
        Public Sub StringToUShortTest()
            Dim result = (From customer In Customers _
                    Where Microsoft.VisualBasic.CompilerServices.Conversions.ToUShort(customer.Id) > 1 _
                    Select customer) _
                    .ToList()

            Dim expected = (From customer In Customers.ToList() _
                    Where Microsoft.VisualBasic.CompilerServices.Conversions.ToUShort(customer.Id) > 1 _
                    Select customer) _
                    .ToList()
            Assert.IsTrue(expected.SequenceEqual(result))
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
            Dim store = Query.Store(Of TType)(parameter)
            Dim result = (From customer In store _
                    Where Microsoft.VisualBasic.CompilerServices.Conversions.ToString(customer) <> "test" _
                    Select customer) _
                    .ToList()

            Dim expected = (From customer In store.ToList() _
                    Where Microsoft.VisualBasic.CompilerServices.Conversions.ToString(parameter) <> "test" _
                    Select customer) _
                    .ToList()
            Assert.IsTrue(expected.SequenceEqual(result))
        End Sub
    End Class
End Namespace