using System.IO;
using System.CodeDom.Compiler;
using UnityEditor;
using UnityEngine;

public unsafe static class TypeTreeExport
{
    [MenuItem("Tools/Type Tree/Generate structs.dump")]
    static void GenerateStructsDump()
    {
        using (var sv = new PdbService())
        using (var fs = File.OpenWrite("structs.dump"))
        using (var sw = new StreamWriter(fs))
        {
            fs.SetLength(0);
            TypeTreeUtility.WriteDumpFile(sv, TransferInstructionFlags.SerializeGameRelease, sw);
        }
    }

    [MenuItem("Tools/Type Tree/Generate structs.dat")]
    static void GenerateStructsDat()
    {
        using (var sv = new PdbService())
        using (var fs = File.OpenWrite("structs.dat"))
        using (var bw = new BinaryWriter(fs))
        {
            fs.SetLength(0);
            TypeTreeUtility.WriteDataFile(sv, TransferInstructionFlags.SerializeGameRelease, bw);
        }
    }

    [MenuItem("Tools/Type Tree/Generate strings.dat")]
    static void GenerateStringsDat()
    {
        using (var fs = File.OpenWrite("strings.dat"))
        using (var bw = new BinaryWriter(fs))
        {
            fs.SetLength(0);
            TypeTreeUtility.WriteCommonStrings(bw);
        }
    }

    [MenuItem("Tools/Type Tree/Generate classes.json")]
    static void GenerateClassesJson()
    {
        using (var fs = File.OpenWrite("classes.json"))
        using (var sw = new StreamWriter(fs))
        {
            fs.SetLength(0);
            TypeTreeUtility.WriteClassesJson(sw);
        }
    }

    [MenuItem("Tools/Type Tree/Generate ClassID Enum")]
    static void GenerateClassIDEnum()
    {
        using (var cp = CodeDomProvider.CreateProvider("C#"))
        using (var fs = File.OpenWrite("Assets/ClassID.cs"))
        using (var sw = new StreamWriter(fs))
        {
            fs.SetLength(0);
            sw.WriteLine($"// This file was generated with Unity {Application.unityVersion}.");
            sw.WriteLine("public enum ClassID");
            sw.WriteLine("{");

            foreach (var type in UnityType.RuntimeTypes)
            {
                var name = type.GetName();

                // Prepend keywords with an '@' character.
                if (!cp.IsValidIdentifier(name))
                    name = '@' + name;

                sw.WriteLine("    {0} = {1},", name, type.PersistentTypeID);
            }

            sw.WriteLine("}");
        }

        AssetDatabase.Refresh();
    }
}
