using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Linq;

namespace MongoDB.Bson.Path
{
    internal enum QueryOperator
    {
        None = 0,
        Equals = 1,
        NotEquals = 2,
        Exists = 3,
        LessThan = 4,
        LessThanOrEquals = 5,
        GreaterThan = 6,
        GreaterThanOrEquals = 7,
        And = 8,
        Or = 9,
        RegexEquals = 10,
        StrictEquals = 11,
        StrictNotEquals = 12
    }

    internal abstract class QueryExpression
    {
        internal QueryOperator Operator;

        public QueryExpression(QueryOperator @operator)
        {
            Operator = @operator;
        }

        public abstract bool IsMatch(BsonValue root, BsonValue t);
    }

    internal class CompositeExpression : QueryExpression
    {
        public List<QueryExpression> Expressions { get; set; }

        public CompositeExpression(QueryOperator @operator) : base(@operator)
        {
            Expressions = new List<QueryExpression>();
        }

        public override bool IsMatch(BsonValue root, BsonValue t)
        {
            switch (Operator)
            {
                case QueryOperator.And:
                    foreach (QueryExpression e in Expressions)
                    {
                        if (!e.IsMatch(root, t))
                        {
                            return false;
                        }
                    }
                    return true;
                case QueryOperator.Or:
                    foreach (QueryExpression e in Expressions)
                    {
                        if (e.IsMatch(root, t))
                        {
                            return true;
                        }
                    }
                    return false;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }

    internal class BooleanQueryExpression : QueryExpression
    {
        public readonly object Left;
        public readonly object? Right;

        public BooleanQueryExpression(QueryOperator @operator, object left, object? right) : base(@operator)
        {
            Left = left;
            Right = right;
        }

        private IEnumerable<BsonValue> GetResult(BsonValue root, BsonValue t, object? o)
        {
            if (o is BsonValue resultToken)
            {
                return new[] { resultToken };
            }

            if (o is List<PathFilter> pathFilters)
            {
                return BsonPath.Evaluate(pathFilters, root, t, false);
            }

            return Array.Empty<BsonValue>();
        }

        public override bool IsMatch(BsonValue root, BsonValue t)
        {
            if (Operator == QueryOperator.Exists)
            {
                return GetResult(root, t, Left).Any();
            }

            using (IEnumerator<BsonValue> leftResults = GetResult(root, t, Left).GetEnumerator())
            {
                if (leftResults.MoveNext())
                {
                    IEnumerable<BsonValue> rightResultsEn = GetResult(root, t, Right);
                    ICollection<BsonValue> rightResults = rightResultsEn as ICollection<BsonValue> ?? rightResultsEn.ToList();

                    do
                    {
                        BsonValue leftResult = leftResults.Current;
                        foreach (BsonValue rightResult in rightResults)
                        {
                            if (MatchTokens(leftResult, rightResult))
                            {
                                return true;
                            }
                        }
                    } while (leftResults.MoveNext());
                }
            }

            return false;
        }

        static BsonType[] _numberWidthOrder = new BsonType[]
            { BsonType.Int32, BsonType.Int64, BsonType.Decimal128, BsonType.Double };

        internal static BsonType WidestCommonNumberType(BsonType t1, BsonType t2)
        {
            int t1Pos = Array.IndexOf(_numberWidthOrder, t1);
            int t2Pos = Array.IndexOf(_numberWidthOrder, t2);
            return _numberWidthOrder[Math.Max(t1Pos, t2Pos)];
        }

        internal static bool IsNumberValue(BsonValue value) =>
            Array.IndexOf(_numberWidthOrder, value.BsonType) >= 0;

        internal static int CompareNumberValues(BsonValue leftValue, BsonValue rightValue)
        {
            switch (WidestCommonNumberType(leftValue.BsonType, rightValue.BsonType))
            {
                case BsonType.Int32:
                    return leftValue.ToInt32().CompareTo(rightValue.ToInt32());
                case BsonType.Int64:
                    return leftValue.ToInt64().CompareTo(rightValue.ToInt64());
                case BsonType.Decimal128:
                    return leftValue.ToDecimal128().CompareTo(rightValue.ToDecimal128());
                case BsonType.Double:
                    return leftValue.ToDouble().CompareTo(rightValue.ToDouble());
                default:
                    throw new InvalidOperationException("CompareNumberTypes requires numeric inputs");
            }
        }

        internal static int Compare(BsonValue leftValue, BsonValue rightValue)
        {
            BsonType comparisonType = leftValue.BsonType == BsonType.String &&
                    leftValue.BsonType != rightValue.BsonType ?
                    rightValue.BsonType : leftValue.BsonType;
            switch (comparisonType)
            {
                case BsonType.Int32:
                case BsonType.Int64:
                case BsonType.Decimal128:
                case BsonType.Double:
                    return CompareNumberValues(leftValue, rightValue);
                case BsonType.String:
                    return string.CompareOrdinal(leftValue.ToString(), rightValue.ToString());
                case BsonType.Boolean:
                    return leftValue.ToBoolean().CompareTo(rightValue.ToBoolean());
            }
            return leftValue.CompareTo(rightValue);
        }

        private bool MatchTokens(BsonValue leftResult, BsonValue rightResult)
        {
            if (leftResult is BsonValue leftValue && rightResult is BsonValue rightValue)
            {
                switch (Operator)
                {
                    case QueryOperator.RegexEquals:
                        if (RegexEquals(leftValue, rightValue))
                        {
                            return true;
                        }
                        break;
                    case QueryOperator.Equals:
                        if (EqualsWithStringCoercion(leftValue, rightValue))
                        {
                            return true;
                        }
                        break;
                    case QueryOperator.StrictEquals:
                        if (EqualsWithStrictMatch(leftValue, rightValue))
                        {
                            return true;
                        }
                        break;
                    case QueryOperator.NotEquals:
                        if (!EqualsWithStringCoercion(leftValue, rightValue))
                        {
                            return true;
                        }
                        break;
                    case QueryOperator.StrictNotEquals:
                        if (!EqualsWithStrictMatch(leftValue, rightValue))
                        {
                            return true;
                        }
                        break;
                    case QueryOperator.GreaterThan:
                        if (Compare(leftValue, rightValue) > 0)
                        {
                            return true;
                        }
                        break;
                    case QueryOperator.GreaterThanOrEquals:
                        if (Compare(leftValue, rightValue) >= 0)
                        {
                            return true;
                        }
                        break;
                    case QueryOperator.LessThan:
                        if (Compare(leftValue, rightValue) < 0)
                        {
                            return true;
                        }
                        break;
                    case QueryOperator.LessThanOrEquals:
                        if (Compare(leftValue, rightValue) <= 0)
                        {
                            return true;
                        }
                        break;
                    case QueryOperator.Exists:
                        return true;
                }
            }
            else
            {
                switch (Operator)
                {
                    case QueryOperator.Exists:
                    // you can only specify primitive types in a comparison
                    // notequals will always be true
                    case QueryOperator.NotEquals:
                        return true;
                }
            }

            return false;
        }

        private static bool RegexEquals(BsonValue input, BsonValue pattern)
        {
            if (input.BsonType != BsonType.String || pattern.BsonType != BsonType.String)
            {
                return false;
            }

            string regexText = pattern.AsString;
            int patternOptionDelimiterIndex = regexText.LastIndexOf('/');

            string patternText = regexText.Substring(1, patternOptionDelimiterIndex - 1);
            string optionsText = regexText.Substring(patternOptionDelimiterIndex + 1);

            return Regex.IsMatch(input.AsString, patternText, MiscellaneousUtils.GetRegexOptions(optionsText));
        }

        internal static bool EqualsWithStringCoercion(BsonValue value, BsonValue queryValue)
        {
            if (value.Equals(queryValue))
            {
                return true;
            }

            // Handle comparing an integer with a float
            // e.g. Comparing 1 and 1.0
            if (IsNumberValue(value) && IsNumberValue(queryValue))
            {
                return CompareNumberValues(value, queryValue) == 0;
            }

            if (queryValue.BsonType != BsonType.String)
            {
                return false;
            }

            string queryValueString = queryValue.AsString;

            string currentValueString;

            // potential performance issue with converting every value to string?
            switch (value.BsonType)
            {
                case BsonType.Timestamp:
                case BsonType.DateTime:
                    currentValueString = value.ToString();
                    break;
                case BsonType.Binary:
                    {
                        var binaryValue = value.AsBsonBinaryData;
                        if (binaryValue.SubType == BsonBinarySubType.UuidStandard ||
                            binaryValue.SubType == BsonBinarySubType.UuidLegacy)
                        {
                            currentValueString = binaryValue.AsGuid.ToString();
                        }
                        else
                        {
                            currentValueString = Convert.ToBase64String(value.AsBsonBinaryData.Bytes);
                        }
                        break;
                    }
#if FALSE
                case BsonType.Guid:
                case BsonType.TimeSpan:
                    currentValueString = value.Value!.ToString();
                    break;
                case BsonType.Uri:
                    currentValueString = ((Uri)value.Value!).OriginalString;
                    break;
#endif
                default:
                    return false;
            }

            return string.Equals(currentValueString, queryValueString, StringComparison.Ordinal);
        }

        internal static bool EqualsWithStrictMatch(BsonValue value, BsonValue queryValue)
        {
            // ?ValidationUtils.ArgumentNotNull(value, nameof(value));
            // ?ValidationUtils.ArgumentNotNull(queryValue, nameof(queryValue));

            // Handle comparing an integer with a float
            // e.g. Comparing 1 and 1.0
            if (IsNumberValue(value) && IsNumberValue(queryValue))
            {
                return CompareNumberValues(value, queryValue) == 0;
            }

            // we handle floats and integers the exact same way, so they are pseudo equivalent
            if (value.BsonType != queryValue.BsonType)
            {
                return false;
            }

            return value.Equals(queryValue);
        }
    }
}
