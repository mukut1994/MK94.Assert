using NUnit.Framework;
using NUnit.Framework.Internal;
using System;
using System.Collections;
using System.Collections.Generic;
using MK94.Assert;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.IO;
using NUnit.Framework.Interfaces;

[assembly: LevelOfParallelism(10)]

namespace MK94.Assert.NUnit.MatrixTest
{
    [SetUpFixture]
    public class Setup
    {
        [OneTimeSetUp]
        public static void Init()
        {
            AssertConfigure.IsDevEnvironment = true;
            AssertConfigure.EnableWriteMode();

            AssertConfigureHelper
                .WithBaseFolderRelativeToBinary("MK94.Assert", "TestData")
                .WithChecksumStructure(BasedOn.ClassNameTestName)
                .WithFolderStructure(BasedOn.ClassNameTestName);
        }

        [OneTimeTearDown]
        public static void Cleanup()
        {
            DiskAssert.Cleardown();
        }
    }

    public class Source
    {
        public static Stage2Args Arg1 = new Stage2Args { X = 5 };
        public static Stage2Args Arg2 = new Stage2Args { X = 7 };

        public static IEnumerable<Scenario> TestScnearios()
        {
            for (int i = 0; i < 10; i++)
            {
                for (int j = 0; j < 10; j++)
                {
                    yield return new Scenario
                    {
                        Name = $"Scenario {i} {j}",
                        Stages = new List<IStage>
                        {
                            new Stage1Args { Name = i.ToString(), X = i },
                            new Stage2Args { Name = j.ToString(), X = j }
                        },
                        Step1Args = new Stage1Args { Name = i.ToString(), X = i },
                        Step2Args = new Stage2Args { Name = j.ToString(), X = j }
                    };
                }
            }
        }

        public static IEnumerable<TestCaseData> Step1Cases()
        {
            return TestScnearios()
                .GroupBy(x => x.Step1Args.Name)
                .Select(x => x.First())
                .Select(x => new TestCaseData(x.Step1Args).SetArgDisplayNames(x.Step1Args.Name).SetProperty("Scenario", x.Name));
        }

        public static IEnumerable<TestCaseData> Step2Cases()
        {
            foreach (var t in TestScnearios())
            {
                var tc = new TestCaseData(t.Step2Args);

                tc.SetArgDisplayNames($"{t.Step1Args.Name} {t.Step2Args.Name}");

                tc.SetProperty("Scenario", t.Name);

                yield return tc;
            }

            // class = pipeline
            // method = stage
            // DiskAssert = step

            // return TestScnearios().Select(x => new TestCaseData(x.Step2Args).SetArgDisplayNames($"{x.Step1Args.StepName} {x.Step2Args.StepName}").SetProperty("Scenario", x.Name));
        }

        public static IEnumerable<TestCaseData> argsT()
        {
            return new[]
            {
                new TestCaseData(Arg1).SetArgDisplayNames("Case 1"),
                new TestCaseData(Arg2).SetArgDisplayNames("Case 2")
            };
        }

        public static IEnumerable<TestCaseData> Stage_Test1()
        {
            return ScenarioTest.GetStageAsTest(TestScnearios(), 0);
        }
        public static IEnumerable<TestCaseData> Stage_Test2()
        {
            return ScenarioTest.GetStageAsTest(TestScnearios(), 1);
        }

    }

    [Parallelizable]
    public class Tests
    {
        [Test]
        [Order(1)]
        [Parallelizable(ParallelScope.All)]
        [TestCaseSource(typeof(Source), nameof(Source.Stage_Test1))]
        public async Task Test1(Stage1Args arg)
        {
            ScenarioTest.Assert(arg, "state");
            await Task.Delay(1000);
        }

        [Test]
        [Order(2)]
        [Parallelizable(ParallelScope.All)]
        [TestCaseSource(typeof(Source), nameof(Source.Stage_Test2))]
        public async Task Test2(Stage2Args arg)
        {
            var d = ScenarioTest.LoadFromStage<Stage1Args>(Test1, "state").As<Stage1Args>();

            ScenarioTest.Assert(arg.X + d.X, "state");
            await Task.Delay(1000);
        }
    }

    public class AuthStageArgs : IStage
    {
        public string Name { get; set; }
        public string UserName { get; set; }
        public string Password { get; set; }
    }

    public class AddPizzaArgs : IStage
    {
        public string Name { get; set; }
        public Pizza Pizza { get; set; }
    }
    public class DeletePizzaArgs : IStage
    {
        public string Name { get; set; }
        public Pizza Pizza { get; set; }
    }

