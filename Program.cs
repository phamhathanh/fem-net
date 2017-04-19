using System;
using System.Collections.Generic;
using System.IO;
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

            var meshArg = app.Argument("mesh", "Path to the mesh.");
            var timeStepOption = app.Option("-dt", "Time step.", CommandOptionType.SingleValue);
            var conditionFileOption = app.Option("-bc", "Path to the boundary condition file.", CommandOptionType.SingleValue);
            app.OnExecute(() => {
                if (meshArg.Value == null)
                {
                    Console.WriteLine("ERROR: Missing mesh.");
                    return 1;
                }
                double dt = timeStepOption.HasValue() ? double.Parse(timeStepOption.Value()) : 0.1;
                var conditionFileName = conditionFileOption.HasValue() ? conditionFileOption.Value() : "example\\DEFAULT.heat";
                FEM2D.FEM2DProgram.Run(meshArg.Value, conditionFileName, dt);
                return 0;
            });

            try
            {
                Console.WriteLine();
                app.Execute(args);
                Console.WriteLine("Press ENTER to exit...");
                Console.ReadLine();
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
            return;
        }
    }
}
