using System.Collections.Concurrent;
using System.Threading.Tasks;
using System;
using System.Collections.Generic;

namespace CqrsMediatrExample.Extensions
{
    public static class TaskExtensions
    {
        private static readonly ConcurrentDictionary<WeakReference<Task>, object> TaskNames = new ConcurrentDictionary<WeakReference<Task>, object>();

        public static void Tag(this Task pTask, object pTag)
        {
            if (pTask == null) return;
            var weakReference = ContainsTask(pTask);
            if (weakReference == null)
            {
                weakReference = new WeakReference<Task>(pTask);
            }
            TaskNames[weakReference] = pTag;
        }

        public static object Tag(this Task pTask)
        {
            var weakReference = ContainsTask(pTask);
            if (weakReference == null) return null;
            return TaskNames[weakReference];
        }

        private static WeakReference<Task> ContainsTask(Task pTask)
        {
            if (!TaskNames.IsEmpty)
            {
                foreach (var weakReference in TaskNames.Keys)
                {
                    Task taskFromReference;
                    if (!weakReference.TryGetTarget(out taskFromReference))
                    {
                        TaskNames.Remove(weakReference, out object val);
                        continue;
                    }

                    if (pTask == taskFromReference)
                    {
                        return weakReference;
                    }
                }
            }
            return null;
        }
    }
}
