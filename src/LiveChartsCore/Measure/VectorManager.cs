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
    private LinkedListNode<Segment>? _currentNode = list.First;

    public void AddConsecutiveSegment(Segment segment, bool followsPrevious)
    {
        // Segments arrive in ascending-Id order for a single pass. Keep the linked list
        // in the same order, reusing existing nodes when an instance already appears
        // (same reference) and evicting any node whose Id falls at or below the incoming
        // one but does not match by reference — those are stale.
        //
        // Post-condition on exit: _currentNode points at the first node that was NOT
        // touched by this call (i.e. the first node with Id > segment.Id), or null if
        // there is none. This lets TrimTail drop any trailing nodes after the final
        // segment in a sub-segment — and just as importantly, never drops a node that
        // we just added.

        // If a previous call (in a prior sub-segment) left us past the insertion point
        // for this segment, rewind so we can find or place it correctly.
        if (_currentNode is not null && segment.Id < _currentNode.Value.Id)
            _currentNode = list.First;

        while (_currentNode is not null && !ReferenceEquals(_currentNode.Value, segment))
        {
            // If this node belongs AFTER our segment (by Id), stop — our segment should
            // be inserted before it.
            if (_currentNode.Value.Id > segment.Id) break;

            // Id <= segment.Id and not the same instance → stale. If the Ids match, the
            // old node was the "slot" this point used to occupy; inherit its animation
            // state so the line slides smoothly to the new position.
            var next = _currentNode.Next;
            if (followsPrevious && _currentNode.Value.Id == segment.Id)
                segment.Copy(_currentNode.Value);
            list.Remove(_currentNode);
            _currentNode = next;
        }

        if (_currentNode is null)
        {
            // Appending at the tail.
            if (followsPrevious && list.Last is not null)
                segment.Follows(list.Last.Value);
            _ = list.AddLast(segment);
            // _currentNode stays null — nothing left after the tail.
        }
        else if (ReferenceEquals(_currentNode.Value, segment))
        {
            // Same instance already in place.
            _currentNode = _currentNode.Next;
        }
        else
        {
            // _currentNode.Value.Id > segment.Id — insert our segment before it and keep
            // _currentNode pointing at the same (as-yet-unprocessed) node.
            if (followsPrevious && _currentNode.Previous is not null)
                segment.Follows(_currentNode.Previous.Value);
            _ = list.AddBefore(_currentNode, segment);
        }
    }

    // Removes every node from _currentNode to the end of the list. Called after a
    // sub-segment is fully processed to discard segments that used to be in this path
    // but now belong to a different sub-segment (e.g. when a null gap was introduced).
    public void TrimTail()
    {
        while (_currentNode is not null)
        {
            var next = _currentNode.Next;
            list.Remove(_currentNode);
            _currentNode = next;
        }
    }
}
