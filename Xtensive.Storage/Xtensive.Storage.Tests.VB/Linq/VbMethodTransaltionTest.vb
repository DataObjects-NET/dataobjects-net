Imports Xtensive.Storage.Tests.ObjectModel.NorthwindDO
Imports Xtensive.Storage.Tests.ObjectModel
Imports Xtensive.Storage.Configuration
Imports NUnit.Framework

Namespace Linq
    <TestFixture()>
    Public Class VbMethodTransaltionTest
        Inherits NorthwindDOModelTest

        Public ReadOnly Property Customers As IOrderedQueryable(Of Customer)
            Get
                Return Query.All(Of Customer)().OrderBy(Function(c) c.Id)
            End Get
        End Property

        <Test()>
        Public Sub TrimTest()
            Dim result = (From customer In Customers _
                    Select Microsoft.VisualBasic.Strings.Trim("   prefix " + customer.CompanyName + " suffix  ")).ToList()
            Dim expected = (From customer In Customers.ToList() _
                         Select Microsoft.VisualBasic.Strings.Trim("   prefix " + customer.CompanyName + " suffix  ")) _
                         .ToList()
            Assert.IsTrue(expected.SequenceEqual(result))
        End Sub

        <Test()>
        Public Sub LTrimTest()
            Dim result = (From customer In Query.All(Of Customer)().OrderBy(Function(c) c.Id) _
                    Select Microsoft.VisualBasic.Strings.LTrim("   prefix " + customer.CompanyName + " suffix  ")).ToList()
            Dim expected = (From customer In Customers.ToList() _
                         Select Microsoft.VisualBasic.Strings.LTrim("   prefix " + customer.CompanyName + " suffix  ")) _
                         .ToList()
            Assert.IsTrue(expected.SequenceEqual(result))
        End Sub

        <Test()>
        Public Sub RTrimTest()
            Dim result = (From customer In Query.All(Of Customer)().OrderBy(Function(c) c.Id) _
                    Select Microsoft.VisualBasic.Strings.RTrim("   prefix " + customer.CompanyName + " suffix  ")).ToList()
            Dim expected = (From customer In Customers.ToList() _
                         Select Microsoft.VisualBasic.Strings.RTrim("   prefix " + customer.CompanyName + " suffix  ")) _
                         .ToList()
            Assert.IsTrue(expected.SequenceEqual(result))
        End Sub
    End Class
End Namespace