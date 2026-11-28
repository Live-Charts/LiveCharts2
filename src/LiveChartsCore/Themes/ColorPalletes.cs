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

using LiveChartsCore.Drawing;

namespace LiveChartsCore.Themes;

/// <summary>
/// Defines a set of predefined colors to use.
/// </summary>
public static partial class ColorPalletes
{
    /// <summary>
    /// Gets the fluent design colors.
    /// </summary>
    /// <value>
    /// The fluent design.
    /// </value>
    public static LvcColor[] FluentDesign => new LvcColor[]
    {
        RGB(116,77,169),
        RGB(231,72,86),
        RGB(255,140,0),
        RGB(0,153,188),
        RGB(191,0,119),
        RGB(1,133,116),
        RGB(194,57,179),
        RGB(76,74,72),
        RGB(0,183,195)
    };

    /// <summary>
    /// Gets the colors of the default theme, the same in light and dark: vivid Material Design colors, light and dark
    /// ones mixed, so they stay apart for colorblind readers, who tell colors apart mostly by how light they are.
    /// The order keeps every pair of the first six colors apart with red-green color blindness, light green and
    /// amber, the closest pair, are the 7th and the 5th.
    /// </summary>
    /// <value>
    /// Blue, red, cyan, indigo, amber, teal, light green, pink and blue gray.
    /// </value>
    public static LvcColor[] Default => new LvcColor[]
    {
        RGB(33,150,243),    // blue
        RGB(244,67,54),     // red
        RGB(0,188,212),     // cyan
        RGB(63,81,181),     // indigo
        RGB(255,193,7),     // amber
        RGB(0,150,136),     // teal
        RGB(139,195,74),    // light green
        RGB(233,30,99),     // pink
        RGB(96,125,139),    // blue gray
    };

    /// <summary>
    /// Gets the material design500 colors.
    /// </summary>
    /// <value>
    /// The material design500.
    /// </value>
    public static LvcColor[] MaterialDesign500 => new LvcColor[]
    {
        RGB(33,150,243),    // blue
        RGB(244,67,54),     // red
        RGB(139,195,74),    // light green
        RGB(0,188,212),     // cyan
        RGB(63,81,181),     // indigo
        RGB(255,193,7),     // ambar
        RGB(0,150,136),     // teal
        RGB(233,30,99),     // pink
        RGB(96,125,139),    // blue gray
    };

    /// <summary>
    /// Gets the material design200 colors.
    /// </summary>
    /// <value>
    /// The material design200.
    /// </value>
    public static LvcColor[] MaterialDesign200 => new LvcColor[]
    {
        RGB(144,202,249),   // blue
        RGB(239,154,154),   // red
        RGB(197,225,165),   // light green
        RGB(128,222,234),   // cyan
        RGB(159,168,218),   // indigo
        RGB(255,224,130),   // ambar
        RGB(128,203,196),   // teal
        RGB(244,143,177),   // pink
        RGB(176,190,197),   // blue gray
    };

    /// <summary>
    /// Gets the material design800 colors.
    /// </summary>
    /// <value>
    /// The material design800.
    /// </value>
    public static LvcColor[] MaterialDesign800 => new LvcColor[]
    {
        RGB(21,101,192),    // blue
        RGB(198,40,40),     // red
        RGB(85,139,47),     // light green
        RGB(0,131,143),     // cyan
        RGB(40,53,147),     // indigo
        RGB(255,143,0),     // ambar
        RGB(0,105,92),      // teal
        RGB(173,20,87),     // pink
        RGB(55,71,79),      // blue gray
    };

    private static LvcColor RGB(byte r, byte g, byte b)
    {
        return LvcColor.FromArgb(255, r, g, b);
    }
}
