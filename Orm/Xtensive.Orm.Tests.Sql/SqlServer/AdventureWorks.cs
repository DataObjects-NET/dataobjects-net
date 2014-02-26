// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.

using System;
using System.Data;
using NUnit.Framework;
using Xtensive.Sql;
using Xtensive.Sql.Model;
using Xtensive.Sql.Dml;
using Constraint = Xtensive.Sql.Model.Constraint;

namespace Xtensive.Orm.Tests.Sql.SqlServer
{
  [TestFixture]
  public abstract class AdventureWorks
  {
    protected struct DbCommandExecutionResult
    {
      public int FieldCount;
      public string[] FieldNames;
      public int RowCount;

      public override string ToString()
      {
        if (FieldNames==null)
          FieldNames = new string[0];
        return string.Format("Fields: '{0}'; Rows: {1}", string.Join("', '", FieldNames), RowCount);
      }
    }

    protected static DbCommandExecutionResult GetExecuteDataReaderResult(IDbCommand cmd)
    {
      DbCommandExecutionResult result = new DbCommandExecutionResult();
      try {
        cmd.Transaction = cmd.Connection.BeginTransaction();
        int rowCount = 0;
        int fieldCount = 0;
        string[] fieldNames = new string[0];
        using (IDataReader reader = cmd.ExecuteReader()) {
          while (reader.Read()) {
            if (rowCount==0) {
              fieldCount = reader.FieldCount;
              fieldNames = new string[fieldCount];
              for (int i = 0; i<fieldCount; i++) {
                fieldNames[i] = reader.GetName(i);
              }
            }
            rowCount++;
          }
        }
        result.RowCount = rowCount;
        result.FieldCount = fieldCount;
        result.FieldNames = fieldNames;
      }
//      catch (Exception e) {
//        Console.WriteLine(e);
//      }
      finally {
        cmd.Transaction.Rollback();
      }
      return result;
    }

    protected static DbCommandExecutionResult GetExecuteNonQueryResult(IDbCommand cmd)
    {
      DbCommandExecutionResult result = new DbCommandExecutionResult();
      try {
        cmd.Transaction = cmd.Connection.BeginTransaction();
        result.RowCount = cmd.ExecuteNonQuery();
      }
//      catch (Exception e) {
//        Console.WriteLine(e);
//      }
      finally {
        cmd.Transaction.Rollback();
      }
      return result;
    }

    protected string Url { get { return TestConnectionInfoProvider.GetConnectionUrl(); } }
    public Catalog Catalog { get; protected set; }

    protected virtual void CheckRequirements()
    {
      Require.ProviderIs(StorageProvider.SqlServer);
    }

