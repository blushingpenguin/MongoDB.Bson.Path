#region License
// Copyright (c) 2007 James Newton-King
//
// Permission is hereby granted, free of charge, to any person
// obtaining a copy of this software and associated documentation
// files (the "Software"), to deal in the Software without
// restriction, including without limitation the rights to use,
// copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the
// Software is furnished to do so, subject to the following
// conditions:
//
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES
// OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT
// HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY,
// WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING
// FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR
// OTHER DEALINGS IN THE SOFTWARE.
#endregion

using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MongoDB.Bson.Path.Test
{
    [TestFixture]
    [Parallelizable(ParallelScope.All)]
    public class JPathExecuteTests
    {
        [Test]
        public void GreaterThanIssue1518()
        {
            string statusJson = @"{""usingmem"": ""214376""}";//214,376
            BsonDocument jObj = BsonDocument.Parse(statusJson);

            var aa = jObj.SelectToken("$..[?(@.usingmem>10)]");//found,10
            Assert.AreEqual(jObj, aa);

            var bb = jObj.SelectToken("$..[?(@.usingmem>27000)]");//null, 27,000
            Assert.AreEqual(jObj, bb);

            var cc = jObj.SelectToken("$..[?(@.usingmem>21437)]");//found, 21,437
            Assert.AreEqual(jObj, cc);

            var dd = jObj.SelectToken("$..[?(@.usingmem>21438)]");//null,21,438
            Assert.AreEqual(jObj, dd);
        }

        [Test]
        public void GreaterThanWithIntegerParameterAndStringValue()
        {
            string json = @"{
  ""persons"": [
    {
      ""name""  : ""John"",
      ""age"": ""26""
    },
    {
      ""name""  : ""Jane"",
      ""age"": ""2""
    }
  ]
}";

            BsonDocument models = BsonDocument.Parse(json);

            var results = models.SelectTokens("$.persons[?(@.age > 3)]").ToList();

            Assert.AreEqual(1, results.Count);
        }

        [Test]
        public void GreaterThanWithStringParameterAndIntegerValue()
        {
            string json = @"{
  ""persons"": [
    {
      ""name""  : ""John"",
      ""age"": 26
    },
    {
      ""name""  : ""Jane"",
      ""age"": 2
    }
  ]
}";

            BsonDocument models = BsonDocument.Parse(json);

            var results = models.SelectTokens("$.persons[?(@.age > '3')]").ToList();

            Assert.AreEqual(1, results.Count);
        }

        [Test]
        public void RecursiveWildcard()
        {
            string json = @"{
    ""a"": [
        {
            ""id"": 1
        }
    ],
    ""b"": [
        {
            ""id"": 2
        },
        {
            ""id"": 3,
            ""c"": {
                ""id"": 4
            }
        }
    ],
    ""d"": [
        {
            ""id"": 5
        }
    ]
}";

            BsonDocument models = BsonDocument.Parse(json);

            var results = models.SelectTokens("$.b..*.id").ToList();

            Assert.AreEqual(3, results.Count);
            Assert.AreEqual(2, (int)results[0]);
            Assert.AreEqual(3, (int)results[1]);
            Assert.AreEqual(4, (int)results[2]);
        }

        [Test]
        public void ScanFilter()
        {
            string json = @"{
  ""elements"": [
    {
      ""id"": ""A"",
      ""children"": [
        {
          ""id"": ""AA"",
          ""children"": [
            {
              ""id"": ""AAA""
            },
            {
              ""id"": ""AAB""
            }
          ]
        },
        {
          ""id"": ""AB""
        }
      ]
    },
    {
      ""id"": ""B"",
      ""children"": []
    }
  ]
}";

            BsonDocument models = BsonDocument.Parse(json);

            var results = models.SelectTokens("$.elements..[?(@.id=='AAA')]").ToList();

            Assert.AreEqual(1, results.Count);
            Assert.AreEqual(models["elements"][0]["children"][0]["children"][0], results[0]);
        }

        [Test]
        public void FilterTrue()
        {
            string json = @"{
  ""elements"": [
    {
      ""id"": ""A"",
      ""children"": [
        {
          ""id"": ""AA"",
          ""children"": [
            {
              ""id"": ""AAA""
            },
            {
              ""id"": ""AAB""
            }
          ]
        },
        {
          ""id"": ""AB""
        }
      ]
    },
    {
      ""id"": ""B"",
      ""children"": []
    }
  ]
}";

            BsonDocument models = BsonDocument.Parse(json);

            var results = models.SelectTokens("$.elements[?(true)]").ToList();

            Assert.AreEqual(2, results.Count);
            Assert.AreEqual(results[0], models["elements"][0]);
            Assert.AreEqual(results[1], models["elements"][1]);
        }

        [Test]
        public void ScanFilterTrue()
        {
            string json = @"{
  ""elements"": [
    {
      ""id"": ""A"",
      ""children"": [
        {
          ""id"": ""AA"",
          ""children"": [
            {
              ""id"": ""AAA""
            },
            {
              ""id"": ""AAB""
            }
          ]
        },
        {
          ""id"": ""AB""
        }
      ]
    },
    {
      ""id"": ""B"",
      ""children"": []
    }
  ]
}";

            BsonDocument models = BsonDocument.Parse(json);

            var results = models.SelectTokens("$.elements..[?(true)]").ToList();

            Assert.AreEqual(25, results.Count);
        }

        [Test]
        public void ScanQuoted()
        {
            string json = @"{
    ""Node1"": {
        ""Child1"": {
            ""Name"": ""IsMe"",
            ""TargetNode"": {
                ""Prop1"": ""Val1"",
                ""Prop2"": ""Val2""
            }
        },
        ""My.Child.Node"": {
            ""TargetNode"": {
                ""Prop1"": ""Val1"",
                ""Prop2"": ""Val2""
            }
        }
    },
    ""Node2"": {
        ""TargetNode"": {
            ""Prop1"": ""Val1"",
            ""Prop2"": ""Val2""
        }
    }
}";

            BsonDocument models = BsonDocument.Parse(json);

            int result = models.SelectTokens("$..['My.Child.Node']").Count();
            Assert.AreEqual(1, result);

            result = models.SelectTokens("..['My.Child.Node']").Count();
            Assert.AreEqual(1, result);
        }

        [Test]
        public void ScanMultipleQuoted()
        {
            string json = @"{
    ""Node1"": {
        ""Child1"": {
            ""Name"": ""IsMe"",
            ""TargetNode"": {
                ""Prop1"": ""Val1"",
                ""Prop2"": ""Val2""
            }
        },
        ""My.Child.Node"": {
            ""TargetNode"": {
                ""Prop1"": ""Val3"",
                ""Prop2"": ""Val4""
            }
        }
    },
    ""Node2"": {
        ""TargetNode"": {
            ""Prop1"": ""Val5"",
            ""Prop2"": ""Val6""
        }
    }
}";

            BsonDocument models = BsonDocument.Parse(json);

            var results = models.SelectTokens("$..['My.Child.Node','Prop1','Prop2']").ToList();
            Assert.AreEqual("Val1", (string)results[0]);
            Assert.AreEqual("Val2", (string)results[1]);
            Assert.AreEqual(BsonType.Document, results[2].BsonType);
            Assert.AreEqual("Val3", (string)results[3]);
            Assert.AreEqual("Val4", (string)results[4]);
            Assert.AreEqual("Val5", (string)results[5]);
            Assert.AreEqual("Val6", (string)results[6]);
        }

        [Test]
        public void ParseWithEmptyArrayContent()
        {
            var json = @"{
    'controls': [
        {
            'messages': {
                'addSuggestion': {
                    'en-US': 'Add'
                }
            }
        },
        {
            'header': {
                'controls': []
            },
            'controls': [
                {
                    'controls': [
                        {
                            'defaultCaption': {
                                'en-US': 'Sort by'
                            },
                            'sortOptions': [
                                {
                                    'label': {
                                        'en-US': 'Name'
                                    }
                                }
                            ]
                        }
                    ]
                }
            ]
        }
    ]
}";
            BsonDocument jToken = BsonDocument.Parse(json);
            IList<BsonValue> tokens = jToken.SelectTokens("$..en-US").ToList();

            Assert.AreEqual(3, tokens.Count);
            Assert.AreEqual("Add", (string)tokens[0]);
            Assert.AreEqual("Sort by", (string)tokens[1]);
            Assert.AreEqual("Name", (string)tokens[2]);
        }

        [Test]
        public void SelectTokenAfterEmptyContainer()
        {
            string json = @"{
    'cont': [],
    'test': 'no one will find me'
}";

            var o = BsonDocument.Parse(json);

            var results = o.SelectTokens("$..test").ToList();

            Assert.AreEqual(1, results.Count);
            Assert.AreEqual("no one will find me", (string)results[0]);
        }

        [Test]
        public void EvaluatePropertyWithRequired()
        {
            string json = "{\"bookId\":\"1000\"}";
            var o = BsonDocument.Parse(json);

            string bookId = (string)o.SelectToken("bookId", true);

            Assert.AreEqual("1000", bookId);
        }

        [Test]
        public void EvaluateEmptyPropertyIndexer()
        {
            var o = new BsonDocument { { "", 1 } };

            var t = o.SelectToken("['']");
            Assert.AreEqual(1, (int)t);
        }

        [Test]
        public void EvaluateEmptyString()
        {
            var o = new BsonDocument { { "Blah", 1 } };

            var t = o.SelectToken("");
            Assert.AreEqual(o, t);

            t = o.SelectToken("['']");
            Assert.AreEqual(null, t);
        }

        [Test]
        public void EvaluateEmptyStringWithMatchingEmptyProperty()
        {
            var o = new BsonDocument { { " ", 1 } };

            var t = o.SelectToken("[' ']");
            Assert.AreEqual(1, (int)t);
        }

        [Test]
        public void EvaluateWhitespaceString()
        {
            var o = new BsonDocument { { "Blah", 1 } };

            var t = o.SelectToken(" ");
            Assert.AreEqual(o, t);
        }

        [Test]
        public void EvaluateDollarString()
        {
            var o = new BsonDocument { { "Blah", 1 } };

            var t = o.SelectToken("$");
            Assert.AreEqual(o, t);
        }

        [Test]
        public void EvaluateDollarTypeString()
        {
            var o = new BsonDocument
            {
                {  "$values", new BsonArray { 1, 2, 3 } }
            };

            var t = o.SelectToken("$values[1]");
            Assert.AreEqual(2, (int)t);
        }

        [Test]
        public void EvaluateSingleProperty()
        {
            var o = new BsonDocument { { "Blah", 1 } };

            var t = o.SelectToken("Blah");
            Assert.IsNotNull(t);
            Assert.AreEqual(BsonType.Int32, t.BsonType);
            Assert.AreEqual(1, (int)t);
        }

        [Test]
        public void EvaluateWildcardProperty()
        {
            var o = new BsonDocument
            {
                { "Blah", 1 },
                { "Blah2", 2 }
            };

            var t = o.SelectTokens("$.*").ToList();
            Assert.IsNotNull(t);
            Assert.AreEqual(2, t.Count);
            Assert.AreEqual(1, (int)t[0]);
            Assert.AreEqual(2, (int)t[1]);
        }

        [Test]
        public void QuoteName()
        {
            var o = new BsonDocument { { "Blah", 1 } };

            var t = o.SelectToken("['Blah']");
            Assert.IsNotNull(t);
            Assert.AreEqual(BsonType.Int32, t.BsonType);
            Assert.AreEqual(1, (int)t);
        }

        [Test]
        public void EvaluateMissingProperty()
        {
            var o = new BsonDocument { { "Blah", 1 } };

            var t = o.SelectToken("Missing[1]");
            Assert.IsNull(t);
        }

        [Test]
        public void EvaluateIndexerOnObject()
        {
            var o = new BsonDocument { { "Blah", 1 } };

            var t = o.SelectToken("[1]");
            Assert.IsNull(t);
        }

        [Test]
        public void EvaluateIndexerOnObjectWithError()
        {
            var o = new BsonDocument { { "Blah", 1 } };

            Assert.Throws<BsonException>(() => o.SelectToken("[1]", true),
                @"Index 1 not valid on BsonDocument.");
        }

        [Test]
        public void EvaluateWildcardIndexOnObjectWithError()
        {
            var o = new BsonDocument { { "Blah", 1 } };

            Assert.Throws<BsonException>(() => o.SelectToken("[*]", true),
                @"Index * not valid on BsonDocument.");
        }

        [Test]
        public void EvaluateSliceOnObjectWithError()
        {
            var o = new BsonDocument { { "Blah", 1 } };

            Assert.Throws<BsonException>(() => o.SelectToken("[:]", true),
                @"Array slice is not valid on BsonDocument.");
        }

        [Test]
        public void EvaluatePropertyOnArray()
        {
            var a = new BsonArray { 1, 2, 3, 4, 5 };

            var t = a.SelectToken("BlahBlah");
            Assert.IsNull(t);
        }

        [Test]
        public void EvaluateMultipleResultsError()
        {
            var a = new BsonArray { 1, 2, 3, 4, 5 };

            Assert.Throws<BsonException>(() => a.SelectToken("[0, 1]"),
                @"Path returned multiple tokens.");
        }

        [Test]
        public void EvaluatePropertyOnArrayWithError()
        {
            var a = new BsonArray { 1, 2, 3, 4, 5 };

            Assert.Throws<BsonException>(() => a.SelectToken("BlahBlah", true),
                @"Property 'BlahBlah' not valid on BsonArray.");
        }

        [Test]
        public void EvaluateNoResultsWithMultipleArrayIndexes()
        {
            var a = new BsonArray { 1, 2, 3, 4, 5 };

            Assert.Throws<BsonException>(() => a.SelectToken("[9,10]", true),
                @"Index 9 outside the bounds of BsonArray.");
        }

