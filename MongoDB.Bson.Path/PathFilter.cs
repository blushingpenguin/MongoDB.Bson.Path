using System.Collections.Generic;
using System.Globalization;

namespace MongoDB.Bson.Path
{
    internal abstract class PathFilter
    {
        public abstract IEnumerable<BsonValue> ExecuteFilter(BsonValue root, IEnumerable<BsonValue> current, bool errorWhenNoMatch);

        protected static BsonValue? GetTokenIndex(BsonValue t, bool errorWhenNoMatch, int index)
        {
            if (t is BsonArray a)
            {
                if (a.Count <= index)
                {
                    if (errorWhenNoMatch)
                    {
                        throw new BsonException("Index {0} outside the bounds of BsonArray.".FormatWith(CultureInfo.InvariantCulture, index));
                    }

                    return null;
                }

                return a[index];
            }
            else
            {
                if (errorWhenNoMatch)
                {
                    throw new BsonException("Index {0} not valid on {1}.".FormatWith(CultureInfo.InvariantCulture, index, t.GetType().Name));
                }

                return null;
            }
        }

        protected static IEnumerable<(string? Name, BsonValue Value)> GetScanValues(BsonValue container)
        {
            yield return (null, container);
            if (container.BsonType == BsonType.Array)
            {
                var a = container.AsBsonArray;
                foreach (var e in a)
                {
                    foreach (var c in GetScanValues(e))
                    {
                        yield return c;
                    }
                }
            }
            else if (container.BsonType == BsonType.Document)
            {
                var d = container.AsBsonDocument;
                foreach (var e in d)
                {
                    yield return (e.Name, e.Value);
                    foreach (var c in GetScanValues(e.Value))
                    {
                        yield return c;
                    }
                }
            }
            // else
            // {
            //     yield return (null, container);
            // }
        }
#if FALSE
        protected static BsonValue? GetNextScanValue(BsonValue originalParent, BsonValue? container, BsonValue? value)
        {
            // step into container's values
            if (container != null && HasValues(container))
            {
                value = First(container);
            }
            else
            {
                // finished container, move to parent
                while (value != null && value != originalParent && value == value.Parent!.Last)
                {
                    value = value.Parent;
                }

                // finished
                if (value == null || value == originalParent)
                {
                    return null;
                }

                // move to next value in container
                value = value.Next;
            }

            return value;
        }
#endif
    }
}
