using Edlink.Device;
using System;


namespace Edlink.DEV_GBA {
    internal class DeviceCmd : Device.DeviceCmd {

        new DeviceIO dev;
        MenuCmd mcmd;

        public DeviceCmd(Link link) {

            base.dev = dev = new DeviceIO(link);
            mcmd = new MenuCmd(dev);

        }


        public override string DeviceName {

            get {

                switch (dev.Link.DeviceID) {

                    case DeviceIO.DEV_ID_GBA_PRO:
                        return "EverDrive-GBA PRO";
                    default:
                        return "Uknown EverDrive-GBA";
                }
            }
        }

        public override void Reset(string mode) {

            mode = mode.ToLower().Trim();

            if (mode.Equals("hard")) {
                mcmd.ResetToMenu(MenuCmd.RST_MODE_HARD);
            } else {
                mcmd.ResetToMenu(MenuCmd.RST_MODE_SOFT);
            }
        }

        public override void Run(string rom_path, string fpga_path) {

            string app_dst;

            app_dst = base.AppDeploy(rom_path, fpga_path);
            mcmd.Run(app_dst);
        }

        public override void Screen(string path) {

            byte[] vram = new byte[96 * 1024];
            byte[] palette = new byte[1024];
            byte[] regs = new byte[1024];

            mcmd.VramDump(vram, palette, regs);
            MenuImage.MakeImage(path, vram, palette, regs);
        }

        public override string DevInf() {

            string msg = "";

            DeviceIO.SysInfo sys_inf = dev.getSysInf();
            DeviceIO.Vdc vdc = dev.GetVdc();

            string serial = "";
            serial += sys_inf.serial_g.ToString("X8");
            serial += ".";
            serial += sys_inf.serial_l.ToString("X8");

            msg += "device id : " + dev.Link.DeviceID.ToString("X2") + "\n";
            msg += "name      : " + DeviceName + "\n";
            msg += "serial    : " + serial + "\n";

            msg += "build date: " + Tools.TsToDate(sys_inf.asm_date) + "\n";
            msg += "bootloader: " + Tools.TsToVersion(sys_inf.boot_ver) + "\n";
            msg += "firmware  : " + Tools.TsToVersion(sys_inf.sw_ver) + "\n";
            msg += "mcu core  : " + Tools.TsToVersion(sys_inf.sw_date) + "\n";
            msg += "flash size: " + Tools.SizeToStr(sys_inf.flash_size) + "\n";
            msg += "rtc calib : " + dev.RtcCal(DateTime.Now, (byte)DeviceIO.Rtcc.GET_CURCAL) + "\n";
            msg += "game ctr  : " + sys_inf.game_ctr + "\n";
            msg += "boot ctr  : " + sys_inf.boot_ctr + "\n";
            msg += "battery   : " + Tools.VdcToStr(vdc.bat) + "\n";

            return msg;
        }

        public override void Diag() {
            Diagnostics diag = new Diagnostics(dev);
            diag.Start();
        }
    }
}
