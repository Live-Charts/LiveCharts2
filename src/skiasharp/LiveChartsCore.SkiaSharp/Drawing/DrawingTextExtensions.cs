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

using System.Diagnostics;
using System.Text;
using LiveChartsCore.Drawing;
using LiveChartsCore.SkiaSharpView.Drawing.Geometries;
using SkiaSharp;
using SkiaSharp.HarfBuzz;

namespace LiveChartsCore.SkiaSharpView.Drawing;

internal static class DrawingTextExtensions
{
    internal static readonly PositionedBlob s_newLine = new(SKTextBlob.Create(string.Empty, new()), -1);
    private static readonly Dictionary<string, SKShaper> s_knownShapers = [];
    private static readonly object s_lock = new();

    internal static void DrawLabel(this SKCanvas canvas, LabelGeometry label, float opacity = 1)
    {
        label.PeekPaintInfo(out var paint, out _);

        var blobArray = label.BlobArray;
        var size = blobArray.Size;

        var x = label.X;
        var y = label.Y;

        // relative horizontal/vertical alignment, the alignment is relative to the x and y coordinates
        var rha = GetRelativeX(label.HorizontalAlign, size);
        var rva = GetRelativeY(label.VerticalAlign, size);

        // relative aligned x and y coordinates
        var rax = x + rha;
        var ray = y + rva;

#if DEBUG
        if (BaseLabelGeometry.ShowDebugLines)
        {
            using var debugPaint = new SKPaint
            {
                Color = SKColors.Gray.WithAlpha(50),
                Style = SKPaintStyle.Fill,
                StrokeWidth = 3
            };
            using var debugPaint2 = new SKPaint
            {
                Color = SKColors.Red.WithAlpha(100),
                Style = SKPaintStyle.Fill,
                StrokeWidth = 3
            };

            // draws the rectangle where the text will be drawn
            canvas.DrawRect(rax, ray, size.Width, size.Height, debugPaint);

            // highlights the relative alignenment, this draws the original coordinates
            canvas.DrawRect(x - 4f, y - 4f, 8, 8, debugPaint2);
        }
#endif

        if (label.Background != LvcColor.Empty)
        {
            var bg = label.Background;

            var c = new SKColor(bg.R, bg.G, bg.B, (byte)(bg.A * opacity));
            using var bgPaint = new SKPaint { Color = c };

            canvas.DrawRect(rax, ray, size.Width, size.Height, bgPaint);
        }

        var contentWidth = size.Width - label.Padding.Left - label.Padding.Right;

        foreach (var pb in blobArray.Blobs)
        {
            if (pb == s_newLine)
                continue;

            var blobPosition = pb.Position;

            // line alignment offset.
            var lao = blobArray.IsRTL
                ? contentWidth - blobArray.LineWidths[pb.Line]
                : label.LinesAlignment switch
                {
                    Align.Middle => (contentWidth - blobArray.LineWidths[pb.Line]) * 0.5f,
                    Align.End => contentWidth - blobArray.LineWidths[pb.Line],
                    _ => 0f
                };

            canvas.DrawText(
                pb.Blob,
                (int)(rax + blobPosition.X + lao), // truncate to avoid subpixel rendering issues
                (int)(ray + blobPosition.Y),
                paint);
        }
    }

    internal static BlobArray AsBlobArray(this LabelGeometry label)
    {
        label.PeekPaintInfo(out var skPaint, out var skFont);

        var tokenResult = Tokenize(label.Text, skFont);

        if (tokenResult.SuggestedTypeface is not null)
        {
            // a suggested typeface is found when the current typeface does not support
            // the characters in the text, as a default we will use this typeface
            // as the global typeface in the library, the user can override this
            // by defining its own typeface globally or per paint instance.

            LiveChartsSkiaSharp.DefaultTextSettings.DefaultTypeface ??= tokenResult.SuggestedTypeface;
            skFont.Typeface = tokenResult.SuggestedTypeface;
            skPaint.Typeface = tokenResult.SuggestedTypeface;
        }

        var horizontalPadding = label.Padding.Left + label.Padding.Right;
        return BlobArray.Create(tokenResult, skPaint, skFont, label.MaxWidth - horizontalPadding, label.Padding);
    }

