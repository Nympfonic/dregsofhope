using System.Collections.Generic;

using System;
using System.Linq;

public static class LinqExtension
{
    // https://stackoverflow.com/questions/3188693/how-can-i-get-linq-to-return-the-object-which-has-the-max-value-for-a-given-prop/3188751
    public static T MinBy<T>(this IEnumerable<T> list, Func<T, float> value)
    {
        return list.Aggregate
        (
            (a, b) =>
                value(a) < value(b)
                ? a : b
        );
    }

    public static T MaxBy<T>(this IEnumerable<T> list, Func<T, float> value)
    {
        return list.Aggregate
        (
            (a, b) =>
                value(a) > value(b)
                ? a : b
        );
    }
}
