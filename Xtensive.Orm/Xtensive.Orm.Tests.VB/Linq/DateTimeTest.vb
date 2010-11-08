Imports NUnit.Framework
Imports Xtensive.Orm.Tests.ObjectModel.NorthwindDO
Imports Xtensive.Orm.Tests.ObjectModel

Namespace Linq

    Public Class DateTimeTest
        Inherits NorthwindDOModelTest

        Public Shadows ReadOnly Property Orders As IOrderedQueryable(Of Order)
            Get
                Return Query.All(Of Order)().OrderBy(Function(c) c.Id)
            End Get
        End Property

        <Test()>
        <ExpectedException(GetType(NotSupportedException))>
        Public Sub DatePartNotSupported1Test()
            Dim result = (From order In Orders _
                    Where Microsoft.VisualBasic.DateAndTime.DatePart("Day", order.OrderDate) > 0 _
                    Select order) _
                    .ToList()
            Dim expected = (From order In Orders.ToList _
                    Where Microsoft.VisualBasic.DateAndTime.DatePart("Day", order.OrderDate) > 0 _
                    Select order) _
                    .ToList()
            Assert.IsTrue(expected.SequenceEqual(result))
        End Sub

        <Test()>
        <ExpectedException(GetType(NotSupportedException))>
        Public Sub DatePartNotSupported2Test()
            Dim result = (From order In Orders _
                    Where Microsoft.VisualBasic.DateAndTime.DatePart(DateInterval.Day, order.OrderDate.Value) > 0 _
                    Select order) _
                    .ToList()
            Dim expected = (From order In Orders.ToList _
                    Where Microsoft.VisualBasic.DateAndTime.DatePart(DateInterval.Day, order.OrderDate.Value) > 0 _
                    Select order) _
                    .ToList()
            Assert.IsTrue(expected.SequenceEqual(result))
        End Sub

        <Test()>
        Public Sub NowTest()
            Dim result = (From order In Orders _
                    Where Microsoft.VisualBasic.DateAndTime.Now > order.OrderDate.Value _
                    Select order) _
                    .ToList()
            Dim expected = (From order In Orders.ToList _
                    Where Microsoft.VisualBasic.DateAndTime.Now > order.OrderDate.Value _
                    Select order) _
                    .ToList()
            Assert.IsTrue(expected.SequenceEqual(result))
        End Sub

        <Test()>
        Public Sub YearTest()
            Dim result = (From order In Orders _
                    Where Microsoft.VisualBasic.DateAndTime.Year(order.OrderDate.Value) > 0 _
                    Select order) _
                    .ToList()
            Dim expected = (From order In Orders.ToList _
                    Where Microsoft.VisualBasic.DateAndTime.Year(order.OrderDate.Value) > 0 _
                    Select order) _
                    .ToList()
            Assert.IsTrue(expected.SequenceEqual(result))
        End Sub

        <Test()>
        Public Sub MonthTest()
            Dim result = (From order In Orders _
                    Where Microsoft.VisualBasic.DateAndTime.Month(order.OrderDate.Value) > 0 _
                    Select order) _
                    .ToList()
            Dim expected = (From order In Orders.ToList _
                    Where Microsoft.VisualBasic.DateAndTime.Month(order.OrderDate.Value) > 0 _
                    Select order) _
                    .ToList()
            Assert.IsTrue(expected.SequenceEqual(result))
        End Sub

        <Test()>
        Public Sub DayTest()
            Dim result = (From order In Orders _
                    Where Microsoft.VisualBasic.DateAndTime.Day(order.OrderDate.Value) > 0 _
                    Select order) _
                    .ToList()
            Dim expected = (From order In Orders.ToList _
                    Where Microsoft.VisualBasic.DateAndTime.Day(order.OrderDate.Value) > 0 _
                    Select order) _
                    .ToList()
            Assert.IsTrue(expected.SequenceEqual(result))
        End Sub

        <Test()>
        Public Sub HourTest()
            Dim result = (From order In Orders _
                    Where Microsoft.VisualBasic.DateAndTime.Hour(order.OrderDate.Value) > 0 _
                    Select order) _
                    .ToList()
            Dim expected = (From order In Orders.ToList _
                    Where Microsoft.VisualBasic.DateAndTime.Hour(order.OrderDate.Value) > 0 _
                    Select order) _
                    .ToList()
            Assert.IsTrue(expected.SequenceEqual(result))
        End Sub

        <Test()>
        Public Sub MinuteTest()
            Dim result = (From order In Orders _
                    Where Microsoft.VisualBasic.DateAndTime.Minute(order.OrderDate.Value) > 0 _
                    Select order) _
                    .ToList()
            Dim expected = (From order In Orders.ToList _
                    Where Microsoft.VisualBasic.DateAndTime.Minute(order.OrderDate.Value) > 0 _
                    Select order) _
                    .ToList()
            Assert.IsTrue(expected.SequenceEqual(result))
        End Sub

        <Test()>
        Public Sub SecondTest()
            Dim result = (From order In Orders _
                    Where Microsoft.VisualBasic.DateAndTime.Second(order.OrderDate.Value) > 0 _
                    Select order) _
                    .ToList()
            Dim expected = (From order In Orders.ToList _
                    Where Microsoft.VisualBasic.DateAndTime.Second(order.OrderDate.Value) > 0 _
                    Select order) _
                    .ToList()
            Assert.IsTrue(expected.SequenceEqual(result))
        End Sub

        <Test()>
        <ExpectedException(GetType(NotSupportedException))>
        Public Sub MonthNameNotSupported1Test()
            Dim result = (From order In Orders _
                    Where Microsoft.VisualBasic.DateAndTime.MonthName(order.OrderDate.Value.Month, True) <> "test" _
                    Select order) _
                    .ToList()
            Dim expected = (From order In Orders.ToList _
                    Where Microsoft.VisualBasic.DateAndTime.MonthName(order.OrderDate.Value.Month, True) <> "test" _
                    Select order) _
                    .ToList()
            Assert.IsTrue(expected.SequenceEqual(result))
        End Sub

        <Test()>
        <ExpectedException(GetType(NotSupportedException))>
        Public Sub MonthNameNotSupported2Test()
            Dim result = (From order In Orders _
                    Where Microsoft.VisualBasic.DateAndTime.MonthName(order.OrderDate.Value.Month, False) <> "test" _
                    Select order) _
                    .ToList()
            Dim expected = (From order In Orders.ToList _
                    Where Microsoft.VisualBasic.DateAndTime.MonthName(order.OrderDate.Value.Month, False) <> "test" _
                    Select order) _
                    .ToList()
            Assert.IsTrue(expected.SequenceEqual(result))
        End Sub

    End Class
End Namespace