using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.Extensions.CommandLineUtils;

namespace FEM_NET
{
    internal static class Program
    {
        private static void Main(string[] args)
        {
            var app = new CommandLineApplication();
            app.Name = "femnet";
            app.HelpOption("-?|-h|--help");

            var meshArg = app.Argument("mesh", "Path to the mesh");

            var elementTypeOption = app.Option("-t|--type", "Finite element type", CommandOptionType.SingleValue);
            var accuracyOption = app.Option("-a|-accuracy", "Accuracy", CommandOptionType.SingleValue);

            var conditionFileOption = app.Option("-bc", "Path to the boundary condition file", CommandOptionType.SingleValue);
            var timeStepOption = app.Option("-dt", "Time step", CommandOptionType.SingleValue);
            var timeStepNumberOption = app.Option("-it", "Number of time steps", CommandOptionType.SingleValue);
            // TODO: Consider removing.
            
            app.OnExecute(() => {
                if (meshArg.Value == null)
                {
                    Console.WriteLine("ERROR: Missing mesh.");
                    return 1;
                }
                var meshPath = meshArg.Value;
                if (meshPath.EndsWith(".mesh"))
                    meshPath = meshPath.Substring(0, meshPath.Length - 5);

                var feType = elementTypeOption.HasValue() ? elementTypeOption.Value().ToLowerInvariant() : "p1";

                var conditionPath = conditionFileOption.HasValue() ? conditionFileOption.Value() : $"example{Path.DirectorySeparatorChar}DEFAULT.heat";
                double dt = timeStepOption.HasValue() ? double.Parse(timeStepOption.Value()) : 0.1;
                int it = timeStepNumberOption.HasValue() ? int.Parse(timeStepNumberOption.Value()) : 30;
                double acc = accuracyOption.HasValue() ? double.Parse(accuracyOption.Value()) : 1e-6;
                // TODO: Format error.
                FEM2D.ElasticProgram.Run(meshPath, feType, conditionPath, dt, it, acc);
                Console.WriteLine("Press ENTER to exit...");
                Console.ReadLine();
                return 0;
            });

            try
            {
                app.Execute(args);
            }
            catch (CommandParsingException exception)
            {
                Console.WriteLine(exception.Message);
            }
            catch (FileNotFoundException exception)
            {
                Console.WriteLine($"FILE NOT FOUND: {exception.Message}");
            }
            catch (DirectoryNotFoundException exception)
            {
                Console.WriteLine($"DIRECTORY NOT FOUND: {exception.Message}");
            }
        }
    }
}
