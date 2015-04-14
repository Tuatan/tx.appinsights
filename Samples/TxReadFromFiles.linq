<Query Kind="Statements">
  <NuGetReference>Tx.ApplicationInsights</NuGetReference>
  <Namespace>System.Reactive</Namespace>
  <Namespace>System.Reactive.Linq</Namespace>
</Query>

var playback = new Playback();

// Setup lookup folders 
playback.AddApplicationInsightsFiles(
	@"C:\Data\AppInsights\fuelpoints_6017e369258e42cc8cee20cd1956c3d2",
	@"C:\Data\AppInsights\app1_4e49badc530d4187acc4c42e5e7d9ebe");
	
// Define query
playback
	.GetObservable<Tx.ApplicationInsights.TelemetryType.ExceptionEvent>()
	.SelectMany(ie => ie.BasicException)
	.Dump();

// Run the query
playback.Run();