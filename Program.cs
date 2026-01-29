using System;
using System.IO;
using Mono.Cecil;

namespace TerrariaZoomPatcher
{
    class Program
    {
        static void Main(string[] args)
        {
            var terrariaPath = args.Length == 0 ? Path.Combine(Directory.GetCurrentDirectory(), "Terraria.exe") : args[0];
            if (!File.Exists(terrariaPath)) return;

            var newTerrariaPath = Path.Combine(Path.GetDirectoryName(terrariaPath), "Terraria.exe.bak");
            if (File.Exists(newTerrariaPath)) File.Delete(newTerrariaPath);

            File.Move(terrariaPath, newTerrariaPath);

            ModuleDefinition terrariaAsModule = ModuleDefinition.ReadModule(newTerrariaPath);
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
                                Console.WriteLine(instruction.ToString());

                                if (instruction.ToString().Contains("ForcedMinimumZoom") &&
                                    instruction.Previous.ToString().Contains("Max"))
                                {
                                    var insLoop = instruction;
                                    // remove 20 instructions before the selected one
                                    for (var i = 0; i < 20; i++)
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

            terrariaAsModule.Write("Terraria.exe");
        }
    }
}