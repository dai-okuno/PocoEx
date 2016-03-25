using System;
using System.Collections.Generic;
using System.Text;

namespace PocoEx
{
    public static class StringBuilderExtensions
    {
        public static StringBuilder Join(this StringBuilder builder, string separator, IEnumerable<string> values)
        {
            using (var value = values.GetEnumerator())
            {
                if (!value.MoveNext()) return builder;
                builder.Append(value.Current);
                while (value.MoveNext())
                {
                    builder.Append(separator).Append(value.Current);
                }
            }
            return builder;
        }

    }
}
