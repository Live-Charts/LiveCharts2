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
/// Converts a string to a an array of <see cref="LvcColor"/>.
/// </summary>
public class HexToLvcColorArrayTypeConverter : TypeConverter
{
    /// <summary>
    /// Returns whether this converter can convert an object of the given type to the type of this converter, using the specified context.
    /// </summary>
    /// <param name="context">The context.</param>
    /// <param name="sourceType">The source type.</param>
    /// <returns></returns>`
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
            : Parse(str);

    /// <summary>
    ///
    /// </summary>
    /// <param name="hexString"></param>
    /// <returns></returns>
    public static object Parse(string hexString)
    {
        var items = hexString.Split([','], StringSplitOptions.RemoveEmptyEntries);
        var colors = new LvcColor[items.Length];

        for (var i = 0; i < items.Length; i++)
        {
            var item = items[i].Trim();
            colors[i] = LvcColor.TryParse(item, out var color)
                ? color
                : new LvcColor(0, 0, 0, 0);
        }

        return colors;
    }
}
