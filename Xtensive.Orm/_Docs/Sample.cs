// Assume we have following database catalog:

dbCatalog = new Catalog();
dbCatalog.Name = "AdventureWorks";

Schema s;
s = new Schema(dbCatalog, "HumanResources");
s = new Schema(dbCatalog, "Person");
s = new Schema(dbCatalog, "Production");
s = new Schema(dbCatalog, "Purchasing");
s = new Schema(dbCatalog, "Sales");


//Then we can write following queries using Sql.Dom:

//1. Simple SELECT query

//SELECT ProductID, Name, ListPrice
//FROM Production.Product
//WHERE ListPrice > $40
//ORDER BY ListPrice ASC

// Here we create a reference to real table "Product" from our database catalog
SqlTable product = Sql.Table(dbCatalog.Schemas["Production"].Tables["Product"]);

// Creating SELECT statement
SqlSelect select = Sql.Select();

// Setting FROM clause
select.From = product;

// Adding desired columns
select.Columns.AddRange(product["ProductID"], product["Name"], product["ListPrice"]);

// Setting appropriate WHERE condition
select.Where = product["ListPrice"] > 40;

// Some ordering settings
select.OrderBy.Add(product["ListPrice"]);


// 2. SELECT query with JOINs

//SELECT DISTINCT c.CustomerID, s.Name
//FROM Sales.Customer AS c 
//   JOIN
//     Sales.Store AS s
//   ON ( c.CustomerID = s.CustomerID)
//WHERE c.TerritoryID = 1

// Here we create a reference to a real table "Customer" and give it an alias "c"
SqlTable customer = Sql.Table(dbCatalog.Schemas["Sales"].Tables["Customer"], "c");

// The same procedure is applied for table "Store"
SqlTable store = Sql.Table(dbCatalog.Schemas["Sales"].Tables["Store"], "s");


SqlSelect select = Sql.Select();
select.Distinct = true;

// Setting FROM clause. Note how joins are handled.
select.From = customer.InnerJoin(store, customer["CustomerID"] == store["CustomerID"])

// Columns
select.Columns.AddRange(customer["CustomerID"], store["Name"]);

// WHERE condition
select.Where = customer["TerritoryID"] == 1;


// 3. UPDATE query

//UPDATE Person.Address
//SET PostalCode = '98000'
//WHERE City = 'Bothell';

// Creating a reference to the real table again
SqlTable product = Sql.Table(dbCatalog.Schemas["Person"].Tables["Address"]);

// Creating UPDATE statement
SqlUpdate update = Sql.Update();

// Setting a table that will be updated by the query
update.Table = product;

// Setting values
update.Values[product["PostalCode"]] = "98000";

// WHERE clause
update.Where = product["City"] == "Bothell";


// 4. DELETE query with sub SELECT

//DELETE FROM Sales.SalesPersonQuotaHistory 
//WHERE SalesPersonID IN 
//    (SELECT SalesPersonID 
//     FROM Sales.SalesPerson 
//     WHERE SalesYTD > 2500000.00);

// First we create sub SELECT
SqlTable salesPerson = Sql.Table(dbCatalog.Schemas["Sales"].Tables["SalesPerson"]);
SqlSelect select = Sql.Select();
select.From = salesPerson;
select.Columns.Add(salesPerson["SalesPersonID"]);
select.Where = salesPerson["SalesYTD"] > 2500000.00;

// Creating DELETE statement with sub SELECT
SqlTable salesPersonQuotaHistory = Sql.Table(dbCatalog.Schemas["Sales"].Tables["SalesPersonQuotaHistory"]);
SqlDelete delete = Sql.Delete();
delete.Table = salesPersonQuotaHistory;

// Setting WHERE clause using IN expression and sub SELECT statement
delete.Where = Sql.In(salesPersonQuotaHistory["SalesPersonID"], Sql.SubSelect(select));
