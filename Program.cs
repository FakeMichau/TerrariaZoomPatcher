using Mono.Cecil;
using System.IO;

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
                        // Allow for drawing wider than 1920 + 192 * 2
                        if (method.Name == ".cctor")
                        {
                            foreach (var instruction in method.Body.Instructions)
                            {
                                if (instruction.ToString().Contains("Terraria.Main::MaxWorldViewSize")) 
                                {
                                    // keeping it safe so that x + 192 * 2 < 4096
                                    // above 4096 might be possible on Windows
                                    instruction.Previous.Previous.Operand = 3711; // default 1200
                                    instruction.Previous.Previous.Previous.Operand = 3711; // default 1920
                                }
                            }
                        }

                        // Changing MaxWorldViewSize throws off the high res detection so just always force it
                        if (method.Name == "LoadContent_TryEnteringHiDef")
                        {
                            foreach (var instruction in method.Body.Instructions)
                            {
                                if (instruction.ToString().Contains("Support4K") && instruction.Previous.Previous.ToString().Contains("IsXna"))
                                {
                                    instruction.Next.Operand = ((Mono.Cecil.Cil.Instruction)instruction.Next.Operand).Next.Next; // Jump two ins further
                                }
                            }
                        }

                        // Remove the force zoom instructions
                        if (method.Name == "DoDraw")
                        {
                            foreach (var instruction in method.Body.Instructions)
                            {
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
                        }
                    }
                }
            }

             terrariaAsModule.Write("Terraria.exe");
        }
    }
}