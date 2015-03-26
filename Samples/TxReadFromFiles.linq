<Query Kind="Statements">
  <NuGetReference>Tx.AppInsights</NuGetReference>
  <Namespace>System.Reactive</Namespace>
  <Namespace>System.Reactive.Linq</Namespace>
</Query>

var playback = new Playback();

// Setup lookup folders 
playback.AddApplicationInsightsFiles(
	@"C:\AppInsights\fuelpoints_6017e369258e42cc8cee20cd1956c3d2",
	@"C:\AppInsights\app1_4e49badc530d4187acc4c42e5e7d9ebe");
	
// Define query
playback
	.GetObservable<Tx.ApplicationInsights.TelemetryType.PerformanceCounterEvent>()
	.SelectMany(ie => ie.PerformanceCounter)
	.Dump();

// Run the query
playback.Run();