#if FALSE
        [Test]
        public void EvaluateConstructorOutOfBoundsIndxerWithError()
        {
            JConstructor c = new JConstructor("Blah");

            ExceptionAssert.Throws<JsonException>(() => { c.SelectToken("[1]", true); }, @"Index 1 outside the bounds of JConstructor.");
        }

        [Test]
        public void EvaluateConstructorOutOfBoundsIndxer()
        {
            JConstructor c = new JConstructor("Blah");

            Assert.IsNull(c.SelectToken("[1]"));
        }
#endif

        [Test]
        public void EvaluateMissingPropertyWithError()
        {
            var o = new BsonDocument { { "Blah", 1 } };
            Assert.Throws<BsonException>(() => o.SelectToken("Missing", true),
                "Property 'Missing' does not exist on BsonDocument.");
        }

        [Test]
        public void EvaluatePropertyWithoutError()
        {
            var o = new BsonDocument { { "Blah", 1 } };

            var v = o.SelectToken("Blah", true);
            Assert.IsTrue(v == 1);
        }

        [Test]
        public void EvaluateMissingPropertyIndexWithError()
        {
            var o = new BsonDocument { { "Blah", 1 } };

            Assert.Throws<BsonException>(() => o.SelectToken("['Missing','Missing2']", true),
                "Property 'Missing' does not exist on BsonDocument.");
        }

        [Test]
        public void EvaluateMultiPropertyIndexOnArrayWithError()
        {
            var a = new BsonArray { 1, 2, 3, 4, 5 };

            Assert.Throws<BsonException>(() => a.SelectToken("['Missing','Missing2']", true),
                "Properties 'Missing', 'Missing2' not valid on BsonArray.");
        }

        [Test]
        public void EvaluateArraySliceWithError()
        {
            var a = new BsonArray { 1, 2, 3, 4, 5 };

            Assert.Throws<BsonException>(() => a.SelectToken("[99:]", true),
                "Array slice of 99 to * returned no results.");

            Assert.Throws<BsonException>(() => a.SelectToken("[1:-19]", true),
                "Array slice of 1 to -19 returned no results.");

            Assert.Throws<BsonException>(() => a.SelectToken("[:-19]", true),
                "Array slice of * to -19 returned no results.");
            
            a = new BsonArray();

            Assert.Throws<BsonException>(() => a.SelectToken("[:]", true),
                "Array slice of * to * returned no results.");
        }

        [Test]
        public void EvaluateOutOfBoundsIndxer()
        {
            var a = new BsonArray { 1, 2, 3, 4, 5 };

            var t = a.SelectToken("[1000].Ha");
            Assert.IsNull(t);
        }

        [Test]
        public void EvaluateArrayOutOfBoundsIndxerWithError()
        {
            var a = new BsonArray { 1, 2, 3, 4, 5 };

            Assert.Throws<BsonException>(() => a.SelectToken("[1000].Ha", true),
                "Index 1000 outside the bounds of BsonArray.");
        }

        [Test]
        public void EvaluateArray()
        {
            var a = new BsonArray { 1, 2, 3, 4 };

            var t = a.SelectToken("[1]");
            Assert.IsNotNull(t);
            Assert.AreEqual(BsonType.Int32, t.BsonType);
            Assert.AreEqual(2, (int)t);
        }

        [Test]
        public void EvaluateArraySlice()
        {
            var a = new BsonArray { 1, 2, 3, 4, 5, 6, 7, 8, 9 };
            List<BsonValue> t = null;

            t = a.SelectTokens("[-3:]").ToList();
            Assert.AreEqual(3, t.Count);
            Assert.AreEqual(7, (int)t[0]);
            Assert.AreEqual(8, (int)t[1]);
            Assert.AreEqual(9, (int)t[2]);

            t = a.SelectTokens("[-1:-2:-1]").ToList();
            Assert.AreEqual(1, t.Count);
            Assert.AreEqual(9, (int)t[0]);

            t = a.SelectTokens("[-2:-1]").ToList();
            Assert.AreEqual(1, t.Count);
            Assert.AreEqual(8, (int)t[0]);

            t = a.SelectTokens("[1:1]").ToList();
            Assert.AreEqual(0, t.Count);

            t = a.SelectTokens("[1:2]").ToList();
            Assert.AreEqual(1, t.Count);
            Assert.AreEqual(2, (int)t[0]);

            t = a.SelectTokens("[::-1]").ToList();
            Assert.AreEqual(9, t.Count);
            Assert.AreEqual(9, (int)t[0]);
            Assert.AreEqual(8, (int)t[1]);
            Assert.AreEqual(7, (int)t[2]);
            Assert.AreEqual(6, (int)t[3]);
            Assert.AreEqual(5, (int)t[4]);
            Assert.AreEqual(4, (int)t[5]);
            Assert.AreEqual(3, (int)t[6]);
            Assert.AreEqual(2, (int)t[7]);
            Assert.AreEqual(1, (int)t[8]);

            t = a.SelectTokens("[::-2]").ToList();
            Assert.AreEqual(5, t.Count);
            Assert.AreEqual(9, (int)t[0]);
            Assert.AreEqual(7, (int)t[1]);
            Assert.AreEqual(5, (int)t[2]);
            Assert.AreEqual(3, (int)t[3]);
            Assert.AreEqual(1, (int)t[4]);
        }

        [Test]
        public void EvaluateWildcardArray()
        {
            var a = new BsonArray { 1, 2, 3, 4 };

            List<BsonValue> t = a.SelectTokens("[*]").ToList();
            Assert.IsNotNull(t);
            Assert.AreEqual(4, t.Count);
            Assert.AreEqual(1, (int)t[0]);
            Assert.AreEqual(2, (int)t[1]);
            Assert.AreEqual(3, (int)t[2]);
            Assert.AreEqual(4, (int)t[3]);
        }

        [Test]
        public void EvaluateArrayMultipleIndexes()
        {
            var a = new BsonArray { 1, 2, 3, 4 };

            IEnumerable<BsonValue> t = a.SelectTokens("[1,2,0]");
            Assert.IsNotNull(t);
            Assert.AreEqual(3, t.Count());
            Assert.AreEqual(2, (int)t.ElementAt(0));
            Assert.AreEqual(3, (int)t.ElementAt(1));
            Assert.AreEqual(1, (int)t.ElementAt(2));
        }

        [Test]
        public void EvaluateScan()
        {
            BsonDocument o1 = new BsonDocument { { "Name", 1 } };
            BsonDocument o2 = new BsonDocument { { "Name", 2 } };
            var a = new BsonArray { o1, o2 };

            var t = a.SelectTokens("$..Name").ToList();
            Assert.IsNotNull(t);
            Assert.AreEqual(2, t.Count);
            Assert.AreEqual(1, (int)t[0]);
            Assert.AreEqual(2, (int)t[1]);
        }

        [Test]
        public void EvaluateWildcardScan()
        {
            BsonDocument o1 = new BsonDocument { { "Name", 1 } };
            BsonDocument o2 = new BsonDocument { { "Name", 2 } };
            var a = new BsonArray { o1, o2 };

            var t = a.SelectTokens("$..*").ToList();
            Assert.IsNotNull(t);
            Assert.AreEqual(5, t.Count);
            Assert.AreEqual(a, t[0]);
            Assert.AreEqual(o1, t[1]);
            Assert.AreEqual(1, (int)t[2]);
            Assert.AreEqual(o2, t[3]);
            Assert.AreEqual(2, (int)t[4]);
        }

        [Test]
        public void EvaluateScanNestResults()
        {
            BsonDocument o1 = new BsonDocument { { "Name", 1 } };
            BsonDocument o2 = new BsonDocument { { "Name", 2 } };
            BsonDocument o3 = new BsonDocument { { "Name", new BsonDocument { { "Name", new BsonArray { 3 } } } } };
            var a = new BsonArray { o1, o2, o3 };

            var t = a.SelectTokens("$..Name").ToList();
            Assert.IsNotNull(t);
            Assert.AreEqual(4, t.Count);
            Assert.AreEqual(1, (int)t[0]);
            Assert.AreEqual(2, (int)t[1]);
            Assert.AreEqual(new BsonDocument { { "Name", new BsonArray { 3 } } }, t[2]);
            Assert.AreEqual(new BsonArray { 3 }, t[3]);
        }

        [Test]
        public void EvaluateWildcardScanNestResults()
        {
            BsonDocument o1 = new BsonDocument { { "Name", 1 } };
            BsonDocument o2 = new BsonDocument { { "Name", 2 } };
            BsonDocument o3 = new BsonDocument { { "Name", new BsonDocument { { "Name", new BsonArray { 3 } } } } };
            var a = new BsonArray { o1, o2, o3 };

            var t = a.SelectTokens("$..*").ToList();
            Assert.IsNotNull(t);
            Assert.AreEqual(9, t.Count);

            Assert.AreEqual(a, t[0]);
            Assert.AreEqual(o1, t[1]);
            Assert.AreEqual(1, (int)t[2]);
            Assert.AreEqual(o2, t[3]);
            Assert.AreEqual(2, (int)t[4]);
            Assert.AreEqual(o3, t[5]);
            Assert.AreEqual(new BsonDocument { { "Name", new BsonArray { 3 } } }, t[6]);
            Assert.AreEqual(new BsonArray { 3 }, t[7]);
            Assert.AreEqual(3, (int)t[8]);
        }

        [Test]
        public void EvaluateSinglePropertyReturningArray()
        {
            var o = new BsonDocument
            {
                { "Blah", new BsonArray{ 1, 2, 3 } }
            };

            var t = o.SelectToken("Blah");
            Assert.IsNotNull(t);
            Assert.AreEqual(BsonType.Array, t.BsonType);

            t = o.SelectToken("Blah[2]");
            Assert.AreEqual(BsonType.Int32, t.BsonType);
            Assert.AreEqual(3, (int)t);
        }

        [Test]
        public void EvaluateLastSingleCharacterProperty()
        {
            BsonDocument o2 = BsonDocument.Parse("{'People':[{'N':'Jeff'}]}");
            string a2 = (string)o2.SelectToken("People[0].N");

            Assert.AreEqual("Jeff", a2);
        }

        [Test]
        public void ExistsQuery()
        {
            var a = new BsonArray
            {
                new BsonDocument { { "hi", "ho" } },
                new BsonDocument { { "hi2", "ha" } }
            };

            var t = a.SelectTokens("[ ?( @.hi ) ]").ToList();
            Assert.IsNotNull(t);
            Assert.AreEqual(1, t.Count);
            Assert.AreEqual(new BsonDocument { { "hi", "ho" } }, t[0]);
        }

        [Test]
        public void EqualsQuery()
        {
            var a = new BsonArray
            {
                new BsonDocument { { "hi", "ho" } },
                new BsonDocument { { "hi", "ha" } }
            };

            var t = a.SelectTokens("[ ?( @.['hi'] == 'ha' ) ]").ToList();
            Assert.IsNotNull(t);
            Assert.AreEqual(1, t.Count);
            Assert.AreEqual(new BsonDocument { { "hi", "ha" } }, t[0]);
        }

        [Test]
        public void NotEqualsQuery()
        {
            var a = new BsonArray
            {
                new BsonArray { new BsonDocument { { "hi", "ho" } } },
                new BsonArray { new BsonDocument { { "hi", "ha" } } }
            };

            var t = a.SelectTokens("[ ?( @..hi <> 'ha' ) ]").ToList();
            Assert.IsNotNull(t);
            Assert.AreEqual(1, t.Count);
            Assert.AreEqual(new BsonArray { new BsonDocument { { "hi", "ho" } } }, t[0]);
        }

        [Test]
        public void NoPathQuery()
        {
            var a = new BsonArray { 1, 2, 3 };

            var t = a.SelectTokens("[ ?( @ > 1 ) ]").ToList();
            Assert.IsNotNull(t);
            Assert.AreEqual(2, t.Count);
            Assert.AreEqual(2, (int)t[0]);
            Assert.AreEqual(3, (int)t[1]);
        }

        [Test]
        public void MultipleQueries()
        {
            var a = new BsonArray { 1, 2, 3, 4, 5, 6, 7, 8, 9 };

            // json path does item based evaluation - http://www.sitepen.com/blog/2008/03/17/jsonpath-support/
            // first query resolves array to ints
            // int has no children to query
            var t = a.SelectTokens("[?(@ <> 1)][?(@ <> 4)][?(@ < 7)]").ToList();
            Assert.IsNotNull(t);
            Assert.AreEqual(0, t.Count);
        }

        [Test]
        public void GreaterQuery()
        {
            var a = new BsonArray
            {
                new BsonDocument { { "hi", 1 } },
                new BsonDocument { { "hi", 2 } },
                new BsonDocument { { "hi", 3 } }
            };

            var t = a.SelectTokens("[ ?( @.hi > 1 ) ]").ToList();
            Assert.IsNotNull(t);
            Assert.AreEqual(2, t.Count);            
            Assert.AreEqual(new BsonDocument { { "hi", 2 } }, t[0]);
            Assert.AreEqual(new BsonDocument { { "hi", 3 } }, t[1]);
        }

        [Test]
        public void LesserQuery_ValueFirst()
        {
            var a = new BsonArray
            {
                new BsonDocument { { "hi", 1 } },
                new BsonDocument { { "hi", 2 } },
                new BsonDocument { { "hi", 3 } }
            };

            var t = a.SelectTokens("[ ?( 1 < @.hi ) ]").ToList();
            Assert.IsNotNull(t);
            Assert.AreEqual(2, t.Count);
            Assert.AreEqual(new BsonDocument { { "hi", 2 } }, t[0]);
            Assert.AreEqual(new BsonDocument { { "hi", 3 } }, t[1]);
        }


