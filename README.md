# MK94.Assert
A library to assert code against previous runs of the code

# Usage
Supports only NUnit projects at this point in time. Configure the library in OneTimeSetUp like so
```c#
[SetUpFixture]
public class GlobalSetup
{
  [OneTimeSetUp]
  public void Setup()
  {
    AssertConfigureHelper.WithRecommendedSettings("projectName", "TestData")
    
    // Remove the comment to update/fix tests
    // AssertConfigure.EnableWriteMode();
  }

}
```

