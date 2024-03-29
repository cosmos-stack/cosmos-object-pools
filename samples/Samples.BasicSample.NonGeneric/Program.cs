﻿using System;
using System.Threading;
using CosmosStack.Disposables.ObjectPools;
using CosmosStack.Disposables.ObjectPools.Pools;

namespace Samples.BasicSample.NonGeneric
{
    class Program
    {
        static void Main(string[] args)
        {
            var managedModelType = typeof(CustomModel);
            var type = typeof(SampleModel);

            //var pool = new ObjectPool(type, 10, CreateObject, OnGetObject);

            //ObjectPoolManager.Create("OK", 10, CreateObject, OnGetObject);

            // ObjectPoolManager.ManagedModels.Register(managedModelType);
            // ObjectPoolManager.ManagedModels.Create(managedModelType, type, "OK", 10, CreateObject, OnGetObject);

            ObjectPoolManager.NonGeneric.Managed(managedModelType).Register();
            ObjectPoolManager.NonGeneric.Managed(managedModelType).Create(type, "OK", 10, CreateObject, OnGetObject);

            //var pool = ObjectPoolManager.Get<SampleModel>();

            //var pool = ObjectPoolManager.ManagedModels.Get(managedModelType, type, "OK");

            var pool = ObjectPoolManager.NonGeneric.Managed(managedModelType).Get(type, "OK");

            for (var i = 0; i < 100; i++)
            {
                new Thread(() =>
                {
                    for (var j = 0; j < 1_000; j++)
                    {
                        var item = pool.Acquire();
                        //Console.WriteLine($"ThreadId={Thread.CurrentThread.ManagedThreadId}, Value={item.Value.Value}");
                        pool.Recycle(item);
                    }

                    Console.WriteLine(pool.GetStatisticsInfoFully());
                }).Start();
            }

            Console.WriteLine("Hello World!");
        }

        private static SampleModel CreateObject() => new SampleModel();

        private static void OnGetObject(ObjectPayload model)
        {
            if (DateTime.Now.Subtract(model.LastAcquiredTime).TotalSeconds > 3)
            {
                ((SampleModel) model.Value).Value += " +3sec";
            }
        }
    }

    public class SampleModel
    {
        public string Value { get; set; } = DateTime.Now.ToString("yyyy MM dd HH mm ss fff");
    }

    public class CustomModel : ObjectPoolManagedModel { }
}