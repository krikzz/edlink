using System;
using Edlink.Device;

namespace Edlink.DEV_SMC {

    internal class DeviceCmd : Device.DeviceCmd {

        new DeviceIO dev;


        public DeviceCmd(Link link) {

            base.dev = dev = new DeviceIO(link);
        }

        public override string DeviceName {

            get {

                switch (dev.Link.DeviceID) {

                    case DeviceIO.DEV_ID_SMC:
                        return "Soomeecom";
                    default:
                        return "Uknown Soomeecom";
                }
            }
        }

        public override void Reset(string mode) {


        }

        public override void Run(string rom_path, string fpga_path) {


        }

        public override void McuApp(string path) {

            byte[] buff = Stdio.Read(path);
            //dev.EnterServiceMode();
            dev.McuAppLoad(buff);
        }

        public override void McuBoot(string path) {

            byte[] buff = Stdio.Read(path);
            dev.ExitServiceMode();
            dev.McuBootInstall(buff);
        }

        public override void Screen(string path) {

        }

        public override string DevInf() {

            string msg = "";

            string serial = "";
            serial += dev.SysGetInf(DeviceIO.SysInf.INFS_SERIAL_G).ToString("X8");
            serial += ".";
            serial += dev.SysGetInf(DeviceIO.SysInf.INFS_SERIAL_L).ToString("X8");

            msg += "device id : " + dev.Link.DeviceID.ToString("X2") + "\n";
            msg += "name      : " + DeviceName + "\n";
            msg += "serial    : " + serial + "\n";
            msg += "build date: " + Tools.TsToDate(dev.SysGetInf(DeviceIO.SysInf.INFS_TS_ASM)) + "\n";
            msg += "bootloader: " + Tools.TsToVersion(dev.SysGetInf(DeviceIO.SysInf.INFS_TS_BOOT)) + "\n";
            msg += "firmware  : " + Tools.TsToVersion(dev.SysGetInf(DeviceIO.SysInf.INFS_TS_FW)) + "\n";;
            msg += "flash size: " + Tools.SizeToStr(dev.SysGetInf(DeviceIO.SysInf.INFS_FLA_SIZE)) + "\n";
            msg += "boot ctr  : " + dev.SysGetInf(DeviceIO.SysInf.INFD_BOOT_CTR) + "\n";
            msg += "vcc 3.3   : " + Tools.VdcToStr(dev.SysGetInf(DeviceIO.SysInf.INFD_VCC_3V3)) + "\n";
            msg += "vcc 2.5   : " + Tools.VdcToStr(dev.SysGetInf(DeviceIO.SysInf.INFD_VCC_2V5)) + "\n";
            msg += "vcc 1.2   : " + Tools.VdcToStr(dev.SysGetInf(DeviceIO.SysInf.INFD_VCC_1V2)) + "\n";

            return msg;
        }

        public override void Diag() {

        }

        public override void Dscmd(CmdLine cmd) {

        }



    }
}
