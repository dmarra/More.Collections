using System;
using Xunit;
using More.Collections;
using System.Collections;
using System.Collections.Generic;


namespace UnitTests {
    // We do not use the exception value a lot in these tests; we only
    // care they were thrown, so disable this warning
    #pragma warning disable 168
        
    public class DropoutStackTest {  
        [Fact]
        public void TestInitialization() {
            // base constructor
            DropoutStack<int> stackA = new DropoutStack<int>();
            Assert.True(stackA.Capacity == 4, "Did not initialize with expected default capacity");

            // size constructor
            DropoutStack<int> stackB = new DropoutStack<int>(10);
            Assert.True(stackB.Capacity == 10, "Did not initialize with desired capacity");

            // copy constructor
            List<int> intList = new List<int>();
            for (int i = 0; i < 10; i++) {
                intList.Add(i);
            }
            DropoutStack<int> stackC = new DropoutStack<int>(intList);
            Assert.True(stackC.Capacity == 10, "Did not initialize with capacity equaling count of passed enumeration");
            Assert.True(stackC.Count == 10, "Did not initialize with elements of passed enumeration");

            // exception test
            bool caught = false;
            try {
                stackB = new DropoutStack<int>(0);
            } catch (ArgumentOutOfRangeException ex) {
                caught = true;
            }
            Assert.True(caught, "Size constructor allowed a 0 or less capacity");

            caught = false;
            try {
                intList.Clear();
                stackC = new DropoutStack<int>(intList);
            } catch (ArgumentOutOfRangeException ex) {
                caught = true;
            }
            Assert.True(caught, "IEnumerable constructor allowed an IEnumerable that has a capacity of 0");
        }


        [Fact]
        public void TestOrder() {
            DropoutStack<int> dropStack = new DropoutStack<int>(5);
            for (int i = 0; i < 10; i++) {
                dropStack.Push(i);
            }

            // test order
            int lastVal = int.MaxValue;
            do {
                int thisVal = dropStack.Pop();
                Assert.True(thisVal < lastVal, "DropoutStack had an incorrect order of items while being Pop()ed");
                lastVal = thisVal;
            } while (dropStack.Count > 0);
        }


        [Fact]
        public void TestResize() {
            DropoutStack<int> dropStack = new DropoutStack<int>(5);            
                        
            // test resize up
            dropStack.Resize(10);
            Assert.True(dropStack.Count == 0, "DropoutStack was resized, but count showed more elements than were actually in use.");
            for (int i = 0; i < 10; i++) {
                dropStack.Push(i);
            }
            Assert.True(dropStack.Count == 10, "DropoutStack was resized, but count showed incorrect element count of " + dropStack.Count);
       
            // test resize down
            dropStack.Resize(5);
            do {
                int thisVal = dropStack.Pop();
                Assert.True(thisVal >= 5, "DropoutStack did not drop out correct elements when it was downsized");                
            } while (dropStack.Count > 0);

            // test resize 0
            bool caught = false;
            try {
                dropStack.Resize(0);
            } catch (ArgumentOutOfRangeException ex) {
                caught = true;
            }
            Assert.True(caught, "Resize allowed a resize to capacity of 0");
        }


        [Fact]
        public void TestPeek() {
            // test peek
            DropoutStack<int> dropStack = new DropoutStack<int>(5);
            bool caught = false;
            try {                
                dropStack.Peek();
            } catch (InvalidOperationException ex) {
                caught = true;
            }
            Assert.True(caught, "CopyTo() allowed a Peek() on a 0 count stack");

            dropStack.Push(1);
            Assert.True(dropStack.Peek() == 1, "DropoutStack peek was incorrect value.");
            Assert.True(dropStack.Count == 1, "DropoutStack Peek() removed element instead of just returning it");
            dropStack.Pop();
        }
			   

        [Fact]
        public void TestPopException() {
            DropoutStack<int> dropStack = new DropoutStack<int>(1);            
            Assert.Throws<InvalidOperationException>(() => dropStack.Pop());
        }


		[Fact]
		public void TestCopyToCopiesToCorrectIndex() {
			DropoutStack<int> dropStack = new DropoutStack<int>(5);     

            // test copy
            for (int i = 1; i <= 5; i++) {
                dropStack.Push(i);
            }

			int[] actual = new int[6] {-1, -1, -1, -1, -1, -1};
			dropStack.CopyTo(actual, 1);

			int[] expected = new int[6] {-1, 1, 2, 3, 4, 5};
			Assert.Equal(expected, actual);
		}

		[Fact]
		public void TestCopyToDetectsNullArray() {
			DropoutStack<int> dropStack = new DropoutStack<int>(5);     

            // test copy
            for (int i = 1; i <= 5; i++) {
                dropStack.Push(i);
            }

			int[] actual = null;
			Assert.Throws<ArgumentNullException>(() => dropStack.CopyTo(actual, 0));
		}

		[Fact]
		public void TestCopyToDetectsIndexOutOfBounds() {
			DropoutStack<int> dropStack = new DropoutStack<int>(5);     

            // test copy
            for (int i = 1; i <= 5; i++) {
                dropStack.Push(i);
            }

			int[] actual = new int[5];
			Assert.Throws<ArgumentOutOfRangeException>(() => dropStack.CopyTo(actual, 6));
		}
		
		[Fact]
		public void TestCopyToDetectsArrayOverflowWhenArrayIsNotLargeEnough() {
			DropoutStack<int> dropStack = new DropoutStack<int>(5);     

            // test copy
            for (int i = 1; i <= 5; i++) {
                dropStack.Push(i);
            }

			int[] actual = new int[4];
			Assert.Throws<ArgumentException>(() => dropStack.CopyTo(actual, 0));
		}	
		
		[Fact]
		public void TestCopyToDetectsArrayOverflowWhenIndexTooHigh() {
			DropoutStack<int> dropStack = new DropoutStack<int>(5);     

            // test copy
            for (int i = 1; i <= 5; i++) {
                dropStack.Push(i);
            }

			int[] actual = new int[10];
			Assert.Throws<ArgumentException>(() => dropStack.CopyTo(actual, 7));
		}
		
		[Fact]
		/// <summary>
		/// This tests a special case where the CopyTo(Array, int index) form
		/// is called. It ensures we pass it along to the correct overload, and
		/// not itself (something that was detected in static analysis)
		/// </summary>
		public void TestCopyToArrayTypeDoesNotRecurseInfinitely() {
			DropoutStack<int> dropStack = new DropoutStack<int>(5);     

            // test copy
            for (int i = 1; i <= 5; i++) {
                dropStack.Push(i);
            }


			Array subject = Array.CreateInstance(typeof(int), 5);
			dropStack.CopyTo(subject, 0);
			// NOTE: there is no real assertion here because if this fails, its pretty
			//       catastrophic
			Assert.True(true);
		}
       
    }
}
