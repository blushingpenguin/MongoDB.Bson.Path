using System;
using MongoDB.Bson.IO;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Serializers;

namespace MongoDB.Bson.Path.Test
{
    public static class BsonArrayHelpers
    {
        public static BsonArray Parse(string json)
        {
            using (var jsonReader = new JsonReader(json))
            {
                var context = BsonDeserializationContext.CreateRoot(jsonReader);
                var array = BsonArraySerializer.Instance.Deserialize(context);
                if (!jsonReader.IsAtEndOfFile())
                {
                    throw new FormatException("String contains extra non-whitespace characters beyond the end of the document.");
                }
                return array;
            }
        }
    }
}