    public class Stages
    {
        public static AuthStageArgs BobAuth = new AuthStageArgs { Name = "Bob auth", UserName = "Bob", Password = "BobP" };
        public static AuthStageArgs BobAuthPass2 = new AuthStageArgs { Name = "Bob auth pass2", UserName = "Bob", Password = "BobP2" };
        public static AuthStageArgs AliceAuth = new AuthStageArgs { Name = "Alice auth pass2", UserName = "Alice", Password = "AliceP" };

        public static AddPizzaArgs AddPizzaM = new AddPizzaArgs { Name = "Add Margherita pizza", Pizza = Pizza.Margherita };
        public static AddPizzaArgs AddPizzaP = new AddPizzaArgs { Name = "Add Pizzageddon pizza", Pizza = Pizza.Pizzageddon };
        public static AddPizzaArgs AddPizzaT = new AddPizzaArgs { Name = "Add TheMozzarellaFellas pizza", Pizza = Pizza.TheMozzarellaFellas };

        public static DeletePizzaArgs DelPizzaM = new DeletePizzaArgs { Name = "Add Margherita pizza", Pizza = Pizza.Margherita };
        public static DeletePizzaArgs DelPizzaP = new DeletePizzaArgs { Name = "Add Pizzageddon pizza", Pizza = Pizza.Pizzageddon };
        public static DeletePizzaArgs DelPizzaT = new DeletePizzaArgs { Name = "Add TheMozzarellaFellas pizza", Pizza = Pizza.TheMozzarellaFellas };
    }

    [Parallelizable]
    public class PizzaApiPipeline
    {
        static List<Scenario> Scenarios = new List<Scenario>
        {
            new Scenario("Bob auth bad password", Stages.BobAuth, Stages.BobAuthPass2),
            new Scenario("Bob buys pizza M", Stages.BobAuth, Stages.BobAuth, Stages.AddPizzaM),
        };

        InMemoryDb db;
        WebApiForPizza api;

        [SetUp]
        public void Setup()
        {
            db = new InMemoryDb();
            api = new WebApiForPizza(db);
        }

        static IEnumerable<TestCaseData> RegisterCases = ScenarioTest.GetStageAsTest(Scenarios, 0);
        [Test]
        [Order(0)]
        [Parallelizable(ParallelScope.All)]
        [TestCaseSource(nameof(RegisterCases))]
        public void Register(AuthStageArgs args)
        {
            api.RegisterUser(args.UserName, args.Password);

            db.Store();
        }

        static IEnumerable<TestCaseData> LoginCases = ScenarioTest.GetStageAsTest(Scenarios, 1);
        [Test]
        [Order(1)]
        [Parallelizable(ParallelScope.All)]
        [TestCaseSource(nameof(LoginCases))]
        public void Login(AuthStageArgs args)
        {
            db.Load<AuthStageArgs>(Register);

            try
            {
                var token = api.Login(args.UserName, args.Password);
                ScenarioTest.Assert(token, "token");
            }
            catch (ArgumentException e)
            {
                ScenarioTest.Assert(e.Message, "exception");
            }

            db.Store();
        }

        static IEnumerable<TestCaseData> AddPizzaCases = ScenarioTest.GetStageAsTest(Scenarios, 2);
        [Test]
        [Order(2)]
        [Parallelizable(ParallelScope.All)]
        [TestCaseSource(nameof(AddPizzaCases))]
        public void AddPizza(AddPizzaArgs args)
        {
            db.Load<AuthStageArgs>(Login);

            var token = ScenarioTest.LoadFromStage<AuthStageArgs>(Login, "token").As<string>();

            api.AddPizza(token, args.Pizza);

            db.Store();
        }
    }

    [TestFixture]
    public class qwioneqowihjeoqwine
    {
        [X]
        public void Test1()
        {

        }
    }

    public class XAttribute : NUnitAttribute, ISimpleTestBuilder, IApplyToTest, IImplyFixture
    {
        public void ApplyToTest(Test test)
        {

        }

        public TestMethod BuildFrom(IMethodInfo method, Test suite)
        {
            var r = new TestMethod(method);

            r.Tests.Add(suite);

            return r;
        }
    }

    public interface IStage
    {
        string Name { get; }
    }

    public interface IScenario
    {
        public string Name { get; }
        public List<IStage> Stages { get; }
    }

    public class Scenario : IScenario
    {
        public string Name { get; set; }
        public Stage1Args Step1Args { get; set; }
        public Stage2Args Step2Args { get; set; }

        public List<IStage> Stages { get; set; }

        public Scenario() { }

        public Scenario(string name, params IStage[] stages)
        {
            Name = name;
            Stages = stages.ToList();
        }
    }

    public class Stage1Args : IStage
    {
        public int X{ get; set; }

        public string Name { get; set; }
    }

    public class Stage2Args : IStage
    {
        public int X { get; set; }
        public string Name { get; set; }
    }
}