using CqrsMediatrExample.DataStore;
using CqrsMediatrExample.Queries;
using MediatR;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Threading;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using CqrsMediatrExample.Extensions;

namespace CqrsMediatrExample.Handlers
{
    public class GetHelloWorldHandler : IRequestHandler<GetHelloWorldQuery, string>
    {
        private readonly ILogger<GetHelloWorldHandler> _logger;

        public GetHelloWorldHandler(ILogger<GetHelloWorldHandler> logger) => _logger = logger;

        // Объект, с которым производятся операции
        public static string Data = "Hello world!";

        public async Task<string> Handle(GetHelloWorldQuery request,
            CancellationToken cancellationToken)
        {
            _logger.LogInformation("Request {@request} received", request);

            // Соблюдение потокобезопасности
            var semaphore = new SemaphoreSlim(1, 1);

            // Инициализация задач
            var taskOne = TaskOne(semaphore, cancellationToken);       
            var taskTwo = TaskTwo(semaphore, cancellationToken);

            /// Присвоение именных признаков посредством WeakReference <see cref="TaskExtensions"/>
            /// для лучшего восприятия кода
            taskOne.Tag(nameof(TaskOne));
            taskTwo.Tag(nameof(TaskTwo));

            var taskList = new List<Task>() { taskOne, taskTwo };

            // Ждём завершения всех задач
            await Task.WhenAll(taskList)
                .ContinueWith(result =>
                {
                    // Если задачи завершились неудачно, компенсируем изменения
                    if (result.IsFaulted || result.IsCanceled)
                    {
                        // Проверка состояния задач, компенсация изменений требуется только
                        // в случае, когда TaskOne завершилась успешно, а TaskTwo - нет
                        var faultedOne = taskList.Find(
                                x => (x.IsFaulted || x.IsCanceled) &&
                                nameof(TaskOne) == (string)x.Tag()
                            );

                        var faultedTwo = taskList.Find(
                                x => (x.IsFaulted || x.IsCanceled) &&
                                nameof(TaskTwo) == (string)x.Tag()                                
                            );
                        
                        if (faultedOne == null && faultedTwo != null)
                        {
                            // Выполняем компенсацию
                            semaphore.Wait();
                            Data = Data.Replace("Updated", "Compensated");
                            semaphore.Release();
                        }

                        // Бросаем AggregateException
                        throw result.Exception;
                    }
                }, 
                TaskContinuationOptions.AttachedToParent & 
                TaskContinuationOptions.ExecuteSynchronously);

            return Data;
        }

        private async Task TaskOne(SemaphoreSlim semaphore, CancellationToken cancellationToken)
        {
            await Task.Delay(TimeSpan.FromSeconds(2), cancellationToken);

            semaphore.Wait(cancellationToken);
            Data += " Updated";
            semaphore.Release();
        }

        private async Task TaskTwo(SemaphoreSlim semaphore, CancellationToken cancellationToken)
        {
            await Task.Delay(TimeSpan.FromSeconds(1), cancellationToken);

            throw new Exception("Exception Hello world!");
        }
    }
}
