<Query Kind="Expression">
  <NuGetReference>Tx.ApplicationInsights</NuGetReference>
  <Namespace>Tx.ApplicationInsights</Namespace>
  <Namespace>Tx.ApplicationInsights.TelemetryType</Namespace>
</Query>

ApplicationInsightsFileStorage
	.ReadFromFiles<CustomEvent>(
		@"C:\Data\AppInsights\fuelpoints_6017e369258e42cc8cee20cd1956c3d2",
		@"C:\Data\AppInsights\app1_4e49badc530d4187acc4c42e5e7d9ebe")	
	.SelectMany(i => i.Event)