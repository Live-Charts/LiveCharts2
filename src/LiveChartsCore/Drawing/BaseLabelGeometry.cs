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
using LiveChartsCore.Painting;
using LiveChartsCore.Generators;

namespace LiveChartsCore.Drawing;

/// <summary>
/// Defines a label geometry in the user interface.
/// </summary>
public abstract partial class BaseLabelGeometry : Animatable, IDrawnElement
{
    private IDrawnElement? _parent;

    /// <summary>
    /// Initializes a new instance of the <see cref="BaseLabelGeometry"/> class.
    /// </summary>
    protected BaseLabelGeometry()
    {
        _ScaleTransformMotionProperty = new(new(1f, 1f));
        _SkewTransformMotionProperty = new(new(1f, 1f));
        _OpacityMotionProperty = new(1f);
        _TextSizeMotionProperty = new(11f);
        _BackgroundMotionProperty = new(LvcColor.Empty);
        _PaddingMotionProperty = new(new(0f));
    }

    /// <inheritdoc cref="IDrawnElement.Parent"/>
    IDrawnElement? IDrawnElement.Parent { get => _parent; set => _parent = value; }

    /// <inheritdoc cref="IDrawnElement.Opacity"/>
    [MotionProperty]
    public partial float Opacity { get; set; }

    /// <inheritdoc cref="IDrawnElement.X"/>
    [MotionProperty(HasExplicitAcessors = true)]
    public partial float X
    {
        get => _parent is null
            ? _XMotionProperty.GetMovement(this)
            : _XMotionProperty.GetMovement(this) + _parent.X;
        set => _XMotionProperty.SetMovement(value, this);
    }

    /// <inheritdoc cref="IDrawnElement.Y"/>
    [MotionProperty(HasExplicitAcessors = true)]
    public partial float Y
    {
        get => _parent is null
            ? _YMotionProperty.GetMovement(this)
            : _YMotionProperty.GetMovement(this) + _parent.Y;
        set => _YMotionProperty.SetMovement(value, this);
    }

    /// <inheritdoc cref="IDrawnElement.TransformOrigin"/>
    [MotionProperty]
    public partial LvcPoint TransformOrigin { get; set; }

    /// <inheritdoc cref="IDrawnElement.TranslateTransform"/>
    [MotionProperty]
    public partial LvcPoint TranslateTransform { get; set; }
    partial void OnTranslateTransformChanged(LvcPoint value) => HasTransform = true;

    /// <inheritdoc cref="IDrawnElement.RotateTransform"/>
    [MotionProperty]
    public partial float RotateTransform { get; set; }
    partial void OnRotateTransformChanged(float value) => HasTransform = true;

    /// <inheritdoc cref="IDrawnElement.ScaleTransform"/>
    [MotionProperty]
    public partial LvcPoint ScaleTransform { get; set; }
    partial void OnScaleTransformChanged(LvcPoint value) => HasTransform = true;

    /// <inheritdoc cref="IDrawnElement.SkewTransform"/>
    [MotionProperty]
    public partial LvcPoint SkewTransform { get; set; }
    partial void OnSkewTransformChanged(LvcPoint value) => HasTransform = true;

    /// <inheritdoc cref="IDrawnElement.DropShadow"/>
    [MotionProperty]
    public partial LvcDropShadow? DropShadow { get; set; }

    /// <summary>
    /// Gets or sets the size of the text.
    /// </summary>
    [MotionProperty]
    public partial float TextSize { get; set; }

    /// <summary>
    /// Gets or sets the background color.
    /// </summary>
    [MotionProperty]
    public partial LvcColor Background { get; set; }

    /// <summary>
    /// Gets or sets the padding.
    /// </summary>
    [MotionProperty]
    public partial Padding Padding { get; set; }

    /// <inheritdoc cref="IDrawnElement.Paint"/>
    [MotionProperty(HasExplicitAcessors = true)]
    public partial Paint? Paint
    {
        get => _PaintMotionProperty.GetMovement(this);
        set
        {
            _PaintMotionProperty.SetMovement(value, this);
            value?.PaintStyle = PaintStyle.Text;
        }
    }

    /// <summary>
    /// Gets or sets the maximum width, when the text exceeds this width, it will be wrapped.
    /// </summary>
    public float MaxWidth { get; set; } = float.MaxValue;

    /// <inheritdoc cref="IDrawnElement.HasTransform"/>
    public bool HasTransform { get; protected set; }

    float IDrawnElement.StrokeThickness { get; set; } = 1;

    /// <inheritdoc cref="IDrawnElement.ClippingBounds"/>
    public LvcRectangle ClippingBounds { get; set; } = LvcRectangle.Unset;

    bool IDrawnElement.HasTranslate
    {
        get
        {
            var t = TranslateTransform;
            return t.X != 0 || t.Y != 0;
        }
    }

    bool IDrawnElement.HasScale
    {
        get
        {
            var s = ScaleTransform;
            return s.X != 1 || s.Y != 1;
        }
    }

    bool IDrawnElement.HasSkew
    {
        get
        {
            var s = SkewTransform;
            return s.X != 1 || s.Y != 1;
        }
    }

    bool IDrawnElement.HasRotation => Math.Abs(RotateTransform) > 0;

    /// <summary>
    /// Gets or sets the vertical align.
    /// </summary>
    /// <value>
    /// The vertical align.
    /// </value>
    public Align VerticalAlign { get; set; } = Align.Middle;

    /// <summary>
    /// Gets or sets the horizontal align.
    /// </summary>
    /// <value>
    /// The horizontal align.
    /// </value>
    public Align HorizontalAlign { get; set; } = Align.Middle;

    /// <summary>
    /// Gets or sets how each line of a multi-line label aligns horizontally inside the label
    /// block (the block itself is placed by <see cref="HorizontalAlign"/>). Default is
    /// <see cref="Align.Start"/>. Right-to-left text always aligns lines to the end.
    /// </summary>
    public Align LinesAlignment { get; set; } = Align.Start;

    /// <summary>
    /// Gets or sets the text.
    /// </summary>
    public string Text { get; set; } = string.Empty;

    internal static bool ShowDebugLines { get; set; }

    Paint? IDrawnElement.Stroke { get; set; }

    // quick hack...
    // draw labels using the fill property when activepaint is not null.
    Paint? IDrawnElement.Fill
    {
        get => Paint;
        set => Paint = value;
    }

    /// <inheritdoc cref="IDrawnElement.Measure()"/>
    public abstract LvcSize Measure();

    void IDrawnElement.DisposePaints()
    {
        ((IDrawnElement)this).Stroke?.DisposeTask();
        ((IDrawnElement)this).Fill?.DisposeTask();
        Paint?.DisposeTask();

        OnDisposed();
    }

    internal virtual void OnDisposed() { }
}
