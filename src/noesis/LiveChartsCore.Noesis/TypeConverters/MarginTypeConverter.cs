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
using LiveChartsCore.Measure;

namespace LiveChartsCore.Noesis.TypeConverters;

/// <summary>
/// Converts a string to a <see cref="Margin"/> object.
/// </summary>
public class MarginTypeConverter : TypeConverter
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
            : ParseMargin(str);

    /// <summary>
    /// Parses a string representation of margin values and returns a <see cref="Margin"/> object.
    /// </summary>
    /// <remarks>The input string should contain one, two, or four numeric values separated by commas. These
    /// values represent the margin dimensions in the following order: <list type="bullet"> <item>One value: All sides
    /// of the margin are set to the same value.</item> <item>Two values: The first value sets the top and bottom
    /// margins, and the second value sets the left and right margins.</item> <item>Four values: The values set the top,
    /// right, bottom, and left margins, respectively.</item> </list> If the input string contains an invalid number of
    /// values, a default <see cref="Margin"/> object is returned.</remarks>
    /// <param name="value">A comma-separated string containing one, two, or four numeric values representing margin dimensions.</param>
    /// <returns>A <see cref="Margin"/> object initialized with the parsed values. If the input string is empty or does not
    /// contain a valid number of values, a default <see cref="Margin"/> object is returned.</returns>
    public static object ParseMargin(string value)
    {
        var parts = value.Split(',');

        return parts.Length switch
        {
            1 => new Margin(Parse(parts[0])),
            2 => new Margin(Parse(parts[0]), Parse(parts[1])),
            4 => new Margin(Parse(parts[0]), Parse(parts[1]), Parse(parts[2]), Parse(parts[3])),
            _ => new Margin(),
        };
    }

    private static float Parse(string value)
    {
        return string.Equals(value.Trim().ToLowerInvariant(), "auto")
            ? Margin.Auto
            : float.TryParse(value, out var result) ? result : 0;
    }
}
