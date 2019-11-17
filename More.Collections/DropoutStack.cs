using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Threading;


namespace More.Collections {
    ///<summary>
    /// A Dropout Stack is a form of stack where the size is fixed. When new elements
    /// are pushed onto the stack, if the asdded element would exceed the defined 
    /// capacity of the stack, the oldest element "drops out". In other words, the
    /// oldest element willfall out of the bottom, and all old elements pushed down by
    /// one in order to accomidate the new item.
    /// 
    /// A classic use case for such a container is to store undo history in an application.
    /// The standard Stack would grow out of control, but a DropoutStack allows for
    /// intelligent storage of user events without it growing out of control.   
    /// </summary>
    /// <example> 
    ///     <code>
    ///         DropoutStack&lt;int&gt; myStack = new DropoutStack&lt;int&gt;(5);
    ///     
    ///         Console.Write("Pushing onto stack:\n");
    ///         for(int i = 1; i &lt;= 10; i++) {
    ///             myStack.Push(i);
    ///             Console.WriteLine("Pushed " + i);
    ///         }
    ///     
    ///         Console.Write("\nPopping off stack:\n");
    ///         do {
    ///             Console.WriteLine("Popped: " + myStack.Pop());
    ///         } while(myStack.Count $gt; 0);
    ///     
    ///         // Output:
    ///         Pushing onto stack:
    ///         Pushed 1
    ///         Pushed 2
    ///         Pushed 3
    ///         Pushed 4
    ///         Pushed 5
    ///         Pushed 6
    ///         Pushed 7
    ///         Pushed 8
    ///         Pushed 9
    ///         Pushed 10
    ///     
    ///         Popping off stack:
    ///         Popped: 10
    ///         Popped: 9
    ///         Popped: 8
    ///         Popped: 7
    ///         Popped: 6
    ///     </code>
    /// </example>
    /// <typeparam name="T">The type of object you want to store</typeparam>
    [DebuggerDisplay("Count = {Count}, Capacity = {Capacity}"), DebuggerTypeProxy(typeof(System_DropoutStackDebugView<>)), ComVisible(false)]
    [Serializable]
    public class DropoutStack<T> : IEnumerable<T>, ICollection, IEnumerable {
        private T[] items;
        private object syncRoot;

        #region ENUMERATION
        [Serializable]
        ///<summary>
        ///Enumerates the elements of a DropoutStack
        ///</summary>            
        public struct Enumerator : IEnumerator<T>, IDisposable, IEnumerator, IEquatable<T> {
            DropoutStack<T> stack;
            int position;

            internal Enumerator(DropoutStack<T> stack) {
                this.stack = stack;
                position   = -1;
            }

            public T Current {
                get { 
                    try {
                        return stack.items[position];
                    } catch (IndexOutOfRangeException) {
                        throw new InvalidOperationException();
                    }
                }
            }            
            

            object IEnumerator.Current {
                get {
                    return (object)Current;                    
                }
            }


            public bool Equals(T other) {
                return Current.Equals(other);
            }

            public bool Equals(Enumerator other) {
                return stack.Equals(other.stack) && position == other.position;
            }

            public static bool operator ==(DropoutStack<T>.Enumerator e1,
                                           DropoutStack<T>.Enumerator e2 ) {
                return e1.Equals(e2);
            }

            public static bool operator !=(DropoutStack<T>.Enumerator e1,
                                           DropoutStack<T>.Enumerator e2 ) {
                return !e1.Equals(e2);
            }


            public bool MoveNext() {
                position++;
                return (position < stack.Count);
            }

            public void Reset() {
                position = -1;
            }

            void IDisposable.Dispose() {
                Reset();
            }
        }


        public DropoutStack<T>.Enumerator GetEnumerator() {
            return new DropoutStack<T>.Enumerator(this);
        }

        IEnumerator IEnumerable.GetEnumerator() {
            return GetEnumerator();
        }

        IEnumerator<T> IEnumerable<T>.GetEnumerator() {
            return GetEnumerator();
        }
        #endregion


        #region PROPERTIES
        /// <summary>
        /// Returns number of elements are in the stack
        /// </summary>
        /// <returns>The number of elements currently in the container</returns>
        public int Count { get; private set; }
        
        /// <summary>
        /// Returns the capacity of the stack. Elements that get pushed beyond the capacity
        /// of a DropoutStack will cause the oldest element to "drop out"
        /// </summary>
        /// <returns>The capacity of the container</returns>
        public int Capacity { get; private set; }

        /// <summary>
        /// Indicates whether access to the container is thread-safe. This is always false.
        /// Make sure to lock the container (or container.SyncRoot) before use if used accorss threads
        /// </summary>
        /// <returns>false</returns>
        public bool IsSynchronized {
            get { return false; }
        }

        /// <summary>
        /// Gets an object that can be used to synchronize access to the <see cref="T:System.Collections.ICollection" />.
        /// </summary>
		/// <returns>
        /// An object that can be used to synchronize access to the <see cref="T:System.Collections.ICollection" />.          
        /// </returns>		
        public object SyncRoot {
            get {
                if (this.syncRoot == null) {
					Interlocked.CompareExchange(ref this.syncRoot, new object(), null);
				}
				return this.syncRoot;
            }
        }
        #endregion


        #region CONSTRUCTORS
        /// <summary>
        /// Base constructor; defaults to a capacity of 4
        /// </summary>
        public DropoutStack() : this(4) {            
        }

        /// <summary>
        /// Constructor whch allows setting of capacity
        /// </summary>
        /// <param name="size">Desired capacity of the stack</param>
        public DropoutStack(int size) {    
            Init(size);                                
        }

