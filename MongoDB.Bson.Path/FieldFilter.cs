using System.Collections.Generic;
using System.Globalization;
using MongoDB.Bson;

namespace MongoDB.Bson.Path
{
    internal class FieldFilter : PathFilter
    {
        internal string? Name;

        public FieldFilter(string? name)
        {
            Name = name;
        }

        public override IEnumerable<BsonValue> ExecuteFilter(BsonValue root, IEnumerable<BsonValue> current, bool errorWhenNoMatch)
        {
            foreach (BsonValue t in current)
            {
                if (t is BsonDocument o)
                {
                    if (Name != null)
                    {
                        if (o.TryGetValue(Name, out var v))
                        {
                            yield return v;
                        }
                        else if (errorWhenNoMatch)
                        {
                            throw new BsonException("Property '{0}' does not exist on BsonDocument.".FormatWith(CultureInfo.InvariantCulture, Name));
                        }
                    }
                    else
                    {
                        foreach (var p in o)
                        {
                            yield return p.Value;
                        }
                    }
                }
                else
                {
                    if (errorWhenNoMatch)
                    {
                        throw new BsonException("Property '{0}' not valid on {1}.".FormatWith(CultureInfo.InvariantCulture, Name ?? "*", t.GetType().Name));
                    }
                }
            }
        }
    }
}