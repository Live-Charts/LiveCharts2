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

namespace LiveChartsCore.SkiaSharpView.Drawing.Geometries;

/// <summary>
/// Open/High/Low/Close I-bar geometry: a vertical line from High to Low with a
/// left tick at Open and a right tick at Close. Shares
/// <see cref="BaseCandlestickGeometry"/> motion properties with
/// <see cref="CandlestickGeometry"/> so it slots into any
/// <see cref="CoreFinancialSeries{TModel, TVisual, TLabel, TMiniatureGeometry}"/>
/// pipeline without changes to measurement or animation.
/// </summary>
public class OhlcGeometry : BaseCandlestickGeometry, IDrawnElement<SkiaSharpDrawingContext>
{
    /// <inheritdoc cref="IDrawnElement{TDrawingContext}.Draw(TDrawingContext)" />
    public virtual void Draw(SkiaSharpDrawingContext context)
    {
        var paint = context.ActiveSkiaPaint;

        var w = Width;
        var cx = X + w * 0.5f;
        var high = Y;
        var low = Low;
        var open = Open;
        var close = Close;

        // High → Low spine
        context.Canvas.DrawLine(cx, high, cx, low, paint);
        // Open tick (left half)
        context.Canvas.DrawLine(X, open, cx, open, paint);
        // Close tick (right half)
        context.Canvas.DrawLine(cx, close, X + w, close, paint);
    }
}
