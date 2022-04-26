using System.Text.Json.Serialization;

namespace MK94.Assert
{
    public class AssertOperation
    {
        public OperationMode Mode { get; }

        public string Step { get; }

        public AssertOperation(OperationMode mode, string step)
        {
            Step = step;
            Mode = mode;
        }
    }

    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum OperationMode
    {
        Input,
        Output
    }
}