    private static TokenResult Tokenize(string text, SKFont currentFont)
    {
        var tokens = new List<string>(text.Length);
        var sb = new StringBuilder();
        var isRLT = false;
        SKTypeface? suggestedTypeface = null;

        for (var i = 0; i < text.Length; i++)
        {
            var c = text[i];
            int codepoint;

            // CRLF normalization: drop the CR so it does not render as a notdef
            // glyph; the following LF still produces the line break.
            if (c == '\r') continue;

            // custom "GetRunes", we cant use System.Text.Rune in net462 or netstandard2.0
            if (char.IsHighSurrogate(c) && i + 1 < text.Length && char.IsLowSurrogate(text[i + 1]))
            {
                codepoint = char.ConvertToUtf32(c, text[i + 1]);
                i++; // Skip low surrogate
            }
            else
            {
                codepoint = c;
            }

            // Line break: flush the buffer WITHOUT the '\n', then emit "\n" as
            // its own token so ShapeAndPlace recognises it and returns s_newLine.
            // Issue #2266: a bare '\n' with no preceding whitespace used to be
            // glued to the previous token (e.g. "9.4700\n") and shaped as a tofu
            // glyph instead of breaking the line.
            if (codepoint == '\n')
            {
                if (sb.Length > 0)
                {
                    tokens.Add(sb.ToString());
                    _ = sb.Clear();
                }
                tokens.Add("\n");
                continue;
            }

            // Append full character (could be surrogate pair)
            _ = sb.Append(char.ConvertFromUtf32(codepoint));

            // is RTL?
            if (IsRTL(codepoint))
                isRLT = true;

            // is char supported by the typeface?
            if (suggestedTypeface is null && !currentFont.Typeface.ContainsGlyph(codepoint))
            {
                suggestedTypeface = SKFontManager.Default.MatchCharacter(codepoint);
#if DEBUG
                if (suggestedTypeface is not null)
                {
                    Trace.WriteLine(
                        $"[LiveCharts] Missing glyph for U+{codepoint:X}, " +
                        $"{suggestedTypeface.FamilyName} will be used as a fallback," +
                        $"You could instead configure LiveCharts with your own typeface.");
                }
#endif
            }

            // Token boundary (other whitespace stays glued to the preceding token)
            if (IsTokenBoundary(codepoint) && sb.Length > 0)
            {
                tokens.Add(sb.ToString());
                _ = sb.Clear();
            }
        }

        if (sb.Length > 0)
            tokens.Add(sb.ToString());

        return new([.. tokens], isRLT, suggestedTypeface);
    }

    private static bool IsTokenBoundary(int codepoint)
    {
        // Line breaks
        if (codepoint is '\n' or '\r')
            return true;

        // Other whitespace
        return char.IsWhiteSpace(char.ConvertFromUtf32(codepoint), 0);
    }

    private static bool IsRTL(int codepoint)
    {
        return codepoint is
            // Core RTL scripts
            (>= 0x0590 and <= 0x05FF) or // Hebrew
            (>= 0x0600 and <= 0x06FF) or // Arabic
            (>= 0x0700 and <= 0x074F) or // Syriac
            (>= 0x0750 and <= 0x077F) or // Arabic Supplement
            (>= 0x0780 and <= 0x07BF) or // Thaana
            (>= 0x07C0 and <= 0x07FF) or // N'Ko
            (>= 0x0800 and <= 0x083F) or // Samaritan
            (>= 0x0840 and <= 0x085F) or // Mandaic
            (>= 0x08A0 and <= 0x08FF) or // Arabic Extended-A

            // RTL presentation forms
            (>= 0xFB1D and <= 0xFDFF) or // Hebrew and Arabic presentation forms
            (>= 0xFE70 and <= 0xFEFC) or // Arabic presentation forms-B

            // RTL formatting characters
            0x200F or // Right-to-Left Mark
            0x202B or // Right-to-Left Embedding
            0x202E or // Right-to-Left Override

            // Optional: Rumi Numeral Symbols (used in Arabic contexts)
            (>= 0x10E60 and <= 0x10E7F);
    }

    private static float GetRelativeX(Align align, LvcSize size)
    {
        return align switch
        {
            Align.Start => 0f,
            Align.Middle => -size.Width / 2f,
            Align.End => -size.Width,
            _ => throw new ArgumentOutOfRangeException(nameof(align), align, null)
        };
    }

