using System.Data;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using NUnit.Framework;
using Xtensive.Sql.Common;
using Xtensive.Sql.Dom.Database;
using Xtensive.Sql.Dom.Dml;
using Constraint=Xtensive.Sql.Dom.Database.Constraint;

namespace Xtensive.Sql.Dom.Tests.VistaDb
{
  [TestFixture, Explicit]
  public class VistaDBAdventureWorks
  {
    private Model model;

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

    public Catalog Catalog
    {
      get { return model.DefaultServer.DefaultCatalog; }
    }

    [TestFixtureSetUp]
    public virtual void SetUp()
    {
//      BinaryModelProvider bmp = new BinaryModelProvider(@"C:/Debug/VistaDBAdventureWorks.bin");
//      model = Database.Model.Build(bmp);
//
//      if (model!=null)
//        return;

      model = new Model("default");
      model.CreateServer("localhost");
      model.DefaultServer.CreateCatalog("AdventureWorks");
      model.DefaultServer.DefaultCatalog.CreateSchema("dbo");

      Catalog.Schemas["dbo"].CreateCollation("Traditional_Spanish_CI_AI");
      Catalog.Schemas["dbo"].CreateCollation("SQL_Latin1_General_CP1_CI_AS");

      Table t;
      View v;
      TableColumn c;
      Constraint cs;

      t = Catalog.Schemas["dbo"].CreateTable("Production_TransactionHistoryArchive");
      t.CreateColumn("TransactionID", new SqlValueType(SqlDataType.Int32));
      t.CreateColumn("ProductID", new SqlValueType(SqlDataType.Int32));
      t.CreateColumn("ReferenceOrderID", new SqlValueType(SqlDataType.Int32));
      t.CreateColumn("ReferenceOrderLineID", new SqlValueType(SqlDataType.Int32));
      t.CreateColumn("TransactionDate", new SqlValueType(SqlDataType.DateTime));
      t.CreateColumn("TransactionType", new SqlValueType(SqlDataType.VarChar));
      t.CreateColumn("Quantity", new SqlValueType(SqlDataType.Int32));
      t.CreateColumn("ActualCost", new SqlValueType(SqlDataType.Money));
      t.CreateColumn("ModifiedDate", new SqlValueType(SqlDataType.DateTime));

      t = Catalog.Schemas["dbo"].CreateTable("Sales_CreditCard");
      t.CreateColumn("CreditCardID", new SqlValueType(SqlDataType.Int32));
      t.CreateColumn("CardType", new SqlValueType(SqlDataType.VarChar));
      t.CreateColumn("CardNumber", new SqlValueType(SqlDataType.VarChar));
      t.CreateColumn("ExpMonth", new SqlValueType(SqlDataType.Byte));
      t.CreateColumn("ExpYear", new SqlValueType(SqlDataType.Int16));
      t.CreateColumn("ModifiedDate", new SqlValueType(SqlDataType.DateTime));

      t = Catalog.Schemas["dbo"].CreateTable("Production_Document");
      t.CreateColumn("DocumentID", new SqlValueType(SqlDataType.Int32));
      t.CreateColumn("Title", new SqlValueType(SqlDataType.VarChar));
      t.CreateColumn("FileName", new SqlValueType(SqlDataType.VarChar));
      t.CreateColumn("FileExtension", new SqlValueType(SqlDataType.VarChar));
      t.CreateColumn("Revision", new SqlValueType(SqlDataType.VarChar));
      t.CreateColumn("ChangeNumber", new SqlValueType(SqlDataType.Int32));
      t.CreateColumn("Status", new SqlValueType(SqlDataType.Byte));
      t.CreateColumn("DocumentSummary", new SqlValueType(SqlDataType.VarChar));
      t.CreateColumn("Document", new SqlValueType(SqlDataType.VarBinaryMax));
      t.CreateColumn("ModifiedDate", new SqlValueType(SqlDataType.DateTime));

      t = Catalog.Schemas["dbo"].CreateTable("Production_Illustration");
      t.CreateColumn("IllustrationID", new SqlValueType(SqlDataType.Int32));
      t.CreateColumn("Diagram", new SqlValueType(SqlDataType.Xml));
      t.CreateColumn("ModifiedDate", new SqlValueType(SqlDataType.DateTime));

      t = Catalog.Schemas["dbo"].CreateTable("Production_ProductDescription");
      t.CreateColumn("ProductDescriptionID", new SqlValueType(SqlDataType.Int32));
      t.CreateColumn("Description", new SqlValueType(SqlDataType.VarChar));
      t.CreateColumn("ModifiedDate", new SqlValueType(SqlDataType.DateTime));

      t = Catalog.Schemas["dbo"].CreateTable("Sales_SpecialOffer");
      t.CreateColumn("SpecialOfferID", new SqlValueType(SqlDataType.Int32));
      t.CreateColumn("Description", new SqlValueType(SqlDataType.VarChar));
      t.CreateColumn("DiscountPct", new SqlValueType(SqlDataType.Int32));
      t.CreateColumn("Type", new SqlValueType(SqlDataType.VarChar));
      t.CreateColumn("Category", new SqlValueType(SqlDataType.VarChar));
      t.CreateColumn("StartDate", new SqlValueType(SqlDataType.DateTime));
      t.CreateColumn("EndDate", new SqlValueType(SqlDataType.DateTime));
      t.CreateColumn("MinQty", new SqlValueType(SqlDataType.Int32));
      t.CreateColumn("MaxQty", new SqlValueType(SqlDataType.Int32));
      t.CreateColumn("ModifiedDate", new SqlValueType(SqlDataType.DateTime));

      t = Catalog.Schemas["dbo"].CreateTable("Production_ProductPhoto");
      t.CreateColumn("ProductPhotoID", new SqlValueType(SqlDataType.Int32));
      t.CreateColumn("ThumbNailPhoto", new SqlValueType(SqlDataType.VarBinaryMax));
      t.CreateColumn("ThumbnailPhotoFileName", new SqlValueType(SqlDataType.VarChar));
      t.CreateColumn("LargePhoto", new SqlValueType(SqlDataType.VarBinaryMax));
      t.CreateColumn("LargePhotoFileName", new SqlValueType(SqlDataType.VarChar));
      t.CreateColumn("ModifiedDate", new SqlValueType(SqlDataType.DateTime));

      t = Catalog.Schemas["dbo"].CreateTable("Sales_Customer");
      t.CreateColumn("CustomerID", new SqlValueType(SqlDataType.Int32));
      t.CreateColumn("TerritoryID", new SqlValueType(SqlDataType.Int32));
      t.CreateColumn("AccountNumber", new SqlValueType(SqlDataType.VarChar));
      t.CreateColumn("CustomerType", new SqlValueType(SqlDataType.VarChar));
      t.CreateColumn("ModifiedDate", new SqlValueType(SqlDataType.DateTime));

      t = Catalog.Schemas["dbo"].CreateTable("Sales_CustomerAddress");
      t.CreateColumn("CustomerID", new SqlValueType(SqlDataType.Int32));
      t.CreateColumn("AddressID", new SqlValueType(SqlDataType.Int32));
      t.CreateColumn("AddressTypeID", new SqlValueType(SqlDataType.Int32));
      t.CreateColumn("ModifiedDate", new SqlValueType(SqlDataType.DateTime));

      t = Catalog.Schemas["dbo"].CreateTable("HumanResources_EmployeeAddress");
      t.CreateColumn("EmployeeID", new SqlValueType(SqlDataType.Int32));
      t.CreateColumn("AddressID", new SqlValueType(SqlDataType.Int32));
      t.CreateColumn("ModifiedDate", new SqlValueType(SqlDataType.DateTime));

      t = Catalog.Schemas["dbo"].CreateTable("Purchasing_VendorAddress");
      t.CreateColumn("VendorID", new SqlValueType(SqlDataType.Int32));
      t.CreateColumn("AddressID", new SqlValueType(SqlDataType.Int32));
      t.CreateColumn("AddressTypeID", new SqlValueType(SqlDataType.Int32));
      t.CreateColumn("ModifiedDate", new SqlValueType(SqlDataType.DateTime));

      t = Catalog.Schemas["dbo"].CreateTable("Purchasing_ProductVendor");
      t.CreateColumn("ProductID", new SqlValueType(SqlDataType.Int32));
      t.CreateColumn("VendorID", new SqlValueType(SqlDataType.Int32));
      t.CreateColumn("AverageLeadTime", new SqlValueType(SqlDataType.Int32));
      t.CreateColumn("StandardPrice", new SqlValueType(SqlDataType.Money));
      t.CreateColumn("LastReceiptCost", new SqlValueType(SqlDataType.Money));
      t.CreateColumn("LastReceiptDate", new SqlValueType(SqlDataType.DateTime));
      t.CreateColumn("MinOrderQty", new SqlValueType(SqlDataType.Int32));
      t.CreateColumn("MaxOrderQty", new SqlValueType(SqlDataType.Int32));
      t.CreateColumn("OnOrderQty", new SqlValueType(SqlDataType.Int32));
      t.CreateColumn("UnitMeasureCode", new SqlValueType(SqlDataType.VarChar));
      t.CreateColumn("ModifiedDate", new SqlValueType(SqlDataType.DateTime));

      t = Catalog.Schemas["dbo"].CreateTable("Production_BillOfMaterials");
      t.CreateColumn("BillOfMaterialsID", new SqlValueType(SqlDataType.Int32));
      t.CreateColumn("ProductAssemblyID", new SqlValueType(SqlDataType.Int32));
      t.CreateColumn("ComponentID", new SqlValueType(SqlDataType.Int32));
      t.CreateColumn("StartDate", new SqlValueType(SqlDataType.DateTime));
      t.CreateColumn("EndDate", new SqlValueType(SqlDataType.DateTime));
      t.CreateColumn("UnitMeasureCode", new SqlValueType(SqlDataType.VarChar));
      t.CreateColumn("BOMLevel", new SqlValueType(SqlDataType.Int16));
      t.CreateColumn("PerAssemblyQty", new SqlValueType(SqlDataType.Decimal));
      t.CreateColumn("ModifiedDate", new SqlValueType(SqlDataType.DateTime));

      t = Catalog.Schemas["dbo"].CreateTable("Purchasing_PurchaseOrderHeader");
      t.CreateColumn("PurchaseOrderID", new SqlValueType(SqlDataType.Int32));
      t.CreateColumn("RevisionNumber", new SqlValueType(SqlDataType.Byte));
      t.CreateColumn("Status", new SqlValueType(SqlDataType.Byte));
      t.CreateColumn("EmployeeID", new SqlValueType(SqlDataType.Int32));
      t.CreateColumn("VendorID", new SqlValueType(SqlDataType.Int32));
      t.CreateColumn("ShipMethodID", new SqlValueType(SqlDataType.Int32));
      t.CreateColumn("OrderDate", new SqlValueType(SqlDataType.DateTime));
      t.CreateColumn("ShipDate", new SqlValueType(SqlDataType.DateTime));
      t.CreateColumn("SubTotal", new SqlValueType(SqlDataType.Money));
      t.CreateColumn("TaxAmt", new SqlValueType(SqlDataType.Money));
      t.CreateColumn("Freight", new SqlValueType(SqlDataType.Money));
      t.CreateColumn("TotalDue", new SqlValueType(SqlDataType.Money));
      t.CreateColumn("ModifiedDate", new SqlValueType(SqlDataType.DateTime));

      t = Catalog.Schemas["dbo"].CreateTable("Purchasing_VendorContact");
      t.CreateColumn("VendorID", new SqlValueType(SqlDataType.Int32));
      t.CreateColumn("ContactID", new SqlValueType(SqlDataType.Int32));
      t.CreateColumn("ContactTypeID", new SqlValueType(SqlDataType.Int32));
      t.CreateColumn("ModifiedDate", new SqlValueType(SqlDataType.DateTime));

      t = Catalog.Schemas["dbo"].CreateTable("Sales_ContactCreditCard");
      t.CreateColumn("ContactID", new SqlValueType(SqlDataType.Int32));
      t.CreateColumn("CreditCardID", new SqlValueType(SqlDataType.Int32));
      t.CreateColumn("ModifiedDate", new SqlValueType(SqlDataType.DateTime));

      t = Catalog.Schemas["dbo"].CreateTable("Sales_StoreContact");
      t.CreateColumn("CustomerID", new SqlValueType(SqlDataType.Int32));
      t.CreateColumn("ContactID", new SqlValueType(SqlDataType.Int32));
      t.CreateColumn("ContactTypeID", new SqlValueType(SqlDataType.Int32));
      t.CreateColumn("ModifiedDate", new SqlValueType(SqlDataType.DateTime));

      t = Catalog.Schemas["dbo"].CreateTable("Purchasing_PurchaseOrderDetail");
      t.CreateColumn("PurchaseOrderID", new SqlValueType(SqlDataType.Int32));
      t.CreateColumn("PurchaseOrderDetailID", new SqlValueType(SqlDataType.Int32));
      t.CreateColumn("DueDate", new SqlValueType(SqlDataType.Int32));
      t.CreateColumn("OrderQty", new SqlValueType(SqlDataType.Int32));
      t.CreateColumn("ProductID", new SqlValueType(SqlDataType.Int32));
      t.CreateColumn("UnitPrice", new SqlValueType(SqlDataType.Int32));
      c = t.CreateColumn("LineTotal");
      SqlTableRef pod = Sql.TableRef(t);
      c.Expression = pod["OrderQty"]*pod["UnitPrice"];
      c.IsPersisted = false;
      t.CreateColumn("ReceivedQty", new SqlValueType(SqlDataType.Int32));
      t.CreateColumn("RejectedQty", new SqlValueType(SqlDataType.Int32));
      t.CreateColumn("StockedQty", new SqlValueType(SqlDataType.Int32));
      t.CreateColumn("ModifiedDate", new SqlValueType(SqlDataType.DateTime));

      t = Catalog.Schemas["dbo"].CreateTable("Production_WorkOrderRouting");
      t.CreateColumn("WorkOrderID", new SqlValueType(SqlDataType.Int32));
      t.CreateColumn("ProductID", new SqlValueType(SqlDataType.Int32));
      t.CreateColumn("OperationSequence", new SqlValueType(SqlDataType.Int16));
      t.CreateColumn("LocationID", new SqlValueType(SqlDataType.Int32));
      t.CreateColumn("ScheduledStartDate", new SqlValueType(SqlDataType.DateTime));
      t.CreateColumn("ScheduledEndDate", new SqlValueType(SqlDataType.DateTime));
      t.CreateColumn("ActualStartDate", new SqlValueType(SqlDataType.DateTime));
      t.CreateColumn("ActualEndDate", new SqlValueType(SqlDataType.DateTime));
      t.CreateColumn("ActualResourceHrs", new SqlValueType(SqlDataType.Decimal));
      t.CreateColumn("PlannedCost", new SqlValueType(SqlDataType.Money));
      t.CreateColumn("ActualCost", new SqlValueType(SqlDataType.Money));
      t.CreateColumn("ModifiedDate", new SqlValueType(SqlDataType.DateTime));

      t = Catalog.Schemas["dbo"].CreateTable("Sales_CountryRegionCurrency");
      t.CreateColumn("CountryRegionCode", new SqlValueType(SqlDataType.VarChar));
      t.CreateColumn("CurrencyCode", new SqlValueType(SqlDataType.VarChar));
      t.CreateColumn("ModifiedDate", new SqlValueType(SqlDataType.DateTime));

      t = Catalog.Schemas["dbo"].CreateTable("Production_ProductModelProductDescriptionCulture");
      t.CreateColumn("ProductModelID", new SqlValueType(SqlDataType.Int32));
      t.CreateColumn("ProductDescriptionID", new SqlValueType(SqlDataType.Int32));
      t.CreateColumn("CultureID", new SqlValueType(SqlDataType.VarChar));
      t.CreateColumn("ModifiedDate", new SqlValueType(SqlDataType.DateTime));

      t = Catalog.Schemas["dbo"].CreateTable("Sales_CurrencyRate");
      t.CreateColumn("CurrencyRateID", new SqlValueType(SqlDataType.Int32));
      t.CreateColumn("CurrencyRateDate", new SqlValueType(SqlDataType.DateTime));
      t.CreateColumn("FromCurrencyCode", new SqlValueType(SqlDataType.VarChar));
      t.CreateColumn("ToCurrencyCode", new SqlValueType(SqlDataType.VarChar));
      t.CreateColumn("AverageRate", new SqlValueType(SqlDataType.Money));
      t.CreateColumn("EndOfDayRate", new SqlValueType(SqlDataType.Money));
      t.CreateColumn("ModifiedDate", new SqlValueType(SqlDataType.DateTime));

      t = Catalog.Schemas["dbo"].CreateTable("Sales_SalesOrderDetail");
      t.CreateColumn("SalesOrderID", new SqlValueType(SqlDataType.Int32));
      t.CreateColumn("SalesOrderDetailID", new SqlValueType(SqlDataType.Int32));
      t.CreateColumn("CarrierTrackingNumber", new SqlValueType(SqlDataType.VarChar));
      t.CreateColumn("OrderQty", new SqlValueType(SqlDataType.Int16));
      t.CreateColumn("ProductID", new SqlValueType(SqlDataType.Int32));
      t.CreateColumn("SpecialOfferID", new SqlValueType(SqlDataType.Int32));
      t.CreateColumn("UnitPrice", new SqlValueType(SqlDataType.Money));
      t.CreateColumn("UnitPriceDiscount", new SqlValueType(SqlDataType.Money));
      t.CreateColumn("LineTotal", new SqlValueType(SqlDataType.Int32));
      t.CreateColumn("ModifiedDate", new SqlValueType(SqlDataType.DateTime));

      t = Catalog.Schemas["dbo"].CreateTable("Sales_SalesOrderHeaderSalesReason");
      t.CreateColumn("SalesOrderID", new SqlValueType(SqlDataType.Int32));
      t.CreateColumn("SalesReasonID", new SqlValueType(SqlDataType.Int32));
      t.CreateColumn("ModifiedDate", new SqlValueType(SqlDataType.DateTime));

      t = Catalog.Schemas["dbo"].CreateTable("HumanResources_EmployeeDepartmentHistory");
      t.CreateColumn("EmployeeID", new SqlValueType(SqlDataType.Int32));
      t.CreateColumn("DepartmentID", new SqlValueType(SqlDataType.Int16));
      t.CreateColumn("ShiftID", new SqlValueType(SqlDataType.DateTime));
      t.CreateColumn("StartDate", new SqlValueType(SqlDataType.DateTime));
      t.CreateColumn("EndDate", new SqlValueType(SqlDataType.DateTime));
      t.CreateColumn("ModifiedDate", new SqlValueType(SqlDataType.DateTime));

      t = Catalog.Schemas["dbo"].CreateTable("Production_ProductDocument");
      t.CreateColumn("ProductID", new SqlValueType(SqlDataType.Int32));
      t.CreateColumn("DocumentID", new SqlValueType(SqlDataType.Int32));
      t.CreateColumn("ModifiedDate", new SqlValueType(SqlDataType.DateTime));

      t = Catalog.Schemas["dbo"].CreateTable("HumanResources_EmployeePayHistory");
      t.CreateColumn("EmployeeID", new SqlValueType(SqlDataType.Int32));
      t.CreateColumn("RateChangeDate", new SqlValueType(SqlDataType.DateTime));
      t.CreateColumn("Rate", new SqlValueType(SqlDataType.Money));
      t.CreateColumn("PayFrequency", new SqlValueType(SqlDataType.Byte));
      t.CreateColumn("ModifiedDate", new SqlValueType(SqlDataType.DateTime));

      t = Catalog.Schemas["dbo"].CreateTable("Sales_SalesPerson");
      t.CreateColumn("SalesPersonID", new SqlValueType(SqlDataType.Int32));
      t.CreateColumn("TerritoryID", new SqlValueType(SqlDataType.Int32));
      t.CreateColumn("SalesQuota", new SqlValueType(SqlDataType.Money));
      t.CreateColumn("Bonus", new SqlValueType(SqlDataType.Money));
      t.CreateColumn("CommissionPct", new SqlValueType(SqlDataType.Money));
      t.CreateColumn("SalesYTD", new SqlValueType(SqlDataType.Money));
      t.CreateColumn("SalesLastYear", new SqlValueType(SqlDataType.Money));
      t.CreateColumn("ModifiedDate", new SqlValueType(SqlDataType.DateTime));

      t = Catalog.Schemas["dbo"].CreateTable("Sales_SalesPersonQuotaHistory");
      t.CreateColumn("SalesPersonID", new SqlValueType(SqlDataType.Int32));
      t.CreateColumn("QuotaDate", new SqlValueType(SqlDataType.DateTime));
      t.CreateColumn("SalesQuota", new SqlValueType(SqlDataType.Money));
      t.CreateColumn("ModifiedDate", new SqlValueType(SqlDataType.DateTime));

      t = Catalog.Schemas["dbo"].CreateTable("Sales_SalesTerritoryHistory");
      t.CreateColumn("SalesPersonID", new SqlValueType(SqlDataType.Int32));
      t.CreateColumn("TerritoryID", new SqlValueType(SqlDataType.Int32));
      t.CreateColumn("StartDate", new SqlValueType(SqlDataType.DateTime));
      t.CreateColumn("EndDate", new SqlValueType(SqlDataType.DateTime));
      t.CreateColumn("ModifiedDate", new SqlValueType(SqlDataType.DateTime));

      t = Catalog.Schemas["dbo"].CreateTable("Production_ProductModelIllustration");
      t.CreateColumn("ProductModelID", new SqlValueType(SqlDataType.Int32));
      t.CreateColumn("IllustrationID", new SqlValueType(SqlDataType.Int32));
      t.CreateColumn("ModifiedDate", new SqlValueType(SqlDataType.DateTime));

      t = Catalog.Schemas["dbo"].CreateTable("Production_ProductInventory");
      t.CreateColumn("ProductID", new SqlValueType(SqlDataType.Int32));
      t.CreateColumn("LocationID", new SqlValueType(SqlDataType.Int32));
      t.CreateColumn("Shelf", new SqlValueType(SqlDataType.VarChar));
      t.CreateColumn("Bin", new SqlValueType(SqlDataType.Byte));
      t.CreateColumn("Quantity", new SqlValueType(SqlDataType.Byte));
      t.CreateColumn("ModifiedDate", new SqlValueType(SqlDataType.DateTime));

      t = Catalog.Schemas["dbo"].CreateTable("Production_WorkOrder");
      t.CreateColumn("WorkOrderID", new SqlValueType(SqlDataType.Int32));
      t.CreateColumn("ProductID", new SqlValueType(SqlDataType.Int32));
      t.CreateColumn("OrderQty", new SqlValueType(SqlDataType.Int32));
      t.CreateColumn("StockedQty", new SqlValueType(SqlDataType.Int32));
      t.CreateColumn("ScrappedQty", new SqlValueType(SqlDataType.Int16));
      t.CreateColumn("StartDate", new SqlValueType(SqlDataType.DateTime));
      t.CreateColumn("EndDate", new SqlValueType(SqlDataType.DateTime));
      t.CreateColumn("DueDate", new SqlValueType(SqlDataType.DateTime));
      t.CreateColumn("ScrapReasonID", new SqlValueType(SqlDataType.Int16));
      t.CreateColumn("ModifiedDate", new SqlValueType(SqlDataType.DateTime));

      t = Catalog.Schemas["dbo"].CreateTable("Production_TransactionHistory");
      t.CreateColumn("TransactionID", new SqlValueType(SqlDataType.Int32));
      t.CreateColumn("ProductID", new SqlValueType(SqlDataType.Int32));
      t.CreateColumn("ReferenceOrderID", new SqlValueType(SqlDataType.Int32));
      t.CreateColumn("ReferenceOrderLineID", new SqlValueType(SqlDataType.Int32));
      t.CreateColumn("TransactionDate", new SqlValueType(SqlDataType.DateTime));
      t.CreateColumn("TransactionType", new SqlValueType(SqlDataType.VarChar));
      t.CreateColumn("Quantity", new SqlValueType(SqlDataType.Int32));
      t.CreateColumn("ActualCost", new SqlValueType(SqlDataType.Money));
      t.CreateColumn("ModifiedDate", new SqlValueType(SqlDataType.DateTime));

      t = Catalog.Schemas["dbo"].CreateTable("Sales_ShoppingCartItem");
      t.CreateColumn("ShoppingCartItemID", new SqlValueType(SqlDataType.Int32));
      t.CreateColumn("ShoppingCartID", new SqlValueType(SqlDataType.VarChar));
      t.CreateColumn("Quantity", new SqlValueType(SqlDataType.Int32));
      t.CreateColumn("ProductID", new SqlValueType(SqlDataType.Int32));
      t.CreateColumn("DateCreated", new SqlValueType(SqlDataType.DateTime));
      t.CreateColumn("ModifiedDate", new SqlValueType(SqlDataType.DateTime));

      t = Catalog.Schemas["dbo"].CreateTable("Production_ProductListPriceHistory");
      t.CreateColumn("ProductID", new SqlValueType(SqlDataType.Int32));
      t.CreateColumn("StartDate", new SqlValueType(SqlDataType.DateTime));
      t.CreateColumn("EndDate", new SqlValueType(SqlDataType.DateTime));
      t.CreateColumn("ListPrice", new SqlValueType(SqlDataType.Money));
      t.CreateColumn("ModifiedDate", new SqlValueType(SqlDataType.DateTime));

      t = Catalog.Schemas["dbo"].CreateTable("Sales_SpecialOfferProduct");
      t.CreateColumn("SpecialOfferID", new SqlValueType(SqlDataType.Int32));
      t.CreateColumn("ProductID", new SqlValueType(SqlDataType.Int32));
      t.CreateColumn("ModifiedDate", new SqlValueType(SqlDataType.DateTime));

      t = Catalog.Schemas["dbo"].CreateTable("Production_ProductCostHistory");
      t.CreateColumn("ProductID", new SqlValueType(SqlDataType.Int32));
      t.CreateColumn("StartDate", new SqlValueType(SqlDataType.DateTime));
      t.CreateColumn("EndDate", new SqlValueType(SqlDataType.DateTime));
      t.CreateColumn("StandardCost", new SqlValueType(SqlDataType.Money));
      t.CreateColumn("ModifiedDate", new SqlValueType(SqlDataType.DateTime));

      t = Catalog.Schemas["dbo"].CreateTable("Person_Address");
      t.CreateColumn("AddressID", new SqlValueType(SqlDataType.Int32));
      t.CreateColumn("AddressLine1", new SqlValueType(SqlDataType.VarChar));
      t.CreateColumn("AddressLine2", new SqlValueType(SqlDataType.VarChar));
      t.CreateColumn("City", new SqlValueType(SqlDataType.VarChar));
      t.CreateColumn("StateProvinceID", new SqlValueType(SqlDataType.Int32));
      t.CreateColumn("PostalCode", new SqlValueType(SqlDataType.VarChar));
      t.CreateColumn("ModifiedDate", new SqlValueType(SqlDataType.DateTime));

      t = Catalog.Schemas["dbo"].CreateTable("Purchasing_Vendor");
      t.CreateColumn("VendorID", new SqlValueType(SqlDataType.Int32));
      t.CreateColumn("AccountNumber", new SqlValueType(SqlDataType.VarChar));
      t.CreateColumn("Name", new SqlValueType(SqlDataType.VarChar));
      t.CreateColumn("CreditRating", new SqlValueType(SqlDataType.Byte));
      t.CreateColumn("PreferredVendorStatus", new SqlValueType(SqlDataType.Boolean));
      t.CreateColumn("ActiveFlag", new SqlValueType(SqlDataType.Boolean));
      t.CreateColumn("PurchasingWebServiceURL", new SqlValueType(SqlDataType.VarChar));
      t.CreateColumn("ModifiedDate", new SqlValueType(SqlDataType.DateTime));

      t = Catalog.Schemas["dbo"].CreateTable("Sales_SalesOrderHeader");
      t.CreateColumn("SalesOrderID", new SqlValueType(SqlDataType.Int32));
      t.CreateColumn("RevisionNumber", new SqlValueType(SqlDataType.Byte));
      t.CreateColumn("OrderDate", new SqlValueType(SqlDataType.DateTime));
      t.CreateColumn("DueDate", new SqlValueType(SqlDataType.DateTime));
      t.CreateColumn("ShipDate", new SqlValueType(SqlDataType.DateTime));
      t.CreateColumn("Status", new SqlValueType(SqlDataType.Byte));
      t.CreateColumn("OnlineOrderFlag", new SqlValueType(SqlDataType.Boolean));
      t.CreateColumn("SalesOrderNumber", new SqlValueType(SqlDataType.VarChar));
      t.CreateColumn("PurchaseOrderNumber", new SqlValueType(SqlDataType.VarChar));
      t.CreateColumn("AccountNumber", new SqlValueType(SqlDataType.VarChar));
      t.CreateColumn("CustomerID", new SqlValueType(SqlDataType.Int32));
      t.CreateColumn("ContactID", new SqlValueType(SqlDataType.Int32));
      t.CreateColumn("SalesPersonID", new SqlValueType(SqlDataType.Int32));
      t.CreateColumn("TerritoryID", new SqlValueType(SqlDataType.Int32));
      t.CreateColumn("BillToAddressID", new SqlValueType(SqlDataType.Int32));
      t.CreateColumn("ShipToAddressID", new SqlValueType(SqlDataType.Int32));
      t.CreateColumn("ShipMethodID", new SqlValueType(SqlDataType.Int32));
      t.CreateColumn("CreditCardID", new SqlValueType(SqlDataType.Int32));
      t.CreateColumn("CreditCardApprovalCode", new SqlValueType(SqlDataType.VarChar));
      t.CreateColumn("CurrencyRateID", new SqlValueType(SqlDataType.Int32));
      t.CreateColumn("SubTotal", new SqlValueType(SqlDataType.Money));
      t.CreateColumn("TaxAmt", new SqlValueType(SqlDataType.Money));
      t.CreateColumn("Freight", new SqlValueType(SqlDataType.Money));
      t.CreateColumn("TotalDue", new SqlValueType(SqlDataType.Money));
      t.CreateColumn("Comment", new SqlValueType(SqlDataType.VarChar));
      t.CreateColumn("ModifiedDate", new SqlValueType(SqlDataType.DateTime));

      t = Catalog.Schemas["dbo"].CreateTable("HumanResources_Employee");
      t.CreateColumn("EmployeeID", new SqlValueType(SqlDataType.Int32));
      t.CreateColumn("NationalIDNumber", new SqlValueType(SqlDataType.VarChar));
      t.CreateColumn("ContactID", new SqlValueType(SqlDataType.Int32));
      t.CreateColumn("LoginID", new SqlValueType(SqlDataType.VarChar));
      t.CreateColumn("ManagerID", new SqlValueType(SqlDataType.Int32));
      t.CreateColumn("Title", new SqlValueType(SqlDataType.VarChar));
      t.CreateColumn("BirthDate", new SqlValueType(SqlDataType.DateTime));
      t.CreateColumn("MaritalStatus", new SqlValueType(SqlDataType.VarChar));
      t.CreateColumn("Gender", new SqlValueType(SqlDataType.VarChar));
      t.CreateColumn("HireDate", new SqlValueType(SqlDataType.DateTime));
      t.CreateColumn("SalariedFlag", new SqlValueType(SqlDataType.Boolean));
      t.CreateColumn("VacationHours", new SqlValueType(SqlDataType.Int16));
      t.CreateColumn("SickLeaveHours", new SqlValueType(SqlDataType.Int16));
      t.CreateColumn("CurrentFlag", new SqlValueType(SqlDataType.Boolean));
      t.CreateColumn("ModifiedDate", new SqlValueType(SqlDataType.DateTime));

      t = Catalog.Schemas["dbo"].CreateTable("Production_ProductProductPhoto");
      t.CreateColumn("ProductID", new SqlValueType(SqlDataType.Int32));
      t.CreateColumn("ProductPhotoID", new SqlValueType(SqlDataType.Int32));
      t.CreateColumn("Primary", new SqlValueType(SqlDataType.VarChar));
      t.CreateColumn("ModifiedDate", new SqlValueType(SqlDataType.DateTime));

      t = Catalog.Schemas["dbo"].CreateTable("Person_StateProvince");
      t.CreateColumn("StateProvinceID", new SqlValueType(SqlDataType.Int32));
      t.CreateColumn("StateProvinceCode", new SqlValueType(SqlDataType.VarChar));
      t.CreateColumn("CountryRegionCode", new SqlValueType(SqlDataType.VarChar));
      t.CreateColumn("IsOnlyStateProvinceFlag", new SqlValueType(SqlDataType.Boolean));
      t.CreateColumn("Name", new SqlValueType(SqlDataType.VarChar));
      t.CreateColumn("TerritoryID", new SqlValueType(SqlDataType.Int32));
      t.CreateColumn("ModifiedDate", new SqlValueType(SqlDataType.DateTime));

      t = Catalog.Schemas["dbo"].CreateTable("Production_ProductModel");
      t.CreateColumn("ProductModelID", new SqlValueType(SqlDataType.Int32));
      t.CreateColumn("Name", new SqlValueType(SqlDataType.VarChar));
      t.CreateColumn("CatalogDescription", new SqlValueType(SqlDataType.Xml));
      t.CreateColumn("Instructions", new SqlValueType(SqlDataType.Xml));
      t.CreateColumn("ModifiedDate", new SqlValueType(SqlDataType.DateTime));

      t = Catalog.Schemas["dbo"].CreateTable("Production_Product");
      c = t.CreateColumn("ProductID", new SqlValueType(SqlDataType.Int32));
      c.SequenceDescriptor = new SequenceDescriptor(c, 1, 1);
      c.IsNullable = false;
      c = t.CreateColumn("Name", new SqlValueType(SqlDataType.VarChar, 50));
      c.IsNullable = false;
      c = t.CreateColumn("ProductNumber", new SqlValueType(SqlDataType.VarChar, 25));
      c.IsNullable = false;
      c = t.CreateColumn("MakeFlag", new SqlValueType(SqlDataType.Boolean));
      c.DefaultValue = 1;
      c.IsNullable = false;
      c = t.CreateColumn("FinishedGoodsFlag", new SqlValueType(SqlDataType.Boolean));
      c.DefaultValue = 1;
      c.IsNullable = false;
      t.CreateColumn("Color", new SqlValueType(SqlDataType.VarChar, 15));
      c = t.CreateColumn("SafetyStockLevel", new SqlValueType(SqlDataType.Int16));
      c.IsNullable = false;
      c = t.CreateColumn("ReorderPoint", new SqlValueType(SqlDataType.Int16));
      c.IsNullable = false;
      c = t.CreateColumn("StandardCost", new SqlValueType(SqlDataType.Money));
      c.IsNullable = false;
      c = t.CreateColumn("ListPrice", new SqlValueType(SqlDataType.Money));
      c.IsNullable = false;
      t.CreateColumn("Size", new SqlValueType(SqlDataType.VarChar, 5));
      t.CreateColumn("SizeUnitMeasureCode", new SqlValueType(SqlDataType.Char, 3));
      t.CreateColumn("WeightUnitMeasureCode", new SqlValueType(SqlDataType.Char, 3));
      t.CreateColumn("Weight", new SqlValueType(SqlDataType.Decimal, 8, 2));
      c = t.CreateColumn("DaysToManufacture", new SqlValueType(SqlDataType.Int32));
      c.IsNullable = false;
      t.CreateColumn("ProductLine", new SqlValueType(SqlDataType.Char, 2));
      t.CreateColumn("Class", new SqlValueType(SqlDataType.Char, 2));
      t.CreateColumn("Style", new SqlValueType(SqlDataType.Char, 2));
      t.CreateColumn("ProductSubcategoryID", new SqlValueType(SqlDataType.Int32));
      t.CreateColumn("ProductModelID", new SqlValueType(SqlDataType.Int32));
      c = t.CreateColumn("SellStartDate", new SqlValueType(SqlDataType.DateTime));
      c.IsNullable = false;
      t.CreateColumn("SellEndDate", new SqlValueType(SqlDataType.DateTime));
      t.CreateColumn("DiscontinuedDate", new SqlValueType(SqlDataType.DateTime));
      c = t.CreateColumn("ModifiedDate", new SqlValueType(SqlDataType.DateTime));
      c.IsNullable = false;
      SqlTableRef st = Sql.TableRef(t);
      t.CreateCheckConstraint("CK_Product_Class", Sql.Upper(st["Class"])=='H' ||
        Sql.Upper(st["Class"])=='M' ||
          Sql.Upper(st["Class"])=='L' ||
            Sql.IsNull(st["Class"]));
      t.CreateCheckConstraint("CK_Product_DaysToManufacture", Sql.Upper(st["DaysToManufacture"])>=0);
      t.CreateCheckConstraint("CK_Product_ListPrice", Sql.Upper(st["ListPrice"])>=0);
      t.CreateCheckConstraint("CK_Product_ProductLine", Sql.Upper(st["ProductLine"])=='R' ||
        Sql.Upper(st["ProductLine"])=='M' ||
          Sql.Upper(st["ProductLine"])=='T' ||
            Sql.Upper(st["ProductLine"])=='S' ||
              Sql.IsNull(st["ProductLine"]));
      t.CreateCheckConstraint("CK_Product_ReorderPoint", Sql.Upper(st["ReorderPoint"])>0);
      t.CreateCheckConstraint("CK_Product_SafetyStockLevel", Sql.Upper(st["SafetyStockLevel"])>0);
      t.CreateCheckConstraint("CK_Product_SellEndDate", Sql.Upper(st["SellEndDate"])>st["SellStartDate"] ||
        Sql.IsNull(st["SellEndDate"]));
      t.CreateCheckConstraint("CK_Product_StandardCost", Sql.Upper(st["StandardCost"])>=0);
      t.CreateCheckConstraint("CK_Product_Style", Sql.Upper(st["Style"])=='U' ||
        Sql.Upper(st["Style"])=='M' ||
          Sql.Upper(st["Style"])=='W' ||
            Sql.IsNull(st["Style"]));
      t.CreateCheckConstraint("CK_Product_Weight", Sql.Upper(st["Weight"])>0);
      t.CreatePrimaryKey("PK_Product_ProductID", t.TableColumns["ProductID"]);
      cs = t.CreateForeignKey("FK_Product_ProductModel_ProductModelID");
      ((ForeignKey)cs).Columns.Add(t.TableColumns["ProductModelID"]);
      ((ForeignKey)cs).ReferencedColumns.Add(Catalog.Schemas["dbo"].Tables["Production_ProductModel"].TableColumns["ProductModelID"]);

      t = Catalog.Schemas["dbo"].CreateTable("Person_Contact");
      t.CreateColumn("ContactID", new SqlValueType(SqlDataType.Int32));
      t.CreateColumn("NameStyle", new SqlValueType(SqlDataType.Int32));
      t.CreateColumn("Title", new SqlValueType(SqlDataType.VarChar));
      t.CreateColumn("FirstName", new SqlValueType(SqlDataType.VarChar));
      t.CreateColumn("MiddleName", new SqlValueType(SqlDataType.VarChar));
      t.CreateColumn("LastName", new SqlValueType(SqlDataType.VarChar));
      t.CreateColumn("Suffix", new SqlValueType(SqlDataType.VarChar));
      t.CreateColumn("EmailAddress", new SqlValueType(SqlDataType.VarChar));
      t.CreateColumn("EmailPromotion", new SqlValueType(SqlDataType.Int32));
      t.CreateColumn("Phone", new SqlValueType(SqlDataType.VarChar));
      t.CreateColumn("PasswordHash", new SqlValueType(SqlDataType.VarChar));
      t.CreateColumn("PasswordSalt", new SqlValueType(SqlDataType.VarChar));
      t.CreateColumn("AdditionalContactInfo", new SqlValueType(SqlDataType.Xml));
      t.CreateColumn("ModifiedDate", new SqlValueType(SqlDataType.DateTime));

      t = Catalog.Schemas["dbo"].CreateTable("Production_UnitMeasure");
      t.CreateColumn("UnitMeasureCode", new SqlValueType(SqlDataType.VarChar));
      t.CreateColumn("Name", new SqlValueType(SqlDataType.VarChar));
      t.CreateColumn("ModifiedDate", new SqlValueType(SqlDataType.DateTime));

      t = Catalog.Schemas["dbo"].CreateTable("Production_ProductReview");
      t.CreateColumn("ProductReviewID", new SqlValueType(SqlDataType.Int32));
      t.CreateColumn("ProductID", new SqlValueType(SqlDataType.Int32));
      t.CreateColumn("ReviewerName", new SqlValueType(SqlDataType.VarChar));
      t.CreateColumn("ReviewDate", new SqlValueType(SqlDataType.DateTime));
      t.CreateColumn("EmailAddress", new SqlValueType(SqlDataType.VarChar));
      t.CreateColumn("Rating", new SqlValueType(SqlDataType.Int32));
      t.CreateColumn("Comments", new SqlValueType(SqlDataType.VarChar));
      t.CreateColumn("ModifiedDate", new SqlValueType(SqlDataType.DateTime));

      t = Catalog.Schemas["dbo"].CreateTable("Production_ProductSubcategory");
      t.CreateColumn("ProductSubcategoryID", new SqlValueType(SqlDataType.Int32));
      t.CreateColumn("ProductCategoryID", new SqlValueType(SqlDataType.Int32));
      t.CreateColumn("Name", new SqlValueType(SqlDataType.VarChar));
      t.CreateColumn("ModifiedDate", new SqlValueType(SqlDataType.DateTime));

      t = Catalog.Schemas["dbo"].CreateTable("Person_AddressType");
      t.CreateColumn("AddressTypeID", new SqlValueType(SqlDataType.Int32));
      t.CreateColumn("Name", new SqlValueType(SqlDataType.VarChar));
      t.CreateColumn("ModifiedDate", new SqlValueType(SqlDataType.DateTime));

      t = Catalog.Schemas["dbo"].CreateTable("Sales_SalesReason");
      t.CreateColumn("SalesReasonID", new SqlValueType(SqlDataType.Int32));
      t.CreateColumn("Name", new SqlValueType(SqlDataType.VarChar));
      t.CreateColumn("ReasonType", new SqlValueType(SqlDataType.VarChar));
      t.CreateColumn("ModifiedDate", new SqlValueType(SqlDataType.DateTime));

      t = Catalog.Schemas["dbo"].CreateTable("HumanResources_Department");
      t.CreateColumn("DepartmentID", new SqlValueType(SqlDataType.Int32));
      t.CreateColumn("Name", new SqlValueType(SqlDataType.VarChar));
      t.CreateColumn("GroupName", new SqlValueType(SqlDataType.VarChar));
      t.CreateColumn("ModifiedDate", new SqlValueType(SqlDataType.DateTime));

      t = Catalog.Schemas["dbo"].CreateTable("Person_CountryRegion");
      t.CreateColumn("CountryRegionCode", new SqlValueType(SqlDataType.VarChar));
      t.CreateColumn("Name", new SqlValueType(SqlDataType.VarChar));
      t.CreateColumn("ModifiedDate", new SqlValueType(SqlDataType.DateTime));

      t = Catalog.Schemas["dbo"].CreateTable("Production_Culture");
      t.CreateColumn("CultureID", new SqlValueType(SqlDataType.Int32));
      t.CreateColumn("Name", new SqlValueType(SqlDataType.VarChar));
      t.CreateColumn("ModifiedDate", new SqlValueType(SqlDataType.DateTime));

      t = Catalog.Schemas["dbo"].CreateTable("Sales_Currency");
      t.CreateColumn("CurrencyCode", new SqlValueType(SqlDataType.VarChar));
      t.CreateColumn("Name", new SqlValueType(SqlDataType.VarChar));
      t.CreateColumn("ModifiedDate", new SqlValueType(SqlDataType.DateTime));

      t = Catalog.Schemas["dbo"].CreateTable("Person_ContactType");
      t.CreateColumn("ContactTypeID", new SqlValueType(SqlDataType.Int32));
      t.CreateColumn("Name", new SqlValueType(SqlDataType.VarChar));
      t.CreateColumn("ModifiedDate", new SqlValueType(SqlDataType.DateTime));

      t = Catalog.Schemas["dbo"].CreateTable("Sales_SalesTaxRate");
      t.CreateColumn("SalesTaxRateID", new SqlValueType(SqlDataType.Int32));
      t.CreateColumn("StateProvinceID", new SqlValueType(SqlDataType.Int32));
      t.CreateColumn("TaxType", new SqlValueType(SqlDataType.Byte));
      t.CreateColumn("TaxRate", new SqlValueType(SqlDataType.Money));
      t.CreateColumn("Name", new SqlValueType(SqlDataType.VarChar));
      t.CreateColumn("ModifiedDate", new SqlValueType(SqlDataType.DateTime));

      t = Catalog.Schemas["dbo"].CreateTable("Production_Location");
      t.CreateColumn("LocationID", new SqlValueType(SqlDataType.Int32));
      t.CreateColumn("Name", new SqlValueType(SqlDataType.VarChar));
      t.CreateColumn("CostRate", new SqlValueType(SqlDataType.Money));
      t.CreateColumn("Availability", new SqlValueType(SqlDataType.Decimal));
      t.CreateColumn("ModifiedDate", new SqlValueType(SqlDataType.DateTime));

      t = Catalog.Schemas["dbo"].CreateTable("Sales_SalesTerritory");
      t.CreateColumn("TerritoryID", new SqlValueType(SqlDataType.Int32));
      t.CreateColumn("Name", new SqlValueType(SqlDataType.VarChar));
      t.CreateColumn("CountryRegionCode", new SqlValueType(SqlDataType.VarChar));
      t.CreateColumn("Group", new SqlValueType(SqlDataType.VarChar));
      t.CreateColumn("TaxRate", new SqlValueType(SqlDataType.Money));
      t.CreateColumn("SalesYTD", new SqlValueType(SqlDataType.Money));
      t.CreateColumn("SalesLastYear", new SqlValueType(SqlDataType.Money));
      t.CreateColumn("CostYTD", new SqlValueType(SqlDataType.Money));
      t.CreateColumn("CostLastYear", new SqlValueType(SqlDataType.Money));
      t.CreateColumn("ModifiedDate", new SqlValueType(SqlDataType.DateTime));

      t = Catalog.Schemas["dbo"].CreateTable("Production_ScrapReason");
      t.CreateColumn("ScrapReasonID", new SqlValueType(SqlDataType.Int32));
      t.CreateColumn("Name", new SqlValueType(SqlDataType.VarChar));
      t.CreateColumn("ModifiedDate", new SqlValueType(SqlDataType.DateTime));

      t = Catalog.Schemas["dbo"].CreateTable("HumanResources_Shift");
      t.CreateColumn("ShiftID", new SqlValueType(SqlDataType.Int32));
      t.CreateColumn("Name", new SqlValueType(SqlDataType.VarChar));
      t.CreateColumn("StartTime", new SqlValueType(SqlDataType.DateTime));
      t.CreateColumn("EndTime", new SqlValueType(SqlDataType.DateTime));
      t.CreateColumn("ModifiedDate", new SqlValueType(SqlDataType.DateTime));

      t = Catalog.Schemas["dbo"].CreateTable("Production_ProductCategory");
      t.CreateColumn("ProductCategoryID", new SqlValueType(SqlDataType.Int32));
      t.CreateColumn("Name", new SqlValueType(SqlDataType.VarChar));
      t.CreateColumn("ModifiedDate", new SqlValueType(SqlDataType.DateTime));

      t = Catalog.Schemas["dbo"].CreateTable("Purchasing_ShipMethod");
      t.CreateColumn("ShipMethodID", new SqlValueType(SqlDataType.Int32));
      t.CreateColumn("Name", new SqlValueType(SqlDataType.VarChar));
      t.CreateColumn("ShipBase", new SqlValueType(SqlDataType.Money));
      t.CreateColumn("ShipRate", new SqlValueType(SqlDataType.Money));
      t.CreateColumn("ModifiedDate", new SqlValueType(SqlDataType.DateTime));

      t = Catalog.Schemas["dbo"].CreateTable("Sales_Store");
      t.CreateColumn("CustomerID", new SqlValueType(SqlDataType.Int32));
      t.CreateColumn("Name", new SqlValueType(SqlDataType.VarChar));
      t.CreateColumn("SalesPersonID", new SqlValueType(SqlDataType.Int32));
      t.CreateColumn("Demographics", new SqlValueType(SqlDataType.Xml));
      t.CreateColumn("ModifiedDate", new SqlValueType(SqlDataType.DateTime));

      t = Catalog.Schemas["dbo"].CreateTable("Sales_Individual");
      t.CreateColumn("CustomerID", new SqlValueType(SqlDataType.Int32));
      t.CreateColumn("ContactID", new SqlValueType(SqlDataType.Int32));
      t.CreateColumn("Demographics", new SqlValueType(SqlDataType.Xml));
      t.CreateColumn("ModifiedDate", new SqlValueType(SqlDataType.DateTime));

      t = Catalog.Schemas["dbo"].CreateTable("HumanResources_JobCandidate");
      t.CreateColumn("JobCandidateID", new SqlValueType(SqlDataType.Int32));
      t.CreateColumn("EmployeeID", new SqlValueType(SqlDataType.Int32));
      t.CreateColumn("Resume", new SqlValueType(SqlDataType.Xml));
      t.CreateColumn("ModifiedDate", new SqlValueType(SqlDataType.DateTime));

      SqlTableRef e = Sql.TableRef(Catalog.Schemas["dbo"].Tables["HumanResources_Employee"], "e");
      SqlTableRef cRef = Sql.TableRef(Catalog.Schemas["dbo"].Tables["Person_Contact"], "c");
      SqlTableRef ea = Sql.TableRef(Catalog.Schemas["dbo"].Tables["HumanResources_EmployeeAddress"], "ea");
      SqlTableRef a = Sql.TableRef(Catalog.Schemas["dbo"].Tables["Person_Address"], "a");
      SqlTableRef sp = Sql.TableRef(Catalog.Schemas["dbo"].Tables["Person_StateProvince"], "sp");
      SqlTableRef cr = Sql.TableRef(Catalog.Schemas["dbo"].Tables["Person_CountryRegion"], "cr");

      SqlSelect select = Sql.Select(e);
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
      //v = Catalog.Schemas["dbo"].CreateView("HumanResources_vEmployee", select); FIXME

//      MemoryStream ms = new MemoryStream();
//      BinaryFormatter bf = new BinaryFormatter();
//      bf.Serialize(ms, model);
//
//      ms.Seek(0, SeekOrigin.Begin);
//      model = (Model)bf.Deserialize(ms);

//      bmp.Save(model);
    
    }
  }
}