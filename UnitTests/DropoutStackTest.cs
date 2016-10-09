using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using More.Collections;
using System.Collections;
using System.Collections.Generic;


namespace UnitTests {
    // We do not use the exception value a lot in these tests; we only
    // care they were thrown, so disable this warning
    #pragma warning disable 168

    [TestClass]
    public class DropoutStackTest {  
        [TestMethod]
        public void TestInitialization() {
            // base constructor
            DropoutStack<int> stackA = new DropoutStack<int>();
            Assert.IsTrue(stackA.Capacity == 4, "Did not initialize with expected default capacity");

            // size constructor
            DropoutStack<int> stackB = new DropoutStack<int>(10);
            Assert.IsTrue(stackB.Capacity == 10, "Did not initialize with desrired capactiy");

            // copy constructor
            List<int> intList = new List<int>();
            for (int i = 0; i < 10; i++) {
                intList.Add(i);
            }
            DropoutStack<int> stackC = new DropoutStack<int>(intList);
            Assert.IsTrue(stackC.Capacity == 10, "Did not initialize with capacity equaling count of passed enumeration");
            Assert.IsTrue(stackC.Count == 10, "Did not initialize with elements of passed enumeration");

            // exception test
            bool caught = false;
            try {
                stackB = new DropoutStack<int>(0);
            } catch (ArgumentOutOfRangeException ex) {
                caught = true;
            }
            Assert.IsTrue(caught, "Size constructor allowed a 0 or less capacity");

            caught = false;
            try {
                intList.Clear();
                stackC = new DropoutStack<int>(intList);
            } catch (ArgumentOutOfRangeException ex) {
                caught = true;
            }
            Assert.IsTrue(caught, "IEnumerable constructor allowed an IEnumerable that has a capacity of 0");
        }


        [TestMethod]
        public void TestOrder() {
            DropoutStack<int> dropStack = new DropoutStack<int>(5);
            for (int i = 0; i < 10; i++) {
                dropStack.Push(i);
            }

            // test order
            int lastVal = int.MaxValue;
            do {
                int thisVal = dropStack.Pop();
                Assert.IsTrue(thisVal < lastVal, "DropoutStack had an incorrect order of items while being Pop()ed");
                lastVal = thisVal;
            } while (dropStack.Count > 0);
        }


        [TestMethod]
        public void TestResize() {
            DropoutStack<int> dropStack = new DropoutStack<int>(5);            
                        
            // test resize up
            dropStack.Resize(10);
            Assert.IsTrue(dropStack.Count == 0, "DropoutStack was resized, but count showed more elements than were actually in use.");
            for (int i = 0; i < 10; i++) {
                dropStack.Push(i);
            }
            Assert.IsTrue(dropStack.Count == 10, "DropoutStack was resized, but count showed incorrect element count of " + dropStack.Count);
       
            // test resize down
            dropStack.Resize(5);
            do {
                int thisVal = dropStack.Pop();
                Assert.IsTrue(thisVal >= 5, "DropoutStack did not drop out correct elements when it was downsized");                
            } while (dropStack.Count > 0);

            // test resize 0
            bool caught = false;
            try {
                dropStack.Resize(0);
            } catch (ArgumentOutOfRangeException ex) {
                caught = true;
            }
            Assert.IsTrue(caught, "Resize allowed a resize to capacity of 0");
        }


        [TestMethod]
        public void TestPeek() {
            // test peek
            DropoutStack<int> dropStack = new DropoutStack<int>(5);
            bool caught = false;
            try {                
                dropStack.Peek();
            } catch (InvalidOperationException ex) {
                caught = true;
            }
            Assert.IsTrue(caught, "CopyTo() allowed a Peek() on a 0 count stack");

            dropStack.Push(1);
            Assert.IsTrue(dropStack.Peek() == 1, "DropoutStack peek was incorrect value.");
            Assert.IsTrue(dropStack.Count == 1, "DropoutStack Peek() removed element instead of just returning it");
            dropStack.Pop();
        }


        [TestMethod]        
        public void TestCopy() {
            DropoutStack<int> dropStack = new DropoutStack<int>(10);     

            // test copy
            for (int i = 0; i < 10; i++) {
                dropStack.Push(i);
            }
            int[] intArray = new int[10];            
            dropStack.CopyTo(intArray, 0);

            Assert.IsTrue(intArray.Length == 10, "DropoutStack CopyTo() failed");

            int lastVal = int.MaxValue;
            for (int i = intArray.Length - 1; i >= 0; i--) {               
                Assert.IsTrue(intArray[i] < lastVal, "DropoutStack had an incorrect order of items in array copy");
                lastVal = intArray[i];
            }        
   
            // text exceptions
            bool caught = false;
            try {
                int[] nullArray = null;
                dropStack.CopyTo(nullArray, 0);
            } catch (ArgumentNullException ex) {
                caught = true;
            }
            Assert.IsTrue(caught, "CopyTo() allowed a null array");

            caught = false;
            try {                
                dropStack.CopyTo(intArray, -1);
            } catch (ArgumentOutOfRangeException ex) {
                caught = true;
            }
            Assert.IsTrue(caught, "CopyTo() allowed a negative index");

            caught = false;
            try {                
                dropStack.CopyTo(intArray, 11);
            } catch (ArgumentOutOfRangeException ex) {
                caught = true;
            }
            Assert.IsTrue(caught, "CopyTo() allowed an out of range index");
            
            caught = false;
            dropStack.Resize(20);
            for (int i = 0; i < 10; i++) {
                dropStack.Push(i);
            }
            Assert.IsTrue(dropStack.Count == 20, "DropoutStack did not have expected number of elements. " + dropStack.ToString());
            try {
                dropStack.CopyTo(intArray, 0);
            } catch (ArgumentException ex) {
                caught = true;
            }
            Assert.IsTrue(caught, "CopyTo() allowed a copy to an array with an insufficient capacity");
        }


        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException), "Pop() allowed us to pop a 0 count stack")]
        public void TestPopException() {
            DropoutStack<int> dropStack = new DropoutStack<int>(1);
            dropStack.Pop();
        }
       
    }
}
