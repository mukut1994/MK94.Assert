using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks.Sources;

namespace MK94.Assert
{
    public struct Difference
    {
        public Difference(string location, string expected, string actual)
        {
            Location = location;
            Expected = expected;
            Actual = actual;
        }

        public string Location { get; set; }

        public string Expected { get; set; }

        public string Actual { get; set; }
    }

    public interface IDifferenceFormatter<in T>
    {
        IEnumerable<Difference> FindDifferences(T expected, T actual);
    }

    public class JsonDifferenceFormatter : IDifferenceFormatter<string>
    {
        public static JsonDifferenceFormatter Instance { get; } = new JsonDifferenceFormatter();

        public IEnumerable<Difference> FindDifferences(string expected, string actual)
        {
            var expectedJson = JsonDocument.Parse(expected);
            var actualJson = JsonDocument.Parse(actual);

            return FindDifferences("$", expectedJson.RootElement, actualJson.RootElement);
        }

        private IEnumerable<Difference> FindDifferences(string jsonPath, JsonElement expected, JsonElement actual)
        {
            if(expected.ValueKind == JsonValueKind.Object)
                return FindDifferencesInObject(jsonPath, expected, actual);

            if (expected.ValueKind == JsonValueKind.Array)
                return FindDifferencesInArray(jsonPath, expected, actual);

            if (expected.ValueKind != actual.ValueKind)
                return new[] { new Difference(jsonPath, expected.ValueKind.ToString(), actual.ValueKind.ToString()) };

            if (!expected.GetRawText().Equals(actual.GetRawText()))
                return new[] { new Difference(jsonPath, expected.GetRawText(), actual.GetRawText()) };

            return Array.Empty<Difference>();
        }

        private IEnumerable<Difference> FindDifferencesInArray(string jsonPath, JsonElement expected, JsonElement actual)
        {
            var expectedLength = expected.GetArrayLength();
            var actualLength = actual.GetArrayLength();

            if (expectedLength != actualLength)
                yield return new Difference(jsonPath, $"Length: {expectedLength}", $"Length: {actualLength}");

            var minLength = Math.Min(expectedLength, actualLength);

            for(var i = 0; i < minLength; i++)
            {
                var expectedItem = expected[i];
                var actualItem = actual[i];
                var path = $"{jsonPath}[{i}]";

                foreach (var diff in FindDifferences(path, expectedItem, actualItem))
                    yield return diff;
            }

            if (expectedLength == actualLength)
                yield break;

            var smallArrayLength = Math.Min(expectedLength, actualLength);
            var largeArrayLength = Math.Max(expectedLength, actualLength);
            var largerArray = expectedLength > actualLength ? expected : actual;

            for(int i = smallArrayLength; i < largeArrayLength; i++)
                yield return new Difference($"{jsonPath}[{i}]", $"Extra item {largerArray[i]}", "null");
        }

        private IEnumerable<Difference> FindDifferencesInObject(string jsonPath, JsonElement expected, JsonElement actual)
        {
            if(expected.ValueKind == JsonValueKind.Null ^ actual.ValueKind == JsonValueKind.Null)
            {
                yield return new Difference(jsonPath, expected.ValueKind == JsonValueKind.Null ? "null" : expected.ToString(), actual.ValueKind == JsonValueKind.Null ? "null" : actual.ToString());

                yield break;
            }

            var checkedProps = new HashSet<string>();

            foreach(var expectedProperty in expected.EnumerateObject())
            {
                checkedProps.Add(expectedProperty.Name);

                var path = $"{jsonPath}.{expectedProperty.Name}";

                if (!actual.TryGetProperty(expectedProperty.Name, out var actualProperty))
                    yield return new Difference(path, expectedProperty.Value.ToString(), "undefined");
                else
                {
                    foreach (var diff in FindDifferences(path, expectedProperty.Value, actualProperty))
                        yield return diff;
                }
            }

            foreach (var actualProperty in actual.EnumerateObject().Where(x => !checkedProps.Contains(x.Name)))
            {
                var path = $"{jsonPath}.{actualProperty.Name}";
                if (!expected.TryGetProperty(actualProperty.Name, out var expectedProperty))
                    yield return new Difference(path, "undefined", actualProperty.Value.ToString());
                else
                {
                    foreach (var diff in FindDifferences(path, expectedProperty, actualProperty.Value))
                        yield return diff;
                }
            }
        }
    }
}
