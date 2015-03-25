namespace Tx.ApplicationInsights.InternalData
{
    public class ExceptionEventData
    {
        public string Id;

        public string TypeName;

        public string HandledAt;

        public int Count;

        public string Method;

        public string ExceptionType;

        public string Assembly;

        public string ProblemId;

        public string OuterExceptionType;

        public string OuterExceptionThrownAtMethod;

        public string OuterExceptionThrownAtAssembly;

        public string OuterExceptionMessage;

        public string FailedUserCodeMethod;

        public string FailedUserCodeAssembly;

        public string ExceptionGroup;

        public string OuterId;

        public string Message;

        public bool HasFullStack;

        public string Stack;

        public ExceptionStackData[] ParsedStack;
    }
}