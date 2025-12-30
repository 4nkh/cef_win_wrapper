using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Management;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace cefWinWrapper
{
    internal class MachineIdentity
    {
        private const string RegistryPath = @"Software\cefWinWrapper";
        private const string RegistryKey = "DeviceUUID";
        public static string GetPersistentMachineID()
        {
            // Try to get the ID from the registry
            string storedID = GetStoredID();
            if (!string.IsNullOrEmpty(storedID))
            {
                string hwID = GetHardwareUUID();
                if (!string.IsNullOrEmpty(hwID))
                {
                    if (storedID == hwID)
                    {
                        return storedID;
                    }
                    else
                    {
                        SaveID(hwID);
                        return hwID;
                    }
                }
                return storedID;
            }

            // When nothing's stored, get hardware UUID
            string hardwareID = GetHardwareUUID();

            // When BIOS not responding, we generate a GUID
            if (string.IsNullOrEmpty(hardwareID) || hardwareID == "FFFFFFFF-FFFF-FFFF-FFFF-FFFFFFFFFFFF")
            {

                hardwareID = Guid.NewGuid().ToString();
            }

            SaveID(hardwareID);

            return hardwareID;
        }

        private static string GetHardwareUUID()
        {
            try
            {
                // Get the BIOS identity (UUID)
                ManagementObjectSearcher searcher =
                    new ManagementObjectSearcher("SELECT UUID FROM Win32_ComputerSystemProduct");

                ManagementObjectCollection collection = searcher.Get();

                foreach (ManagementObject obj in collection)
                {
                    return obj["UUID"].ToString();
                }
            }
            catch (System.Exception ex)
            {
                MessageBox.Show($"ID access error : {ex.Message}");
            }
            return "ID_NOT_FOUND";
        }

        private static void SaveID(string id)
        {
            try
            {
                using (RegistryKey key = Registry.CurrentUser.CreateSubKey(RegistryPath))
                {
                    key?.SetValue(RegistryKey, id);
                }
            }
            catch { }
        }

        private static string GetStoredID()
        {
            try
            {
                using (RegistryKey key = Registry.CurrentUser.OpenSubKey(RegistryPath))
                {
                    return key?.GetValue(RegistryKey) as string;
                }
            }
            catch { return null; }
        }
    }
}
