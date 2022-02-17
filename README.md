# MK94.Assert
A library to assert code against previous runs of the code.  
Spend less time on fixing test code and spend more time on actual code.

MK94.Assert asserts the actual data in test runs against expected data on disk.  
Expected data can be updated by calling ```DiskAssert.EnableWriteMode();``` in the TestSetup or anywhere in code before any calls to ```DiskAssert.Matches```  

## Supported Testing Frameworks
Only NUnit projects are supported. 
|Testing Framework|Supported|
|-----------------|---------|
|NUnit            |✅       |
|xUnit            |❌       |
|MSTest           |❌       |

#NUnit
## Setup
Configure the library in OneTimeSetUp like so
```c#
[SetUpFixture]
public class GlobalSetup
{
  [OneTimeSetUp]
  public void Setup()
  {
    SetupDiskAssert.WithRecommendedSettings("MK94.Assert");
    
    // Remove the comment to update/fix tests
    // AssertConfigure.EnableWriteMode();
  }
}
```

## File Storage Options

By defult Files are stored in a "TestData/{Class Name}/{Test Name}/{Step Name}" pattern.  
A root.json file is placed at "TestData" to keep track of file hashes to help reduce disk wear.

"TestData" can be overridden via the second parameter in 
```c#
SetupDiskAssert.WithRecommendedSettings("MK94.Assert", "CustomTestData");
```

The {Class Name}/{Test Name} structure can be overridden by setting the Output on the DiskAsserter or calling  
```c#
SetupDiskAssert.WithRecommendedSettings("MK94.Assert", "CustomTestData").WithDeduplication();
// OR
myDiskAsserter.WithDeduplication();
// OR
myDiskAsserter.Output = new MyCustomOutputStructure(); // implements ITestOutput
```
WithDeduplication stores the files in a "TestData/{File content Hash}" structure. It's useful to save disk space if many tests output the same data by saving them only once. The root.json contains the actual file paths and their content hash.  
Because of the structure the files are less accessible and this mode is not generally recommended.

## Chain Unit tests to form an Integration Test

Unit tests are great because they allow small pieces of code to be debugged faster.  
Integration tests are great because they check the whole system works well together.  

MK94.Assert allows chaining Unit tests to effectively create integration tests with the benefits of both.

```C#
 [Test]
 public void TestCase1()
 {
   // Matches saves the file to disk. In TestCase2 we need this object as an input
   DiskAssert.Matches("Step 1", new TestObject { A = 1, B = 2, C = 3 });
 }

 [Test]
 public void TestCase2()
 {
     var inputs = DiskAssert
         .WithInputs()
         .From(TestCase1);

     // context is the same to the object in TestCase1
     // File type must be included in the name here
     var context = inputs.Read<TestObject>("Step 1.json"); 
 }
```
TestCase1 and TestCase2 can be run independently in parallel or individually (assuming TestCase1 has been run once in the past with EnableWriteMode).  
They do not share any data during test runs.

## Mocking with DiskAssert

```C#
// Setup a mock
// The first parameter should create the actual implementation type to be used in Production builds.
DiskAssert.Default
    .WithMocks()
    .Of<IDatabase>(() => new Database(), out var database);

// Initialise object to test via DI and call a method that needs access to the database
var controller = new WeatherController(database);

controller.GetWeather(); // Makes a call to database.SelectAll() internally
```

On runs with EnableWriteMode() 
 - The  actual object passed in via the first parameter are used
 - Calls are intercepted, arguments sent to the object and its result are recorded

On Subsequent runs
 - Check the method is called in the correct order
 - Check all the arguments match
 - Return the result from the previous run

