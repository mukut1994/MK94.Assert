using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MK94.Assert.NUnit.MatrixTest
{
    public class ScenarioTest
    {
        public static IEnumerable<TestCaseData> GetStageAsTest(IEnumerable<IScenario> scenarios, int stageNumber)
        {
            Dictionary<string, TestCaseData> toSkip = new Dictionary<string, TestCaseData>();

            foreach (var s in scenarios)
            {
                if (s.Stages.Count <= stageNumber)
                    continue;

                var stage = s.Stages[stageNumber];

                var tc = new TestCaseData(stage);

                var preStageName = stageNumber > 0 ? s.Stages.Take(stageNumber).Select(x => x.Name).Aggregate((a, b) => $"{a}, {b}") : "";
                var stageName = s.Stages.Take(stageNumber + 1).Select(x => x.Name).Aggregate((a, b) => $"{a}, {b}");

                if (toSkip.ContainsKey(stageName))
                {
                    toSkip[stageName].SetCategory(s.Name);
                    continue;
                }

                toSkip.Add(stageName, tc);

                tc.SetCategory(s.Name);
                tc.SetProperty("Pre", preStageName);
                tc.SetProperty("Post", stageName);
                tc.SetArgDisplayNames(s.Stages.Take(stageNumber + 1).Select(x => x.Name).ToArray());
            }

            return toSkip.Values;
        }

        public static T Assert<T>(T obj, string step)
        {
            return DiskAssert.Matches(obj, Path.Combine(TestContext.CurrentContext.Test.Properties["Post"].First() as string, step));
        }

        public static FileInput LoadFromStage<T>(Func<T, Task> a, string step)
        {
            return FileInput.FromTest(a, Path.Combine(TestContext.CurrentContext.Test.Properties["Pre"].First() as string, step));
        }
        public static FileInput LoadFromStage<T>(Action<T> a, string step)
        {
            return FileInput.FromTest(a, Path.Combine(TestContext.CurrentContext.Test.Properties["Pre"].First() as string, step));
        }
        public static FileInput LoadFromStage(Delegate a, string step)
        {
            return FileInput.FromTest(a, Path.Combine(TestContext.CurrentContext.Test.Properties["Pre"].First() as string, step));
        }
    }
}