        /// <summary>
        /// Constructor that creates a DropoutStack from another enumerable.
        /// The created DropoutStack will have a capacity equal to the element
        /// count of the source enumerable.
        /// </summary>
        /// <exception cref="T:System.ArgumentNullException">
        ///     <paramref name="source" /> is null.
        /// </exception>
        /// <param name="source">Container of type IEnumerable to fill with</param>
        public DropoutStack(IEnumerable<T> source) {
            if (source == null) {
                throw new ArgumentNullException();
            }            

            int size = 0;
            if (source.GetType().IsArray) {
                size = ((T[])source).Length;
            } else if (source is ICollection || source is ICollection<T>) {
                size = ((ICollection)source).Count;
            }

            if (size <= 0) {
                throw new ArgumentOutOfRangeException("Passed IEnumerable has no elements");
            }

            Init(size);
            int i = 0;
            foreach (T item in source) {
                items[i] = item;
                i++;
            }
            Count = size;
        }
        #endregion


        #region PRIVATE_HELPERS
        private void Init(int size) {
            if (size <= 0) {
                throw new ArgumentOutOfRangeException("Capacity must be positive, and non-zero");
            }

            this.Capacity = size;
            items = new T[size];
        }
        #endregion

        /// <summary>
        /// Allows for a resize of the capacity. If the requested size 
        /// is smaller than the current capacity, any elements that do not
        /// fit will drop out of the bottom.
        /// </summary>
        /// <param name="newCapacity">Desired size of the DropoutStack</param>
        public void Resize(int newCapacity) {
            if (newCapacity <= 0) {
                throw new ArgumentOutOfRangeException("Capacity must be positive, and non-zero");
            }
            Capacity = newCapacity;

            T[] newItems = new T[Capacity];            
            int newCount = (Count < Capacity) ? Count : Capacity;
            Array.Copy(items, Count - newCount, newItems, 0, newCount);

            Count = newCount;
            items = newItems;
        }

        /// <summary>
        /// Clears all items from stack.
        /// </summary>
        public void Clear() {
            for (int i = 0; i < Count; i++) {
                items[i] = default(T);
            }
            Count = 0;
        }

        /// <summary>
        /// Checks to see if an item is contained in the stack.
        /// </summary>
        /// <param name="item">item to look for</param>
        /// <returns>true if found; false otherwise</returns>
        public bool Contains(T item) {
            for (int i = 0; i < Count; i++) {
                if (EqualityComparer<T>.Default.Equals(items[i], item)) {
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// Copies the elements of this stack to the specified array, starting
        /// at the passed array index. 
        /// </summary>
        /// <exception cref="T:System.ArgumentNullException">
        ///     <paramref name="array"/> is null
        /// </exception>
        /// <exception cref="T:System.ArgumentOutOfRangeException">
        ///     <paramref name="arrayIndex"/> index is out of range
        /// </exception>
        /// <exception cref="T:System.ArgumentException">
        ///     <paramref name="arrayIndex"/> is set to a value that would cause an overflow of <paramref name="array"/>
        /// </exception>
        /// <param name="array">array to copy to</param>
        /// <param name="arrayIndex">The index in the array to start the copying on</param>
        public void CopyTo(T[] array, int arrayIndex) {
            if (array == null) {
                throw new ArgumentNullException("Array is null");
            } else if(arrayIndex < 0 || arrayIndex > array.Length) {
                throw new ArgumentOutOfRangeException("Start array index is out of range");
            } else if (Count > array.Length - arrayIndex) {
                throw new ArgumentException("Array index would overflow");
            }

            for (int i = 0; i < Count; i++) {
                array[arrayIndex + i] = items[i];
            }
        }

        /// <summary>
        /// Returns the next element without removing it from the stack.
        /// </summary>
        /// <exception cref="T:System.InvalidOperationException"> Count is zero </exception>
        /// <returns>Next item the stack (result of Pop(), without the pop)</returns>
        public T Peek() {
            if (Count == 0) {
                throw new InvalidOperationException("DropoutStack is empty");
            }
            return items[0];
        }
                
        /// <summary>
        /// Returns the next element in the stack, and removes it
        /// </summary>
        /// <exception cref="T:System.InvalidOperationException"> count is zero</exception>
        /// <returns>Next element</returns>
        public T Pop() {
            if (Count == 0) {
                throw new InvalidOperationException("DropoutStack is empty");
            }

            T item = items[Count - 1];
            Count--;            
            return item;
        }

        /// <summary>
        /// Adds an item to the top of the stack. If the item woudl cause the 
        /// stack to exceed capacity, the oldest element atthe bottom of the stack
        /// will "drop out"
        /// </summary>
        /// <param name="item">Item to add to stack</param>
        public void Push(T item) {
            if (Count == Capacity) {
                T[] buffer = new T[Capacity];
                Array.Copy(items, 1, buffer, 0, Capacity - 1);
            } else {
                Count++;
            }
            items[Count - 1] = item;            
        }

        /// <summary>
        /// Returns a copy in array format
        /// </summary>
        /// <returns>Copy of stack in array format</returns>
        public T[] ToArray() {
            T[] items = new T[Count];
            CopyTo(items, 0);
            return items;
        }
            
        public override string ToString() {
            return "DropoutStack<" + typeof(T).ToString() + "> {Count: " + Count + ", Capacity: " + Capacity + "}";
        }


        public void CopyTo(Array array, int index) {
            CopyTo(array, index);
        }        
    }
}