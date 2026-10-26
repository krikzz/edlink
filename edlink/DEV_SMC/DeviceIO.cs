using System;
using Edlink.Device;

namespace Edlink.DEV_SMC {

    internal class DeviceIO : DeviceIO_V2 {

        public const int PROTOCOL_ID = 0x08;
        public const int DEV_ID_SMC = 0x28;

        protected override int ADDR_FCI_FIFO => (ADDR_FCI_SYS + 0x10000);

        const int ADDR_FCI_SYS = 0x10000000;//system registers
        internal const int ADDR_FCI_RAM0 = 0x0000000;
        internal const int ADDR_FCI_RAM1 = 0x4000000;


        public enum SysInf {

            //static vals
            INFS_DEV_ID = 1,
            INFS_HW_VER,
            INFS_SERIAL_G,
            INFS_SERIAL_L,
            INFS_TS_ASM,
            INFS_TS_FW,
            INFS_TS_BOOT,
            INFS_FLA_SIZE,
            INFS_MAX_ROM_SIZE,
            INFS_TS_CIC,

            //dynamic vals
            INFD_BOOT_CTR = 128,
            INFD_GAME_CTR,
            INFD_RST_SRC,
            INFD_BOOT_MODE,
            INFD_PWR_SYS,
            INFD_PWR_USB,
            INFD_BAT_DRY,
            INFD_VCC_BAT,
            INFD_VCC_1V2,
            INFD_VCC_1V8,
            INFD_VCC_2V5,
            INFD_VCC_3V3,
            INFD_VCC_5V0,
        }

        public DeviceIO(Link link) {

            if (link.ProtocolID != PROTOCOL_ID) {
                throw new NotSupportedException("invalid protocol id: 0x" + link.ProtocolID.ToString("X02"));
            }

            this.link = link;
            link.SwapEndians = false;
        }

        internal int[] SysGetInf(SysInf[] request) {

            int[] request_int = new int[request.Length];

            for (int i = 0; i < request.Length; i++) {
                request_int[i] = (int)request[i];
            }

            return GetSysInf(request_int);
        }

        internal int SysGetInf(SysInf request) {
            return SysGetInf(new SysInf[] { request })[0];
        }

        protected override int AppTout() {
            return 7;//sec time
        }

    }
}
