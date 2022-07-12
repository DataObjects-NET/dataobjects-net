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
      finally {
        cmd.Transaction.Rollback();
      }
      return result;
    }

    protected static DbCommandExecutionResult GetExecuteNonQueryResult(IDbCommand cmd)
    {
      var result = new DbCommandExecutionResult();
      try {
        cmd.Transaction = cmd.Connection.BeginTransaction();
        result.RowCount = cmd.ExecuteNonQuery();
      }
      finally {
        cmd.Transaction.Rollback();
      }
      return result;
    }

    protected string Url { get { return TestConnectionInfoProvider.GetConnectionUrl(); } }

    protected virtual bool InMemory => false;

    public Catalog Catalog { get; protected set; }

    protected virtual void CheckRequirements() => Require.ProviderIs(StorageProvider.SqlServer);

    [OneTimeSetUp]
    public virtual void SetUp()
    {
      CheckRequirements();

      if (!InMemory) {
        var driver = TestSqlDriver.Create(Url);
        using (var connection = driver.CreateConnection()) {
          connection.Open();
          Catalog = driver.ExtractCatalog(connection);
          connection.Close();
        }
      }
      else {
        BuildCatalogInMemory();
      }
      //SqlDriver mssqlDriver = new MssqlDriver(new MssqlVersionInfo(new Version()));
      //v = Catalog.Schemas["HumanResources"].CreateView("vEmployee",
//        Sql.Native(mssqlDriver.Compile(select).CommandText));
//      bmp.Save(model);
    }

    protected void BuildCatalogInMemory()
    {
      Catalog = new Catalog("AdventureWorks");

      _ = Catalog.CreateSchema("HumanResources");
      _ = Catalog.CreateSchema("Person");
      _ = Catalog.CreateSchema("Production");
      _ = Catalog.CreateSchema("Purchasing");
      _ = Catalog.CreateSchema("Sales");

      _ = Catalog.Schemas["HumanResources"].CreateCollation("Traditional_Spanish_CI_AI");
      _ = Catalog.Schemas["Person"].CreateCollation("Traditional_Spanish_CI_AI");
      _ = Catalog.Schemas["Production"].CreateCollation("Traditional_Spanish_CI_AI");
      _ = Catalog.Schemas["Purchasing"].CreateCollation("Traditional_Spanish_CI_AI");
      _ = Catalog.Schemas["Sales"].CreateCollation("Traditional_Spanish_CI_AI");

      _ = Catalog.Schemas["HumanResources"].CreateCollation("SQL_Latin1_General_CP1_CI_AS");
      _ = Catalog.Schemas["Person"].CreateCollation("SQL_Latin1_General_CP1_CI_AS");
      _ = Catalog.Schemas["Production"].CreateCollation("SQL_Latin1_General_CP1_CI_AS");
      _ = Catalog.Schemas["Purchasing"].CreateCollation("SQL_Latin1_General_CP1_CI_AS");
      _ = Catalog.Schemas["Sales"].CreateCollation("SQL_Latin1_General_CP1_CI_AS");

      Table t;
      TableColumn c;
      Constraint cs;

      t = Catalog.Schemas["Production"].CreateTable("TransactionHistoryArchive");
      _ = t.CreateColumn("TransactionID", new SqlValueType(SqlType.Int32));
      _ = t.CreateColumn("ProductID", new SqlValueType(SqlType.Int32));
      _ = t.CreateColumn("ReferenceOrderID", new SqlValueType(SqlType.Int32));
      _ = t.CreateColumn("ReferenceOrderLineID", new SqlValueType(SqlType.Int32));
      _ = t.CreateColumn("TransactionDate", new SqlValueType(SqlType.DateTime));
      _ = t.CreateColumn("TransactionType", new SqlValueType(SqlType.VarChar));
      _ = t.CreateColumn("Quantity", new SqlValueType(SqlType.Int32));
      _ = t.CreateColumn("ActualCost", new SqlValueType("money"));
      _ = t.CreateColumn("ModifiedDate", new SqlValueType(SqlType.DateTime));

      t = Catalog.Schemas["Sales"].CreateTable("CreditCard");
      _ = t.CreateColumn("CreditCardID", new SqlValueType(SqlType.Int32));
      _ = t.CreateColumn("CardType", new SqlValueType(SqlType.VarChar));
      _ = t.CreateColumn("CardNumber", new SqlValueType(SqlType.VarChar));
      _ = t.CreateColumn("ExpMonth", new SqlValueType(SqlType.UInt8));
      _ = t.CreateColumn("ExpYear", new SqlValueType(SqlType.Int16));
      _ = t.CreateColumn("ModifiedDate", new SqlValueType(SqlType.DateTime));

      t = Catalog.Schemas["Production"].CreateTable("Document");
      _ = t.CreateColumn("DocumentID", new SqlValueType(SqlType.Int32));
      _ = t.CreateColumn("Title", new SqlValueType(SqlType.VarChar));
      _ = t.CreateColumn("FileName", new SqlValueType(SqlType.VarChar));
      _ = t.CreateColumn("FileExtension", new SqlValueType(SqlType.VarChar));
      _ = t.CreateColumn("Revision", new SqlValueType(SqlType.VarChar));
      _ = t.CreateColumn("ChangeNumber", new SqlValueType(SqlType.Int32));
      _ = t.CreateColumn("Status", new SqlValueType(SqlType.UInt8));
      _ = t.CreateColumn("DocumentSummary", new SqlValueType(SqlType.VarChar));
      _ = t.CreateColumn("Document", new SqlValueType(SqlType.VarBinaryMax));
      _ = t.CreateColumn("ModifiedDate", new SqlValueType(SqlType.DateTime));

      t = Catalog.Schemas["Production"].CreateTable("Illustration");
      _ = t.CreateColumn("IllustrationID", new SqlValueType(SqlType.Int32));
      _ = t.CreateColumn("Diagram", new SqlValueType("xml"));
      _ = t.CreateColumn("ModifiedDate", new SqlValueType(SqlType.DateTime));

      t = Catalog.Schemas["Production"].CreateTable("ProductDescription");
      _ = t.CreateColumn("ProductDescriptionID", new SqlValueType(SqlType.Int32));
      _ = t.CreateColumn("Description", new SqlValueType(SqlType.VarChar));
      _ = t.CreateColumn("ModifiedDate", new SqlValueType(SqlType.DateTime));

      t = Catalog.Schemas["Sales"].CreateTable("SpecialOffer");
      _ = t.CreateColumn("SpecialOfferID", new SqlValueType(SqlType.Int32));
      _ = t.CreateColumn("Description", new SqlValueType(SqlType.VarChar));
      _ = t.CreateColumn("DiscountPct", new SqlValueType(SqlType.Int32));
      _ = t.CreateColumn("Type", new SqlValueType(SqlType.VarChar));
      _ = t.CreateColumn("Category", new SqlValueType(SqlType.VarChar));
      _ = t.CreateColumn("StartDate", new SqlValueType(SqlType.DateTime));
      _ = t.CreateColumn("EndDate", new SqlValueType(SqlType.DateTime));
      _ = t.CreateColumn("MinQty", new SqlValueType(SqlType.Int32));
      _ = t.CreateColumn("MaxQty", new SqlValueType(SqlType.Int32));
      _ = t.CreateColumn("ModifiedDate", new SqlValueType(SqlType.DateTime));

      t = Catalog.Schemas["Production"].CreateTable("ProductPhoto");
      _ = t.CreateColumn("ProductPhotoID", new SqlValueType(SqlType.Int32));
      _ = t.CreateColumn("ThumbNailPhoto", new SqlValueType(SqlType.VarBinaryMax));
      _ = t.CreateColumn("ThumbnailPhotoFileName", new SqlValueType(SqlType.VarChar));
      _ = t.CreateColumn("LargePhoto", new SqlValueType(SqlType.VarBinaryMax));
      _ = t.CreateColumn("LargePhotoFileName", new SqlValueType(SqlType.VarChar));
      _ = t.CreateColumn("ModifiedDate", new SqlValueType(SqlType.DateTime));

      t = Catalog.Schemas["Sales"].CreateTable("Customer");
      _ = t.CreateColumn("CustomerID", new SqlValueType(SqlType.Int32));
      _ = t.CreateColumn("TerritoryID", new SqlValueType(SqlType.Int32));
      _ = t.CreateColumn("AccountNumber", new SqlValueType(SqlType.VarChar));
      _ = t.CreateColumn("CustomerType", new SqlValueType(SqlType.VarChar));
      _ = t.CreateColumn("ModifiedDate", new SqlValueType(SqlType.DateTime));

      t = Catalog.Schemas["Sales"].CreateTable("CustomerAddress");
      _ = t.CreateColumn("CustomerID", new SqlValueType(SqlType.Int32));
      _ = t.CreateColumn("AddressID", new SqlValueType(SqlType.Int32));
      _ = t.CreateColumn("AddressTypeID", new SqlValueType(SqlType.Int32));
      _ = t.CreateColumn("ModifiedDate", new SqlValueType(SqlType.DateTime));

      t = Catalog.Schemas["HumanResources"].CreateTable("EmployeeAddress");
      _ = t.CreateColumn("EmployeeID", new SqlValueType(SqlType.Int32));
      _ = t.CreateColumn("AddressID", new SqlValueType(SqlType.Int32));
      _ = t.CreateColumn("ModifiedDate", new SqlValueType(SqlType.DateTime));

      t = Catalog.Schemas["Purchasing"].CreateTable("VendorAddress");
      _ = t.CreateColumn("VendorID", new SqlValueType(SqlType.Int32));
      _ = t.CreateColumn("AddressID", new SqlValueType(SqlType.Int32));
      _ = t.CreateColumn("AddressTypeID", new SqlValueType(SqlType.Int32));
      _ = t.CreateColumn("ModifiedDate", new SqlValueType(SqlType.DateTime));

      t = Catalog.Schemas["Purchasing"].CreateTable("ProductVendor");
      _ = t.CreateColumn("ProductID", new SqlValueType(SqlType.Int32));
      _ = t.CreateColumn("VendorID", new SqlValueType(SqlType.Int32));
      _ = t.CreateColumn("AverageLeadTime", new SqlValueType(SqlType.Int32));
      _ = t.CreateColumn("StandardPrice", new SqlValueType("money"));
      _ = t.CreateColumn("LastReceiptCost", new SqlValueType("money"));
      _ = t.CreateColumn("LastReceiptDate", new SqlValueType(SqlType.DateTime));
      _ = t.CreateColumn("MinOrderQty", new SqlValueType(SqlType.Int32));
      _ = t.CreateColumn("MaxOrderQty", new SqlValueType(SqlType.Int32));
      _ = t.CreateColumn("OnOrderQty", new SqlValueType(SqlType.Int32));
      _ = t.CreateColumn("UnitMeasureCode", new SqlValueType(SqlType.VarChar));
      _ = t.CreateColumn("ModifiedDate", new SqlValueType(SqlType.DateTime));

      t = Catalog.Schemas["Production"].CreateTable("BillOfMaterials");
      _ = t.CreateColumn("BillOfMaterialsID", new SqlValueType(SqlType.Int32));
      _ = t.CreateColumn("ProductAssemblyID", new SqlValueType(SqlType.Int32));
      _ = t.CreateColumn("ComponentID", new SqlValueType(SqlType.Int32));
      _ = t.CreateColumn("StartDate", new SqlValueType(SqlType.DateTime));
      _ = t.CreateColumn("EndDate", new SqlValueType(SqlType.DateTime));
      _ = t.CreateColumn("UnitMeasureCode", new SqlValueType(SqlType.VarChar));
      _ = t.CreateColumn("BOMLevel", new SqlValueType(SqlType.Int16));
      _ = t.CreateColumn("PerAssemblyQty", new SqlValueType(SqlType.Decimal));
      _ = t.CreateColumn("ModifiedDate", new SqlValueType(SqlType.DateTime));

      t = Catalog.Schemas["Purchasing"].CreateTable("PurchaseOrderHeader");
      _ = t.CreateColumn("PurchaseOrderID", new SqlValueType(SqlType.Int32));
      _ = t.CreateColumn("RevisionNumber", new SqlValueType(SqlType.UInt8));
      _ = t.CreateColumn("Status", new SqlValueType(SqlType.UInt8));
      _ = t.CreateColumn("EmployeeID", new SqlValueType(SqlType.Int32));
      _ = t.CreateColumn("VendorID", new SqlValueType(SqlType.Int32));
      _ = t.CreateColumn("ShipMethodID", new SqlValueType(SqlType.Int32));
      _ = t.CreateColumn("OrderDate", new SqlValueType(SqlType.DateTime));
      _ = t.CreateColumn("ShipDate", new SqlValueType(SqlType.DateTime));
      _ = t.CreateColumn("SubTotal", new SqlValueType("money"));
      _ = t.CreateColumn("TaxAmt", new SqlValueType("money"));
      _ = t.CreateColumn("Freight", new SqlValueType("money"));
      _ = t.CreateColumn("TotalDue", new SqlValueType("money"));
      _ = t.CreateColumn("ModifiedDate", new SqlValueType(SqlType.DateTime));

      t = Catalog.Schemas["Purchasing"].CreateTable("VendorContact");
      _ = t.CreateColumn("VendorID", new SqlValueType(SqlType.Int32));
      _ = t.CreateColumn("ContactID", new SqlValueType(SqlType.Int32));
      _ = t.CreateColumn("ContactTypeID", new SqlValueType(SqlType.Int32));
      _ = t.CreateColumn("ModifiedDate", new SqlValueType(SqlType.DateTime));

      t = Catalog.Schemas["Sales"].CreateTable("ContactCreditCard");
      _ = t.CreateColumn("ContactID", new SqlValueType(SqlType.Int32));
      _ = t.CreateColumn("CreditCardID", new SqlValueType(SqlType.Int32));
      _ = t.CreateColumn("ModifiedDate", new SqlValueType(SqlType.DateTime));

      t = Catalog.Schemas["Sales"].CreateTable("StoreContact");
      _ = t.CreateColumn("CustomerID", new SqlValueType(SqlType.Int32));
      _ = t.CreateColumn("ContactID", new SqlValueType(SqlType.Int32));
      _ = t.CreateColumn("ContactTypeID", new SqlValueType(SqlType.Int32));
      _ = t.CreateColumn("ModifiedDate", new SqlValueType(SqlType.DateTime));

      t = Catalog.Schemas["Purchasing"].CreateTable("PurchaseOrderDetail");
      _ = t.CreateColumn("PurchaseOrderID", new SqlValueType(SqlType.Int32));
      _ = t.CreateColumn("PurchaseOrderDetailID", new SqlValueType(SqlType.Int32));
      _ = t.CreateColumn("DueDate", new SqlValueType(SqlType.Int32));
      _ = t.CreateColumn("OrderQty", new SqlValueType(SqlType.Int32));
      _ = t.CreateColumn("ProductID", new SqlValueType(SqlType.Int32));
      _ = t.CreateColumn("UnitPrice", new SqlValueType(SqlType.Int32));
      c = t.CreateColumn("LineTotal");
      SqlTableRef pod = SqlDml.TableRef(t);
      c.Expression = pod["OrderQty"] * pod["UnitPrice"];
      c.IsPersisted = false;
      _ = t.CreateColumn("ReceivedQty", new SqlValueType(SqlType.Int32));
      _ = t.CreateColumn("RejectedQty", new SqlValueType(SqlType.Int32));
      _ = t.CreateColumn("StockedQty", new SqlValueType(SqlType.Int32));
      _ = t.CreateColumn("ModifiedDate", new SqlValueType(SqlType.DateTime));

      t = Catalog.Schemas["Production"].CreateTable("WorkOrderRouting");
      _ = t.CreateColumn("WorkOrderID", new SqlValueType(SqlType.Int32));
      _ = t.CreateColumn("ProductID", new SqlValueType(SqlType.Int32));
      _ = t.CreateColumn("OperationSequence", new SqlValueType(SqlType.Int16));
      _ = t.CreateColumn("LocationID", new SqlValueType(SqlType.Int32));
      _ = t.CreateColumn("ScheduledStartDate", new SqlValueType(SqlType.DateTime));
      _ = t.CreateColumn("ScheduledEndDate", new SqlValueType(SqlType.DateTime));
      _ = t.CreateColumn("ActualStartDate", new SqlValueType(SqlType.DateTime));
      _ = t.CreateColumn("ActualEndDate", new SqlValueType(SqlType.DateTime));
      _ = t.CreateColumn("ActualResourceHrs", new SqlValueType(SqlType.Decimal));
      _ = t.CreateColumn("PlannedCost", new SqlValueType("money"));
      _ = t.CreateColumn("ActualCost", new SqlValueType("money"));
      _ = t.CreateColumn("ModifiedDate", new SqlValueType(SqlType.DateTime));

      t = Catalog.Schemas["Sales"].CreateTable("CountryRegionCurrency");
      _ = t.CreateColumn("CountryRegionCode", new SqlValueType(SqlType.VarChar));
      _ = t.CreateColumn("CurrencyCode", new SqlValueType(SqlType.VarChar));
      _ = t.CreateColumn("ModifiedDate", new SqlValueType(SqlType.DateTime));

      t = Catalog.Schemas["Production"].CreateTable("ProductModelProductDescriptionCulture");
      _ = t.CreateColumn("ProductModelID", new SqlValueType(SqlType.Int32));
      _ = t.CreateColumn("ProductDescriptionID", new SqlValueType(SqlType.Int32));
      _ = t.CreateColumn("CultureID", new SqlValueType(SqlType.VarChar));
      _ = t.CreateColumn("ModifiedDate", new SqlValueType(SqlType.DateTime));

      t = Catalog.Schemas["Sales"].CreateTable("CurrencyRate");
      _ = t.CreateColumn("CurrencyRateID", new SqlValueType(SqlType.Int32));
      _ = t.CreateColumn("CurrencyRateDate", new SqlValueType(SqlType.DateTime));
      _ = t.CreateColumn("FromCurrencyCode", new SqlValueType(SqlType.VarChar));
      _ = t.CreateColumn("ToCurrencyCode", new SqlValueType(SqlType.VarChar));
      _ = t.CreateColumn("AverageRate", new SqlValueType("money"));
      _ = t.CreateColumn("EndOfDayRate", new SqlValueType("money"));
      _ = t.CreateColumn("ModifiedDate", new SqlValueType(SqlType.DateTime));

      t = Catalog.Schemas["Sales"].CreateTable("SalesOrderDetail");
      _ = t.CreateColumn("SalesOrderID", new SqlValueType(SqlType.Int32));
      _ = t.CreateColumn("SalesOrderDetailID", new SqlValueType(SqlType.Int32));
      _ = t.CreateColumn("CarrierTrackingNumber", new SqlValueType(SqlType.VarChar));
      _ = t.CreateColumn("OrderQty", new SqlValueType(SqlType.Int16));
      _ = t.CreateColumn("ProductID", new SqlValueType(SqlType.Int32));
      _ = t.CreateColumn("SpecialOfferID", new SqlValueType(SqlType.Int32));
      _ = t.CreateColumn("UnitPrice", new SqlValueType("money"));
      _ = t.CreateColumn("UnitPriceDiscount", new SqlValueType("money"));
      _ = t.CreateColumn("LineTotal", new SqlValueType(SqlType.Int32));
      _ = t.CreateColumn("ModifiedDate", new SqlValueType(SqlType.DateTime));

      t = Catalog.Schemas["Sales"].CreateTable("SalesOrderHeaderSalesReason");
      _ = t.CreateColumn("SalesOrderID", new SqlValueType(SqlType.Int32));
      _ = t.CreateColumn("SalesReasonID", new SqlValueType(SqlType.Int32));
      _ = t.CreateColumn("ModifiedDate", new SqlValueType(SqlType.DateTime));

      t = Catalog.Schemas["HumanResources"].CreateTable("EmployeeDepartmentHistory");
      _ = t.CreateColumn("EmployeeID", new SqlValueType(SqlType.Int32));
      _ = t.CreateColumn("DepartmentID", new SqlValueType(SqlType.Int16));
      _ = t.CreateColumn("ShiftID", new SqlValueType(SqlType.DateTime));
      _ = t.CreateColumn("StartDate", new SqlValueType(SqlType.DateTime));
      _ = t.CreateColumn("EndDate", new SqlValueType(SqlType.DateTime));
      _ = t.CreateColumn("ModifiedDate", new SqlValueType(SqlType.DateTime));

      t = Catalog.Schemas["Production"].CreateTable("ProductDocument");
      _ = t.CreateColumn("ProductID", new SqlValueType(SqlType.Int32));
      _ = t.CreateColumn("DocumentID", new SqlValueType(SqlType.Int32));
      _ = t.CreateColumn("ModifiedDate", new SqlValueType(SqlType.DateTime));

      t = Catalog.Schemas["HumanResources"].CreateTable("EmployeePayHistory");
      _ = t.CreateColumn("EmployeeID", new SqlValueType(SqlType.Int32));
      _ = t.CreateColumn("RateChangeDate", new SqlValueType(SqlType.DateTime));
      _ = t.CreateColumn("Rate", new SqlValueType("money"));
      _ = t.CreateColumn("PayFrequency", new SqlValueType(SqlType.UInt8));
      _ = t.CreateColumn("ModifiedDate", new SqlValueType(SqlType.DateTime));

      t = Catalog.Schemas["Sales"].CreateTable("SalesPerson");
      _ = t.CreateColumn("SalesPersonID", new SqlValueType(SqlType.Int32));
      _ = t.CreateColumn("TerritoryID", new SqlValueType(SqlType.Int32));
      _ = t.CreateColumn("SalesQuota", new SqlValueType("money"));
      _ = t.CreateColumn("Bonus", new SqlValueType("money"));
      _ = t.CreateColumn("CommissionPct", new SqlValueType("money"));
      _ = t.CreateColumn("SalesYTD", new SqlValueType("money"));
      _ = t.CreateColumn("SalesLastYear", new SqlValueType("money"));
      _ = t.CreateColumn("ModifiedDate", new SqlValueType(SqlType.DateTime));

      t = Catalog.Schemas["Sales"].CreateTable("SalesPersonQuotaHistory");
      _ = t.CreateColumn("SalesPersonID", new SqlValueType(SqlType.Int32));
      _ = t.CreateColumn("QuotaDate", new SqlValueType(SqlType.DateTime));
      _ = t.CreateColumn("SalesQuota", new SqlValueType("money"));
      _ = t.CreateColumn("ModifiedDate", new SqlValueType(SqlType.DateTime));

      t = Catalog.Schemas["Sales"].CreateTable("SalesTerritoryHistory");
      _ = t.CreateColumn("SalesPersonID", new SqlValueType(SqlType.Int32));
      _ = t.CreateColumn("TerritoryID", new SqlValueType(SqlType.Int32));
      _ = t.CreateColumn("StartDate", new SqlValueType(SqlType.DateTime));
      _ = t.CreateColumn("EndDate", new SqlValueType(SqlType.DateTime));
      _ = t.CreateColumn("ModifiedDate", new SqlValueType(SqlType.DateTime));

      t = Catalog.Schemas["Production"].CreateTable("ProductModelIllustration");
      _ = t.CreateColumn("ProductModelID", new SqlValueType(SqlType.Int32));
      _ = t.CreateColumn("IllustrationID", new SqlValueType(SqlType.Int32));
      _ = t.CreateColumn("ModifiedDate", new SqlValueType(SqlType.DateTime));

      t = Catalog.Schemas["Production"].CreateTable("ProductInventory");
      _ = t.CreateColumn("ProductID", new SqlValueType(SqlType.Int32));
      _ = t.CreateColumn("LocationID", new SqlValueType(SqlType.Int32));
      _ = t.CreateColumn("Shelf", new SqlValueType(SqlType.VarChar));
      _ = t.CreateColumn("Bin", new SqlValueType(SqlType.UInt8));
      _ = t.CreateColumn("Quantity", new SqlValueType(SqlType.UInt8));
      _ = t.CreateColumn("ModifiedDate", new SqlValueType(SqlType.DateTime));

      t = Catalog.Schemas["Production"].CreateTable("WorkOrder");
      _ = t.CreateColumn("WorkOrderID", new SqlValueType(SqlType.Int32));
      _ = t.CreateColumn("ProductID", new SqlValueType(SqlType.Int32));
      _ = t.CreateColumn("OrderQty", new SqlValueType(SqlType.Int32));
      _ = t.CreateColumn("StockedQty", new SqlValueType(SqlType.Int32));
      _ = t.CreateColumn("ScrappedQty", new SqlValueType(SqlType.Int16));
      _ = t.CreateColumn("StartDate", new SqlValueType(SqlType.DateTime));
      _ = t.CreateColumn("EndDate", new SqlValueType(SqlType.DateTime));
      _ = t.CreateColumn("DueDate", new SqlValueType(SqlType.DateTime));
      _ = t.CreateColumn("ScrapReasonID", new SqlValueType(SqlType.Int16));
      _ = t.CreateColumn("ModifiedDate", new SqlValueType(SqlType.DateTime));

      t = Catalog.Schemas["Production"].CreateTable("TransactionHistory");
      _ = t.CreateColumn("TransactionID", new SqlValueType(SqlType.Int32));
      _ = t.CreateColumn("ProductID", new SqlValueType(SqlType.Int32));
      _ = t.CreateColumn("ReferenceOrderID", new SqlValueType(SqlType.Int32));
      _ = t.CreateColumn("ReferenceOrderLineID", new SqlValueType(SqlType.Int32));
      _ = t.CreateColumn("TransactionDate", new SqlValueType(SqlType.DateTime));
      _ = t.CreateColumn("TransactionType", new SqlValueType(SqlType.VarChar));
      _ = t.CreateColumn("Quantity", new SqlValueType(SqlType.Int32));
      _ = t.CreateColumn("ActualCost", new SqlValueType("money"));
      _ = t.CreateColumn("ModifiedDate", new SqlValueType(SqlType.DateTime));

      t = Catalog.Schemas["Sales"].CreateTable("ShoppingCartItem");
      _ = t.CreateColumn("ShoppingCartItemID", new SqlValueType(SqlType.Int32));
      _ = t.CreateColumn("ShoppingCartID", new SqlValueType(SqlType.VarChar));
      _ = t.CreateColumn("Quantity", new SqlValueType(SqlType.Int32));
      _ = t.CreateColumn("ProductID", new SqlValueType(SqlType.Int32));
      _ = t.CreateColumn("DateCreated", new SqlValueType(SqlType.DateTime));
      _ = t.CreateColumn("ModifiedDate", new SqlValueType(SqlType.DateTime));

      t = Catalog.Schemas["Production"].CreateTable("ProductListPriceHistory");
      _ = t.CreateColumn("ProductID", new SqlValueType(SqlType.Int32));
      _ = t.CreateColumn("StartDate", new SqlValueType(SqlType.DateTime));
      _ = t.CreateColumn("EndDate", new SqlValueType(SqlType.DateTime));
      _ = t.CreateColumn("ListPrice", new SqlValueType("money"));
      _ = t.CreateColumn("ModifiedDate", new SqlValueType(SqlType.DateTime));

      t = Catalog.Schemas["Sales"].CreateTable("SpecialOfferProduct");
      _ = t.CreateColumn("SpecialOfferID", new SqlValueType(SqlType.Int32));
      _ = t.CreateColumn("ProductID", new SqlValueType(SqlType.Int32));
      _ = t.CreateColumn("ModifiedDate", new SqlValueType(SqlType.DateTime));

      t = Catalog.Schemas["Production"].CreateTable("ProductCostHistory");
      _ = t.CreateColumn("ProductID", new SqlValueType(SqlType.Int32));
      _ = t.CreateColumn("StartDate", new SqlValueType(SqlType.DateTime));
      _ = t.CreateColumn("EndDate", new SqlValueType(SqlType.DateTime));
      _ = t.CreateColumn("StandardCost", new SqlValueType("money"));
      _ = t.CreateColumn("ModifiedDate", new SqlValueType(SqlType.DateTime));

      t = Catalog.Schemas["Person"].CreateTable("Address");
      _ = t.CreateColumn("AddressID", new SqlValueType(SqlType.Int32));
      _ = t.CreateColumn("AddressLine1", new SqlValueType(SqlType.VarChar));
      _ = t.CreateColumn("AddressLine2", new SqlValueType(SqlType.VarChar));
      _ = t.CreateColumn("City", new SqlValueType(SqlType.VarChar));
      _ = t.CreateColumn("StateProvinceID", new SqlValueType(SqlType.Int32));
      _ = t.CreateColumn("PostalCode", new SqlValueType(SqlType.VarChar));
      _ = t.CreateColumn("ModifiedDate", new SqlValueType(SqlType.DateTime));

      t = Catalog.Schemas["Purchasing"].CreateTable("Vendor");
      _ = t.CreateColumn("VendorID", new SqlValueType(SqlType.Int32));
      _ = t.CreateColumn("AccountNumber", new SqlValueType(SqlType.VarChar));
      _ = t.CreateColumn("Name", new SqlValueType(SqlType.VarChar));
      _ = t.CreateColumn("CreditRating", new SqlValueType(SqlType.UInt8));
      _ = t.CreateColumn("PreferredVendorStatus", new SqlValueType(SqlType.Boolean));
      _ = t.CreateColumn("ActiveFlag", new SqlValueType(SqlType.Boolean));
      _ = t.CreateColumn("PurchasingWebServiceURL", new SqlValueType(SqlType.VarChar));
      _ = t.CreateColumn("ModifiedDate", new SqlValueType(SqlType.DateTime));

      t = Catalog.Schemas["Sales"].CreateTable("SalesOrderHeader");
      _ = t.CreateColumn("SalesOrderID", new SqlValueType(SqlType.Int32));
      _ = t.CreateColumn("RevisionNumber", new SqlValueType(SqlType.UInt8));
      _ = t.CreateColumn("OrderDate", new SqlValueType(SqlType.DateTime));
      _ = t.CreateColumn("DueDate", new SqlValueType(SqlType.DateTime));
      _ = t.CreateColumn("ShipDate", new SqlValueType(SqlType.DateTime));
      _ = t.CreateColumn("Status", new SqlValueType(SqlType.UInt8));
      _ = t.CreateColumn("OnlineOrderFlag", new SqlValueType(SqlType.Boolean));
      _ = t.CreateColumn("SalesOrderNumber", new SqlValueType(SqlType.VarChar));
      _ = t.CreateColumn("PurchaseOrderNumber", new SqlValueType(SqlType.VarChar));
      _ = t.CreateColumn("AccountNumber", new SqlValueType(SqlType.VarChar));
      _ = t.CreateColumn("CustomerID", new SqlValueType(SqlType.Int32));
      _ = t.CreateColumn("ContactID", new SqlValueType(SqlType.Int32));
      _ = t.CreateColumn("SalesPersonID", new SqlValueType(SqlType.Int32));
      _ = t.CreateColumn("TerritoryID", new SqlValueType(SqlType.Int32));
      _ = t.CreateColumn("BillToAddressID", new SqlValueType(SqlType.Int32));
      _ = t.CreateColumn("ShipToAddressID", new SqlValueType(SqlType.Int32));
      _ = t.CreateColumn("ShipMethodID", new SqlValueType(SqlType.Int32));
      _ = t.CreateColumn("CreditCardID", new SqlValueType(SqlType.Int32));
      _ = t.CreateColumn("CreditCardApprovalCode", new SqlValueType(SqlType.VarChar));
      _ = t.CreateColumn("CurrencyRateID", new SqlValueType(SqlType.Int32));
      _ = t.CreateColumn("SubTotal", new SqlValueType("money"));
      _ = t.CreateColumn("TaxAmt", new SqlValueType("money"));
      _ = t.CreateColumn("Freight", new SqlValueType("money"));
      _ = t.CreateColumn("TotalDue", new SqlValueType("money"));
      _ = t.CreateColumn("Comment", new SqlValueType(SqlType.VarChar));
      _ = t.CreateColumn("ModifiedDate", new SqlValueType(SqlType.DateTime));

      t = Catalog.Schemas["HumanResources"].CreateTable("Employee");
      _ = t.CreateColumn("EmployeeID", new SqlValueType(SqlType.Int32));
      _ = t.CreateColumn("NationalIDNumber", new SqlValueType(SqlType.VarChar));
      _ = t.CreateColumn("ContactID", new SqlValueType(SqlType.Int32));
      _ = t.CreateColumn("LoginID", new SqlValueType(SqlType.VarChar));
      _ = t.CreateColumn("ManagerID", new SqlValueType(SqlType.Int32));
      _ = t.CreateColumn("Title", new SqlValueType(SqlType.VarChar));
      _ = t.CreateColumn("BirthDate", new SqlValueType(SqlType.DateTime));
      _ = t.CreateColumn("MaritalStatus", new SqlValueType(SqlType.VarChar));
      _ = t.CreateColumn("Gender", new SqlValueType(SqlType.VarChar));
      _ = t.CreateColumn("HireDate", new SqlValueType(SqlType.DateTime));
      _ = t.CreateColumn("SalariedFlag", new SqlValueType(SqlType.Boolean));
      _ = t.CreateColumn("VacationHours", new SqlValueType(SqlType.Int16));
      _ = t.CreateColumn("SickLeaveHours", new SqlValueType(SqlType.Int16));
      _ = t.CreateColumn("CurrentFlag", new SqlValueType(SqlType.Boolean));
      _ = t.CreateColumn("ModifiedDate", new SqlValueType(SqlType.DateTime));

      t = Catalog.Schemas["Production"].CreateTable("ProductProductPhoto");
      _ = t.CreateColumn("ProductID", new SqlValueType(SqlType.Int32));
      _ = t.CreateColumn("ProductPhotoID", new SqlValueType(SqlType.Int32));
      _ = t.CreateColumn("Primary", new SqlValueType(SqlType.VarChar));
      _ = t.CreateColumn("ModifiedDate", new SqlValueType(SqlType.DateTime));

      t = Catalog.Schemas["Person"].CreateTable("StateProvince");
      _ = t.CreateColumn("StateProvinceID", new SqlValueType(SqlType.Int32));
      _ = t.CreateColumn("StateProvinceCode", new SqlValueType(SqlType.VarChar));
      _ = t.CreateColumn("CountryRegionCode", new SqlValueType(SqlType.VarChar));
      _ = t.CreateColumn("IsOnlyStateProvinceFlag", new SqlValueType(SqlType.Boolean));
      _ = t.CreateColumn("Name", new SqlValueType(SqlType.VarChar));
      _ = t.CreateColumn("TerritoryID", new SqlValueType(SqlType.Int32));
      _ = t.CreateColumn("ModifiedDate", new SqlValueType(SqlType.DateTime));

      t = Catalog.Schemas["Production"].CreateTable("ProductModel");
      _ = t.CreateColumn("ProductModelID", new SqlValueType(SqlType.Int32));
      _ = t.CreateColumn("Name", new SqlValueType(SqlType.VarChar));
      _ = t.CreateColumn("CatalogDescription", new SqlValueType("xml"));
      _ = t.CreateColumn("Instructions", new SqlValueType("xml"));
      _ = t.CreateColumn("ModifiedDate", new SqlValueType(SqlType.DateTime));

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
      _ = t.CreateColumn("Weight", new SqlValueType(SqlType.Decimal, 8, 2));
      c = t.CreateColumn("DaysToManufacture", new SqlValueType(SqlType.Int32));
      c.IsNullable = false;
      c = t.CreateColumn("ProductLine", new SqlValueType(SqlType.Char, 2));
      c.Collation = Catalog.Schemas["Production"].Collations["SQL_Latin1_General_CP1_CI_AS"];
      c = t.CreateColumn("Class", new SqlValueType(SqlType.Char, 2));
      c.Collation = Catalog.Schemas["Production"].Collations["SQL_Latin1_General_CP1_CI_AS"];
      c = t.CreateColumn("Style", new SqlValueType(SqlType.Char, 2));
      c.Collation = Catalog.Schemas["Production"].Collations["SQL_Latin1_General_CP1_CI_AS"];
      _ = t.CreateColumn("ProductSubcategoryID", new SqlValueType(SqlType.Int32));
      _ = t.CreateColumn("ProductModelID", new SqlValueType(SqlType.Int32));
      c = t.CreateColumn("SellStartDate", new SqlValueType(SqlType.DateTime));
      c.IsNullable = false;
      _ = t.CreateColumn("SellEndDate", new SqlValueType(SqlType.DateTime));
      _ = t.CreateColumn("DiscontinuedDate", new SqlValueType(SqlType.DateTime));
      c = t.CreateColumn("ModifiedDate", new SqlValueType(SqlType.DateTime));
      c.DefaultValue = SqlDml.CurrentDate();
      c.IsNullable = false;
      var st = SqlDml.TableRef(t);
      _ = t.CreateCheckConstraint("CK_Product_Class", SqlDml.Upper(st["Class"]) == 'H' ||
        SqlDml.Upper(st["Class"]) == 'M' ||
          SqlDml.Upper(st["Class"]) == 'L' ||
            SqlDml.IsNull(st["Class"]));
      _ = t.CreateCheckConstraint("CK_Product_DaysToManufacture", SqlDml.Upper(st["DaysToManufacture"]) >= 0);
      _ = t.CreateCheckConstraint("CK_Product_ListPrice", SqlDml.Upper(st["ListPrice"]) >= 0);
      _ = t.CreateCheckConstraint("CK_Product_ProductLine", SqlDml.Upper(st["ProductLine"]) == 'R' ||
        SqlDml.Upper(st["ProductLine"]) == 'M' ||
          SqlDml.Upper(st["ProductLine"]) == 'T' ||
            SqlDml.Upper(st["ProductLine"]) == 'S' ||
              SqlDml.IsNull(st["ProductLine"]));
      _ = t.CreateCheckConstraint("CK_Product_ReorderPoint", SqlDml.Upper(st["ReorderPoint"]) > 0);
      _ = t.CreateCheckConstraint("CK_Product_SafetyStockLevel", SqlDml.Upper(st["SafetyStockLevel"]) > 0);
      _ = t.CreateCheckConstraint("CK_Product_SellEndDate", SqlDml.Upper(st["SellEndDate"]) > st["SellStartDate"] ||
        SqlDml.IsNull(st["SellEndDate"]));
      _ = t.CreateCheckConstraint("CK_Product_StandardCost", SqlDml.Upper(st["StandardCost"]) >= 0);
      _ = t.CreateCheckConstraint("CK_Product_Style", SqlDml.Upper(st["Style"]) == 'U' ||
        SqlDml.Upper(st["Style"]) == 'M' ||
          SqlDml.Upper(st["Style"]) == 'W' ||
            SqlDml.IsNull(st["Style"]));
      _ = t.CreateCheckConstraint("CK_Product_Weight", SqlDml.Upper(st["Weight"]) > 0);
      _ = t.CreatePrimaryKey("PK_Product_ProductID", t.TableColumns["ProductID"]);
      cs = t.CreateForeignKey("FK_Product_ProductModel_ProductModelID");
      ((ForeignKey) cs).Columns.Add(t.TableColumns["ProductModelID"]);
      ((ForeignKey) cs).ReferencedColumns.Add(Catalog.Schemas["Production"].Tables["ProductModel"].TableColumns["ProductModelID"]);

      t = Catalog.Schemas["Person"].CreateTable("Contact");
      _ = t.CreateColumn("ContactID", new SqlValueType(SqlType.Int32));
      _ = t.CreateColumn("NameStyle", new SqlValueType(SqlType.Int32));
      _ = t.CreateColumn("Title", new SqlValueType(SqlType.VarChar));
      _ = t.CreateColumn("FirstName", new SqlValueType(SqlType.VarChar));
      _ = t.CreateColumn("MiddleName", new SqlValueType(SqlType.VarChar));
      _ = t.CreateColumn("LastName", new SqlValueType(SqlType.VarChar));
      _ = t.CreateColumn("Suffix", new SqlValueType(SqlType.VarChar));
      _ = t.CreateColumn("EmailAddress", new SqlValueType(SqlType.VarChar));
      _ = t.CreateColumn("EmailPromotion", new SqlValueType(SqlType.Int32));
      _ = t.CreateColumn("Phone", new SqlValueType(SqlType.VarChar));
      _ = t.CreateColumn("PasswordHash", new SqlValueType(SqlType.VarChar));
      _ = t.CreateColumn("PasswordSalt", new SqlValueType(SqlType.VarChar));
      _ = t.CreateColumn("AdditionalContactInfo", new SqlValueType("xml"));
      _ = t.CreateColumn("ModifiedDate", new SqlValueType(SqlType.DateTime));

      t = Catalog.Schemas["Production"].CreateTable("UnitMeasure");
      _ = t.CreateColumn("UnitMeasureCode", new SqlValueType(SqlType.VarChar));
      _ = t.CreateColumn("Name", new SqlValueType(SqlType.VarChar));
      _ = t.CreateColumn("ModifiedDate", new SqlValueType(SqlType.DateTime));

      t = Catalog.Schemas["Production"].CreateTable("ProductReview");
      _ = t.CreateColumn("ProductReviewID", new SqlValueType(SqlType.Int32));
      _ = t.CreateColumn("ProductID", new SqlValueType(SqlType.Int32));
      _ = t.CreateColumn("ReviewerName", new SqlValueType(SqlType.VarChar));
      _ = t.CreateColumn("ReviewDate", new SqlValueType(SqlType.DateTime));
      _ = t.CreateColumn("EmailAddress", new SqlValueType(SqlType.VarChar));
      _ = t.CreateColumn("Rating", new SqlValueType(SqlType.Int32));
      _ = t.CreateColumn("Comments", new SqlValueType(SqlType.VarChar));
      _ = t.CreateColumn("ModifiedDate", new SqlValueType(SqlType.DateTime));

      t = Catalog.Schemas["Production"].CreateTable("ProductSubcategory");
      _ = t.CreateColumn("ProductSubcategoryID", new SqlValueType(SqlType.Int32));
      _ = t.CreateColumn("ProductCategoryID", new SqlValueType(SqlType.Int32));
      _ = t.CreateColumn("Name", new SqlValueType(SqlType.VarChar));
      _ = t.CreateColumn("ModifiedDate", new SqlValueType(SqlType.DateTime));

      t = Catalog.Schemas["Person"].CreateTable("AddressType");
      _ = t.CreateColumn("AddressTypeID", new SqlValueType(SqlType.Int32));
      _ = t.CreateColumn("Name", new SqlValueType(SqlType.VarChar));
      _ = t.CreateColumn("ModifiedDate", new SqlValueType(SqlType.DateTime));

      t = Catalog.Schemas["Sales"].CreateTable("SalesReason");
      _ = t.CreateColumn("SalesReasonID", new SqlValueType(SqlType.Int32));
      _ = t.CreateColumn("Name", new SqlValueType(SqlType.VarChar));
      _ = t.CreateColumn("ReasonType", new SqlValueType(SqlType.VarChar));
      _ = t.CreateColumn("ModifiedDate", new SqlValueType(SqlType.DateTime));

      t = Catalog.Schemas["HumanResources"].CreateTable("Department");
      _ = t.CreateColumn("DepartmentID", new SqlValueType(SqlType.Int32));
      _ = t.CreateColumn("Name", new SqlValueType(SqlType.VarChar));
      _ = t.CreateColumn("GroupName", new SqlValueType(SqlType.VarChar));
      _ = t.CreateColumn("ModifiedDate", new SqlValueType(SqlType.DateTime));

      t = Catalog.Schemas["Person"].CreateTable("CountryRegion");
      _ = t.CreateColumn("CountryRegionCode", new SqlValueType(SqlType.VarChar));
      _ = t.CreateColumn("Name", new SqlValueType(SqlType.VarChar));
      _ = t.CreateColumn("ModifiedDate", new SqlValueType(SqlType.DateTime));

      t = Catalog.Schemas["Production"].CreateTable("Culture");
      _ = t.CreateColumn("CultureID", new SqlValueType(SqlType.Int32));
      _ = t.CreateColumn("Name", new SqlValueType(SqlType.VarChar));
      _ = t.CreateColumn("ModifiedDate", new SqlValueType(SqlType.DateTime));

      t = Catalog.Schemas["Sales"].CreateTable("Currency");
      _ = t.CreateColumn("CurrencyCode", new SqlValueType(SqlType.VarChar));
      _ = t.CreateColumn("Name", new SqlValueType(SqlType.VarChar));
      _ = t.CreateColumn("ModifiedDate", new SqlValueType(SqlType.DateTime));

      t = Catalog.Schemas["Person"].CreateTable("ContactType");
      _ = t.CreateColumn("ContactTypeID", new SqlValueType(SqlType.Int32));
      _ = t.CreateColumn("Name", new SqlValueType(SqlType.VarChar));
      _ = t.CreateColumn("ModifiedDate", new SqlValueType(SqlType.DateTime));

      t = Catalog.Schemas["Sales"].CreateTable("SalesTaxRate");
      _ = t.CreateColumn("SalesTaxRateID", new SqlValueType(SqlType.Int32));
      _ = t.CreateColumn("StateProvinceID", new SqlValueType(SqlType.Int32));
      _ = t.CreateColumn("TaxType", new SqlValueType(SqlType.UInt8));
      _ = t.CreateColumn("TaxRate", new SqlValueType("money"));
      _ = t.CreateColumn("Name", new SqlValueType(SqlType.VarChar));
      _ = t.CreateColumn("ModifiedDate", new SqlValueType(SqlType.DateTime));

      t = Catalog.Schemas["Production"].CreateTable("Location");
      _ = t.CreateColumn("LocationID", new SqlValueType(SqlType.Int32));
      _ = t.CreateColumn("Name", new SqlValueType(SqlType.VarChar));
      _ = t.CreateColumn("CostRate", new SqlValueType("money"));
      _ = t.CreateColumn("Availability", new SqlValueType(SqlType.Decimal));
      _ = t.CreateColumn("ModifiedDate", new SqlValueType(SqlType.DateTime));

      t = Catalog.Schemas["Sales"].CreateTable("SalesTerritory");
      _ = t.CreateColumn("TerritoryID", new SqlValueType(SqlType.Int32));
      _ = t.CreateColumn("Name", new SqlValueType(SqlType.VarChar));
      _ = t.CreateColumn("CountryRegionCode", new SqlValueType(SqlType.VarChar));
      _ = t.CreateColumn("Group", new SqlValueType(SqlType.VarChar));
      _ = t.CreateColumn("TaxRate", new SqlValueType("money"));
      _ = t.CreateColumn("SalesYTD", new SqlValueType("money"));
      _ = t.CreateColumn("SalesLastYear", new SqlValueType("money"));
      _ = t.CreateColumn("CostYTD", new SqlValueType("money"));
      _ = t.CreateColumn("CostLastYear", new SqlValueType("money"));
      _ = t.CreateColumn("ModifiedDate", new SqlValueType(SqlType.DateTime));

      t = Catalog.Schemas["Production"].CreateTable("ScrapReason");
      _ = t.CreateColumn("ScrapReasonID", new SqlValueType(SqlType.Int32));
      _ = t.CreateColumn("Name", new SqlValueType(SqlType.VarChar));
      _ = t.CreateColumn("ModifiedDate", new SqlValueType(SqlType.DateTime));

      t = Catalog.Schemas["HumanResources"].CreateTable("Shift");
      _ = t.CreateColumn("ShiftID", new SqlValueType(SqlType.Int32));
      _ = t.CreateColumn("Name", new SqlValueType(SqlType.VarChar));
      _ = t.CreateColumn("StartTime", new SqlValueType(SqlType.DateTime));
      _ = t.CreateColumn("EndTime", new SqlValueType(SqlType.DateTime));
      _ = t.CreateColumn("ModifiedDate", new SqlValueType(SqlType.DateTime));

      t = Catalog.Schemas["Production"].CreateTable("ProductCategory");
      _ = t.CreateColumn("ProductCategoryID", new SqlValueType(SqlType.Int32));
      _ = t.CreateColumn("Name", new SqlValueType(SqlType.VarChar));
      _ = t.CreateColumn("ModifiedDate", new SqlValueType(SqlType.DateTime));

      t = Catalog.Schemas["Purchasing"].CreateTable("ShipMethod");
      _ = t.CreateColumn("ShipMethodID", new SqlValueType(SqlType.Int32));
      _ = t.CreateColumn("Name", new SqlValueType(SqlType.VarChar));
      _ = t.CreateColumn("ShipBase", new SqlValueType("money"));
      _ = t.CreateColumn("ShipRate", new SqlValueType("money"));
      _ = t.CreateColumn("ModifiedDate", new SqlValueType(SqlType.DateTime));

      t = Catalog.Schemas["Sales"].CreateTable("Store");
      _ = t.CreateColumn("CustomerID", new SqlValueType(SqlType.Int32));
      _ = t.CreateColumn("Name", new SqlValueType(SqlType.VarChar));
      _ = t.CreateColumn("SalesPersonID", new SqlValueType(SqlType.Int32));
      _ = t.CreateColumn("Demographics", new SqlValueType("xml"));
      _ = t.CreateColumn("ModifiedDate", new SqlValueType(SqlType.DateTime));

      t = Catalog.Schemas["Sales"].CreateTable("Individual");
      _ = t.CreateColumn("CustomerID", new SqlValueType(SqlType.Int32));
      _ = t.CreateColumn("ContactID", new SqlValueType(SqlType.Int32));
      _ = t.CreateColumn("Demographics", new SqlValueType("xml"));
      _ = t.CreateColumn("ModifiedDate", new SqlValueType(SqlType.DateTime));

      t = Catalog.Schemas["HumanResources"].CreateTable("JobCandidate");
      _ = t.CreateColumn("JobCandidateID", new SqlValueType(SqlType.Int32));
      _ = t.CreateColumn("EmployeeID", new SqlValueType(SqlType.Int32));
      _ = t.CreateColumn("Resume", new SqlValueType("xml"));
      _ = t.CreateColumn("ModifiedDate", new SqlValueType(SqlType.DateTime));

      var e = SqlDml.TableRef(Catalog.Schemas["HumanResources"].Tables["Employee"], "e");
      var cRef = SqlDml.TableRef(Catalog.Schemas["Person"].Tables["Contact"], "c");
      var ea = SqlDml.TableRef(Catalog.Schemas["HumanResources"].Tables["EmployeeAddress"], "ea");
      var a = SqlDml.TableRef(Catalog.Schemas["Person"].Tables["Address"], "a");
      var sp = SqlDml.TableRef(Catalog.Schemas["Person"].Tables["StateProvince"], "sp");
      var cr = SqlDml.TableRef(Catalog.Schemas["Person"].Tables["CountryRegion"], "cr");

      var select = SqlDml.Select(e);
      select.From = select.From.InnerJoin(cRef, cRef["ContactID"] == e["ContactID"]);
      select.From = select.From.InnerJoin(ea, e["EmployeeID"] == ea["EmployeeID"]);
      select.From = select.From.InnerJoin(a, ea["AddressID"] == a["AddressID"]);
      select.From = select.From.InnerJoin(sp, sp["StateProvinceID"] == a["StateProvinceID"]);
      select.From = select.From.InnerJoin(cr, cr["CountryRegionCode"] == sp["CountryRegionCode"]);
      select.Columns.AddRange(e["EmployeeID"], cRef["Title"], cRef["FirstName"], cRef["MiddleName"],
        cRef["LastName"], cRef["Suffix"]);
      select.Columns.Add(e["Title"], "JobTitle");
      select.Columns.AddRange(cRef["Phone"], cRef["EmailAddress"], cRef["EmailPromotion"],
        a["AddressLine1"], a["AddressLine2"], a["City"]);
      select.Columns.Add(sp["Name"], "StateProvinceName");
      select.Columns.Add(a["PostalCode"]);
      select.Columns.Add(cr["Name"], "CountryRegionName");
      select.Columns.Add(cRef["AdditionalContactInfo"]);
    }
  }
}