    [TestFixtureSetUp]
    public virtual void SetUp()
    {
      CheckRequirements();
      var driver = TestSqlDriver.Create(Url);
      using (var connection = driver.CreateConnection())
      {
        connection.Open();
        Catalog = driver.ExtractCatalog(connection);
        connection.Close();
      }
//      BinaryModelProvider bmp = new BinaryModelProvider(@"C:/Debug/AdventureWorks.bin");
//      model = Database.Model.Build(bmp);
//
//      if (model!=null)
//        return;

      Catalog = new Catalog("AdventureWorks");

      Catalog.CreateSchema("HumanResources");
      Catalog.CreateSchema("Person");
      Catalog.CreateSchema("Production");
      Catalog.CreateSchema("Purchasing");
      Catalog.CreateSchema("Sales");

      Catalog.Schemas["HumanResources"].CreateCollation("Traditional_Spanish_CI_AI");
      Catalog.Schemas["Person"].CreateCollation("Traditional_Spanish_CI_AI");
      Catalog.Schemas["Production"].CreateCollation("Traditional_Spanish_CI_AI");
      Catalog.Schemas["Purchasing"].CreateCollation("Traditional_Spanish_CI_AI");
      Catalog.Schemas["Sales"].CreateCollation("Traditional_Spanish_CI_AI");

      Catalog.Schemas["HumanResources"].CreateCollation("SQL_Latin1_General_CP1_CI_AS");
      Catalog.Schemas["Person"].CreateCollation("SQL_Latin1_General_CP1_CI_AS");
      Catalog.Schemas["Production"].CreateCollation("SQL_Latin1_General_CP1_CI_AS");
      Catalog.Schemas["Purchasing"].CreateCollation("SQL_Latin1_General_CP1_CI_AS");
      Catalog.Schemas["Sales"].CreateCollation("SQL_Latin1_General_CP1_CI_AS");

      Table t;
      View v;
      TableColumn c;
      Constraint cs;

      t = Catalog.Schemas["Production"].CreateTable("TransactionHistoryArchive");
      t.CreateColumn("TransactionID", new SqlValueType(SqlType.Int32));
      t.CreateColumn("ProductID", new SqlValueType(SqlType.Int32));
      t.CreateColumn("ReferenceOrderID", new SqlValueType(SqlType.Int32));
      t.CreateColumn("ReferenceOrderLineID", new SqlValueType(SqlType.Int32));
      t.CreateColumn("TransactionDate", new SqlValueType(SqlType.DateTime));
      t.CreateColumn("TransactionType", new SqlValueType(SqlType.VarChar));
      t.CreateColumn("Quantity", new SqlValueType(SqlType.Int32));
      t.CreateColumn("ActualCost", new SqlValueType("money"));
      t.CreateColumn("ModifiedDate", new SqlValueType(SqlType.DateTime));

      t = Catalog.Schemas["Sales"].CreateTable("CreditCard");
      t.CreateColumn("CreditCardID", new SqlValueType(SqlType.Int32));
      t.CreateColumn("CardType", new SqlValueType(SqlType.VarChar));
      t.CreateColumn("CardNumber", new SqlValueType(SqlType.VarChar));
      t.CreateColumn("ExpMonth", new SqlValueType(SqlType.UInt8));
      t.CreateColumn("ExpYear", new SqlValueType(SqlType.Int16));
      t.CreateColumn("ModifiedDate", new SqlValueType(SqlType.DateTime));

      t = Catalog.Schemas["Production"].CreateTable("Document");
      t.CreateColumn("DocumentID", new SqlValueType(SqlType.Int32));
      t.CreateColumn("Title", new SqlValueType(SqlType.VarChar));
      t.CreateColumn("FileName", new SqlValueType(SqlType.VarChar));
      t.CreateColumn("FileExtension", new SqlValueType(SqlType.VarChar));
      t.CreateColumn("Revision", new SqlValueType(SqlType.VarChar));
      t.CreateColumn("ChangeNumber", new SqlValueType(SqlType.Int32));
      t.CreateColumn("Status", new SqlValueType(SqlType.UInt8));
      t.CreateColumn("DocumentSummary", new SqlValueType(SqlType.VarChar));
      t.CreateColumn("Document", new SqlValueType(SqlType.VarBinaryMax));
      t.CreateColumn("ModifiedDate", new SqlValueType(SqlType.DateTime));

      t = Catalog.Schemas["Production"].CreateTable("Illustration");
      t.CreateColumn("IllustrationID", new SqlValueType(SqlType.Int32));
      t.CreateColumn("Diagram", new SqlValueType("xml"));
      t.CreateColumn("ModifiedDate", new SqlValueType(SqlType.DateTime));

      t = Catalog.Schemas["Production"].CreateTable("ProductDescription");
      t.CreateColumn("ProductDescriptionID", new SqlValueType(SqlType.Int32));
      t.CreateColumn("Description", new SqlValueType(SqlType.VarChar));
      t.CreateColumn("ModifiedDate", new SqlValueType(SqlType.DateTime));

      t = Catalog.Schemas["Sales"].CreateTable("SpecialOffer");
      t.CreateColumn("SpecialOfferID", new SqlValueType(SqlType.Int32));
      t.CreateColumn("Description", new SqlValueType(SqlType.VarChar));
      t.CreateColumn("DiscountPct", new SqlValueType(SqlType.Int32));
      t.CreateColumn("Type", new SqlValueType(SqlType.VarChar));
      t.CreateColumn("Category", new SqlValueType(SqlType.VarChar));
      t.CreateColumn("StartDate", new SqlValueType(SqlType.DateTime));
      t.CreateColumn("EndDate", new SqlValueType(SqlType.DateTime));
      t.CreateColumn("MinQty", new SqlValueType(SqlType.Int32));
      t.CreateColumn("MaxQty", new SqlValueType(SqlType.Int32));
      t.CreateColumn("ModifiedDate", new SqlValueType(SqlType.DateTime));

      t = Catalog.Schemas["Production"].CreateTable("ProductPhoto");
      t.CreateColumn("ProductPhotoID", new SqlValueType(SqlType.Int32));
      t.CreateColumn("ThumbNailPhoto", new SqlValueType(SqlType.VarBinaryMax));
      t.CreateColumn("ThumbnailPhotoFileName", new SqlValueType(SqlType.VarChar));
      t.CreateColumn("LargePhoto", new SqlValueType(SqlType.VarBinaryMax));
      t.CreateColumn("LargePhotoFileName", new SqlValueType(SqlType.VarChar));
      t.CreateColumn("ModifiedDate", new SqlValueType(SqlType.DateTime));

      t = Catalog.Schemas["Sales"].CreateTable("Customer");
      t.CreateColumn("CustomerID", new SqlValueType(SqlType.Int32));
      t.CreateColumn("TerritoryID", new SqlValueType(SqlType.Int32));
      t.CreateColumn("AccountNumber", new SqlValueType(SqlType.VarChar));
      t.CreateColumn("CustomerType", new SqlValueType(SqlType.VarChar));
      t.CreateColumn("ModifiedDate", new SqlValueType(SqlType.DateTime));

      t = Catalog.Schemas["Sales"].CreateTable("CustomerAddress");
      t.CreateColumn("CustomerID", new SqlValueType(SqlType.Int32));
      t.CreateColumn("AddressID", new SqlValueType(SqlType.Int32));
      t.CreateColumn("AddressTypeID", new SqlValueType(SqlType.Int32));
      t.CreateColumn("ModifiedDate", new SqlValueType(SqlType.DateTime));

      t = Catalog.Schemas["HumanResources"].CreateTable("EmployeeAddress");
      t.CreateColumn("EmployeeID", new SqlValueType(SqlType.Int32));
      t.CreateColumn("AddressID", new SqlValueType(SqlType.Int32));
      t.CreateColumn("ModifiedDate", new SqlValueType(SqlType.DateTime));

      t = Catalog.Schemas["Purchasing"].CreateTable("VendorAddress");
      t.CreateColumn("VendorID", new SqlValueType(SqlType.Int32));
      t.CreateColumn("AddressID", new SqlValueType(SqlType.Int32));
      t.CreateColumn("AddressTypeID", new SqlValueType(SqlType.Int32));
      t.CreateColumn("ModifiedDate", new SqlValueType(SqlType.DateTime));

      t = Catalog.Schemas["Purchasing"].CreateTable("ProductVendor");
      t.CreateColumn("ProductID", new SqlValueType(SqlType.Int32));
      t.CreateColumn("VendorID", new SqlValueType(SqlType.Int32));
      t.CreateColumn("AverageLeadTime", new SqlValueType(SqlType.Int32));
      t.CreateColumn("StandardPrice", new SqlValueType("money"));
      t.CreateColumn("LastReceiptCost", new SqlValueType("money"));
      t.CreateColumn("LastReceiptDate", new SqlValueType(SqlType.DateTime));
      t.CreateColumn("MinOrderQty", new SqlValueType(SqlType.Int32));
      t.CreateColumn("MaxOrderQty", new SqlValueType(SqlType.Int32));
      t.CreateColumn("OnOrderQty", new SqlValueType(SqlType.Int32));
      t.CreateColumn("UnitMeasureCode", new SqlValueType(SqlType.VarChar));
      t.CreateColumn("ModifiedDate", new SqlValueType(SqlType.DateTime));

      t = Catalog.Schemas["Production"].CreateTable("BillOfMaterials");
      t.CreateColumn("BillOfMaterialsID", new SqlValueType(SqlType.Int32));
      t.CreateColumn("ProductAssemblyID", new SqlValueType(SqlType.Int32));
      t.CreateColumn("ComponentID", new SqlValueType(SqlType.Int32));
      t.CreateColumn("StartDate", new SqlValueType(SqlType.DateTime));
      t.CreateColumn("EndDate", new SqlValueType(SqlType.DateTime));
      t.CreateColumn("UnitMeasureCode", new SqlValueType(SqlType.VarChar));
      t.CreateColumn("BOMLevel", new SqlValueType(SqlType.Int16));
      t.CreateColumn("PerAssemblyQty", new SqlValueType(SqlType.Decimal));
      t.CreateColumn("ModifiedDate", new SqlValueType(SqlType.DateTime));

      t = Catalog.Schemas["Purchasing"].CreateTable("PurchaseOrderHeader");
      t.CreateColumn("PurchaseOrderID", new SqlValueType(SqlType.Int32));
      t.CreateColumn("RevisionNumber", new SqlValueType(SqlType.UInt8));
      t.CreateColumn("Status", new SqlValueType(SqlType.UInt8));
      t.CreateColumn("EmployeeID", new SqlValueType(SqlType.Int32));
      t.CreateColumn("VendorID", new SqlValueType(SqlType.Int32));
      t.CreateColumn("ShipMethodID", new SqlValueType(SqlType.Int32));
      t.CreateColumn("OrderDate", new SqlValueType(SqlType.DateTime));
      t.CreateColumn("ShipDate", new SqlValueType(SqlType.DateTime));
      t.CreateColumn("SubTotal", new SqlValueType("money"));
      t.CreateColumn("TaxAmt", new SqlValueType("money"));
      t.CreateColumn("Freight", new SqlValueType("money"));
      t.CreateColumn("TotalDue", new SqlValueType("money"));
      t.CreateColumn("ModifiedDate", new SqlValueType(SqlType.DateTime));

      t = Catalog.Schemas["Purchasing"].CreateTable("VendorContact");
      t.CreateColumn("VendorID", new SqlValueType(SqlType.Int32));
      t.CreateColumn("ContactID", new SqlValueType(SqlType.Int32));
      t.CreateColumn("ContactTypeID", new SqlValueType(SqlType.Int32));
      t.CreateColumn("ModifiedDate", new SqlValueType(SqlType.DateTime));

      t = Catalog.Schemas["Sales"].CreateTable("ContactCreditCard");
      t.CreateColumn("ContactID", new SqlValueType(SqlType.Int32));
      t.CreateColumn("CreditCardID", new SqlValueType(SqlType.Int32));
      t.CreateColumn("ModifiedDate", new SqlValueType(SqlType.DateTime));

      t = Catalog.Schemas["Sales"].CreateTable("StoreContact");
      t.CreateColumn("CustomerID", new SqlValueType(SqlType.Int32));
      t.CreateColumn("ContactID", new SqlValueType(SqlType.Int32));
      t.CreateColumn("ContactTypeID", new SqlValueType(SqlType.Int32));
      t.CreateColumn("ModifiedDate", new SqlValueType(SqlType.DateTime));

      t = Catalog.Schemas["Purchasing"].CreateTable("PurchaseOrderDetail");
      t.CreateColumn("PurchaseOrderID", new SqlValueType(SqlType.Int32));
      t.CreateColumn("PurchaseOrderDetailID", new SqlValueType(SqlType.Int32));
      t.CreateColumn("DueDate", new SqlValueType(SqlType.Int32));
      t.CreateColumn("OrderQty", new SqlValueType(SqlType.Int32));
      t.CreateColumn("ProductID", new SqlValueType(SqlType.Int32));
      t.CreateColumn("UnitPrice", new SqlValueType(SqlType.Int32));
      c = t.CreateColumn("LineTotal");
      SqlTableRef pod = SqlDml.TableRef(t);
      c.Expression = pod["OrderQty"]*pod["UnitPrice"];
      c.IsPersisted = false;
      t.CreateColumn("ReceivedQty", new SqlValueType(SqlType.Int32));
      t.CreateColumn("RejectedQty", new SqlValueType(SqlType.Int32));
      t.CreateColumn("StockedQty", new SqlValueType(SqlType.Int32));
      t.CreateColumn("ModifiedDate", new SqlValueType(SqlType.DateTime));

      t = Catalog.Schemas["Production"].CreateTable("WorkOrderRouting");
      t.CreateColumn("WorkOrderID", new SqlValueType(SqlType.Int32));
      t.CreateColumn("ProductID", new SqlValueType(SqlType.Int32));
      t.CreateColumn("OperationSequence", new SqlValueType(SqlType.Int16));
      t.CreateColumn("LocationID", new SqlValueType(SqlType.Int32));
      t.CreateColumn("ScheduledStartDate", new SqlValueType(SqlType.DateTime));
      t.CreateColumn("ScheduledEndDate", new SqlValueType(SqlType.DateTime));
      t.CreateColumn("ActualStartDate", new SqlValueType(SqlType.DateTime));
      t.CreateColumn("ActualEndDate", new SqlValueType(SqlType.DateTime));
      t.CreateColumn("ActualResourceHrs", new SqlValueType(SqlType.Decimal));
      t.CreateColumn("PlannedCost", new SqlValueType("money"));
      t.CreateColumn("ActualCost", new SqlValueType("money"));
      t.CreateColumn("ModifiedDate", new SqlValueType(SqlType.DateTime));

      t = Catalog.Schemas["Sales"].CreateTable("CountryRegionCurrency");
      t.CreateColumn("CountryRegionCode", new SqlValueType(SqlType.VarChar));
      t.CreateColumn("CurrencyCode", new SqlValueType(SqlType.VarChar));
      t.CreateColumn("ModifiedDate", new SqlValueType(SqlType.DateTime));

      t = Catalog.Schemas["Production"].CreateTable("ProductModelProductDescriptionCulture");
      t.CreateColumn("ProductModelID", new SqlValueType(SqlType.Int32));
      t.CreateColumn("ProductDescriptionID", new SqlValueType(SqlType.Int32));
      t.CreateColumn("CultureID", new SqlValueType(SqlType.VarChar));
      t.CreateColumn("ModifiedDate", new SqlValueType(SqlType.DateTime));

      t = Catalog.Schemas["Sales"].CreateTable("CurrencyRate");
      t.CreateColumn("CurrencyRateID", new SqlValueType(SqlType.Int32));
      t.CreateColumn("CurrencyRateDate", new SqlValueType(SqlType.DateTime));
      t.CreateColumn("FromCurrencyCode", new SqlValueType(SqlType.VarChar));
      t.CreateColumn("ToCurrencyCode", new SqlValueType(SqlType.VarChar));
      t.CreateColumn("AverageRate", new SqlValueType("money"));
      t.CreateColumn("EndOfDayRate", new SqlValueType("money"));
      t.CreateColumn("ModifiedDate", new SqlValueType(SqlType.DateTime));

      t = Catalog.Schemas["Sales"].CreateTable("SalesOrderDetail");
      t.CreateColumn("SalesOrderID", new SqlValueType(SqlType.Int32));
      t.CreateColumn("SalesOrderDetailID", new SqlValueType(SqlType.Int32));
      t.CreateColumn("CarrierTrackingNumber", new SqlValueType(SqlType.VarChar));
      t.CreateColumn("OrderQty", new SqlValueType(SqlType.Int16));
      t.CreateColumn("ProductID", new SqlValueType(SqlType.Int32));
      t.CreateColumn("SpecialOfferID", new SqlValueType(SqlType.Int32));
      t.CreateColumn("UnitPrice", new SqlValueType("money"));
      t.CreateColumn("UnitPriceDiscount", new SqlValueType("money"));
      t.CreateColumn("LineTotal", new SqlValueType(SqlType.Int32));
      t.CreateColumn("ModifiedDate", new SqlValueType(SqlType.DateTime));

      t = Catalog.Schemas["Sales"].CreateTable("SalesOrderHeaderSalesReason");
      t.CreateColumn("SalesOrderID", new SqlValueType(SqlType.Int32));
      t.CreateColumn("SalesReasonID", new SqlValueType(SqlType.Int32));
      t.CreateColumn("ModifiedDate", new SqlValueType(SqlType.DateTime));

      t = Catalog.Schemas["HumanResources"].CreateTable("EmployeeDepartmentHistory");
      t.CreateColumn("EmployeeID", new SqlValueType(SqlType.Int32));
      t.CreateColumn("DepartmentID", new SqlValueType(SqlType.Int16));
      t.CreateColumn("ShiftID", new SqlValueType(SqlType.DateTime));
      t.CreateColumn("StartDate", new SqlValueType(SqlType.DateTime));
      t.CreateColumn("EndDate", new SqlValueType(SqlType.DateTime));
      t.CreateColumn("ModifiedDate", new SqlValueType(SqlType.DateTime));

      t = Catalog.Schemas["Production"].CreateTable("ProductDocument");
      t.CreateColumn("ProductID", new SqlValueType(SqlType.Int32));
      t.CreateColumn("DocumentID", new SqlValueType(SqlType.Int32));
      t.CreateColumn("ModifiedDate", new SqlValueType(SqlType.DateTime));

      t = Catalog.Schemas["HumanResources"].CreateTable("EmployeePayHistory");
      t.CreateColumn("EmployeeID", new SqlValueType(SqlType.Int32));
      t.CreateColumn("RateChangeDate", new SqlValueType(SqlType.DateTime));
      t.CreateColumn("Rate", new SqlValueType("money"));
      t.CreateColumn("PayFrequency", new SqlValueType(SqlType.UInt8));
      t.CreateColumn("ModifiedDate", new SqlValueType(SqlType.DateTime));

      t = Catalog.Schemas["Sales"].CreateTable("SalesPerson");
      t.CreateColumn("SalesPersonID", new SqlValueType(SqlType.Int32));
      t.CreateColumn("TerritoryID", new SqlValueType(SqlType.Int32));
      t.CreateColumn("SalesQuota", new SqlValueType("money"));
      t.CreateColumn("Bonus", new SqlValueType("money"));
      t.CreateColumn("CommissionPct", new SqlValueType("money"));
      t.CreateColumn("SalesYTD", new SqlValueType("money"));
      t.CreateColumn("SalesLastYear", new SqlValueType("money"));
      t.CreateColumn("ModifiedDate", new SqlValueType(SqlType.DateTime));

      t = Catalog.Schemas["Sales"].CreateTable("SalesPersonQuotaHistory");
      t.CreateColumn("SalesPersonID", new SqlValueType(SqlType.Int32));
      t.CreateColumn("QuotaDate", new SqlValueType(SqlType.DateTime));
      t.CreateColumn("SalesQuota", new SqlValueType("money"));
      t.CreateColumn("ModifiedDate", new SqlValueType(SqlType.DateTime));

      t = Catalog.Schemas["Sales"].CreateTable("SalesTerritoryHistory");
      t.CreateColumn("SalesPersonID", new SqlValueType(SqlType.Int32));
      t.CreateColumn("TerritoryID", new SqlValueType(SqlType.Int32));
      t.CreateColumn("StartDate", new SqlValueType(SqlType.DateTime));
      t.CreateColumn("EndDate", new SqlValueType(SqlType.DateTime));
      t.CreateColumn("ModifiedDate", new SqlValueType(SqlType.DateTime));

      t = Catalog.Schemas["Production"].CreateTable("ProductModelIllustration");
      t.CreateColumn("ProductModelID", new SqlValueType(SqlType.Int32));
      t.CreateColumn("IllustrationID", new SqlValueType(SqlType.Int32));
      t.CreateColumn("ModifiedDate", new SqlValueType(SqlType.DateTime));

      t = Catalog.Schemas["Production"].CreateTable("ProductInventory");
      t.CreateColumn("ProductID", new SqlValueType(SqlType.Int32));
      t.CreateColumn("LocationID", new SqlValueType(SqlType.Int32));
      t.CreateColumn("Shelf", new SqlValueType(SqlType.VarChar));
      t.CreateColumn("Bin", new SqlValueType(SqlType.UInt8));
      t.CreateColumn("Quantity", new SqlValueType(SqlType.UInt8));
      t.CreateColumn("ModifiedDate", new SqlValueType(SqlType.DateTime));

      t = Catalog.Schemas["Production"].CreateTable("WorkOrder");
      t.CreateColumn("WorkOrderID", new SqlValueType(SqlType.Int32));
      t.CreateColumn("ProductID", new SqlValueType(SqlType.Int32));
      t.CreateColumn("OrderQty", new SqlValueType(SqlType.Int32));
      t.CreateColumn("StockedQty", new SqlValueType(SqlType.Int32));
      t.CreateColumn("ScrappedQty", new SqlValueType(SqlType.Int16));
      t.CreateColumn("StartDate", new SqlValueType(SqlType.DateTime));
      t.CreateColumn("EndDate", new SqlValueType(SqlType.DateTime));
      t.CreateColumn("DueDate", new SqlValueType(SqlType.DateTime));
      t.CreateColumn("ScrapReasonID", new SqlValueType(SqlType.Int16));
      t.CreateColumn("ModifiedDate", new SqlValueType(SqlType.DateTime));

      t = Catalog.Schemas["Production"].CreateTable("TransactionHistory");
      t.CreateColumn("TransactionID", new SqlValueType(SqlType.Int32));
      t.CreateColumn("ProductID", new SqlValueType(SqlType.Int32));
      t.CreateColumn("ReferenceOrderID", new SqlValueType(SqlType.Int32));
      t.CreateColumn("ReferenceOrderLineID", new SqlValueType(SqlType.Int32));
      t.CreateColumn("TransactionDate", new SqlValueType(SqlType.DateTime));
      t.CreateColumn("TransactionType", new SqlValueType(SqlType.VarChar));
      t.CreateColumn("Quantity", new SqlValueType(SqlType.Int32));
      t.CreateColumn("ActualCost", new SqlValueType("money"));
      t.CreateColumn("ModifiedDate", new SqlValueType(SqlType.DateTime));

      t = Catalog.Schemas["Sales"].CreateTable("ShoppingCartItem");
      t.CreateColumn("ShoppingCartItemID", new SqlValueType(SqlType.Int32));
      t.CreateColumn("ShoppingCartID", new SqlValueType(SqlType.VarChar));
      t.CreateColumn("Quantity", new SqlValueType(SqlType.Int32));
      t.CreateColumn("ProductID", new SqlValueType(SqlType.Int32));
      t.CreateColumn("DateCreated", new SqlValueType(SqlType.DateTime));
      t.CreateColumn("ModifiedDate", new SqlValueType(SqlType.DateTime));

      t = Catalog.Schemas["Production"].CreateTable("ProductListPriceHistory");
      t.CreateColumn("ProductID", new SqlValueType(SqlType.Int32));
      t.CreateColumn("StartDate", new SqlValueType(SqlType.DateTime));
      t.CreateColumn("EndDate", new SqlValueType(SqlType.DateTime));
      t.CreateColumn("ListPrice", new SqlValueType("money"));
      t.CreateColumn("ModifiedDate", new SqlValueType(SqlType.DateTime));

      t = Catalog.Schemas["Sales"].CreateTable("SpecialOfferProduct");
      t.CreateColumn("SpecialOfferID", new SqlValueType(SqlType.Int32));
      t.CreateColumn("ProductID", new SqlValueType(SqlType.Int32));
      t.CreateColumn("ModifiedDate", new SqlValueType(SqlType.DateTime));

      t = Catalog.Schemas["Production"].CreateTable("ProductCostHistory");
      t.CreateColumn("ProductID", new SqlValueType(SqlType.Int32));
      t.CreateColumn("StartDate", new SqlValueType(SqlType.DateTime));
      t.CreateColumn("EndDate", new SqlValueType(SqlType.DateTime));
      t.CreateColumn("StandardCost", new SqlValueType("money"));
      t.CreateColumn("ModifiedDate", new SqlValueType(SqlType.DateTime));

      t = Catalog.Schemas["Person"].CreateTable("Address");
      t.CreateColumn("AddressID", new SqlValueType(SqlType.Int32));
      t.CreateColumn("AddressLine1", new SqlValueType(SqlType.VarChar));
      t.CreateColumn("AddressLine2", new SqlValueType(SqlType.VarChar));
      t.CreateColumn("City", new SqlValueType(SqlType.VarChar));
      t.CreateColumn("StateProvinceID", new SqlValueType(SqlType.Int32));
      t.CreateColumn("PostalCode", new SqlValueType(SqlType.VarChar));
      t.CreateColumn("ModifiedDate", new SqlValueType(SqlType.DateTime));

      t = Catalog.Schemas["Purchasing"].CreateTable("Vendor");
      t.CreateColumn("VendorID", new SqlValueType(SqlType.Int32));
      t.CreateColumn("AccountNumber", new SqlValueType(SqlType.VarChar));
      t.CreateColumn("Name", new SqlValueType(SqlType.VarChar));
      t.CreateColumn("CreditRating", new SqlValueType(SqlType.UInt8));
      t.CreateColumn("PreferredVendorStatus", new SqlValueType(SqlType.Boolean));
      t.CreateColumn("ActiveFlag", new SqlValueType(SqlType.Boolean));
      t.CreateColumn("PurchasingWebServiceURL", new SqlValueType(SqlType.VarChar));
      t.CreateColumn("ModifiedDate", new SqlValueType(SqlType.DateTime));

      t = Catalog.Schemas["Sales"].CreateTable("SalesOrderHeader");
      t.CreateColumn("SalesOrderID", new SqlValueType(SqlType.Int32));
      t.CreateColumn("RevisionNumber", new SqlValueType(SqlType.UInt8));
      t.CreateColumn("OrderDate", new SqlValueType(SqlType.DateTime));
      t.CreateColumn("DueDate", new SqlValueType(SqlType.DateTime));
      t.CreateColumn("ShipDate", new SqlValueType(SqlType.DateTime));
      t.CreateColumn("Status", new SqlValueType(SqlType.UInt8));
      t.CreateColumn("OnlineOrderFlag", new SqlValueType(SqlType.Boolean));
      t.CreateColumn("SalesOrderNumber", new SqlValueType(SqlType.VarChar));
      t.CreateColumn("PurchaseOrderNumber", new SqlValueType(SqlType.VarChar));
      t.CreateColumn("AccountNumber", new SqlValueType(SqlType.VarChar));
      t.CreateColumn("CustomerID", new SqlValueType(SqlType.Int32));
      t.CreateColumn("ContactID", new SqlValueType(SqlType.Int32));
      t.CreateColumn("SalesPersonID", new SqlValueType(SqlType.Int32));
      t.CreateColumn("TerritoryID", new SqlValueType(SqlType.Int32));
      t.CreateColumn("BillToAddressID", new SqlValueType(SqlType.Int32));
      t.CreateColumn("ShipToAddressID", new SqlValueType(SqlType.Int32));
      t.CreateColumn("ShipMethodID", new SqlValueType(SqlType.Int32));
      t.CreateColumn("CreditCardID", new SqlValueType(SqlType.Int32));
      t.CreateColumn("CreditCardApprovalCode", new SqlValueType(SqlType.VarChar));
      t.CreateColumn("CurrencyRateID", new SqlValueType(SqlType.Int32));
      t.CreateColumn("SubTotal", new SqlValueType("money"));
      t.CreateColumn("TaxAmt", new SqlValueType("money"));
      t.CreateColumn("Freight", new SqlValueType("money"));
      t.CreateColumn("TotalDue", new SqlValueType("money"));
      t.CreateColumn("Comment", new SqlValueType(SqlType.VarChar));
      t.CreateColumn("ModifiedDate", new SqlValueType(SqlType.DateTime));

      t = Catalog.Schemas["HumanResources"].CreateTable("Employee");
      t.CreateColumn("EmployeeID", new SqlValueType(SqlType.Int32));
      t.CreateColumn("NationalIDNumber", new SqlValueType(SqlType.VarChar));
      t.CreateColumn("ContactID", new SqlValueType(SqlType.Int32));
      t.CreateColumn("LoginID", new SqlValueType(SqlType.VarChar));
      t.CreateColumn("ManagerID", new SqlValueType(SqlType.Int32));
      t.CreateColumn("Title", new SqlValueType(SqlType.VarChar));
      t.CreateColumn("BirthDate", new SqlValueType(SqlType.DateTime));
      t.CreateColumn("MaritalStatus", new SqlValueType(SqlType.VarChar));
      t.CreateColumn("Gender", new SqlValueType(SqlType.VarChar));
      t.CreateColumn("HireDate", new SqlValueType(SqlType.DateTime));
      t.CreateColumn("SalariedFlag", new SqlValueType(SqlType.Boolean));
      t.CreateColumn("VacationHours", new SqlValueType(SqlType.Int16));
      t.CreateColumn("SickLeaveHours", new SqlValueType(SqlType.Int16));
      t.CreateColumn("CurrentFlag", new SqlValueType(SqlType.Boolean));
      t.CreateColumn("ModifiedDate", new SqlValueType(SqlType.DateTime));

      t = Catalog.Schemas["Production"].CreateTable("ProductProductPhoto");
      t.CreateColumn("ProductID", new SqlValueType(SqlType.Int32));
      t.CreateColumn("ProductPhotoID", new SqlValueType(SqlType.Int32));
      t.CreateColumn("Primary", new SqlValueType(SqlType.VarChar));
      t.CreateColumn("ModifiedDate", new SqlValueType(SqlType.DateTime));

      t = Catalog.Schemas["Person"].CreateTable("StateProvince");
      t.CreateColumn("StateProvinceID", new SqlValueType(SqlType.Int32));
      t.CreateColumn("StateProvinceCode", new SqlValueType(SqlType.VarChar));
      t.CreateColumn("CountryRegionCode", new SqlValueType(SqlType.VarChar));
      t.CreateColumn("IsOnlyStateProvinceFlag", new SqlValueType(SqlType.Boolean));
      t.CreateColumn("Name", new SqlValueType(SqlType.VarChar));
      t.CreateColumn("TerritoryID", new SqlValueType(SqlType.Int32));
      t.CreateColumn("ModifiedDate", new SqlValueType(SqlType.DateTime));

      t = Catalog.Schemas["Production"].CreateTable("ProductModel");
      t.CreateColumn("ProductModelID", new SqlValueType(SqlType.Int32));
      t.CreateColumn("Name", new SqlValueType(SqlType.VarChar));
      t.CreateColumn("CatalogDescription", new SqlValueType("xml"));
      t.CreateColumn("Instructions", new SqlValueType("xml"));
      t.CreateColumn("ModifiedDate", new SqlValueType(SqlType.DateTime));

      t = Catalog.Schemas["Production"].CreateTable("Product");
      c = t.CreateColumn("ProductID", new SqlValueType(SqlType.Int32));
      c.SequenceDescriptor = new SequenceDescriptor(c, 1, 1);
      c.IsNullable = false;
      c = t.CreateColumn("Name", new SqlValueType(SqlType.VarChar, 50));
      c.IsNullable = false;
      c = t.CreateColumn("ProductNumber", new SqlValueType(SqlType.VarChar, 25));
      c.Collation = Catalog.Schemas["Production"].Collations["SQL_Latin1_General_CP1_CI_AS"];
      c.IsNullable = false;
      c = t.CreateColumn("MakeFlag", new SqlValueType(SqlType.Boolean));
      c.DefaultValue = 1;
      c.IsNullable = false;
      c = t.CreateColumn("FinishedGoodsFlag", new SqlValueType(SqlType.Boolean));
      c.DefaultValue = 1;
      c.IsNullable = false;
      c = t.CreateColumn("Color", new SqlValueType(SqlType.VarChar, 15));
      c.Collation = Catalog.Schemas["Production"].Collations["SQL_Latin1_General_CP1_CI_AS"];
      c = t.CreateColumn("SafetyStockLevel", new SqlValueType(SqlType.Int16));
      c.IsNullable = false;
      c = t.CreateColumn("ReorderPoint", new SqlValueType(SqlType.Int16));
      c.IsNullable = false;
      c = t.CreateColumn("StandardCost", new SqlValueType("money"));
      c.IsNullable = false;
      c = t.CreateColumn("ListPrice", new SqlValueType("money"));
      c.IsNullable = false;
      c = t.CreateColumn("Size", new SqlValueType(SqlType.VarChar, 5));
      c.Collation = Catalog.Schemas["Production"].Collations["SQL_Latin1_General_CP1_CI_AS"];
      c = t.CreateColumn("SizeUnitMeasureCode", new SqlValueType(SqlType.Char, 3));
      c.Collation = Catalog.Schemas["Production"].Collations["SQL_Latin1_General_CP1_CI_AS"];
      c = t.CreateColumn("WeightUnitMeasureCode", new SqlValueType(SqlType.Char, 3));
      c.Collation = Catalog.Schemas["Production"].Collations["SQL_Latin1_General_CP1_CI_AS"];
      t.CreateColumn("Weight", new SqlValueType(SqlType.Decimal, 8, 2));
      c = t.CreateColumn("DaysToManufacture", new SqlValueType(SqlType.Int32));
      c.IsNullable = false;
      c = t.CreateColumn("ProductLine", new SqlValueType(SqlType.Char, 2));
      c.Collation = Catalog.Schemas["Production"].Collations["SQL_Latin1_General_CP1_CI_AS"];
      c = t.CreateColumn("Class", new SqlValueType(SqlType.Char, 2));
      c.Collation = Catalog.Schemas["Production"].Collations["SQL_Latin1_General_CP1_CI_AS"];
      c = t.CreateColumn("Style", new SqlValueType(SqlType.Char, 2));
      c.Collation = Catalog.Schemas["Production"].Collations["SQL_Latin1_General_CP1_CI_AS"];
      t.CreateColumn("ProductSubcategoryID", new SqlValueType(SqlType.Int32));
      t.CreateColumn("ProductModelID", new SqlValueType(SqlType.Int32));
      c = t.CreateColumn("SellStartDate", new SqlValueType(SqlType.DateTime));
      c.IsNullable = false;
      t.CreateColumn("SellEndDate", new SqlValueType(SqlType.DateTime));
      t.CreateColumn("DiscontinuedDate", new SqlValueType(SqlType.DateTime));
      c = t.CreateColumn("ModifiedDate", new SqlValueType(SqlType.DateTime));
      c.DefaultValue = SqlDml.CurrentDate();
      c.IsNullable = false;
      SqlTableRef st = SqlDml.TableRef(t);
      t.CreateCheckConstraint("CK_Product_Class", SqlDml.Upper(st["Class"])=='H' ||
        SqlDml.Upper(st["Class"])=='M' ||
          SqlDml.Upper(st["Class"])=='L' ||
            SqlDml.IsNull(st["Class"]));
      t.CreateCheckConstraint("CK_Product_DaysToManufacture", SqlDml.Upper(st["DaysToManufacture"])>=0);
      t.CreateCheckConstraint("CK_Product_ListPrice", SqlDml.Upper(st["ListPrice"])>=0);
      t.CreateCheckConstraint("CK_Product_ProductLine", SqlDml.Upper(st["ProductLine"])=='R' ||
        SqlDml.Upper(st["ProductLine"])=='M' ||
          SqlDml.Upper(st["ProductLine"])=='T' ||
            SqlDml.Upper(st["ProductLine"])=='S' ||
              SqlDml.IsNull(st["ProductLine"]));
      t.CreateCheckConstraint("CK_Product_ReorderPoint", SqlDml.Upper(st["ReorderPoint"])>0);
      t.CreateCheckConstraint("CK_Product_SafetyStockLevel", SqlDml.Upper(st["SafetyStockLevel"])>0);
      t.CreateCheckConstraint("CK_Product_SellEndDate", SqlDml.Upper(st["SellEndDate"])>st["SellStartDate"] ||
        SqlDml.IsNull(st["SellEndDate"]));
      t.CreateCheckConstraint("CK_Product_StandardCost", SqlDml.Upper(st["StandardCost"])>=0);
      t.CreateCheckConstraint("CK_Product_Style", SqlDml.Upper(st["Style"])=='U' ||
        SqlDml.Upper(st["Style"])=='M' ||
          SqlDml.Upper(st["Style"])=='W' ||
            SqlDml.IsNull(st["Style"]));
      t.CreateCheckConstraint("CK_Product_Weight", SqlDml.Upper(st["Weight"])>0);
      t.CreatePrimaryKey("PK_Product_ProductID", t.TableColumns["ProductID"]);
      cs = t.CreateForeignKey("FK_Product_ProductModel_ProductModelID");
      ((ForeignKey)cs).Columns.Add(t.TableColumns["ProductModelID"]);
      ((ForeignKey)cs).ReferencedColumns.Add(Catalog.Schemas["Production"].Tables["ProductModel"].TableColumns["ProductModelID"]);
      
      t = Catalog.Schemas["Person"].CreateTable("Contact");
      t.CreateColumn("ContactID", new SqlValueType(SqlType.Int32));
      t.CreateColumn("NameStyle", new SqlValueType(SqlType.Int32));
      t.CreateColumn("Title", new SqlValueType(SqlType.VarChar));
      t.CreateColumn("FirstName", new SqlValueType(SqlType.VarChar));
      t.CreateColumn("MiddleName", new SqlValueType(SqlType.VarChar));
      t.CreateColumn("LastName", new SqlValueType(SqlType.VarChar));
      t.CreateColumn("Suffix", new SqlValueType(SqlType.VarChar));
      t.CreateColumn("EmailAddress", new SqlValueType(SqlType.VarChar));
      t.CreateColumn("EmailPromotion", new SqlValueType(SqlType.Int32));
      t.CreateColumn("Phone", new SqlValueType(SqlType.VarChar));
      t.CreateColumn("PasswordHash", new SqlValueType(SqlType.VarChar));
      t.CreateColumn("PasswordSalt", new SqlValueType(SqlType.VarChar));
      t.CreateColumn("AdditionalContactInfo", new SqlValueType("xml"));
      t.CreateColumn("ModifiedDate", new SqlValueType(SqlType.DateTime));

      t = Catalog.Schemas["Production"].CreateTable("UnitMeasure");
      t.CreateColumn("UnitMeasureCode", new SqlValueType(SqlType.VarChar));
      t.CreateColumn("Name", new SqlValueType(SqlType.VarChar));
      t.CreateColumn("ModifiedDate", new SqlValueType(SqlType.DateTime));

      t = Catalog.Schemas["Production"].CreateTable("ProductReview");
      t.CreateColumn("ProductReviewID", new SqlValueType(SqlType.Int32));
      t.CreateColumn("ProductID", new SqlValueType(SqlType.Int32));
      t.CreateColumn("ReviewerName", new SqlValueType(SqlType.VarChar));
      t.CreateColumn("ReviewDate", new SqlValueType(SqlType.DateTime));
      t.CreateColumn("EmailAddress", new SqlValueType(SqlType.VarChar));
      t.CreateColumn("Rating", new SqlValueType(SqlType.Int32));
      t.CreateColumn("Comments", new SqlValueType(SqlType.VarChar));
      t.CreateColumn("ModifiedDate", new SqlValueType(SqlType.DateTime));

      t = Catalog.Schemas["Production"].CreateTable("ProductSubcategory");
      t.CreateColumn("ProductSubcategoryID", new SqlValueType(SqlType.Int32));
      t.CreateColumn("ProductCategoryID", new SqlValueType(SqlType.Int32));
      t.CreateColumn("Name", new SqlValueType(SqlType.VarChar));
      t.CreateColumn("ModifiedDate", new SqlValueType(SqlType.DateTime));

      t = Catalog.Schemas["Person"].CreateTable("AddressType");
      t.CreateColumn("AddressTypeID", new SqlValueType(SqlType.Int32));
      t.CreateColumn("Name", new SqlValueType(SqlType.VarChar));
      t.CreateColumn("ModifiedDate", new SqlValueType(SqlType.DateTime));

      t = Catalog.Schemas["Sales"].CreateTable("SalesReason");
      t.CreateColumn("SalesReasonID", new SqlValueType(SqlType.Int32));
      t.CreateColumn("Name", new SqlValueType(SqlType.VarChar));
      t.CreateColumn("ReasonType", new SqlValueType(SqlType.VarChar));
      t.CreateColumn("ModifiedDate", new SqlValueType(SqlType.DateTime));

      t = Catalog.Schemas["HumanResources"].CreateTable("Department");
      t.CreateColumn("DepartmentID", new SqlValueType(SqlType.Int32));
      t.CreateColumn("Name", new SqlValueType(SqlType.VarChar));
      t.CreateColumn("GroupName", new SqlValueType(SqlType.VarChar));
      t.CreateColumn("ModifiedDate", new SqlValueType(SqlType.DateTime));

      t = Catalog.Schemas["Person"].CreateTable("CountryRegion");
      t.CreateColumn("CountryRegionCode", new SqlValueType(SqlType.VarChar));
      t.CreateColumn("Name", new SqlValueType(SqlType.VarChar));
      t.CreateColumn("ModifiedDate", new SqlValueType(SqlType.DateTime));

      t = Catalog.Schemas["Production"].CreateTable("Culture");
      t.CreateColumn("CultureID", new SqlValueType(SqlType.Int32));
      t.CreateColumn("Name", new SqlValueType(SqlType.VarChar));
      t.CreateColumn("ModifiedDate", new SqlValueType(SqlType.DateTime));

      t = Catalog.Schemas["Sales"].CreateTable("Currency");
      t.CreateColumn("CurrencyCode", new SqlValueType(SqlType.VarChar));
      t.CreateColumn("Name", new SqlValueType(SqlType.VarChar));
      t.CreateColumn("ModifiedDate", new SqlValueType(SqlType.DateTime));

      t = Catalog.Schemas["Person"].CreateTable("ContactType");
      t.CreateColumn("ContactTypeID", new SqlValueType(SqlType.Int32));
      t.CreateColumn("Name", new SqlValueType(SqlType.VarChar));
      t.CreateColumn("ModifiedDate", new SqlValueType(SqlType.DateTime));

      t = Catalog.Schemas["Sales"].CreateTable("SalesTaxRate");
      t.CreateColumn("SalesTaxRateID", new SqlValueType(SqlType.Int32));
      t.CreateColumn("StateProvinceID", new SqlValueType(SqlType.Int32));
      t.CreateColumn("TaxType", new SqlValueType(SqlType.UInt8));
      t.CreateColumn("TaxRate", new SqlValueType("money"));
      t.CreateColumn("Name", new SqlValueType(SqlType.VarChar));
      t.CreateColumn("ModifiedDate", new SqlValueType(SqlType.DateTime));

      t = Catalog.Schemas["Production"].CreateTable("Location");
      t.CreateColumn("LocationID", new SqlValueType(SqlType.Int32));
      t.CreateColumn("Name", new SqlValueType(SqlType.VarChar));
      t.CreateColumn("CostRate", new SqlValueType("money"));
      t.CreateColumn("Availability", new SqlValueType(SqlType.Decimal));
      t.CreateColumn("ModifiedDate", new SqlValueType(SqlType.DateTime));

      t = Catalog.Schemas["Sales"].CreateTable("SalesTerritory");
      t.CreateColumn("TerritoryID", new SqlValueType(SqlType.Int32));
      t.CreateColumn("Name", new SqlValueType(SqlType.VarChar));
      t.CreateColumn("CountryRegionCode", new SqlValueType(SqlType.VarChar));
      t.CreateColumn("Group", new SqlValueType(SqlType.VarChar));
      t.CreateColumn("TaxRate", new SqlValueType("money"));
      t.CreateColumn("SalesYTD", new SqlValueType("money"));
      t.CreateColumn("SalesLastYear", new SqlValueType("money"));
      t.CreateColumn("CostYTD", new SqlValueType("money"));
      t.CreateColumn("CostLastYear", new SqlValueType("money"));
      t.CreateColumn("ModifiedDate", new SqlValueType(SqlType.DateTime));

      t = Catalog.Schemas["Production"].CreateTable("ScrapReason");
      t.CreateColumn("ScrapReasonID", new SqlValueType(SqlType.Int32));
      t.CreateColumn("Name", new SqlValueType(SqlType.VarChar));
      t.CreateColumn("ModifiedDate", new SqlValueType(SqlType.DateTime));

      t = Catalog.Schemas["HumanResources"].CreateTable("Shift");
      t.CreateColumn("ShiftID", new SqlValueType(SqlType.Int32));
      t.CreateColumn("Name", new SqlValueType(SqlType.VarChar));
      t.CreateColumn("StartTime", new SqlValueType(SqlType.DateTime));
      t.CreateColumn("EndTime", new SqlValueType(SqlType.DateTime));
      t.CreateColumn("ModifiedDate", new SqlValueType(SqlType.DateTime));

      t = Catalog.Schemas["Production"].CreateTable("ProductCategory");
      t.CreateColumn("ProductCategoryID", new SqlValueType(SqlType.Int32));
      t.CreateColumn("Name", new SqlValueType(SqlType.VarChar));
      t.CreateColumn("ModifiedDate", new SqlValueType(SqlType.DateTime));

      t = Catalog.Schemas["Purchasing"].CreateTable("ShipMethod");
      t.CreateColumn("ShipMethodID", new SqlValueType(SqlType.Int32));
      t.CreateColumn("Name", new SqlValueType(SqlType.VarChar));
      t.CreateColumn("ShipBase", new SqlValueType("money"));
      t.CreateColumn("ShipRate", new SqlValueType("money"));
      t.CreateColumn("ModifiedDate", new SqlValueType(SqlType.DateTime));

      t = Catalog.Schemas["Sales"].CreateTable("Store");
      t.CreateColumn("CustomerID", new SqlValueType(SqlType.Int32));
      t.CreateColumn("Name", new SqlValueType(SqlType.VarChar));
      t.CreateColumn("SalesPersonID", new SqlValueType(SqlType.Int32));
      t.CreateColumn("Demographics", new SqlValueType("xml"));
      t.CreateColumn("ModifiedDate", new SqlValueType(SqlType.DateTime));

      t = Catalog.Schemas["Sales"].CreateTable("Individual");
      t.CreateColumn("CustomerID", new SqlValueType(SqlType.Int32));
      t.CreateColumn("ContactID", new SqlValueType(SqlType.Int32));
      t.CreateColumn("Demographics", new SqlValueType("xml"));
      t.CreateColumn("ModifiedDate", new SqlValueType(SqlType.DateTime));

      t = Catalog.Schemas["HumanResources"].CreateTable("JobCandidate");
      t.CreateColumn("JobCandidateID", new SqlValueType(SqlType.Int32));
      t.CreateColumn("EmployeeID", new SqlValueType(SqlType.Int32));
      t.CreateColumn("Resume", new SqlValueType("xml"));
      t.CreateColumn("ModifiedDate", new SqlValueType(SqlType.DateTime));

      SqlTableRef e = SqlDml.TableRef(Catalog.Schemas["HumanResources"].Tables["Employee"], "e");
      SqlTableRef cRef = SqlDml.TableRef(Catalog.Schemas["Person"].Tables["Contact"], "c");
      SqlTableRef ea = SqlDml.TableRef(Catalog.Schemas["HumanResources"].Tables["EmployeeAddress"], "ea");
      SqlTableRef a = SqlDml.TableRef(Catalog.Schemas["Person"].Tables["Address"], "a");
      SqlTableRef sp = SqlDml.TableRef(Catalog.Schemas["Person"].Tables["StateProvince"], "sp");
      SqlTableRef cr = SqlDml.TableRef(Catalog.Schemas["Person"].Tables["CountryRegion"], "cr");

      SqlSelect select = SqlDml.Select(e);
      select.From = select.From.InnerJoin(cRef, cRef["ContactID"]==e["ContactID"]);
      select.From = select.From.InnerJoin(ea, e["EmployeeID"]==ea["EmployeeID"]);
      select.From = select.From.InnerJoin(a, ea["AddressID"]==a["AddressID"]);
      select.From = select.From.InnerJoin(sp, sp["StateProvinceID"]==a["StateProvinceID"]);
      select.From = select.From.InnerJoin(cr, cr["CountryRegionCode"]==sp["CountryRegionCode"]);
      select.Columns.AddRange(e["EmployeeID"], cRef["Title"], cRef["FirstName"], cRef["MiddleName"],
        cRef["LastName"], cRef["Suffix"]);
      select.Columns.Add(e["Title"], "JobTitle");
      select.Columns.AddRange(cRef["Phone"], cRef["EmailAddress"], cRef["EmailPromotion"],
        a["AddressLine1"], a["AddressLine2"], a["City"]);
      select.Columns.Add(sp["Name"], "StateProvinceName");
      select.Columns.Add(a["PostalCode"]);
      select.Columns.Add(cr["Name"], "CountryRegionName");
      select.Columns.Add(cRef["AdditionalContactInfo"]);

      //SqlDriver mssqlDriver = new MssqlDriver(new MssqlVersionInfo(new Version()));
      //v = Catalog.Schemas["HumanResources"].CreateView("vEmployee",
//        Sql.Native(mssqlDriver.Compile(select).CommandText));
//      bmp.Save(model);
    }
  }
}