#if FALSE && ( !(PORTABLE || DNXCORE50 || PORTABLE40 || NET35 || NET20) || NETSTANDARD1_3 || NETSTANDARD2_0 )
        [Test]
        public void GreaterQueryBigInteger()
        {
            var a = new BsonArray 
            {
                new BsonDocument { { "hi", new BigInteger(1) } },
                new BsonDocument { { "hi", new BigInteger(2) } },
                new BsonDocument { { "hi", new BigInteger(3) } }
            };

            var t = a.SelectTokens("[ ?( @.hi > 1 ) ]").ToList();
            Assert.IsNotNull(t);
            Assert.AreEqual(2, t.Count);
            Assert.IsTrue(JToken.DeepEquals(new BsonDocument { { "hi", 2 } }, t[0]));
            Assert.IsTrue(JToken.DeepEquals(new BsonDocument { { "hi", 3 } }, t[1]));
        }
#endif

        [Test]
        public void GreaterOrEqualQuery()
        {
            var a = new BsonArray
            {
                new BsonDocument { { "hi", 1 } },
                new BsonDocument { { "hi", 2 } },
                new BsonDocument { { "hi", 2.0 } },
                new BsonDocument { { "hi", 3 } }
            };

            var t = a.SelectTokens("[ ?( @.hi >= 1 ) ]").ToList();
            Assert.IsNotNull(t);
            Assert.AreEqual(4, t.Count);
            Assert.AreEqual(new BsonDocument { { "hi", 1 } }, t[0]);
            Assert.AreEqual(new BsonDocument { { "hi", 2 } }, t[1]);
            Assert.AreEqual(new BsonDocument { { "hi", 2.0 } }, t[2]);
            Assert.AreEqual(new BsonDocument { { "hi", 3 } }, t[3]);
        }

        [Test]
        public void NestedQuery()
        {
            var a = new BsonArray
            {
                new BsonDocument
                {
                    { "name", "Bad Boys" },
                    { "cast", new BsonArray
                       {
                            new BsonDocument { { "name", "Will Smith" } }
                        }
                    }
                },
                new BsonDocument
                {
                    { "name", "Independence Day" },
                    {  "cast", new BsonArray
                        {
                            new BsonDocument { { "name", "Will Smith" } }
                        }
                    }
                },
                new BsonDocument
                {
                    { "name", "The Rock" },
                    {  "cast", new BsonArray
                        {
                            new BsonDocument { { "name", "Nick Cage" } }
                        }
                    }
                }
            };

            var t = a.SelectTokens("[?(@.cast[?(@.name=='Will Smith')])].name").ToList();
            Assert.IsNotNull(t);
            Assert.AreEqual(2, t.Count);
            Assert.AreEqual("Bad Boys", (string)t[0]);
            Assert.AreEqual("Independence Day", (string)t[1]);
        }

