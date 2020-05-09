using System;

namespace Unity.Core
{ 
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Method, AllowMultiple = false, Inherited = false)]
    internal sealed class PdbSymbolAttribute : Attribute
    {
        public string SymbolName { get; }

        public PdbSymbolAttribute(string symbolName)
        {
            SymbolName = symbolName;
        }
    }
}
