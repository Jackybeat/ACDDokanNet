﻿using System;
using System.Collections.Generic;
using System.Linq;
using Azi.Tools;
using DokanNet;

namespace Azi.Cloud.DokanNet
{
    public class VirtualDriveWrapper
    {
        private readonly VirtualDrive virtualDrive;

        private char mountLetter;

        public VirtualDriveWrapper(FSProvider provider)
        {
            virtualDrive = new VirtualDrive(provider);
            virtualDrive.OnMount = () =>
            {
                Mounted?.Invoke(mountLetter);
            };
            virtualDrive.OnUnmount = () =>
            {
                Unmounted?.Invoke(mountLetter);
            };
        }

        public Action<char> Mounted { get; set; }

        public Action<char> Unmounted { get; set; }

        public static IList<char> GetFreeDriveLettes()
        {
            return Enumerable.Range('C', 'Z' - 'C' + 1).Select(c => (char)c).Except(Environment.GetLogicalDrives().Select(s => s[0])).ToList();
        }

        public static void Unmount(char letter)
        {
            Dokan.Unmount(letter);
        }

        public void Mount(char letter, bool readOnly)
        {
            try
            {
                virtualDrive.ReadOnly = readOnly;
#if DEBUG
                virtualDrive.Mount(letter + ":\\", DokanOptions.DebugMode | DokanOptions.AltStream | DokanOptions.FixedDrive, 0, 800, TimeSpan.FromSeconds(30));
#else
                virtualDrive.Mount(letter + ":\\", DokanOptions.AltStream | DokanOptions.FixedDrive, 0, 800, TimeSpan.FromSeconds(30));
#endif
                virtualDrive.MountPath = letter + ":\\";
                mountLetter = letter;
            }
            catch (DokanException e)
            {
                Log.Error(e);
                throw new InvalidOperationException(e.Message, e);
            }
            catch (Exception e)
            {
                Log.Error(e);
            }
        }
    }
}