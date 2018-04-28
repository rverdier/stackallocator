stackallocator
==============

Stackallocator is very tiny C# experimental project that explores the possibility to allocate and manipulate things like [.NET strings](https://github.com/rverdier/stackallocator/blob/master/README.md#stack-allocated-strings) and [arrays](https://github.com/rverdier/stackallocator/blob/master/README.md#stack-allocated-arrays) on the stack.

### Limitations of stackalloc

In an unsafe context, the `stackalloc` keywork in C# can be used to allocate a block of memory on the stack:

 ```csharp
var values = stackalloc int[] {3, 1, 2};
```

Even though the syntax looks very much like if you were instantiating a managed array of structs, the `stackalloc` instruction doesn't give you back an array reference. Instead, it gives you a raw typed pointer to that stack allocated block of memory (`values` is a `int*` in the previous sample).

Manipulating this data through a pointer usually makes a lot of sense in that context, but it restricts you quite a bit too.

Consider the previous example: you may have 3 integers allocated sequentially in memory like you would if you would have instanciated a `int[]` on the heap, but you can't directly take advantage of any code dealing with managed `int` arrays (like `Array.Sort()` for example).

For the same reason, if you allocate chars on the stack using `stackalloc`, you will get back a `char*`, but it won't be of any use in case you would like to call any method dealing with strings but only accepting managed references as inputs.

### Stackallocator to the rescue

This project provides a very hacky way to bypass these limitations, and to use stack allocated data as managed struct array references, as well as managed string references. It's more a POC than anything else really, and I would not recommand to use it seriously to anyone.

#### Stack allocated arrays

Instead of using the `stackalloc` keyword directly to allocated structs on the stack, you can use the generic static method `UnsafeStackAllocator.NewArrayOf`.

The API is a definately clunky but it allows you to specify the kind of data you want to allocate on the stack (type and length), and then give you a way to manipulate it through a made up managed array reference.

```csharp
UnsafeStackAllocator.NewArrayOf<int>(3, a =>
{
    // The lambda parameter 'a' is typed as a managed int array 
    // reference even though it points to the stack allocated memory.

    a[0] = 3;
    a[1] = 1;
    a[2] = 2;

    // Here you can use external code dealing with managed arrays
    Array.Sort(a);

    for (int i = 0; i < a.Length; i++)
    {
        Console.WriteLine(a[i]);
    }
});
```

#### Stack allocated strings

Most of the time, frameworks and libraries that need to deal with strings only accept managed string references, and very rarely provide entry points supporting raw pointer as inputs.

This is why the static `UnsafeStackAllocator` class also offers a no less clunky way to fool the world and to manipulate stack allocated data through managed string references.

```csharp
var asciiStringBytes = new byte [] { (byte)'f', (byte)'o', (byte)'o' };
var asciiDecoder = Encoding.ASCII.GetDecoder();

UnsafeStackAllocator.NewString(asciiStringBytes, asciiStringBytes.Length, asciiDecoder, s =>
{
    // Here you can use the 'asciiStringBytes' stack-allocated data through the 
    // lambda parameter 's' as a managed string reference.
    // It means that you're now able to call any external code
    // depending on managed strings without having to actually allocate these strings on the heap.
    Console.WriteLine(s);
});
```
