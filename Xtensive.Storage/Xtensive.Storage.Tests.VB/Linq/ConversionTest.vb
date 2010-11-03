Option Strict On

Imports Xtensive.Storage.Tests.ObjectModel
Imports Xtensive.Storage.Tests.ObjectModel.NorthwindDO
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

    End Class
End Namespace