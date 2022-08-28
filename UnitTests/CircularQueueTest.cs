using System;
using Xunit;
using More.Collections;


namespace UnitTests {
    // We do not use the exception value a lot in these tests; we only
    // care they were thrown, so disable this warning
    #pragma warning disable 168

    public class CircularQueueTest {
        [Fact]
        public void TestInitialization() {
            int[] testList = new int[] {1,2,3,4,5};
            CircularQueue<int> cq = new CircularQueue<int>(testList);
            Assert.True(cq.Count == 5, "Count is incorrect");
            Assert.True(cq.Current == 1, "Unexpected initial value. Expected 1, but value was " + cq.Current);
        }

        [Fact]
        public void TestEnumeration() {
            int[] testList = new int[] {1,2,3,4,5};
            CircularQueue<int> cq = new CircularQueue<int>(testList);

            // test foreach looping
            int current = cq.Current;
            int iterationCount = 1;
            foreach(int item in cq) {
                Assert.True(
                    current == item, 
                    "Unexpected value encountered during foreach; start at natural beginning. \n" +
                    "local current: " + current + "\n" +
                    "foreach item: " + item + "\n" +
                    "iteration count: " + iterationCount
                );
                current++;
                iterationCount++;
            }

            // test with a new beginning
            cq.Next();
            Assert.True(cq.Current == 2, "Unexpected current value; expected 2");
            bool first = true;
            current = 0;
            foreach (int item in cq) {
                if (first) {
                    Assert.True(item == 2, "Foreach did not start at expected item; started at " + item);
                    first = false;
                }
                current = item;
            }           
            Assert.True(current == 1, "Unexpected current value after foreach iteration. current: " + current);
        }

        [Fact]
        public void TestPeek() {
            int[] testList = new int[] {1,2,3,4,5};
            CircularQueue<int> cq = new CircularQueue<int>(testList);

            Assert.True(cq.Peek() == 1, "Unexpected Peek value; expected 1");
            cq.Next();
            Assert.True(cq.Peek() == 2, "Unexpected Peek value; expected 2, but value was " + cq.Peek());
        }


        [Fact]
        public void TestNext() {
            int[] testList = new int[] {1,2,3,4,5};
            CircularQueue<int> cq = new CircularQueue<int>(testList);

            cq.Next();
            Assert.True(cq.Peek() == 2, "Unexpected current value");

            cq.Enqueue(6);
            do {
                cq.Next();
            } while (cq.Peek() != 1);
            cq.Next();
            Assert.True(cq.Peek() == 6, "Value was not enqueued int he proper position after a Next() operation.");
        }


        [Fact]
        public void TestEnqueueDequeue() {
            CircularQueue<int> cq = new CircularQueue<int>();

            for (int i = 1; i <= 5; i++) {
                cq.Enqueue(i);
            }
            Assert.True(cq.Current == 1, "Unexpected current value after full enqueuing; expected 1");

            int item = cq.Dequeue();
            Assert.True(cq.Count == 4, "Unexpected count after dequeue");
            Assert.True(item == 1, "Dequeue() returned wrong item");
            Assert.True(cq.Current == 2, "Current is not correct after a Dequeue(); item was not advanced");

            for (int i = cq.Count; i > 0; i--) {
                cq.Dequeue();
            }
            Assert.True(cq.Count == 0, "Queue is not empty as expected.");

            bool caught = false;
            try {
                cq.Dequeue();               
            } catch (InvalidOperationException ex) {
                caught = true;
            }
            Assert.True(caught, "Expected exception was not throwns; Dequeue() allowed on 0 sized queue.");
        }


        [Fact]
        public void TestBeginnning() {
            int[] testList = new int[] {1,2,3,4,5};
            CircularQueue<int> cq = new CircularQueue<int>(testList);

            cq.Next();
            cq.Next();
            Assert.True(cq.Current == 3, "Unexpected current value; value is " + cq.Current);

            cq.MarkBeginning();
            Assert.True(cq.IsBeginning, "Beginning not marked correctly");

            int i = 0;
            do {
                cq.Next();
                i++;
            } while (!cq.IsBeginning && i < 6);

            Assert.True(i < 6, "IsBeginning detection failed; would have looped forever");
            Assert.True(cq.Current == 3, "Unexpected beginning");

            // test the shifting of the beginning due to a dequeue
            cq.Dequeue();
            Assert.True(cq.Current == 4, "Unexpected current value; value is " + cq.Current);
            Assert.True(cq.IsBeginning, "Did not reposition to new beginning");

        }


        [Fact]
        public void TestGC() {
            CircularQueue<int> cq = new CircularQueue<int>();

            for (int i = 1; i <= 5; i++) {
                cq.Enqueue(i);
            }


            WeakReference reference = null;
            new Action(
                () => {
                    int item = cq.Dequeue();  
                    reference = new WeakReference(item, true);
                }
            )();

            // Item should have gone out of scope about now, 
            // so the garbage collector can clean it up
            GC.Collect();
            GC.WaitForPendingFinalizers();

            Assert.Null(reference.Target);
        }
    }
}
