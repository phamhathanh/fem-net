using System;
using System.Collections.Generic;
using System.IO;

namespace FEM_NET
{
    internal static class Program
    {
        private static double dt = 0.1;
        private static string meshName = null,
            conditionFileName = "example\\DEFAULT.heat";

        private static void Main(string[] args)
        {
            Console.WriteLine();
            try
            {
                ParseArgs(args);
                FEM2D.FEM2DProgram.Run(meshName, conditionFileName, dt);
            }
            catch (ArgumentException exception)
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

            Console.WriteLine("Press ENTER to exit...");
            Console.ReadLine();
        }

        private static void ParseArgs(string[] args)
        {
            var argsQueue = new Queue<string>(args);
            while (argsQueue.Count != 0)
            {
                var arg = argsQueue.Dequeue();
                if (arg[0] != '-')
                {
                    if (meshName != null)
                        Console.WriteLine($"Argument \"{arg}\" is ignored.");
                    else
                        meshName = arg;
                        // TODO: Sanitize.
                    continue;
                }
                switch (arg)
                {
                    case "-dt":
                        if (argsQueue.Count == 0)
                            throw new ArgumentException("ARGUMENT ERROR: Missing parameter for option \"-dt\".");
                        dt = double.Parse(argsQueue.Dequeue());
                        // TODO: Format exception.
                    break;
                    case "-bc":
                        if (argsQueue.Count == 0)
                            throw new ArgumentException("ARGUMENT ERROR: Missing parameter for option \"-bc\".");
                        conditionFileName = argsQueue.Dequeue();
                        // TODO: Sanitize.
                    break;
                    // TODO: -h / --help
                    default:
                        throw new ArgumentException($"ARGUMENT ERROR: Unknown option \"{arg}\".");
                }
            }
            if (meshName == null)
                throw new ArgumentException("ARGUMENT ERROR: Missing mesh name.");
            if (meshName.EndsWith(".mesh"))
                meshName = meshName.Substring(0, meshName.Length - 5);
        }
    }
}
