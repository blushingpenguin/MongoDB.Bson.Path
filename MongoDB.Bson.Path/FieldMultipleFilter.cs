using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using MongoDB.Bson;

namespace MongoDB.Bson.Path
{
    internal class FieldMultipleFilter : PathFilter
    {
        internal List<string> Names;

        public FieldMultipleFilter(List<string> names)
        {
            Names = names;
        }

        public override IEnumerable<BsonValue> ExecuteFilter(BsonValue root, IEnumerable<BsonValue> current, bool errorWhenNoMatch)
        {
            foreach (BsonValue t in current)
            {
                if (t is BsonDocument o)
                {
                    foreach (string name in Names)
                    {
                        if (o.TryGetValue(name, out var v))
                        {
                            yield return v;
                        }

                        if (errorWhenNoMatch)
                        {
                            throw new BsonException("Property '{0}' does not exist on BsonDocument.".FormatWith(CultureInfo.InvariantCulture, name));
                        }
                    }
                }
                else
                {
                    if (errorWhenNoMatch)
                    {
                        throw new BsonException("Properties {0} not valid on {1}.".FormatWith(CultureInfo.InvariantCulture, string.Join(", ", Names.Select(n => "'" + n + "'")
#if !HAVE_STRING_JOIN_WITH_ENUMERABLE
                            .ToArray()
#endif
                            ), t.GetType().Name));
                    }
                }
            }
        }
    }
}