#if FALSE
        [Test]
        public void PathWithConstructor()
        {
            var a = BsonArrayHelpers.Parse(@"[
  {
    ""Property1"": [
      1,
      [
        [
          []
        ]
      ]
    ]
  },
  {
    ""Property2"": new Constructor1(
      null,
      [
        1
      ]
    )
  }
]");

            var v = a.SelectToken("[1].Property2[1][0]");
            Assert.AreEqual(1L, v); // TODO
        }
#endif

        [Test]
        public void MultiplePaths()
        {
            var a = BsonArrayHelpers.Parse(@"[
  {
    ""price"": 199,
    ""max_price"": 200
  },
  {
    ""price"": 200,
    ""max_price"": 200
  },
  {
    ""price"": 201,
    ""max_price"": 200
  }
]");

            var results = a.SelectTokens("[?(@.price > @.max_price)]").ToList();
            Assert.AreEqual(1, results.Count);
            Assert.AreEqual(a[2], results[0]);
        }

        [Test]
        public void Exists_True()
        {
            var a = BsonArrayHelpers.Parse(@"[
  {
    ""price"": 199,
    ""max_price"": 200
  },
  {
    ""price"": 200,
    ""max_price"": 200
  },
  {
    ""price"": 201,
    ""max_price"": 200
  }
]");

            var results = a.SelectTokens("[?(true)]").ToList();
            Assert.AreEqual(3, results.Count);
            Assert.AreEqual(a[0], results[0]);
            Assert.AreEqual(a[1], results[1]);
            Assert.AreEqual(a[2], results[2]);
        }

        [Test]
        public void Exists_Null()
        {
            var a = BsonArrayHelpers.Parse(@"[
  {
    ""price"": 199,
    ""max_price"": 200
  },
  {
    ""price"": 200,
    ""max_price"": 200
  },
  {
    ""price"": 201,
    ""max_price"": 200
  }
]");

            var results = a.SelectTokens("[?(true)]").ToList();
            Assert.AreEqual(3, results.Count);
            Assert.AreEqual(a[0], results[0]);
            Assert.AreEqual(a[1], results[1]);
            Assert.AreEqual(a[2], results[2]);
        }

        [Test]
        public void WildcardWithProperty()
        {
            var o = BsonDocument.Parse(@"{
    ""station"": 92000041000001, 
    ""containers"": [
        {
            ""id"": 1,
            ""text"": ""Sort system"",
            ""containers"": [
                {
                    ""id"": ""2"",
                    ""text"": ""Yard 11""
                },
                {
                    ""id"": ""92000020100006"",
                    ""text"": ""Sort yard 12""
                },
                {
                    ""id"": ""92000020100005"",
                    ""text"": ""Yard 13""
                } 
            ]
        }, 
        {
            ""id"": ""92000020100011"",
            ""text"": ""TSP-1""
        }, 
        {
            ""id"":""92000020100007"",
            ""text"": ""Passenger 15""
        }
    ]
}");

            var tokens = o.SelectTokens("$..*[?(@.text)]").ToList();
            int i = 0;
            Assert.AreEqual("Sort system", (string)tokens[i++]["text"]);
            Assert.AreEqual("TSP-1", (string)tokens[i++]["text"]);
            Assert.AreEqual("Passenger 15", (string)tokens[i++]["text"]);
            Assert.AreEqual("Yard 11", (string)tokens[i++]["text"]);
            Assert.AreEqual("Sort yard 12", (string)tokens[i++]["text"]);
            Assert.AreEqual("Yard 13", (string)tokens[i++]["text"]);
            Assert.AreEqual(6, tokens.Count);
        }

        [Test]
        public void QueryAgainstNonStringValues()
        {
            IList<object> values = new List<object>
            {
                "ff2dc672-6e15-4aa2-afb0-18f4f69596ad",
                new Guid("ff2dc672-6e15-4aa2-afb0-18f4f69596ad"),
                "http://localhost",
                // new Uri("http://localhost"),
                "2000-12-05T05:07:59Z",
                new DateTime(2000, 12, 5, 5, 7, 59, DateTimeKind.Utc),
                "2000-12-05T05:07:59-10:00",
                // new DateTimeOffset(2000, 12, 5, 5, 7, 59, -TimeSpan.FromHours(10)),
                "SGVsbG8gd29ybGQ=",
                Encoding.UTF8.GetBytes("Hello world"),
                "365.23:59:59"
                // new TimeSpan(365, 23, 59, 59)
            };

            var o = new BsonDocument
            {
                { "prop",
                    new BsonArray(
                        values.Select(v => new BsonDocument { { "childProp", BsonValue.Create(v) } })
                    )
                }
            };
            var ffs = o.ToString();

            var t = o.SelectTokens("$.prop[?(@.childProp =='ff2dc672-6e15-4aa2-afb0-18f4f69596ad')]").ToList();
            Assert.AreEqual(2, t.Count);

            t = o.SelectTokens("$.prop[?(@.childProp =='http://localhost')]").ToList();
            Assert.AreEqual(1, t.Count);

            t = o.SelectTokens("$.prop[?(@.childProp =='2000-12-05T05:07:59Z')]").ToList();
            Assert.AreEqual(2, t.Count);

            t = o.SelectTokens("$.prop[?(@.childProp =='2000-12-05T05:07:59-10:00')]").ToList();
            Assert.AreEqual(1, t.Count);

            t = o.SelectTokens("$.prop[?(@.childProp =='SGVsbG8gd29ybGQ=')]").ToList();
            Assert.AreEqual(2, t.Count);

            t = o.SelectTokens("$.prop[?(@.childProp =='365.23:59:59')]").ToList();
            Assert.AreEqual(1, t.Count);
        }

        [Test]
        public void Example()
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

            Assert.AreEqual(2, storeNames.Count);
            Assert.AreEqual("Lambton Quay", storeNames[0]);
            Assert.AreEqual("Willis Street", storeNames[1]);
            Assert.AreEqual(2, firstProductNames.Count);
            Assert.AreEqual(null, firstProductNames[0]);
            Assert.AreEqual("Headlight Fluid", firstProductNames[1]);
            Assert.AreEqual(149.95m, totalPrice);
        }

        [Test]
        public void NotEqualsAndNonPrimativeValues()
        {
            string json = @"[
  {
    ""name"": ""string"",
    ""value"": ""aString""
  },
  {
    ""name"": ""number"",
    ""value"": 123
  },
  {
    ""name"": ""array"",
    ""value"": [
      1,
      2,
      3,
      4
    ]
  },
  {
    ""name"": ""object"",
    ""value"": {
      ""1"": 1
    }
  }
]";

            var a = BsonArrayHelpers.Parse(json);

            var result = a.SelectTokens("$.[?(@.value!=1)]").ToList();
            Assert.AreEqual(4, result.Count);

            result = a.SelectTokens("$.[?(@.value!='2000-12-05T05:07:59-10:00')]").ToList();
            Assert.AreEqual(4, result.Count);

            result = a.SelectTokens("$.[?(@.value!=null)]").ToList();
            Assert.AreEqual(4, result.Count);

            result = a.SelectTokens("$.[?(@.value!=123)]").ToList();
            Assert.AreEqual(3, result.Count);

            result = a.SelectTokens("$.[?(@.value)]").ToList();
            Assert.AreEqual(4, result.Count);
        }

        [Test]
        public void RootInFilter()
        {
            string json = @"[
   {
      ""store"" : {
         ""book"" : [
            {
               ""category"" : ""reference"",
               ""author"" : ""Nigel Rees"",
               ""title"" : ""Sayings of the Century"",
               ""price"" : 8.95
            },
            {
               ""category"" : ""fiction"",
               ""author"" : ""Evelyn Waugh"",
               ""title"" : ""Sword of Honour"",
               ""price"" : 12.99
            },
            {
               ""category"" : ""fiction"",
               ""author"" : ""Herman Melville"",
               ""title"" : ""Moby Dick"",
               ""isbn"" : ""0-553-21311-3"",
               ""price"" : 8.99
            },
            {
               ""category"" : ""fiction"",
               ""author"" : ""J. R. R. Tolkien"",
               ""title"" : ""The Lord of the Rings"",
               ""isbn"" : ""0-395-19395-8"",
               ""price"" : 22.99
            }
         ],
         ""bicycle"" : {
            ""color"" : ""red"",
            ""price"" : 19.95
         }
      },
      ""expensive"" : 10
   }
]";

            var a = BsonArrayHelpers.Parse(json);

            var result = a.SelectTokens("$.[?($.[0].store.bicycle.price < 20)]").ToList();
            Assert.AreEqual(1, result.Count);

            result = a.SelectTokens("$.[?($.[0].store.bicycle.price < 10)]").ToList();
            Assert.AreEqual(0, result.Count);
        }

        [Test]
        public void RootInFilterWithRootObject()
        {
            string json = @"{
                ""store"" : {
                    ""book"" : [
                        {
                            ""category"" : ""reference"",
                            ""author"" : ""Nigel Rees"",
                            ""title"" : ""Sayings of the Century"",
                            ""price"" : 8.95
                        },
                        {
                            ""category"" : ""fiction"",
                            ""author"" : ""Evelyn Waugh"",
                            ""title"" : ""Sword of Honour"",
                            ""price"" : 12.99
                        },
                        {
                            ""category"" : ""fiction"",
                            ""author"" : ""Herman Melville"",
                            ""title"" : ""Moby Dick"",
                            ""isbn"" : ""0-553-21311-3"",
                            ""price"" : 8.99
                        },
                        {
                            ""category"" : ""fiction"",
                            ""author"" : ""J. R. R. Tolkien"",
                            ""title"" : ""The Lord of the Rings"",
                            ""isbn"" : ""0-395-19395-8"",
                            ""price"" : 22.99
                        }
                    ],
                    ""bicycle"" : [
                        {
                            ""color"" : ""red"",
                            ""price"" : 19.95
                        }
                    ]
                },
                ""expensive"" : 10
            }";

            BsonDocument a = BsonDocument.Parse(json);

            var result = a.SelectTokens("$..book[?(@.price <= $['expensive'])]").ToList();
            Assert.AreEqual(2, result.Count);

            result = a.SelectTokens("$.store..[?(@.price > $.expensive)]").ToList();
            Assert.AreEqual(3, result.Count);
        }

        [Test]
        public void RootInFilterWithInitializers()
        {
            BsonDocument rootObject = new BsonDocument
            {
                { "referenceDate", new BsonDateTime(DateTime.MinValue) },
                {
                    "dateObjectsArray",
                    new BsonArray()
                    {
                        new BsonDocument { { "date", new BsonDateTime(DateTime.MinValue) } },
                        new BsonDocument { { "date", new BsonDateTime(DateTime.MaxValue) } },
                        new BsonDocument { { "date", new BsonDateTime(DateTime.Now) } },
                        new BsonDocument { { "date", new BsonDateTime(DateTime.MinValue) } },
                    }
                }
            };

            var result = rootObject.SelectTokens("$.dateObjectsArray[?(@.date == $.referenceDate)]").ToList();
            Assert.AreEqual(2, result.Count);
        }

        [Test]
        public void IdentityOperator()
        {
            var o = BsonDocument.Parse(@"{
	            'Values': [{

                    'Coercible': 1,
                    'Name': 'Number'

                }, {
		            'Coercible': '1',
		            'Name': 'String'
	            }]
            }");

            // just to verify expected behavior hasn't changed
            IEnumerable<string> sanity1 = o.SelectTokens("Values[?(@.Coercible == '1')].Name").Select(x => (string)x);
            IEnumerable<string> sanity2 = o.SelectTokens("Values[?(@.Coercible != '1')].Name").Select(x => (string)x);
            // new behavior
            IEnumerable<string> mustBeNumber1 = o.SelectTokens("Values[?(@.Coercible === 1)].Name").Select(x => (string)x);
            IEnumerable<string> mustBeString1 = o.SelectTokens("Values[?(@.Coercible !== 1)].Name").Select(x => (string)x);
            IEnumerable<string> mustBeString2 = o.SelectTokens("Values[?(@.Coercible === '1')].Name").Select(x => (string)x);
            IEnumerable<string> mustBeNumber2 = o.SelectTokens("Values[?(@.Coercible !== '1')].Name").Select(x => (string)x);

            // FAILS-- JPath returns { "String" }
            //CollectionAssert.AreEquivalent(new[] { "Number", "String" }, sanity1);
            // FAILS-- JPath returns { "Number" }
            //Assert.IsTrue(!sanity2.Any());
            Assert.AreEqual("Number", mustBeNumber1.Single());
            Assert.AreEqual("String", mustBeString1.Single());
            Assert.AreEqual("Number", mustBeNumber2.Single());
            Assert.AreEqual("String", mustBeString2.Single());
        }

        [Test]
        public void QueryWithEscapedPath()
        {
            var t = BsonDocument.Parse(@"{
""Property"": [
          {
            ""@Name"": ""x"",
            ""@Value"": ""y"",
            ""@Type"": ""FindMe""
          }
   ]
}");

            var tokens = t.SelectTokens("$..[?(@.['@Type'] == 'FindMe')]").ToList();
            Assert.AreEqual(1, tokens.Count);
        }

        [Test]
        public void Equals_FloatWithInt()
        {
            var t = BsonDocument.Parse(@"{
  ""Values"": [
    {
      ""Property"": 1
    }
  ]
}");

            Assert.IsNotNull(t.SelectToken(@"Values[?(@.Property == 1.0)]"));
        }

