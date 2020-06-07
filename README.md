[![ci.appveyor.com](https://ci.appveyor.com/api/projects/status/github/blushingpenguin/MongoDB.Bson.Path?branch=master&svg=true)](https://ci.appveyor.com/api/projects/status/github/blushingpenguin/MongoDB.Bson.Path?branch=master&svg=true)
[![codecov.io](https://codecov.io/gh/blushingpenguin/MongoDB.Bson.Path/coverage.svg?branch=master)](https://codecov.io/gh/blushingpenguin/MongoDB.Bson.Path?branch=master)

# MongoDB.Bson.Path #

MongoDB.Bson.Path is a jsonpath implementation for MongoDB.Bson. It is a port of the jsonpath implementation from Newtonsoft.Json.

## Packages ##

MongoDB.Bson.Path can also be installed from [nuget.org](https://www.nuget.org/packages/MongoDB.Bson.Path/).

Install with package manager:

    Install-Package MongoDB.Bson.Path

or with nuget:

    nuget install MongoDB.Bson.Path

Or with dotnet:

    dotnet add package MongoDB.Bson.Path

## Example usage ##

```csharp
using MongoDB.Bson.Path;

void Example()
{
var o = BsonDocument.Parse(@"{
""Stores"": [
    ""Lambton Quay"",
    ""Willis Street""
],
""Manufacturers"": [
    {
    ""Name"": ""Acme Co"",
    ""Products"": [
        {
        ""Name"": ""Anvil"",
        ""Price"": 50
        }
    ]
    },
    {
    ""Name"": ""Contoso"",
    ""Products"": [
        {
        ""Name"": ""Elbow Grease"",
        ""Price"": 99.95
        },
        {
        ""Name"": ""Headlight Fluid"",
        ""Price"": 4
        }
    ]
    }
]
}");

string name = (string)o.SelectToken("Manufacturers[0].Name");
// Acme Co

decimal productPrice = o.SelectToken("Manufacturers[0].Products[0].Price").ToDecimal();
// 50

string productName = (string)o.SelectToken("Manufacturers[1].Products[0].Name");
// Elbow Grease

Assert.AreEqual("Acme Co", name);
Assert.AreEqual(50m, productPrice);
Assert.AreEqual("Elbow Grease", productName);

IList<string> storeNames = o.SelectToken("Stores").AsBsonArray.Select(s => (string)s).ToList();
// Lambton Quay
// Willis Street

IList<string> firstProductNames = o["Manufacturers"].AsBsonArray.Select(
    m => (string)m.AsBsonDocument.SelectToken("Products[1].Name")).ToList();
// null
// Headlight Fluid

decimal totalPrice = o["Manufacturers"].AsBsonArray.Aggregate(
    0M, (sum, m) => sum + m.AsBsonDocument.SelectToken("Products[0].Price").ToDecimal());
// 149.95
}

```

## Local Development ##

Hacking on `MongoDB.Bson.Path` is easy! To quickly get started clone the repo:

    git clone https://github.com/blushingpenguin/MongoDB.Bson.Path.git
    cd MongoDB.Bson.Path

To compile the code and run the tests just open the solution in
Visual Studio 2019 Community Edition.  To generate a code coverage report
run cover.ps1 from the solution directory.
