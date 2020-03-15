using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection.Emit;
using Cottle.Documents.Compiled;

namespace Cottle.Documents.Emitted
{
    internal readonly struct Program
    {
        private static readonly Type OutValue = typeof(Value).MakeByRefType();

        public static Program Create(ICommandGenerator generator)
        {
#if COTTLE_IL_SAVE && !NETSTANDARD
            var assemblyName = new System.Reflection.AssemblyName("Test");
            var fileName = assemblyName.Name + ".dll";

            var saveAssembly = AssemblyBuilder.DefineDynamicAssembly(assemblyName, AssemblyBuilderAccess.RunAndSave);
            var saveModule = saveAssembly.DefineDynamicModule(assemblyName.Name, fileName);
            var saveProgram = saveModule.DefineType("Program", System.Reflection.TypeAttributes.Public);
            var saveMethod = saveProgram.DefineMethod("Main",
                System.Reflection.MethodAttributes.Public | System.Reflection.MethodAttributes.Static,
                System.Reflection.CallingConventions.Any, typeof(bool), arguments);

            Program.Emit(new Emitter(saveMethod.GetILGenerator()), generator);

            saveProgram.CreateType();
            saveAssembly.Save(assemblyName.Name + ".dll");

            var saveDestination = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), fileName);
            var saveSource = Path.Combine(Environment.CurrentDirectory, fileName);

            File.Copy(saveSource, saveDestination, true);
#endif

            var arguments = new[] { typeof(IReadOnlyList<Value>), typeof(Frame), typeof(TextWriter), Program.OutValue };
            var method = new DynamicMethod(string.Empty, typeof(bool), arguments, typeof(EmittedDocument));
            var emitter = new Emitter(method.GetILGenerator());

            Program.Emit(emitter, generator);

            var executable = (Executable)method.CreateDelegate(typeof(Executable));

            return new Program(executable, emitter.CreateConstants());
        }

        private static void Emit(Emitter emitter, ICommandGenerator generator)
        {
            if (!generator.Generate(emitter))
                emitter.LoadBoolean(false);

            emitter.Return();
        }

        public readonly IReadOnlyList<Value> Constants;
        public readonly Executable Executable;

        private Program(Executable executable, IReadOnlyList<Value> constants)
        {
            Constants = constants;
            Executable = executable;
        }
    }
}