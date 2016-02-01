using System;
using System.ComponentModel;
using System.IO;
using System.Runtime.InteropServices;

namespace BlockChainSharp.PInvoke
{
    /// <summary>
    /// 
    /// </summary>
    public class Disk
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="lpRootPathName"></param>
        /// <param name="lpSectorsPerCluster"></param>
        /// <param name="lpBytesPerSector"></param>
        /// <param name="lpNumberOfFreeClusters"></param>
        /// <param name="lpTotalNumberOfClusters"></param>
        /// <returns></returns>
        [DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Auto)]
        public static extern bool GetDiskFreeSpace(string lpRootPathName, out uint lpSectorsPerCluster, out uint lpBytesPerSector, out uint lpNumberOfFreeClusters, out uint lpTotalNumberOfClusters);

        /// <summary>
        /// Returns the cluster size of the local disk
        /// </summary>
        /// <param name="directoryInfo"></param>
        /// <returns></returns>
        public static long GetDiskClusterSize(DirectoryInfo directoryInfo)
        {
            try
            {
                uint sectorsPerCluster;
                uint bytesPerSector;
                uint numberOfFreeClusters;
                uint totalNumberOfClusters;

                var rootPathName = Path.GetPathRoot(directoryInfo.FullName);
                var success = GetDiskFreeSpace(rootPathName, out sectorsPerCluster, out bytesPerSector, out numberOfFreeClusters, out totalNumberOfClusters);

                if (!success)
                {
                    throw new Win32Exception();
                }

                return checked((sectorsPerCluster * bytesPerSector));
            }
            catch (Exception)
            {
                return 4096;
            }
        }
    }
}
