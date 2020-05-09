using System.Diagnostics;
using UnityEditor;
using Dia2Lib;
using Dia;

namespace Unity.Core
{
    [InitializeOnLoad]
    internal static class PdbSession
    {
        public static IDiaSession Current { get; }

        static PdbSession()
        {
            var module = Process.GetCurrentProcess().MainModule;
            var pdb    = DiaSourceFactory.CreateInstance();
            pdb.loadDataForExe(module.FileName, null, null);
            pdb.openSession(out IDiaSession session);
            Current = session;
        }
    }
}
