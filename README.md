
# DisruptorPlayground

**Methodology**

- Measure early and frequently

- Focus on critical paths

**Framework specifics**

- Beware of `Exception` throwing performance cost

- Avoid string manipulation, use `StringBuilder` or better alternative

- Avoid Reflection

	-  [Use `DynamicMethod`](https://youtu.be/-H5oEgOdO6U?t=2284)

- Careful with `foreach` loop (state machine allocation)

- Careful with `async` (state machine allocation)

**Beware of Lock contention**

- [See LMAX Disruptor `lock` vs atomic write vs single thread benchmarks](https://youtu.be/DCdGlxBbKU4?t=328)

- Queue induce a lot of contention

**Object Reuse**

- Object caching

**Object Pooling**

- Promote `ArrayPool`

- Promote `RecyclableMemoryStream`

- [Use object pool implementation](https://github.com/thomasraynal/DisruptorPlayground/blob/master/FalseSharing/ObjectPool.cs)


**Avoid Allocation**

-  `async` - use `ValueTask` when high probability to return the task result directly

	- [`ValueTask` all async call](https://blog.marcgravell.com/2019/08/prefer-valuetask-to-task-always-and.html)

- Avoid closures in lambda

- Avoid yield (state machine allocation)

- Avoid LINQ

- Promote allocation on stack - `stackalloc` and/or `Span<T>` for temporary local data storage

- Promote `ValueTuple`, being a `struct`

- Promote `struct`

	- Allocating a reference type has a cost, but passing it around is cheap, allocation a value type is cheap, but passing it around has a cost (which can be handled using a `struct` ByRef)

	- Thread safe

	- JIT optimization

	- Avoid boxing of value types

		- Use generics with constraints `Do<T> where T: ISomething` - `readonly struct TImplem: ISomething {}`, which is JIT optimization friendly

		- use `readonly`, `ref` and `in` keyword for flexibility, both as function parameter and return value signature

			-  `ref` : pass by reference, property modification allowed- use a `readonly struct` otherwise a defensive copy is created

			-  `in` => pass by reference, no property modification allowed - use a `readonly struct` otherwise a defensive copy is created

		- use `ref struct` definition to garantee that the struct will never be heap allocated