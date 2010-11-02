Imports Xtensive.Orm.Tests.ObjectModel.NorthwindDO
Imports Xtensive.Orm.Tests.ObjectModel
Imports Xtensive.Orm.Configuration
Imports NUnit.Framework

Namespace Linq
    <TestFixture()>
    Public Class StringTests
        Inherits NorthwindDOModelTest

        Public Shadows ReadOnly Property Customers As IOrderedQueryable(Of Customer)
            Get
                Return Query.All(Of Customer)().OrderBy(Function(c) c.Id)
            End Get
        End Property

        <Test()>
        Public Sub TrimTest()
            Dim result = (From customer In Customers _
                    Where Microsoft.VisualBasic.Strings.Trim("   prefix " + customer.CompanyName + " suffix  ") <> "test" _
                    Select customer) _
                    .ToList()
            Dim expected = (From customer In Customers.ToList() _
                    Where Microsoft.VisualBasic.Strings.Trim("   prefix " + customer.CompanyName + " suffix  ") <> "test" _
                    Select customer) _
                    .ToList()
            Assert.IsTrue(expected.SequenceEqual(result))
        End Sub

        <Test()>
        Public Sub LTrimTest()
            Dim result = (From customer In Customers _
                    Where Microsoft.VisualBasic.Strings.LTrim("   prefix " + customer.CompanyName + " suffix  ") <> "test" _
                    Select customer) _
                    .ToList()
            Dim expected = (From customer In Customers.ToList() _
                    Where Microsoft.VisualBasic.Strings.LTrim("   prefix " + customer.CompanyName + " suffix  ") <> "test" _
                    Select customer) _
                    .ToList()
            Assert.IsTrue(expected.SequenceEqual(result))
        End Sub

        <Test()>
        Public Sub RTrimTest()
            Dim result = (From customer In Customers _
                    Where Microsoft.VisualBasic.Strings.RTrim("   prefix " + customer.CompanyName + " suffix  ") <> "test" _
                    Select customer) _
                    .ToList()
            Dim expected = (From customer In Customers.ToList() _
                    Where Microsoft.VisualBasic.Strings.RTrim("   prefix " + customer.CompanyName + " suffix  ") <> "test" _
                    Select customer) _
                    .ToList()
            Assert.IsTrue(expected.SequenceEqual(result))
        End Sub

        <Test()>
        Public Sub LenTest()
            Dim result = (From customer In Customers _
                    Where Microsoft.VisualBasic.Strings.Len("   prefix " + customer.CompanyName + " suffix  ") > 0 _
                    Select customer) _
                    .ToList()
            Dim expected = (From customer In Customers.ToList() _
                    Where Microsoft.VisualBasic.Strings.Len("   prefix " + customer.CompanyName + " suffix  ") <> 0 _
                    Select customer) _
                    .ToList()
            Assert.IsTrue(expected.SequenceEqual(result))
        End Sub

        <Test()>
        Public Sub LeftTest()
            Dim result = (From customer In Customers _
                    Where Microsoft.VisualBasic.Strings.Left("   prefix " + customer.CompanyName + " suffix  ", 2) <> "test" _
                    Select customer) _
                    .ToList()
            Dim expected = (From customer In Customers.ToList() _
                    Where Microsoft.VisualBasic.Strings.Left("   prefix " + customer.CompanyName + " suffix  ", 2) <> "test" _
                    Select customer) _
                    .ToList()
            Assert.IsTrue(expected.SequenceEqual(result))
        End Sub


        <Test()>
        Public Sub RightTest()
            Dim result = (From customer In Customers _
                    Where Microsoft.VisualBasic.Strings.Right("   prefix " + customer.CompanyName + " suffix  ", 2) <> "test" _
                    Select customer) _
                    .ToList()
            Dim expected = (From customer In Customers.ToList() _
                    Where Microsoft.VisualBasic.Strings.Right("   prefix " + customer.CompanyName + " suffix  ", 2) <> "test" _
                    Select customer) _
                    .ToList()
            Assert.IsTrue(expected.SequenceEqual(result))
        End Sub

        <Test()>
        Public Sub Mid1Test()
            Dim result = (From customer In Customers _
                    Where Microsoft.VisualBasic.Strings.Mid("   prefix " + customer.CompanyName + " suffix  ", 2) <> "test" _
                    Select customer) _
                    .ToList()
            Dim expected = (From customer In Customers.ToList() _
                    Where Microsoft.VisualBasic.Strings.Mid("   prefix " + customer.CompanyName + " suffix  ", 2) <> "test" _
                    Select customer) _
                    .ToList()
            Assert.IsTrue(expected.SequenceEqual(result))
        End Sub

        <Test()>
        Public Sub Mid2Test()
            Dim result = (From customer In Customers _
                    Where Microsoft.VisualBasic.Strings.Mid("   prefix " + customer.CompanyName + " suffix  ", 2, 1) <> "test" _
                    Select customer) _
                    .ToList()
            Dim expected = (From customer In Customers.ToList() _
                    Where Microsoft.VisualBasic.Strings.Mid("   prefix " + customer.CompanyName + " suffix  ", 2, 1) <> "test" _
                    Select customer) _
                    .ToList()
            Assert.IsTrue(expected.SequenceEqual(result))
        End Sub

        <Test()>
        Public Sub UCaseTest()
            Dim result = (From customer In Customers _
                    Where Microsoft.VisualBasic.Strings.UCase("   prefix " + customer.CompanyName + " suffix  ") <> "test" _
                    Select customer) _
                    .ToList()
            Dim expected = (From customer In Customers.ToList() _
                    Where Microsoft.VisualBasic.Strings.UCase("   prefix " + customer.CompanyName + " suffix  ") <> "test" _
                    Select customer) _
                    .ToList()
            Assert.IsTrue(expected.SequenceEqual(result))
        End Sub

        <Test()>
        Public Sub LCaseTest()
            Dim result = (From customer In Customers _
                    Where Microsoft.VisualBasic.Strings.LCase("   prefix " + customer.CompanyName + " suffix  ") <> "test" _
                    Select customer) _
                    .ToList()
            Dim expected = (From customer In Customers.ToList() _
                    Where Microsoft.VisualBasic.Strings.LCase("   prefix " + customer.CompanyName + " suffix  ") <> "test" _
                    Select customer) _
                    .ToList()
            Assert.IsTrue(expected.SequenceEqual(result))
        End Sub
    End Class
End Namespace