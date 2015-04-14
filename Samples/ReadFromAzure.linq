<Query Kind="Expression">
  <NuGetReference>Tx.ApplicationInsights.Azure</NuGetReference>
  <Namespace>Tx.ApplicationInsights.Azure</Namespace>
  <Namespace>Tx.ApplicationInsights.TelemetryType</Namespace>
</Query>

ApplicationInsightsAzureStorage
	.ReadFromFiles<CustomEvent>(
		@"DefaultEndpointsProtocol=https;AccountName=#accountName#;AccountKey=#accountKey#",
		@"#containerName#",
		@"C:\AppInsights\")
	.SelectMany(i => i.Event)