using System;
using System.IO;
using System.Linq;
using System.Reflection;

using Mono.Cecil;
using Mono.Cecil.Cil;

namespace TerrariaZoomPatcher
{
    class Program
    {
        public static DirectoryInfo GetExecutingDirectory()
        {
            var location = new Uri(Assembly.GetEntryAssembly().GetName().CodeBase);
            return new FileInfo(location.AbsolutePath).Directory;
        }
        static void Main(string[] args)
        {
            var currentDirectory = GetExecutingDirectory();
            Console.WriteLine(currentDirectory);
            var terrariaPath = currentDirectory.GetFiles("Terraria.exe")[0];    // will fail
            Console.WriteLine(terrariaPath);
            ModuleDefinition terrariaAsModule = ModuleDefinition.ReadModule(terrariaPath.ToString());
            foreach (var type in terrariaAsModule.Types)
            {
                if (type.Name == "Main")
                {
                    foreach (var method in type.Methods)
                    {
                        if (method.Name == "DoDraw")
                        {
                            foreach (var instruction in method.Body.Instructions)
                            {
                                if (instruction.ToString().Contains("ForcedMinimumZoom") &&
                                    instruction.Previous.ToString().Contains("Max"))
                                {
                                    var insLoop = instruction;
                                    for (var i = 0; i < 16; i++)
                                    {
                                        var toBeRemoved = insLoop;
                                        insLoop = insLoop.Previous;
                                        method.Body.Instructions.Remove(toBeRemoved);
                                    }
                                    break;
                                }
                            }

                            Console.WriteLine(method);
                        }
                    }
                }
            }

            Console.ReadKey();
            terrariaAsModule.Write(Path.Combine(currentDirectory.ToString(), "copy.exe"));
        }
    }
}