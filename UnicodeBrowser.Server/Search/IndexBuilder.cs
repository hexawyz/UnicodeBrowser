using System;
using System.Collections.Generic;

namespace UnicodeBrowser.Search
{
	internal sealed class IndexBuilder
    {
        private readonly Stack<Node.Builder[]> _nodeArrayStack = new Stack<Node.Builder[]>(40);
        private readonly Stack<List<FoundCodePoint>> _codePointListStack = new Stack<List<FoundCodePoint>>(100);
        private Node.Builder _rootNode;

        public Node.Builder[] GetNodeArray() => _nodeArrayStack.Count > 1 ? _nodeArrayStack.Pop() : new Node.Builder[37];

        public void ReleaseNodeArray(ref Node.Builder[] nodes)
        {
            Array.Clear(nodes, 0, nodes.Length);
            if (_nodeArrayStack.Count < 10)
            {
                _nodeArrayStack.Push(nodes);
            }
            nodes = null;
        }

        public List<FoundCodePoint> GetCodePointList() => _codePointListStack.Count > 1 ? _codePointListStack.Pop() : new List<FoundCodePoint>(20);

        public void ReleaseCodePointList(ref List<FoundCodePoint> list)
        {
            list.Clear();
            if (_codePointListStack.Count < 10)
            {
                _codePointListStack.Push(list);
            }
            list = null;
        }

        public void AddWord(string text, int codePoint)
        {
            _rootNode.AddWord(this, text, codePoint);
        }

        public Node Build()
        {
            return _rootNode.BuildAndDispose(this);
        }
    }
}
