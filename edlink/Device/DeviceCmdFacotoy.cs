using System;


namespace Edlink.Device {
    internal class DeviceCmdFacotoy {

        public static DeviceCmd Create(Link link) {

            switch (link.ProtocolID) {

                case DEV_ED64.DeviceIO.PROTOCOL_ID:
                    return new DEV_ED64.DeviceCmd(link);
                case DEV_MEGA.DeviceIO.PROTOCOL_ID:
                    return new DEV_MEGA.DeviceCmd(link);
                case DEV_EDN8.DeviceIO.PROTOCOL_ID:
                    return new DEV_EDN8.DeviceCmd(link);
                case DEV_TED.DeviceIO.PROTOCOL_ID:
                    return new DEV_TED.DeviceCmd(link);
                case DEV_GBA.DeviceIO.PROTOCOL_ID:
                    return new DEV_GBA.DeviceCmd(link);
                case DEV_SMC.DeviceIO.PROTOCOL_ID:
                    return new DEV_SMC.DeviceCmd(link);
                default:
                    throw new NotSupportedException("unsupported protocol id: 0x" + link.ProtocolID.ToString("X02"));
            }
        }
    }
}
