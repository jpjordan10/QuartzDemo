using Quartz.Logging;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Quartz;
using Quartz.Impl;

namespace QuartzDemo
{
    public class UserTrafficSample
    {
        private static void Main(string[] args)
        {
            LogProvider.SetCurrentLogProvider(new ConsoleLogProvider());

            RunProgramRunExample().GetAwaiter().GetResult();

            Console.WriteLine("Application is over, please press any key to close the application.");
            Console.ReadKey();
        }

        private static async Task RunProgramRunExample()
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
                //IJobDetail job = JobBuilder.Create<UserTraffic>()
                //    .WithIdentity("job1", "group1")
                //    .Build();

                IJobDetail job = JobBuilder.Create<UserTraffic>()
                    .WithIdentity("myJob", "group1")
                    .UsingJobData("jobSays", "Hello World!")
                    .UsingJobData("floatValue", 3.141f)
                    .Build();
                /*
                 Run the job with the time defined
                 */

                // Trigger the job to run now, and then repeat every 5 seconds
                ITrigger trigger = TriggerBuilder.Create()
                    .WithIdentity("trigger1", "group1")
                    .StartNow()
                    .WithSimpleSchedule(x => x.WithIntervalInSeconds(5).RepeatForever())
                    .Build();

                // Tell quartz to schedule the job using our trigger
                await scheduler.ScheduleJob(job, trigger);

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

            string jobSays = dataMap.GetString("jobSays");
            float floatValue = dataMap.GetFloat("floatValue");

            await Console.Error.WriteLineAsync("Instance " + key + " of DumbJob says: " + jobSays + ", and val is: " + floatValue);
        }
    }
}
