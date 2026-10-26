using System;


namespace Edlink.Device {
    abstract class DeviceIO_V2 : DeviceIO {

        protected abstract int ADDR_FCI_FIFO { get; } //host fifo

        const byte CMD_STATUS = 0x10;
        const byte CMD_GET_MODE = 0x11;
        const byte CMD_IO_RST = 0x12;
        const byte CMD_NRESP = 0x13;

        const byte CMD_FS = 0x80;
        const byte FS_SCMD_INIT = 0x10;
        const byte FS_SCMD_DIR_OPN = 0x11;
        const byte FS_SCMD_DIR_RD = 0x12;
        const byte FS_SCMD_DIR_LD = 0x13;
        const byte FS_SCMD_DIR_SIZE = 0x14;
        const byte FS_SCMD_DIR_PATH = 0x15;
        const byte FS_SCMD_DIR_GET = 0x16;
        const byte FS_SCMD_FOPN = 0x17;
        const byte FS_SCMD_FCLOSE = 0x18;
        const byte FS_SCMD_FPTR = 0x19;
        const byte FS_SCMD_FINFO = 0x1A;
        const byte FS_SCMD_FCRC = 0x1B;
        const byte FS_SCMD_DIR_MK = 0x1C;
        const byte FS_SCMD_DEL = 0x1D;
        const byte FS_SCMD_SEEK_IDX = 0x1E;
        const byte FS_SCMD_AVB = 0x1F;
        const byte FS_SCMD_FCP = 0x20;
        const byte FS_SCMD_SEEK_PAT = 0x21; //seek data pattern
        const byte FS_SCMD_DTEST = 0x22; //check if dir exists
        const byte FS_SCMD_FTEST = 0x23; //check if file exists

        const byte CMD_EPO = 0x81;
        const byte EPO_SCMD_XFER = 0x10;
        const byte EPO_SCMD_WRE = 0x11;

        const byte CMD_RTC = 0x83;
        const byte RTC_SCMD_GET = 0x10;
        const byte RTC_SCMD_SET = 0x11;
        const byte RTC_SCMD_CAL = 0x12;
        const byte RTC_SCMD_CALSET = 0x13;

        const byte CMD_SYS = 0x84;
        const byte SYS_SCMD_GET_INF = 0x10;
        const byte SYS_SCMD_FPG_INIT = 0x12;
        const byte SYS_SCMD_BOOT_UPD = 0x14;
        const byte SYS_SCMD_SKU = 0x16;

        const byte CMD_BOOT = 0xF0;
        const byte BOOT_SCMD_APP_MODE = 0x10;
        const byte BOOT_SCMD_LOAD_APP = 0x11;

        //************************************************* EPO flags
        const byte EPO_WER_SRC = 0x01;
        const byte EPO_WER_DST = 0x02;

        const uint EPO_WRKEY_FLA = 0x27101983;
        const uint EPO_WRKEY_SDC = 0xDEADBEEF;
        protected enum EpoType {
            LINK = 0x10,//  link (usb or console)
            LINK_ACK, //    link (usb or console)
            FS, //          file
            FCI, //         ed mem
            FLA, //         mcu flash
            EFU, //         efu in flash
            USB,//          usb port
        }
        //*************************************************
        const int BMOD_MCU_APP = 0xA0;//app mode
        const int BMOD_MCU_SER = 0xA1;//service mode
        const int RTC_SIZE = 8;

        public override Link Link {
            get { return link; }
        }

        public override void ExitServiceMode() {

            if (!IsServiceMode()) {
                return;
            }

            link.TxCMD(CMD_BOOT, BOOT_SCMD_APP_MODE);

            BootWait();
            if (IsServiceMode()) {
                throw new Exception("failed to exit service mode");
            }
        }

        public override void EnterServiceMode() {

            if (IsServiceMode()) {
                return;
            }

            link.TxCMD(CMD_IO_RST);
            link.Tx8(BMOD_MCU_SER);

            BootWait();

            if (!IsServiceMode()) {
                throw new Exception("failed to enter service mode");
            }
        }

        public override void MemWR(int addr, byte[] buff, int offset, int len) {

            if (len == 0) {
                return;
            }
            EpoWR(EpoType.FCI, addr, buff, offset, len);
        }

        public override void MemRD(int addr, byte[] buff, int offset, int len) {

            if (len == 0) {
                return;

            }
            EpoRD(EpoType.FCI, addr, buff, offset, len);
        }

        public override void FlaRD(int addr, byte[] buff, int offset, int len) {

            if (len == 0) {
                return;
            }
            EpoRD(EpoType.FLA, addr, buff, offset, len);
        }

        public override void FlaWR(int addr, byte[] buff, int offset, int len) {

            if (len == 0) {
                return;
            }
            EpoWre(EPO_WRKEY_FLA);
            EpoWR(EpoType.FLA, addr, buff, offset, len);
            CheckStatus();
        }

        public override void FifoWR(byte[] data, int offset, int len) {

            MemWR(ADDR_FCI_FIFO, data, offset, len);
        }

        public override void FpgInit(byte[] data) {

            link.TxCMD(CMD_SYS, SYS_SCMD_FPG_INIT);
            link.Tx32(data.Length);
            link.Tx8((int)EpoType.LINK_ACK);
            link.TxDataACK(data, 0, data.Length);
            CheckStatus();
        }

        public override void FpgInit(string path) {

            path = Link.GetDevPath(path);

            FileOpen(Link.GetDevPath(path), FA_READ);
            int size = (int)FileAvailable();

            link.TxCMD(CMD_SYS, SYS_SCMD_FPG_INIT);
            link.Tx32(size);
            link.Tx8((int)EpoType.FS);
            CheckStatus();
        }

