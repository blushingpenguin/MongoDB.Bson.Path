using System;
using System.Collections.Generic;
using System.Linq;

namespace MongoDB.Bson.Path
{
#if FALSE
    public static class BsonDocumentExtensions
    {
        public static IEnumerable<BsonValue> DescendantsAndSelf(this BsonValue source)
        {
            ValidationUtils.ArgumentNotNull(source, nameof(source));

            yield return source;
            if (source is BsonDocument doc)
            {
                foreach (var child in doc.Values)
                {
                    foreach (var descendant in child.DescendantsAndSelf())
                    {
                        yield return descendant;
                    }
                }
            }
            else if (source is BsonArray arr)
            {
                foreach (var elt in arr)
                {
                    foreach (var descendant in elt.DescendantsAndSelf())
                    {
                        yield return descendant;
                    }
                }
            }
        }
    }
#endif

    internal class QueryScanFilter : PathFilter
    {
        internal QueryExpression Expression;

        public QueryScanFilter(QueryExpression expression)
        {
            Expression = expression;
        }

        public override IEnumerable<BsonValue> ExecuteFilter(BsonValue root, IEnumerable<BsonValue> current, bool errorWhenNoMatch)
        {
            foreach (BsonValue t in current)
            {
                foreach (var d in GetScanValues(t))
                // foreach (var d in t.DescendantsAndSelf())
                {
                    if (Expression.IsMatch(root, d.Value))
                    {
                        yield return d.Value;
                    }
                }
            }
        }
    }
}