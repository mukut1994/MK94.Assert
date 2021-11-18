using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace MK94.Assert
{
    public class FileInput
    {
        private readonly string filePath;

        private FileInput(string filePath)
        {
            this.filePath = filePath;
        }

        public static FileInput FromTest(Delegate a, string step, string fileType = null)
        {
            var assertContext = new AssertContext(a.Method.DeclaringType.FullName, a.Method.Name, step);

            var path = AssertConfigure.GetOutputPath(step, fileType ?? "json", assertContext);

            if (!File.Exists(path))
                return null;

            return new FileInput(path);
        }

        public static FileInput FromTest<T>(Action<T> a, string step, string fileType = null) => FromTest((Delegate) a, step, fileType);
        public static FileInput FromTest<T>(Func<T, Task> a, string step, string fileType = null) => FromTest((Delegate) a, step, fileType);

        public T As<T>()
        {
            return JsonSerializer.Deserialize<T>(File.ReadAllText(filePath));
        }
        /*
        public static FileInput FromTest<T>(Action<T> a)
        {
            // var name = method.Method.Name;

            return null;
        }*/
        /*
        public static FileInput FromTest(MethodInfo m)
        {
            return null;
        }*/
    }
}
