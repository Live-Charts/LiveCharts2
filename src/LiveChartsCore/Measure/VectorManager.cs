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

using LiveChartsCore.Drawing.Segments;

namespace LiveChartsCore.Measure;

internal class VectorManager(LinkedList<Segment> list)
{
    // The last node this pass inserted, advanced-past, or matched. All subsequent work
    // starts from _lastTouched.Next — so the walk never revisits nodes we've already
    // handled (which is how a buggy rewind used to evict segments we had just added in
    // the same pass, the root of #2132 "yellow region swoops on resize"). TrimTail uses
    // it to drop only the tail after our last activity.
    private LinkedListNode<Segment>? _lastTouched;

    // Adds `segment` into the list, preserving ascending-Id order. `isNewSegment` says
    // whether the caller considers this segment freshly instantiated — i.e. with no
    // meaningful motion state yet. When true we seed the segment from a same-Id stale
    // predecessor if one exists (Copy) or from the previous tail (Follows), so the line
    // animates into place smoothly. When false the segment already carries live motion
    // state from a prior frame; we leave it alone and let the subsequent setter drive a
    // smooth transition from its current animated position to the new target.
    public void AddConsecutiveSegment(Segment segment, bool isNewSegment)
    {
        // Walk from just past our last activity. Segments arrive in ascending Id order,
        // so anything we see here with Id < segment.Id is stale content from a previous
        // frame and must be evicted. A node whose Id >= segment.Id (or the end of the
        // list) marks the insertion point.
        var current = _lastTouched is null ? list.First : _lastTouched.Next;

        while (current is not null)
        {
            if (ReferenceEquals(current.Value, segment))
            {
                // Already in place — nothing to do but record it for TrimTail.
                _lastTouched = current;
                return;
            }
            if (current.Value.Id >= segment.Id) break;

            // current.Id < segment.Id and not our ref → stale tail-between-matches.
            var next = current.Next;
            list.Remove(current);
            current = next;
        }

        // Same-Id, different ref: the slot this Id used to occupy belongs to a freshly
        // instantiated entity now. If the caller says this segment is new, copy the old
        // node's motion state so the animation slides smoothly to the new position;
        // then evict.
        var didCopy = false;
        if (current is not null && current.Value.Id == segment.Id)
        {
            if (isNewSegment)
            {
                segment.Copy(current.Value);
                didCopy = true;
            }
            var next = current.Next;
            list.Remove(current);
            current = next;
        }

        if (current is null)
        {
            // Appending at the tail. Only seed from the previous tail when this segment
            // is fresh and we didn't already inherit state via Copy.
            if (isNewSegment && !didCopy && list.Last is not null)
                segment.Follows(list.Last.Value);
            _lastTouched = list.AddLast(segment);
        }
        else
        {
            if (isNewSegment && !didCopy && current.Previous is not null)
                segment.Follows(current.Previous.Value);
            _lastTouched = list.AddBefore(current, segment);
        }
    }

    // Removes every node strictly after _lastTouched. Drops stale trailing entries from
    // an earlier render (e.g. segments that used to be in this sub-segment but no longer
    // belong here) without ever dropping a node we added or matched in this pass.
    public void TrimTail()
    {
        if (_lastTouched is null) return;
        while (_lastTouched.Next is not null)
            list.Remove(_lastTouched.Next);
        _lastTouched = null;
    }
}
