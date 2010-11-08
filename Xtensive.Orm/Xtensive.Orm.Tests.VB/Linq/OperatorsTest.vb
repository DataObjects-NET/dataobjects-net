Imports Xtensive.Orm.Tests.ObjectModel
Imports Xtensive.Orm.Tests.ObjectModel.NorthwindDO
Imports NUnit.Framework

Namespace Linq
    <TestFixture()>
    Public Class OperatorsTest
        Inherits NorthwindDOModelTest

        Public Shadows ReadOnly Property Customers As IOrderedQueryable(Of Customer)
            Get
                Return Query.All(Of Customer)().OrderBy(Function(c) c.Id)
            End Get
        End Property

        <Test()>
        Public Sub CompareString1Test()
            Dim result = (From customer In Customers _
                    Where customer.CompanyName > "test" _
                    Select customer) _
                    .ToList()
            ' SQL compares CaseInsensitive by default
            Dim expected = (From customer In Customers.ToList() _
                    Where Microsoft.VisualBasic.CompilerServices.Operators.CompareString(customer.CompanyName, "test", True) > 0 _
                    Select customer) _
                    .ToList()
            Assert.IsTrue(expected.SequenceEqual(result))
        End Sub

        <Test()>
        Public Sub CompareString2Test()
            Dim C = Customers.ToList()
            Dim result = (From customer In Customers _
                    Select customer.CompanyName > "test") _
                    .Where(Function(i) i > 0) _
                    .ToList()
            ' SQL compares CaseInsensitive by default
            Dim expected = (From customer In Customers.ToList() _
                    Select Microsoft.VisualBasic.CompilerServices.Operators.CompareString(customer.CompanyName, "test", True)) _
                    .Where(Function(i) i > 0) _
                    .ToList()
            Assert.IsTrue(expected.SequenceEqual(result))
        End Sub

        <Test()>
        Public Sub BooleanTest()
            Dim x = GetType(Microsoft.VisualBasic.CompilerServices.Operators).AssemblyQualifiedName

            Dim C = Customers.ToList()
            Dim result = (From customer In Customers _
                    Select customer.Id > 10) _
                    .Where(Function(i) i > 0) _
                    .ToList()
            ' SQL compares CaseInsensitive by default
            Dim expected = (From customer In Customers.ToList() _
                    Select customer.Id > 10) _
                    .Where(Function(i) i > 0) _
                    .ToList()
            Assert.IsTrue(expected.SequenceEqual(result))
        End Sub

    End Class
End Namespace