using System.Collections.Generic;

namespace MongoDB.Bson.Path
{
    internal class ScanFilter : PathFilter
    {
        internal string? Name;

        public ScanFilter(string? name)
        {
            Name = name;
        }

        public override IEnumerable<BsonValue> ExecuteFilter(BsonValue root, IEnumerable<BsonValue> current, bool errorWhenNoMatch)
        {
            foreach (BsonValue c in current)
            {
                // if (Name == null)
                // {
                //     yield return c;
                // }

                foreach (var e in GetScanValues(c))
                {
                    System.Diagnostics.Debug.WriteLine($"sv = {e}, me name = {Name}");
                    if (e.Name == Name)
                    {
                        yield return e.Value;
                    }
                }
            }
        }
    }
}