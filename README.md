# More.Collections

More.Collections is a library containing niche container types that follow the same design paradigm as the built-in collections in .NET

This mostly serves as a dump for any niche collections I have needed in my projects, so that I can easily import them into new projects. 

It contains the following containers:

## DropoutStack

A Dropout Stack is a form of stack where the size is fixed. When new elements
are pushed onto the stack, if the asdded element would exceed the defined 
capacity of the stack, the oldest element "drops out". In other words, the
oldest element willfall out of the bottom, and all old elements pushed down by
one in order to accomidate the new item.

A classic use case for such a container is to store undo history in an application.
The standard Stack would grow out of control, but a DropoutStack allows for
intelligent storage of user events without it growing out of control.   

This:

```csharp
        DropoutStack<int> myStack = new DropoutStack<int>(5);
    
        Console.Write("Pushing onto stack:\n");
        for(int i = 1; i <= 10; i++) {
            myStack.Push(i);
            Console.WriteLine("Pushed " + i);
        }
    
        Console.Write("\nPopping off stack:\n");
        do {
            Console.WriteLine("Popped: " + myStack.Pop());
        } while(myStack.Count > 0);
```
Produces the following output:
```        
        Pushing onto stack:
        Pushed 1
        Pushed 2
        Pushed 3
        Pushed 4
        Pushed 5
        Pushed 6
        Pushed 7
        Pushed 8
        Pushed 9
        Pushed 10
    
        Popping off stack:
        Popped: 10
        Popped: 9
        Popped: 8
        Popped: 7
        Popped: 6
```

## CircularQueue

The CircularQueue is a form of queue where the items are cycled
in a circular fashion. The list has no true beginning or end. The beginning
is always defined as the current item, and the end is the previously
visted one. 

When items are Enqueue()'d, they will always be added 
 
This is implimented as a forward only linked list.
