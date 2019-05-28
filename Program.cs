using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Polly;

namespace PollyErrorHandling
{
    class Program
    {
        static async Task SimulateWork()
        {
            var random = new Random();
            if (random.Next() % 2 != 0)
            {
                Console.WriteLine("Error");
                throw new ApplicationException("Something wrong");
            }

            await Task.Delay(TimeSpan.FromSeconds(2));
            Console.WriteLine("Work finished");
        }

        private static int brokenCounter = 0;
        static void ProbablyWillFailOperation()
        {
            Console.WriteLine("");
            Console.WriteLine("Inside function that probably will fail...");
            if (++brokenCounter <= 2)
            {
                Console.WriteLine("");
                Console.WriteLine($"Something broke, failures counter: {brokenCounter}...");
                throw new ApplicationException("This function is not working");
            }
        }

        static void WorkingOperation()
        {
            Console.WriteLine($"Inside working function, that serves as fallback for BrokenOperation");
        }


        static void Main(string[] args)
        {
            Console.WriteLine("Two retries with different time");
            try
            {
                Policy.Handle<ApplicationException>()
                    .WaitAndRetry(new[] {TimeSpan.FromSeconds(5), TimeSpan.FromSeconds(10)})
                    .Execute(async () => await SimulateWork());
            }
            catch (Exception e)
            {
                Console.WriteLine($"Final exception: {e}");
            }
            Console.WriteLine("Finished - Two retries with different time");

            //-------------------------------------------------------------------------------
            Console.WriteLine("");
            Console.WriteLine("10 retries");
            try
            {
                var t = Policy.Handle<ApplicationException>()
                    .WaitAndRetryAsync(10, i => TimeSpan.FromSeconds(i))
                    .ExecuteAsync(async () => await SimulateWork());
                t.Wait();
            }
            catch (Exception e)
            {
                Console.WriteLine($"Final exception: {e}");
            }
            Console.WriteLine("Finished - 10 retries");

            //-------------------------------------------------------------------------------
            Console.WriteLine("");
            Console.WriteLine("Testing Circuit Breaker");
            var circuitBreaker = Policy.Handle<Exception>().CircuitBreaker(2, TimeSpan.FromSeconds(5),
                    (ex, span) => Console.WriteLine($"Failed! Exception: {ex.Message}. Circuit open, waiting {span}"),
                    () => Console.WriteLine("First execution after circuit break succeeded, circuit is reset."));
            
            var fallback = Policy.Handle<Exception>().Fallback(WorkingOperation,
                ex => Console.WriteLine($"Exception {ex.Message} in main method. Going to fallback")).Wrap(circuitBreaker);

            fallback.Execute(ProbablyWillFailOperation);

            fallback.Execute(ProbablyWillFailOperation);

            Console.WriteLine("");
            Console.WriteLine("Here we won't go to the main method, since circuit is broken");
            fallback.Execute(ProbablyWillFailOperation);

            Console.WriteLine("");
            Console.WriteLine("Let's wait till circuit will be reset");
            Thread.Sleep(5000);

            Console.WriteLine("");
            Console.WriteLine("Here we should go again to the main method, since circuit is closed again");
            fallback.Execute(ProbablyWillFailOperation);

            Console.WriteLine("");
            Console.WriteLine("Finished");
            Console.ReadLine();
        }
    }
}
