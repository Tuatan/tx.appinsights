namespace Tx.ApplicationInsights.InternalData
{
    public class Context
    {
        public ApplicationData Application;
        public AppInsightsData Data;
        public DeviceData Device;
        public OperationData Operation;
        public LocationData Location;
        public UserData User;
        public Session Session;
    }

    public class LocationData
    {
        public Coordinate Point;

        public string Clientip;

        public string Continent;

        public string Country;

        public string Province;

        public string City;
    }

    public class Coordinate
    {
        public double Lat;

        public double Lon;
    }

    public class OperationData
    {
        public string Id;

        public string Name;
    }

    public class UserData
    {
        public string AnonId;

        public string AnonAcquisitionDate;

        public string AuthAcquisitionDate;

        public string AccountAcquisitionDate;
    }

    public class DeviceData
    {
        public string Id;

        public string Type;

        public string Os;

        public string OsVersion;

        //public string vmName;

        public string OemName;

        public string DeviceModel;

        public string Netrwork;

        public string Locale;

        public string UserAgent;

        public string Browser;

        public string BrowserVersion;

        public ScreenResolutionData ScreenResolution;
    }

    public class ScreenResolutionData
    {
        public string Value;
    }
}