        public override void FileOpen(string path, int mode) {

            link.TxCMD(CMD_FS, FS_SCMD_FOPN);
            link.Tx8(mode);
            link.TxString(path);
            CheckStatus();
        }
        public override void FileClose() {

            link.TxCMD(CMD_FS, FS_SCMD_FCLOSE);
            CheckStatus();
        }

        public override UInt64 FileAvailable() {

            link.TxCMD(CMD_FS, FS_SCMD_AVB);

            UInt64 lo = (UInt64)link.Rx32();
            UInt64 hi = (UInt64)link.Rx32();

            return lo | (hi << 32);
        }

        public override void FileRead(byte[] buff, int offset, int len) {

            EpoRD(EpoType.FS, 0, buff, offset, len);
        }

        public override void FileWrite(byte[] buff, int offset, int len) {

            EpoWR(EpoType.FS, 0, buff, offset, len);
            CheckStatus();
        }

        public override RtcTime RtcGet() {

            link.TxCMD(CMD_RTC, RTC_SCMD_GET);
            byte[] vals = link.RxData(RTC_SIZE);
            return new RtcTime(vals);
        }

        public override void RtcSet(DateTime dt) {

            RtcTime rtc = new RtcTime(dt);
            byte[] vals = rtc.GetVals();
            link.TxCMD(CMD_RTC, RTC_SCMD_SET);
            link.TxData(vals, 0, RTC_SIZE);
        }

        public override int RtcCal(DateTime dt, byte arg) {

            RtcTime rtc = new RtcTime(dt);
            byte[] vals = rtc.GetVals();

            link.TxCMD(CMD_RTC, RTC_SCMD_CAL);
            link.TxData(vals);
            link.Tx8(arg);

            return link.Rx32();
        }

        public override void RtcCalSet(int ppm_val) {

            link.TxCMD(CMD_RTC, RTC_SCMD_CALSET);
            link.Tx32(ppm_val);
            CheckStatus();
        }

        public void McuAppLoad(byte[] data) {

            link.TxCMD(CMD_BOOT, BOOT_SCMD_LOAD_APP);
            TxApp(data);

            BootWait(AppTout());
            CheckStatus();
        }

        public void McuBootInstall(byte[] data) {

            link.TxCMD(CMD_SYS, SYS_SCMD_BOOT_UPD);
            TxApp(data);

            BootWait(AppTout());
            CheckStatus();
        }

        public override string Sku(string sku, string key) {

            link.TxCMD(CMD_SYS, SYS_SCMD_SKU);
            link.TxString(key);
            link.TxString(sku);
            return link.RxString();
        }
        //************************************************************************************************ protected
        protected int[] GetSysInf(int[] request) {

            int[] resp = new int[request.Length];

            link.TxCMD(CMD_SYS, SYS_SCMD_GET_INF);
            link.Tx8(request.Length);

            for (int i = 0; i < request.Length; i++) {
                link.Tx32(request[i]);
            }

            for (int i = 0; i < request.Length; i++) {
                resp[i] = link.Rx32();
            }

            return resp;
        }

        protected void CheckStatus() {

            int resp = GetStatus();
            if (resp != 0) {
                int nresp = GetNresp(resp);
                throw new Exception("operation error: " + resp.ToString("X2") + "." + nresp.ToString("X2"));
            }
        }

        protected virtual int AppTout() {
            return 2;
        }
        //************************************************************************************************ private
        int GetNresp(int resp) {
            link.TxCMD(CMD_NRESP);
            link.Tx8(resp);
            return link.Rx8();
        }

        bool IsServiceMode() {

            link.TxCMD(CMD_GET_MODE);
            byte resp = link.Rx8();

            if (resp == BMOD_MCU_SER) {
                return true;
            } else {
                return false;
            }
        }


        void EpoWre(uint wr_key) {

            link.TxCMD(CMD_EPO, EPO_SCMD_WRE);
            link.Tx32((int)wr_key);//flash wr protection magic number
        }

        void EpoCmd(EpoType ep_src, EpoType ep_dst, int addr_src, int addr_dst, int len) {

            link.TxCMD(CMD_EPO, EPO_SCMD_XFER);
            link.Tx32(addr_src);
            link.Tx32(addr_dst);
            link.Tx32(len);
            link.Tx8((int)ep_src);
            link.Tx8((int)ep_dst);
            link.Tx16(0);//reserved(for aligment)

            link.Tx8(0);//ack
        }

        void EpoRD(EpoType epo_src, int addr, byte[] buff, int offset, int len) {

            EpoCmd(epo_src, EpoType.LINK, addr, 0, len);
            link.RxData(buff, offset, len);
            CheckStatus();
        }

        void EpoWR(EpoType epo_dst, int addr, byte[] buff, int offset, int len) {

            EpoType epo_src = EpoType.LINK;

            if (epo_dst == EpoType.FLA || epo_dst == EpoType.FS) {
                epo_src = EpoType.LINK_ACK;
            }

            EpoCmd(epo_src, epo_dst, 0, addr, len);

            if (epo_src == EpoType.LINK_ACK) {
                link.TxDataACK(buff, offset, len);
            } else {
                link.TxData(buff, offset, len);
            }

            //CheckStatus();
        }


        void TxApp(byte[] data) {

            link.Tx32(data.Length);
            link.Tx8((byte)EpoType.LINK_ACK);
            link.TxDataACK(data, 0, 512);
            link.TxDataACK(data, 512, data.Length - 512);
        }



    }
}
