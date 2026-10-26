using System;
using Edlink.Device;


namespace Edlink.DEV_GBA {
    internal class MenuCmd {

        const int BMOD_GBA_REBOOT = 0x4252;//'RB' reboot request
        const int BMOD_GBA_VDUMP = 0x4456;//'VD' vram dump
        const int BMOD_GBA_GSTART = 0x5347;//'GS' game start

        public const int RST_MODE_SOFT = 0;
        public const int RST_MODE_HARD = 1;

        DeviceIO dev;
        Link link;

        public MenuCmd(DeviceIO dev) {
            this.dev = dev;
            link = dev.Link;
        }

        internal void ResetToMenu(int mode) {

            if (mode == RST_MODE_SOFT) {
                ResetToMenu_soft();
            } else {
                ResetToMenu_hard();
            }
        }

        public void VramDump(byte[] vram, byte[] palette, byte[] regs) {

            int dump_addr;
            dev.SetBootMode(BMOD_GBA_VDUMP);
            dump_addr = link.Rx32();

            dev.MemRD(dump_addr, vram, 0, 96 * 1024);
            dev.MemRD(dump_addr + 96 * 1024, palette, 0, 1024);
            dev.MemRD(dump_addr + 97 * 1024, regs, 0, 1024);
        }

        public void Run(string path) {

            dev.SetBootMode(BMOD_GBA_GSTART);

            int resp = link.WaitResp(1000);
            if (resp != 0) {
                throw new Exception("mack: cmd error: " + resp.ToString("X2"));
            }

            dev.FifoTxString(Link.GetDevPath(path));

            resp = link.WaitResp(4000);
            if (resp != 0) {
                throw new Exception("mcmd: cmd error: " + resp.ToString("X2"));
            }
        }


        void ResetToMenu_hard() {

            //reset mcu
            dev.BootSW(true);
            dev.EnterServiceMode();
            dev.ExitServiceMode();

            dev.ResetEfu(15);

            dev.BootSW(true);
            dev.SetBootMode(BMOD_GBA_REBOOT);
            dev.BootSW(false);
        }

        void ResetToMenu_soft() {

            dev.ExitServiceMode();
            dev.SetBootMode(BMOD_GBA_REBOOT);
            dev.BootSW(false);
        }
    }
}
