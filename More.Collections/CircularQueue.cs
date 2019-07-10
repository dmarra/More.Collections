using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace More.Collections {
    /// <summary>
    /// The CircularQueue is a form of queue where the items are cycled
    /// in a circular fashion. The list has no true beginning or end. The beginning
    /// is always defined as the current item, and the end is the previously
    /// visted one. 
    /// 
    /// When items are Enqueue()'d, they will always be added 
    ///  
    /// This is implimented as a forward only linked list.
    /// </summary>
    public class CircularQueue<T> : IEnumerable<T>, ICollection, IEnumerable  {       
        private CircularQueueNode<T> currentNode;
        private CircularQueueNode<T> previousNode;
        private CircularQueueNode<T> beginningNode;
        
        private object syncRoot;

        public class CircularQueueNode<T>{
            public T Item { get; set; }
            public CircularQueueNode<T> Next { get; set; }

            public CircularQueueNode(T item) {                
                this.Item = item;
            }
        }

        #region ENUMERATION
        [Serializable]
        ///<summary>
        ///Enumerates the elements of a CircularQueue
        ///</summary>            
        public struct Enumerator : IEnumerator<T>, IDisposable, IEnumerator {            
            CircularQueueNode<T> sNode;
            CircularQueueNode<T> cNode;

            internal Enumerator(CircularQueue<T> queue) {                            
                sNode = queue.currentNode;
                cNode = null;
            }

            public T Current {
                get { 
                    try {
                        if (cNode == null) {
                            throw new InvalidOperationException();
                        }
                        return cNode.Item;
                    } catch (IndexOutOfRangeException) {
                        throw new InvalidOperationException();
                    }
                }
            }                        

            object IEnumerator.Current {
                get {
                    try {
                        return (object)Current;
                    } catch {
                        throw;
                    }
                }
            }

            public bool MoveNext() {
                bool complete = false;
                if (cNode == null) {
                    cNode = sNode;
                    // the following indicates a queue of one item
                    complete = cNode.Next == sNode;

                } else {
                    cNode    = cNode.Next;
                    complete = cNode == sNode;
                }
                return !complete;
            }

            public void Reset() {                
            }

            void IDisposable.Dispose() {
                Reset();
            }
        }

        public IEnumerator<T> GetEnumerator() {
            return new CircularQueue<T>.Enumerator(this);
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
        /// Returns number of elements are in the queue
        /// </summary>
        /// <returns>The number of elements currently in the container</returns>
        public int Count { get; private set; }
        
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

        public T Current {
            get {
                return currentNode.Item;
            }
        }

        public bool IsBeginning {
            get {
                if (beginningNode == null) {
                    throw new InvalidOperationException("Must set a beginning before you may check for it");
                }
                return currentNode == beginningNode;
            }
        }
        #endregion

        #region CONSTRUCTORS

        public CircularQueue() {
            Count = 0;
        }               

        public CircularQueue(IEnumerable<T> source) : this() {
            if (source == null) {
                throw new ArgumentNullException();
            }            

            int size = 0;
            if (source.GetType().IsArray) {
                size = ((T[])source).Length;
            } else if (source is ICollection) {
                size = ((ICollection)source).Count;
            } else if (source is ICollection<T>) {
                size = ((ICollection<T>)source).Count;
            }

            if (size <= 0) {
                throw new ArgumentOutOfRangeException("Passed IEnumerable has no elements");
            }
           
            foreach (T item in source) {
                Enqueue(item);
            }
        }
        #endregion

        public void Enqueue(T item) {
            if (Count == 0) {
                currentNode      = new CircularQueueNode<T>(item);
                currentNode.Next = currentNode;

            } else if (Count == 1) {
                previousNode      = new CircularQueueNode<T>(item);
                previousNode.Next = currentNode;
                currentNode.Next  = previousNode;

            } else {
                CircularQueueNode<T> qn = new CircularQueueNode<T>(item);
                qn.Next                 = currentNode;
                previousNode.Next       = qn;
                previousNode            = qn;
            }
            Count++;
        }

        public T Dequeue() {
            if (Count == 1) {
                T retVal     = currentNode.Item;
                currentNode  = null;
                previousNode = null;
                ClearBeginning();

                Count--;
                return retVal;

            } else if(Count > 1) {   
                T retVal                    = currentNode.Item;
                CircularQueueNode<T> buffer = currentNode;
                currentNode                 = buffer.Next;                                        
                previousNode.Next           = currentNode;
                
                if (beginningNode != null) {
                    MarkBeginning();
                }

                buffer = null;
                Count--;
                return retVal;

            } else {
                throw new System.InvalidOperationException("The current CircularQueue is empty.");
            }
        }

        public T Next() {
            previousNode = currentNode;
            currentNode  = currentNode.Next;
            return currentNode.Item;
        }

        public T Peek() {
            return Current;
        }


        /// <summary>
        /// Marks the current node as the beginning. 
        /// 
        /// Even though circular queues have no true beginning, sometimes
        /// it is conveinent to mark an arbitrary position as the
        /// beginning to keep track of revolutions around the queue.
        /// 
        /// In the event that a node marked as the beginning node is Dequeue()'s
        /// (which effectivly removes it entirely), the beginning node
        /// will become the one directly before it
        /// </summary>
        public void MarkBeginning() {
            beginningNode = currentNode;
        }

        /// <summary>
        /// Clears the beginning node, making the queue again
        /// have no defined beginning or end
        /// </summary>
        public void ClearBeginning() {
            beginningNode = null;
        }
       

        public void CopyTo(T[] array, int index) {
            if (array == null) {
                throw new ArgumentNullException("Array is null");
            } else if(index < 0 || index > array.Length) {
                throw new ArgumentOutOfRangeException("Start array index is out of range");
            } else if (Count > array.Length - index) {
                throw new ArgumentException("Array index would overflow");
            }

            int i = 0;
            foreach (T item in this) {
                array[i] = item;
                i++;
            }            
        }

        public void CopyTo(Array array, int index) {
            CopyTo(array, index);
        } 
    }
}
