using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;

namespace Juniansoft.Upsparkle
{
    internal class Utilites
    {
        [DllImport("kernel32.dll")]
        private static extern IntPtr LoadLibrary(string dllpath);

        internal static IntPtr LoadUnmanagedLibrary(string dllpath)
        {
            if (String.IsNullOrEmpty(dllpath))
                throw new ArgumentNullException("dllpath");

            IntPtr moduleHandle = LoadLibrary(dllpath);
            if (moduleHandle == IntPtr.Zero)
            {
                var lasterror = Marshal.GetLastWin32Error();
                var innerEx = new Win32Exception(lasterror);
                innerEx.Data.Add("LastWin32Error", lasterror);

                throw new Exception("Can't load DLL " + dllpath, innerEx);
            }
            return moduleHandle;
        }

        internal static void LoadUnmanagedLibraryFromResource(
            Assembly assembly,
            string libraryResourceName,
            string libraryName)
        {
            string tempDllPath = string.Empty;
            using (var s = assembly.GetManifestResourceStream(libraryResourceName))
            {
                byte[] data = new BinaryReader(s).ReadBytes((int)s.Length);

                string assemblyPath = Path.GetDirectoryName(assembly.Location);
                //tempDllPath = Path.Combine(assemblyPath, libraryName);
                tempDllPath = Path.Combine(Path.GetTempPath(), "Upsparkle");
                if (!Directory.Exists(tempDllPath))
                    Directory.CreateDirectory(tempDllPath);
                tempDllPath = Path.Combine(tempDllPath, libraryName);

                File.WriteAllBytes(tempDllPath, data);
            }

            LoadLibrary(libraryName);
        }
    }
}
