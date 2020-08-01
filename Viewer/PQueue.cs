using System;

namespace Simulator
{
  /// <summary>
  /// A priority queue using a heap based binary tree
  /// </summary>
  public class PriorityQueue
  {
    private Event[] Heap;
    private uint capacity;
    private uint count;

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="capacity">Initial event capacity</param>
    public PriorityQueue(uint capacity)
    {
      this.capacity = capacity;
      Heap = new Event[capacity];
      count = 0;
    }

    public Event Peek()
    {
      return Heap[1];
    }

    /// <summary>
    /// Remove and return the next event
    /// </summary>
    /// <returns></returns>
    public Event Dequeue()
    {
      Event data = Heap[1];

      if(count == 0)
        throw new ApplicationException("Queue underflow");
      if(count == 1)
      {
        count--;
        return data;
      }

      uint insertpoint = 1;

      while(true)
      {
        uint child, i;

        i = insertpoint;

        child = Left(insertpoint);
        if((child != 0) &&
           (Heap[child].CompareTo(Heap[count]) <0))
          i = child;
          
        Event left = Heap[child];
        child = Right(insertpoint);
        if((child != 0) &&
           (Heap[child].CompareTo(Heap[count]) <0) &&
           (Heap[child].CompareTo(left) < 0))
            i = child;
      
        if(i == insertpoint) // Both children smaller
        {
          // Found the right spot
          Heap[insertpoint] = Heap[count];
          break;
        }

        // Move to child and iterate
        Heap[insertpoint] = Heap[i];
        insertpoint = i;
      }
      count--;
      return data;
    }

    /// <summary>
    /// Queue up an event for later
    /// </summary>
    /// <param name="data"></param>
    public void Enqueue(Event data)
    {
      if(count >= capacity)
        Capacity = capacity * 2;

      uint insertpoint = ++count;

      while(true)
      {
        uint p = Parent(insertpoint);
        if(p == 0) break;
        if(Heap[p].CompareTo(data) < 0) break;
        Heap[insertpoint] = Heap[p];
        insertpoint = p;
      }
      Heap[insertpoint] = data;
    }

    /// <summary>
    /// The current event capacity
    /// </summary>
    public uint Capacity
    {
      get { return capacity; }
      set
      {
        if(capacity < count)
          throw new ArgumentException("Requested capacity is too small for content");
        Event [] newheap = new Event[value];
        Heap.CopyTo(newheap, 0);
        Heap = newheap;
        capacity = value;
      }
    }

    /// <summary>
    /// The number of outstanding events
    /// </summary>
    public uint Count
    { get { return count; } }

    /// <summary>
    /// Locate left child node
    /// </summary>
    /// <param name="node"></param>
    /// <returns></returns>
    private uint Left(uint node)
    {
      node = node << 1;
      return node < count ? node : 0;
    }

    /// <summary>
    /// Locate right child node
    /// </summary>
    /// <param name="node"></param>
    /// <returns></returns>
    private uint Right(uint node)
    {
      node = (node << 1) + 1;
      return node < count ? node : 0;
    }

    /// <summary>
    /// Locate parent node
    /// </summary>
    /// <param name="node"></param>
    /// <returns></returns>
    private uint Parent(uint node)
    {
      return node >> 1;
    }
  }
}
