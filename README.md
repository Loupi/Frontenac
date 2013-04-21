Frontenac
=========

A .NET port of the [Tinkerpop Stack](http://www.tinkerpop.com/ "Title").  

## Credits
Special thanks to the authors and contributors of the original library:
* [alexaverbuch](http://www.github.com/alexaverbuch/ "Title")(Alex Averbuch)
* [bdeggleston](http://www.github.com/bdeggleston/ "Title")(Blake Eggleston)
* [BrynCooke](http://www.github.com/BrynCooke/ "Title")(Bryn Cooke)
* [espeed](http://www.github.com/espeed/ "Title")(James Thornton)
* [joshsh](http://www.github.com/joshsh/ "Title")(Joshua Shinavier)
* [mbroecheler](http://www.github.com/mbroecheler/ "Title")(Matthias Broecheler)
* [okram](http://www.github.com/okram/ "Title")(Marko A. Rodriguez)
* [pangloss](http://www.github.com/pangloss/ "Title")(Darrick Wiebe)
* [peterneubauer](http://www.github.com/peterneubauer/ "Title")(Peter Neubauer)
* [pierredewilde](http://www.github.com/pierredewilde/ "Title")(Pierre De Wilde)
* [spmallette](http://www.github.com/spmallette/ "Title")(stephen mallette)
* [xedin](http://www.github.com/xedin/ "Title")(Pavel Yaskevich)
* and others :)

## What is inside
For now, blueprints-core 2.3.0 has been ported. It is not production ready yet. It includes
* [Property Graph Model](https://github.com/tinkerpop/blueprints/wiki/Property-Graph-Model "Title")
* Implementations
  * [TinkerGraph](https://github.com/tinkerpop/blueprints/wiki/TinkerGraph "Title")
* Utilities
  * Import/Export
     * [GML Reader and Writer Library](https://github.com/tinkerpop/blueprints/wiki/GML-Reader-and-Writer-Library "Title")
     * [GraphML Reader and Writer Library](https://github.com/tinkerpop/blueprints/wiki/GraphML-Reader-and-Writer-Library "Title")
     * [GraphSON Reader and Writer Library](https://github.com/tinkerpop/blueprints/wiki/GraphSON-Reader-and-Writer-Library "Title")
  * Wrappers
     * [Batch Implementation](https://github.com/tinkerpop/blueprints/wiki/Batch-Implementation "Title")
     * [ReadOnly Implementation](https://github.com/tinkerpop/blueprints/wiki/ReadOnly-Implementation "Title")
     * [Event Implementation](https://github.com/tinkerpop/blueprints/wiki/Event-Implementation "Title")
     * [Partition Implementation](https://github.com/tinkerpop/blueprints/wiki/Partition-Implementation "Title")
     * [Id Implementation](https://github.com/tinkerpop/blueprints/wiki/Id-Implementation "Title")

The [Property Graph Model Test Suite](https://github.com/tinkerpop/blueprints/wiki/Property-Graph-Model-Test-Suite "Title") is currently being ported. As of now, 229/229 tests pass. 

## What's Next?
There is still a lot more work that must be done in order to get a full stack implemented in .NET.
Making sure it's root component is solid is fundamental. This is why, before proceeding further on with the port of the other APIs, the test suite must be complete and more than the TinkerGraph implementation must pass through it.  

## Why that name?
This is in reference to the [Château Frontenac](http://en.wikipedia.org/wiki/Chateau_Frontenac "Title") in Quebec City, Canada, because writing softwares is a bit like building castles.
