using System.IO;
using UnityEngine;

public unsafe class ExportBehaviour : MonoBehaviour
{
    void Awake()
    {
        using (var service = new PdbService())
            Export(service);
    }

    void Export(PdbService service)
    {
        using (var fs = File.OpenWrite("structs.dump"))
        using (var sw = new StreamWriter(fs))
        {
            fs.SetLength(0);
            TypeTreeUtility.WriteDumpFile(service, TransferInstructionFlags.SerializeGameRelease, sw);
        }

        using (var fs = File.OpenWrite("structs.dat"))
        using (var bw = new BinaryWriter(fs))
        {
            fs.SetLength(0);
            TypeTreeUtility.WriteDataFile(service, TransferInstructionFlags.SerializeGameRelease, bw);
        }

        using (var fs = File.OpenWrite("strings.dat"))
        using (var bw = new BinaryWriter(fs))
        {
            fs.SetLength(0);
            TypeTreeUtility.WriteCommonStrings(bw);
        }

        using (var fs = File.OpenWrite("classes.json"))
        using (var sw = new StreamWriter(fs))
        {
            fs.SetLength(0);
            TypeTreeUtility.WriteClassesJson(sw);
        }

        Application.Quit();
    }
}
