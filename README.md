Frontenac
=========

A .NET port of the [Tinkerpop Stack](http://www.tinkerpop.com/).

## News
* 2013-07-29 [VelocityDB](http://velocitydb.com/) now supports Frontenac through it's [VelocityGraph](https://github.com/VelocityDB/VelocityGraph) library. Available on NuGet.

## Credits
Special thanks to the authors and contributors of the original library:
* [alexaverbuch](http://www.github.com/alexaverbuch/)(Alex Averbuch)
* [bdeggleston](http://www.github.com/bdeggleston/)(Blake Eggleston)
* [BrynCooke](http://www.github.com/BrynCooke/)(Bryn Cooke)
* [espeed](http://www.github.com/espeed/)(James Thornton)
* [joshsh](http://www.github.com/joshsh/)(Joshua Shinavier)
* [mbroecheler](http://www.github.com/mbroecheler/)(Matthias Broecheler)
* [okram](http://www.github.com/okram/)(Marko A. Rodriguez)
* [pangloss](http://www.github.com/pangloss/)(Darrick Wiebe)
* [peterneubauer](http://www.github.com/peterneubauer/)(Peter Neubauer)
* [pierredewilde](http://www.github.com/pierredewilde/)(Pierre De Wilde)
* [spmallette](http://www.github.com/spmallette/)(stephen mallette)
* [xedin](http://www.github.com/xedin/)(Pavel Yaskevich)
* and others :)

## What is inside
For now, blueprints-core 2.3.0 has been ported. It is not production ready yet. It includes
* [Property Graph Model](https://github.com/tinkerpop/blueprints/wiki/Property-Graph-Model)
* Implementations
  * [TinkerGraph](https://github.com/tinkerpop/blueprints/wiki/TinkerGraph)
* Utilities
  * Import/Export
     * [GML Reader and Writer Library](https://github.com/tinkerpop/blueprints/wiki/GML-Reader-and-Writer-Library)
     * [GraphML Reader and Writer Library](https://github.com/tinkerpop/blueprints/wiki/GraphML-Reader-and-Writer-Library)
     * [GraphSON Reader and Writer Library](https://github.com/tinkerpop/blueprints/wiki/GraphSON-Reader-and-Writer-Library)
  * Wrappers
     * [Batch Implementation](https://github.com/tinkerpop/blueprints/wiki/Batch-Implementation)
     * [ReadOnly Implementation](https://github.com/tinkerpop/blueprints/wiki/ReadOnly-Implementation)
     * [Event Implementation](https://github.com/tinkerpop/blueprints/wiki/Event-Implementation)
     * [Partition Implementation](https://github.com/tinkerpop/blueprints/wiki/Partition-Implementation)
     * [Id Implementation](https://github.com/tinkerpop/blueprints/wiki/Id-Implementation)

The [Property Graph Model Test Suite](https://github.com/tinkerpop/blueprints/wiki/Property-Graph-Model-Test-Suite) is currently being ported. As of now, 357/357 tests pass. 

## What's Next?
There is still a lot more work that must be done in order to get a full stack implemented in .NET.
Making sure it's root component is solid is fundamental. This is why, before proceeding further on with the port of the other APIs, the test suite must be complete and more than the TinkerGraph implementation must pass through it.  

## Why that name?
This is in reference to the [Château Frontenac](http://en.wikipedia.org/wiki/Chateau_Frontenac) in Quebec City, Canada, because writing softwares is a bit like building castles.