#if DNXCORE50
        [Theory]
#endif
        [TestCaseSource(nameof(StrictMatchWithInverseTestData))]
        public static void EqualsStrict(string value1, string value2, bool matchStrict)
        {
            string completeJson = @"{
  ""Values"": [
    {
      ""Property"": " + value1 + @"
    }
  ]
}";
            string completeEqualsStrictPath = "$.Values[?(@.Property === " + value2 + ")]";
            string completeNotEqualsStrictPath = "$.Values[?(@.Property !== " + value2 + ")]";

            var t = BsonDocument.Parse(completeJson);

            bool hasEqualsStrict = t.SelectTokens(completeEqualsStrictPath).Any();
            Assert.AreEqual(
                matchStrict,
                hasEqualsStrict,
                $"Expected {value1} and {value2} to match: {matchStrict}"
                + Environment.NewLine + completeJson + Environment.NewLine + completeEqualsStrictPath);

            bool hasNotEqualsStrict = t.SelectTokens(completeNotEqualsStrictPath).Any();
            Assert.AreNotEqual(
                matchStrict,
                hasNotEqualsStrict,
                $"Expected {value1} and {value2} to match: {!matchStrict}"
                + Environment.NewLine + completeJson + Environment.NewLine + completeEqualsStrictPath);
        }

        public static IEnumerable<object[]> StrictMatchWithInverseTestData()
        {
            foreach (var item in StrictMatchTestData())
            {
                yield return new object[] { item[0], item[1], item[2] };

                if (!item[0].Equals(item[1]))
                {
                    // Test the inverse
                    yield return new object[] { item[1], item[0], item[2] };
                }
            }
        }

        private static IEnumerable<object[]> StrictMatchTestData()
        {
            yield return new object[] { "1", "1", true };
            yield return new object[] { "1", "1.0", true };
            yield return new object[] { "1", "true", false };
            yield return new object[] { "1", "'1'", false };
            yield return new object[] { "'1'", "'1'", true };
            yield return new object[] { "false", "false", true };
            yield return new object[] { "true", "false", false };
            yield return new object[] { "1", "1.1", false };
            yield return new object[] { "1", "null", false };
            yield return new object[] { "null", "null", true };
            yield return new object[] { "null", "'null'", false };
        }
    }
}
