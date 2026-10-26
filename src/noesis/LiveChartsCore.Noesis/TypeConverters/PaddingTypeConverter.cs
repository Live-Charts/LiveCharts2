// The MIT License(MIT)
//
// Copyright(c) 2021 Alberto Rodriguez Orozco & LiveCharts Contributors
//
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in all
// copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
// SOFTWARE.

using System;
using System.ComponentModel;
using System.Globalization;
using LiveChartsCore.Drawing;

namespace LiveChartsCore.Noesis.TypeConverters;

/// <summary>
/// Converts a string to a <see cref="Padding"/> object.
/// </summary>
public class PaddingTypeConverter : TypeConverter
{
    /// <summary>
    /// Returns whether this converter can convert an object of the given type to the type of this converter, using the specified context.
    /// </summary>
    /// <param name="context">The context.</param>
    /// <param name="sourceType">The source type.</param>
    /// <returns></returns>
    public override bool CanConvertFrom(ITypeDescriptorContext? context, Type sourceType)
        => sourceType == typeof(string) || base.CanConvertFrom(context, sourceType);

    /// <summary>
    /// Converts the given value to the type of this converter, using the specified context and culture information.
    /// </summary>
    /// <param name="context">The context.</param>
    /// <param name="culture">The culture.</param>
    /// <param name="value">The value.</param>
    /// <returns>The converted value.</returns>
    public override object? ConvertFrom(ITypeDescriptorContext? context, CultureInfo? culture, object value) =>
        value is not string str
            ? base.ConvertFrom(context, culture, value)
            : ParsePadding(str);

    /// <summary>
    /// Parses a string representation of padding values and returns a <see cref="Padding"/> object.
    /// </summary>
    /// <param name="value">A comma-separated string containing one, two, or four numeric values representing padding.</param>
    /// <returns>A <see cref="Padding"/> object initialized with the parsed values. If the input string contains: <list
    /// type="bullet"> <item>One value: All sides of the padding are set to the same value.</item> <item>Two values: The
    /// first value is applied to the top and bottom, and the second value to the left and right.</item> <item>Four
    /// values: The values are applied to the top, left, bottom, and right, respectively.</item> </list> If the input
    /// string is invalid or does not contain one, two, or four values, a default <see cref="Padding"/> object is
    /// returned.</returns>
    public static object ParsePadding(string value)
    {
        var parts = value.Split(',');
        return parts.Length switch
        {
            1 => new Padding(Parse(parts[0])),
            2 => new Padding(Parse(parts[0]), Parse(parts[1])),
            4 => new Padding(Parse(parts[0]), Parse(parts[1]), Parse(parts[2]), Parse(parts[3])),
            _ => new Padding(),
        };
    }

    private static float Parse(string value) => float.TryParse(value, out var result)
        ? result
        : 0;
}