    private static float GetRelativeY(Align align, LvcSize size)
    {
        return align switch
        {
            Align.Start => 0f,
            Align.Middle => -size.Height / 2f,
            Align.End => -size.Height,
            _ => throw new ArgumentOutOfRangeException(nameof(align), align, null)
        };
    }

    internal class BlobArray
    {
        private BlobArray() { }

        public PositionedBlob[] Blobs { get; private set; } = [];
        public bool IsRTL { get; private set; }
        public List<float> LineWidths { get; private set; } = [];
        public LvcSize Size { get; private set; }

        public static BlobArray Empty() => new();

        public static BlobArray Create(
            TokenResult tokenResult, SKPaint paint, SKFont font, float maxWidth, Padding padding)
        {
            var blobs = tokenResult.Tokens
                .Select(token => ShapeAndPlace(token, font, paint))
                .ToArray();

            var (size, lineWidths) = Measure(blobs, font, maxWidth, padding, tokenResult.IsRTL);

            return new BlobArray
            {
                Blobs = blobs,
                IsRTL = tokenResult.IsRTL,
                LineWidths = lineWidths,
                Size = size
            };
        }

        private static PositionedBlob ShapeAndPlace(string text, SKFont font, SKPaint paint)
        {
            if (text == "\n")
                return s_newLine;

            var typeface = font.Typeface ??
                throw new Exception("A Typeface is required at this point.");

            SKShaper? shaper = null;

            lock (s_lock)
            {
                if (!s_knownShapers.TryGetValue(typeface.FamilyName, out shaper))
                {
                    shaper = new SKShaper(typeface);
                    s_knownShapers[typeface.FamilyName] = shaper;
                }
            }

            var result = shaper.Shape(text, paint);
            var glyphs = Array.ConvertAll(result.Codepoints, cp => (ushort)cp);

            var builder = new SKTextBlobBuilder();
            var runBuffer = builder.AllocatePositionedRun(font, glyphs.Length);

            runBuffer.SetGlyphs(glyphs);
            runBuffer.SetPositions(result.Points);

            return new(builder.Build(), result.Width);
        }

        private static (LvcSize Size, List<float> LineWidths) Measure(
            PositionedBlob[] blobs, SKFont font, float maxWidth, Padding padding, bool isRTL)
        {
            var lineCount = 0;
            var x = 0f;
            var y = 0f;
            var knownWidth = 0f;
            var knownHeight = 0f;
            var metrics = font.Metrics;
            var lineHeight = metrics.Descent - metrics.Ascent + metrics.Leading;
            var widths = new List<float>();

            // For RTL text, reverse the blobs so words are positioned right-to-left
            var orderedBlobs = isRTL ? ((IEnumerable<PositionedBlob>)blobs).Reverse() : blobs;

            foreach (var pb in orderedBlobs)
            {
                pb.Line = lineCount;
                var b = pb.Blob;
                var w = pb.Width;

                if (x + w > maxWidth || pb == s_newLine)
                {
                    lineCount++;
                    widths.Add(x); // Store the width of the line, so we can use it later for alignment.
                    x = 0;
                    y += lineHeight;
                    knownHeight = y;
                }

                pb.Position = new SKPoint(x + padding.Left, y + padding.Top - metrics.Ascent);
                x += w;

                if (x > knownWidth)
                    knownWidth = x;
            }

            widths.Add(x);
            knownHeight += lineHeight;

            var size = new LvcSize(
                width: knownWidth + padding.Left + padding.Right,
                height: knownHeight + padding.Top + padding.Bottom);

            return (size, widths);
        }
    }

    internal class PositionedBlob(SKTextBlob blob, float width)
    {
        public int Line { get; set; }
        public SKPoint Position { get; set; }
        public float Width { get; } = width;
        public SKTextBlob Blob { get; } = blob;
#if DEBUG
        public string Text = string.Empty;
#endif
    }

    internal class TokenResult(string[] tokens, bool isRTL, SKTypeface? suggestedTypeface)
    {
        public string[] Tokens { get; } = tokens;
        public bool IsRTL { get; } = isRTL;
        public SKTypeface? SuggestedTypeface { get; } = suggestedTypeface;
    }
}
