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
using System.Collections.Generic;
using LiveChartsCore.Drawing.Segments;
using LiveChartsCore.Generators;
using LiveChartsCore.Motion;
using LiveChartsCore.Painting;

namespace LiveChartsCore.Drawing;

/// <summary>
/// Defines an area geometry.
/// </summary>
public abstract partial class BaseVectorGeometry : Animatable, IDrawnElement
{
    private IDrawnElement? _parent;

    /// <summary>
    /// Initializes a new instance of the <see cref="BaseVectorGeometry"/> class.
    /// </summary>
    protected BaseVectorGeometry()
    {
        _TransformOriginMotionProperty = new(new(0.5f, 0.5f));
        _ScaleTransformMotionProperty = new(new(1f, 1f));
        _SkewTransformMotionProperty = new(new(1f, 1f));
        _OpacityMotionProperty = new(1f);
        _StrokeThicknessMotionProperty = new(1f);
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

    /// <inheritdoc cref="IDrawnElement.Stroke"/>
    [MotionProperty(HasExplicitAcessors = true)]
    public partial Paint? Stroke
    {
        get => _StrokeMotionProperty.GetMovement(this);
        set
        {
            value?.PaintStyle = PaintStyle.Stroke;
            _StrokeMotionProperty.SetMovement(value, this);
        }
    }

    /// <inheritdoc cref="IDrawnElement.Fill"/>
    [MotionProperty(HasExplicitAcessors = true)]
    public partial Paint? Fill
    {
        get => _FillMotionProperty.GetMovement(this);
        set
        {
            value?.PaintStyle = PaintStyle.Fill;
            _FillMotionProperty.SetMovement(value, this);
        }
    }

    /// <inheritdoc cref="IDrawnElement.DropShadow"/>
    [MotionProperty]
    public partial LvcDropShadow? DropShadow { get; set; }

    /// <inheritdoc cref="IDrawnElement.HasTransform"/>
    public bool HasTransform { get; protected set; }

    /// <inheritdoc cref="IDrawnElement.StrokeThickness"/>
    [MotionProperty]
    public partial float StrokeThickness { get; set; }

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

    Paint? IDrawnElement.Paint { get; set; }

    /// <summary>
    /// Gets the commands in the vector.
    /// </summary>
    public LinkedList<Segment> Commands { get; } = new();

    /// <summary>
    /// Gets or sets the closing method.
    /// </summary>
    public VectorClosingMethod ClosingMethod { get; set; }

    /// <summary>
    /// Gets or sets the pivot.
    /// </summary>
    [MotionProperty]
    public partial float Pivot { get; set; }

    /// <inheritdoc cref="Animatable.CompleteTransition(PropertyDefinition[])" />
    public override void CompleteTransition(params PropertyDefinition[]? properties)
    {
        foreach (var segment in Commands)
        {
            segment.CompleteTransition(properties);
        }

        base.CompleteTransition(properties);
    }

    /// <summary>
    /// Determines whether the specified point is inside this vector geometry.
    /// </summary>
    /// <param name="x">The x coordinate.</param>
    /// <param name="y">The y coordinate.</param>
    /// <returns><c>true</c> if the point is inside the geometry; otherwise, <c>false</c>.</returns>
    public virtual bool ContainsPoint(float x, float y) => false;

    /// <inheritdoc cref="IDrawnElement.Measure()" />
    public LvcSize Measure() => new();

    void IDrawnElement.DisposePaints()
    {
        Stroke?.DisposeTask();
        Fill?.DisposeTask();
        ((IDrawnElement)this).Paint?.DisposeTask();

        OnDisposed();
    }

    /// <summary>
    /// Subclass hook called when the geometry is being removed from a canvas. Use it to release
    /// any cached resources that aren't covered by paint disposal (e.g. native handles owned by
    /// the geometry itself). Mirrors <c>BaseLabelGeometry.OnDisposed</c>.
    /// </summary>
    internal virtual void OnDisposed() { }
}
