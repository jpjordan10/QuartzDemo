using System;
using System.Threading.Tasks;
using System.Collections.Specialized;
using Quartz;
using Quartz.Impl;
using Quartz.Logging;

namespace QuartzDemo
{
    public class UserTrafficSample
    {
        private static void Main(string[] args)
        {
            LogProvider.SetCurrentLogProvider(new ConsoleLogProvider());

            RunExample().GetAwaiter().GetResult();

            Console.WriteLine("Application is over, please press any key to close the application.");
            Console.ReadKey();
        }

        private static async Task RunExample()
        {
            try
            {
                // Grab the Scheduler instance from the Factory
                NameValueCollection props = new NameValueCollection
                {
                    { "quartz.serializer.type", "binary" }
                };
                StdSchedulerFactory factory = new StdSchedulerFactory(props);

                //Get the scheduler
                IScheduler scheduler = await factory.GetScheduler();

                // and start it off
                await scheduler.Start();

                // define the job and tie it to our HelloJob class

                IJobDetail job1 = JobBuilder.Create<UserTraffic>()
                    .WithIdentity("job1", "group1")
                    .UsingJobData("message1", "Message from Job1")
                    .Build();

                IJobDetail job2 = JobBuilder.Create<UserTraffic>()
                    .WithIdentity("job2", "group2")
                    .UsingJobData("message2", "Message from Job2")
                    .Build();

                /*
                 Run the job with the time defined
                 */

                // Trigger the job to run now, and then repeat every 5 seconds
                ITrigger trigger1 = TriggerBuilder.Create()
                    .WithIdentity("trigger1", "group1")
                    .StartNow()
                    .WithSimpleSchedule(x => x.WithIntervalInSeconds(5).RepeatForever())
                    .Build();

                ITrigger trigger2 = TriggerBuilder.Create()
                    .WithIdentity("trigger2", "group2")
                    .StartNow()
                    .WithSimpleSchedule(x => x.WithIntervalInSeconds(5).RepeatForever())
                    .Build();

                // Tell quartz to schedule the job using our trigger
                await scheduler.ScheduleJob(job1, trigger1);
                await scheduler.ScheduleJob(job2, trigger2);

                // some sleep to show what's happening
                await Task.Delay(TimeSpan.FromSeconds(60));

                // and last shut down the scheduler when you are ready to close your program
                await scheduler.Shutdown();
            }
            catch (SchedulerException se)
            {
                Console.WriteLine(se);
            }
        }

        // simple log provider to get something to the console
        private class ConsoleLogProvider : ILogProvider
        {
            public Logger GetLogger(string name)
            {
                return (level, func, exception, parameters) =>
                {
                    if (level >= LogLevel.Info && func != null)
                    {
                        Console.WriteLine("[" + DateTime.Now.ToLongTimeString() + "] [" + level + "] " + func(), parameters);
                    }
                    return true;
                };
            }

            public IDisposable OpenNestedContext(string message)
            {
                throw new NotImplementedException();
            }

            public IDisposable OpenMappedContext(string key, string value)
            {
                throw new NotImplementedException();
            }
        }
    }
    public class UserTraffic : IJob
    {
        public async Task Execute(IJobExecutionContext context)
        {
            JobKey key = context.JobDetail.Key;

            JobDataMap dataMap = context.MergedJobDataMap;

            string messageJob1 = dataMap.GetString("message1");
            string messageJob2 = dataMap.GetString("message2");
            /*
            string jobSays = dataMap.GetString("jobSays");
            float floatValue = dataMap.GetFloat("floatValue");
            await Console.Error.WriteLineAsync("Instance " + key + " of DumbJob says: " + jobSays + ", and val is: " + floatValue);
            */
            await Console.Error.WriteLineAsync("Job 1 : " + messageJob1);
            await Console.Error.WriteLineAsync("Job 2 : " + messageJob2);
            await Console.Error.WriteLineAsync("***********************");
        }
    }
}
