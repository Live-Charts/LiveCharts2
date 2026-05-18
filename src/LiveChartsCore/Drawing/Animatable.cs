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

using System.Collections.Generic;
using LiveChartsCore.Motion;
using LiveChartsCore.VisualStates;

namespace LiveChartsCore.Drawing;

/// <inheritdoc cref="Animatable" />
public abstract class Animatable
{
    internal VisualStatesDictionary.StatesTracker? _statesTracker;
    internal static EmptyAnimatable Empty { get; } = new();

    /// <summary>
    /// Gets the <see cref="PropertyDefinition"/> collection in the <see cref="Animatable"/> type.
    /// </summary>
    public static Dictionary<string, PropertyDefinition> PropertyDefinitions { get; } = [];

    /// <summary>
    /// Gets or sets a value indicating whether this instance is valid, the instance is valid when all the
    /// motion properties in the object finished their animations.
    /// </summary>
    public bool IsValid { get; set; } = true;

    /// <summary>
    /// Gets or sets a value indicating whether this instance should be removed from the canvas when all the animations are completed.
    /// </summary>
    public bool RemoveOnCompleted { get; set; }

    /// <summary>
    /// Sets the transition for the specified properties.
    /// </summary>
    /// <param name="animation">The animation.</param>
    /// <param name="properties">
    /// The properties to animate, when no properties, all properties are selected.
    /// </param>
    public void SetTransition(Animation? animation, params PropertyDefinition[]? properties)
    {
        if (properties is null || properties.Length == 0)
        {
            foreach (var property in GetPropertyDefinitions().Values)
            {
                var motionProperty = property.GetMotion(this);
                if (motionProperty is null) continue;

                motionProperty.Animation = animation;
            }
            return;
        }

        foreach (var property in properties)
        {
            var motionProperty = property.GetMotion(this);
            if (motionProperty is null) continue;

            motionProperty.Animation = animation;
        }
    }

    /// <summary>
    /// Removes the transition for the specified properties.
    /// </summary>
    /// <param name="properties">The properties to remove, when no properties, all properties are selected.
    /// </param>
    public void RemoveTransition(params PropertyDefinition[]? properties)
    {
        if (properties is null || properties.Length == 0)
        {
            foreach (var property in GetPropertyDefinitions().Values)
            {
                var motionProperty = property.GetMotion(this);
                if (motionProperty is null) continue;

                motionProperty.Animation = null;
            }
            return;
        }

        foreach (var property in properties)
        {
            var motionProperty = property.GetMotion(this);
            if (motionProperty is null) continue;

            motionProperty.Animation = null;
        }
    }

    /// <summary>
    /// Completes the transition for the specified properties.
    /// </summary>
    /// <param name="properties">The properties to complete, when no properties, all properties are selected.
    /// </param>
    public virtual void CompleteTransition(params PropertyDefinition[]? properties)
    {
        if (properties is null || properties.Length == 0)
        {
            foreach (var property in GetPropertyDefinitions().Values)
            {
                var motionProperty = property.GetMotion(this);
                if (motionProperty is null) continue;

                motionProperty.Finish();
            }
            return;
        }

        foreach (var property in properties)
        {
            var motionProperty = property.GetMotion(this);
            if (motionProperty is null) continue;

            motionProperty.Finish();
        }
    }

    /// <summary>
    /// Gets the property definition by name.
    /// </summary>
    /// <param name="propertyName">The name of the property.</param>
    /// <returns>The property definition, null if not found.</returns>
    public PropertyDefinition? GetPropertyDefinition(string propertyName) =>
        GetPropertyDefinitions().TryGetValue(propertyName, out var property)
            ? property
            : null;

    /// <summary>
    /// Merges two dictionaries of property definitions into one.
    /// </summary>
    /// <param name="one">The first.</param>
    /// <param name="two">The second.</param>
    /// <returns>A new instance that contains both dictionaries.</returns>
    protected static Dictionary<string, PropertyDefinition> Merge(
        Dictionary<string, PropertyDefinition> one,
        Dictionary<string, PropertyDefinition> two)
    {
        var merged = new Dictionary<string, PropertyDefinition>(one);

        foreach (var item in two)
            merged[item.Key] = item.Value;

        return merged;
    }

    /// <summary>
    /// Gets the motion property definitions in the instance.
    /// </summary>
    /// <returns>The propertt definitions.</returns>
    protected virtual Dictionary<string, PropertyDefinition> GetPropertyDefinitions() => PropertyDefinitions;

    internal class EmptyAnimatable : Animatable { }
}
