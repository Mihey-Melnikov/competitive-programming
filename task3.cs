namespace ConsoleApp1;

public class ConcurrentStack<T> : IStack<T>
{
    private Node? _headNode;

    public void Push(T item)
    {
        var currentNode = new Node(item, _headNode, (_headNode?.Count ?? 0) + 1);
        var spinWait = new SpinWait();
        do
        {
            spinWait.SpinOnce(-1);
            currentNode.Next = _headNode;
        }
        while (Interlocked.CompareExchange(ref _headNode, currentNode, currentNode.Next) != currentNode.Next);
    }

    public bool TryPop(out T? item)
    {
        var spinWait = new SpinWait();
        while (true)
        {
            var prevNode = _headNode;
            if (prevNode == null)
            {
                item = default;
                return false;
            }
            if (Interlocked.CompareExchange(ref _headNode, prevNode.Next, prevNode) == prevNode)
            {
                item = prevNode.Value;
                return true;
            }
            spinWait.SpinOnce(-1);
        }
    }

    public int Count => _headNode?.Count ?? 0;

    private class Node
    {
        public T Value { get; }
        public Node? Next { get; set; }
        public int Count { get; }

        public Node(T value, Node? next, int count)
        {
            Value = value;
            Next = next;
            Count = count;
        }
    }
}
