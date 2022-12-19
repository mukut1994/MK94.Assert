using System.Text.Json;
using NUnit.Framework;

namespace MK94.Assert.NUnit.Test
{
    public class SerializerTest
    {
        [Test]
        public void CustomSerializerOptions()
        {
            var serializerOptions = new JsonSerializerOptions { WriteIndented = false };
            
            var asserter = SetupDiskAssert
                .InstanceWithRecommendedSettings("MK94.Assert", "CustomSerializer")
                .WithSerializer(x => JsonSerializer.Serialize(x, serializerOptions))
                .Build();

            var fileContent = new
            {
                Name = "Name",
                Text = "Text",
                Number = 1
            };

            asserter.Matches("Step 1", fileContent);
        }
        
        [Test]
        public void CustomSerializer_Newtonsoft()
        {
            var asserter = SetupDiskAssert
                .InstanceWithRecommendedSettings("MK94.Assert", "CustomSerializer")
                .WithSerializer(Newtonsoft.Json.JsonConvert.SerializeObject)
                .Build();

            var fileContent = new
            {
                Name = "Name",
                Text = "Text",
                Number = 1
            };

            asserter.Matches("Step 1", fileContent);
        }
    }
}