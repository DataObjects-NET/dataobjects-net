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