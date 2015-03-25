# LINQ to Application Insights traces

This library adds Application Insights support for Tx framework.

If you set up a continious integration feature in the Applocation Insights portal you will have a storage with exported data.

Using Application Insights Tx adaptor you can run any LINQ queries against files from that storage.

Currently 5 Application Insight types are supported:

* Requests
* Traces
* Events
* Performace Counters
* Exceptions.

