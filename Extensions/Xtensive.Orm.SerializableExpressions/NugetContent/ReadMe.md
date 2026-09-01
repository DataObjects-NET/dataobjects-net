Xtensive.Orm.SerializableExpression
=====================

Summary
-------
Allows to convert System.Linq.Expression instances to serializable to json form.

Prerequisites
-------------
DataObjects.Net 7.3.x or later (http://dataobjects.net)

Demo
----

1) Serialization to Json (via System.Text.Json.JsonSerializer)

```csharp
// convert System.Linq.Expresson into serializable form
var serializableExpression = expression.ToSerializableExpression();

// we recommend to reserve references in case json will be deserializalized.
// for one-way serialization it is not so important
var options = new JsonSerializerOptions() { WriteIndented = true, ReferenceHandler = ReferenceHandler.Preserve };
var json = JsonSerializer.Serialize(serializableExpression, options);
```


2) Deserialization from Json (via System.Text.Json.JsonSerializer)

```csharp

// we recommend to reserve references in case json will be deserializalized.
// for one-way serialization it is not so important
// use same settings for deserialization
var options = new JsonSerializerOptions() { WriteIndented = true, ReferenceHandler = ReferenceHandler.Preserve };

var serializableExpression = JsonSerializer.Deserialize<SerializableExpression>(json, options);

var expression = serializableExpression.ToExpression();
```


3) Serialization to Json (via System.Runtime.Serialization.Json.DataContractJsonSerializer)

```csharp
// convert System.Linq.Expresson into serializable form
var serializableExpression = expression.ToSerializableExpression();

// define types which appear in expresson (types of constants for example), apart from SerializableExpression descendants
var knownTypes = new Type[] { typeof (int), typeof (long), typeof (string) };

var settings = new DataContractJsonSerializerSettings { KnownTypes = knownTypes };

var serializer = new DataContractJsonSerializer(typeof(SerializableExpression), settings);

string json = null;
using (var stream = new MemoryStream()) {
    // Write the object to the stream
    serializer.WriteObject(stream, serializableExpression);

    // Convert the stream byte array into a UTF-8 string
    json = Encoding.UTF8.GetString(stream.ToArray());
}

```

4) Deserialization from Json (via System.Runtime.Serialization.Json.DataContractJsonSerializer)

```csharp
var serializableExpression = expression.ToSerializableExpression();

// define types which appear in expresson (types of constants for example), apart from SerializableExpression descendants
var knownTypes = new Type[] { typeof (int), typeof (long), typeof (string) };

var settings = new DataContractJsonSerializerSettings { KnownTypes = knownTypes };

var serializer = new DataContractJsonSerializer(typeof(SerializableExpression), settings);

SerializableExpression serializableExpression = null;

// convert json string into memory stream to be read from
using (var stream = new MemoryStream(Encoding.UTF8.GetBytes(json))) {
    // read an object
    serializableExpression = (SerializableExpression) serializer.ReadObject(ms);
}

var expression = serializableExpression.ToExpression();

```

5) Serialization to Xml (via System.Runtime.Serialization.DataContractSerializer)

```csharp
// convert System.Linq.Expresson into serializable form
var serializableExpression = expression.ToSerializableExpression();

// define types which appear in expresson (types of constants for example), apart from SerializableExpression descendants
var knownTypes = new Type[] { typeof (int), typeof (long), typeof (string) };

// we recommend to reserve references in case xml will be deserializalized.
// for one-way serialization it is not so important
var settings = new DataContractSerializerSettings {
    KnownTypes = SystemTypes,
    PreserveObjectReferences = true
};

var serializer = new DataContractSerializer(typeof(SerializableExpression), settings);

string json = null;
using (var stream = new MemoryStream()) {
    // Write the object to the stream
    serializer.WriteObject(stream, serializableExpression);

    // Convert the stream byte array into a UTF-8 string
    json = Encoding.UTF8.GetString(stream.ToArray());
}

```

6) Deserialization to Xml (via System.Runtime.Serialization.DataContractSerializer)

```csharp
// define types which appear in expresson (types of constants for example), apart from SerializableExpression descendants
var knownTypes = new Type[] { typeof (int), typeof (long), typeof (string) };

// we recommend to reserve references in case xml will be deserializalized.
// for one-way serialization it is not so important
var settings = new DataContractSerializerSettings {
    KnownTypes = SystemTypes,
    PreserveObjectReferences = true
};

var serializer = new DataContractSerializer(typeof(SerializableExpression), settings);

SerializableExpression serializableExpression = null;

// convert json string into memory stream to be read from
using (var stream = new MemoryStream(Encoding.UTF8.GetBytes(json))) {
    // read an object
    serializableExpression = (SerializableExpression) serializer.ReadObject(stream);
}

var expression = serializableExpression.ToExpression();

```

7) Serialize and deserialize query expression

```csharp
string json;

using (var session = domain.OpenSession())
using (var tx = session.OpenTransaction()) {

    var query = session.Query.All<Bar>()
        .Where(c => c.Name == "Bar #1")
        .Where(c => c.Count > 0)
        .Take(5)
        .Skip(0);

    var serializableExpression = query.Expression.ToSerializableExpression();

    var options = new JsonSerializerOptions() { WriteIndented = true, ReferenceHandler = ReferenceHandler.Preserve };
    json = JsonSerializer.Serialize(serializableExpression, options);
}

using (var session = domain.OpenSession())
using (var tx = session.OpenTransaction()) {

    var options = new JsonSerializerOptions() { WriteIndented = true, ReferenceHandler = ReferenceHandler.Preserve };
    var serializableExpression = JsonSerializer.Deserialize<SerializableExpression>(json, options);

    var queryExpression = serializedExpression.ToExpression();
    var query = new Queryable<Bar>(session.Query.Provider, queryExpression);
    var queryResult = query.ToArray();
}
```