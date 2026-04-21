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
        LinkedListNode<Segment>? replaceCandidate = null;
        List<LinkedListNode<Segment>>? deleteCandidates = null;

        if (_currentNode is not null && segment.Id < _currentNode.Value.Id)
        {
            // this is a special case, normally caused because the gaps changed (null points).
            // restart from the beginning to keep the linked list in sync.
            _currentNode = list.First;
        }

        // look for the segment in the list
        while (_currentNode is not null && _currentNode.Value != segment)
        {
            if (_currentNode.Value.Id == segment.Id)
            {
                // save this node, if we can not find the segment
                // but we found a node with the same id,
                // we have a candidate to do a replace.
                replaceCandidate = _currentNode;
            }

            deleteCandidates ??= [];
            deleteCandidates.Add(_currentNode);

            _currentNode = _currentNode?.Next;
        }

        if (_currentNode is null)
        {
            // if we did not find the segment
            // we have to options
            //   1. add it to the end of the list
            //   2. replace the node with the same id

            if (segment.Id <= list.Last?.Value.Id)
            {
                // at this point we know that the path contains this segment
                // but the instance changed, so we replace the node

                if (replaceCandidate is not null)
                {
                    if (followsPrevious)
                        segment.Copy(replaceCandidate.Value);

                    replaceCandidate.Value = segment;
                }

                _currentNode = replaceCandidate?.Next;
            }
            else
            {
                // this is a new segment

                if (followsPrevious && list.Last is not null)
                    segment.Follows(list.Last.Value);

                _currentNode = list.AddLast(segment);
            }
        }
        else
        {
            // we found the segment

            // lets clean any outdated segments
            // the deleteCandidates list contains all the segments
            // that are not used before the current segment
            foreach (var node in deleteCandidates ?? [])
                list.Remove(node);

            _currentNode = _currentNode.Next;
        }
    }

    /// <summary>
    /// Removes all segments that follow the last processed segment.
    /// Call this after a sub-segment is fully processed to discard segments
    /// that used to be in this path but now belong to a different sub-segment
    /// (e.g. when a new null gap is introduced in the data).
    /// </summary>
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
