using Mono.CSharp;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Tester
{
    public class ConsoleSample
    {
        public static void Start()
        {
            MainEvaluator = new Evaluator(CompilerContext.Create());
            MainThreadCancellationToken = new CancellationTokenSource();

            Task.Factory.StartNew(MainThread);

            while (!MainThreadCancellationToken.Token.IsCancellationRequested)
                Thread.Sleep(250);

            OnMainThreadFinished();
        }

        static Evaluator MainEvaluator { get; set; }

        static CancellationTokenSource MainThreadCancellationToken { get; set; }

        static void CheckCancellation()
        {
            MainThreadCancellationToken.Token.ThrowIfCancellationRequested();
        }

        static void MainThread()
        {
            bool keepAlive = true;

            while (keepAlive)
            {
                try
                {
                    //Will throw an exception if the cancellation was requested.
                    CheckCancellation();

                    Console.Write("#:");
                    var input = Console.ReadLine();

                    CheckCancellation();

                    ProcessCommand(input);
                }
                catch (Exception mainThreadException)
                {
                    if (mainThreadException is OperationCanceledException)
                    {
                        keepAlive = false;
                    }
                }
            }
        }

        static void OnMainThreadFinished()
        {
            Console.WriteLine("Press any key to continue...");
            Console.ReadKey();
        }

        static void ProcessCommand(string input)
        {
            if (input.ToLower().StartsWith("compl "))
            {
                ShowCompletions(input.Replace("compl ", string.Empty));
                return;
            }

            switch (input.ToLower())
            {
                case "cls":
                    Console.Clear();
                    break;
                case "exit":
                    MainThreadCancellationToken.Cancel();
                    break;
                default:
                    object result;
                    bool resultSet;
                    MainEvaluator.Evaluate(input, out result, out resultSet);

                    if (resultSet)
                        Console.WriteLine("\r\n--------\t{0}\t--------\r\n", result);
                    break;
            }
        }

        static void ShowCompletions(string input)
        {
            string prefix;
            var completions = MainEvaluator.GetCompletions(input, out prefix);

            foreach (string completion in completions)
            {
                Console.WriteLine("\t{0}{1}", prefix, completion);
            }
        }
    }
}
