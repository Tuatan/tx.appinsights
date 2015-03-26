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

## Samples

LinqPad samples are located in [Samples](./Samples/) folder.

## Nugets

The library is splitted into two nugets:

- [Tx.AppInsights.Azure](http://www.nuget.org/packages/Tx.AppInsights.Azure/) allows you to query Azure Blob, dump files on disk and create queries against that. 

- [Tx.AppInsights](http://www.nuget.org/packages/Tx.AppInsights/) allows you to do the same but as an input requires location of files preloaded from the Azure Blob
