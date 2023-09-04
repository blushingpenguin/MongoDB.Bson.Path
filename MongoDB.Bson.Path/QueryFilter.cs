using System.Collections.Generic;

namespace MongoDB.Bson.Path
{
    internal class QueryFilter : PathFilter
    {
        internal QueryExpression Expression;

        public QueryFilter(QueryExpression expression)
        {
            Expression = expression;
        }

        public override IEnumerable<BsonValue> ExecuteFilter(BsonValue root, IEnumerable<BsonValue> current, bool errorWhenNoMatch)
        {
            foreach (BsonValue t in current)
            {
                if (t.BsonType == BsonType.Array)
                {
                    var a = (BsonArray)t;
                    foreach (BsonValue v in a)
                    {
                        if (Expression.IsMatch(root, v))
                        {
                            yield return v;
                        }
                    }
                }
                else if (t.BsonType == BsonType.Document)
                {
                    var d = (BsonDocument)t;
                    foreach (BsonValue v in d.Values)
                    {
                        if (Expression.IsMatch(root, v))
                        {
                            yield return v;
                        }
                    }
                }
            }
        }
    }
}
