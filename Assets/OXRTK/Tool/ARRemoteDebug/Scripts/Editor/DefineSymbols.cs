using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.IO;

namespace OXRTK.ARRemoteDebug
{
    [InitializeOnLoad]
    public class DefineSymbols
    {
        readonly static string m_Symbol = "ARRemoteDebug";
        [InitializeOnLoadMethod]
        public static void AddSymbol()
        {
            //add 'ARRemoteDebug' define symbol
            AddSymbol(BuildTargetGroup.Standalone);
            AddSymbol(BuildTargetGroup.Android);
            
        }

        
        public static void RemoveSymbols()
        {
            RemoveSymbols(BuildTargetGroup.Standalone);
            RemoveSymbols(BuildTargetGroup.Android);

        }

        static void RemoveSymbols(BuildTargetGroup group)
        {
            string symbols = PlayerSettings.GetScriptingDefineSymbolsForGroup(group);
            string[] symbolsarray = symbols.Split(';');
            List<string> symbolslist = new List<string>();
            bool remotemirror = false;

            //if we have Mirror net plugin still in ARRemoteDebug folder
            //we need to remote Mirror symbols too.
            if (Directory.Exists(Application.dataPath + "/ARRemoteDebug/Mirror"))
                remotemirror = true;

            //remote ARRemoteDebug symbol.
            foreach(string s in symbolsarray)
            {
                if (remotemirror && (s.Contains("MIRROR_") || s == "MIRROR"))
                    continue;

                if (s == m_Symbol)
                    continue;

                symbolslist.Add(s);
            }

            string final_symbols = "";
            foreach (string s in symbolslist)
                final_symbols += s + ";";

            PlayerSettings.SetScriptingDefineSymbolsForGroup(group, final_symbols);
        }

        static void AddSymbol(BuildTargetGroup group)
        {
            string symbols = PlayerSettings.GetScriptingDefineSymbolsForGroup(group);
            if (!symbols.Contains(m_Symbol))
                symbols = symbols + ";" + m_Symbol;
            PlayerSettings.SetScriptingDefineSymbolsForGroup(group, symbols);
        }
    }
}

