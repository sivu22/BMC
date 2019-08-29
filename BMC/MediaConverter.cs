using System;

namespace BMC
{
    class MediaConverter
    {
        public enum IDriveType
        {
            Unknown = 0,
            BR25,
            BR27,
            BR28,
            BR29,
            BR30,
            BR34,
            BR48,
            BR4,
            BR67
        }

        public enum MediaType
        {
            Unknown = 0,
            AAC,
            MP3,
            MP4,
            WMA,
            JPG,
            M3U,
            FLAC
        }

        public static readonly Predicate<string> SearchFilter = IsMediaFile;

        private static bool IsMediaFile(string fileName)
        {
            return fileName.ToLower().EndsWith(".br25") || fileName.ToLower().EndsWith(".br27") || fileName.ToLower().EndsWith(".br28") 
                || fileName.ToLower().EndsWith(".br29") || fileName.ToLower().EndsWith(".br30") || fileName.ToLower().EndsWith(".br34")
                || fileName.ToLower().EndsWith(".br48") || fileName.ToLower().EndsWith(".br4") || fileName.ToLower().EndsWith(".br67");
        }

        public static (string, IDriveType) GetFileNameAndIDriveType(string fileName)
        {
            if (string.IsNullOrWhiteSpace(fileName) || fileName.Length < 5) return ("", IDriveType.Unknown);

            var lastIndex = fileName.LastIndexOf('.', fileName.Length - 1);
            if (lastIndex < 1) return ("", IDriveType.Unknown);

            var ext = fileName.Substring(lastIndex + 1, fileName.Length - 1 - lastIndex).ToLower();
            var name = fileName.Substring(0, lastIndex);
            // No Parse
            switch (ext)
            {
                case "br25":
                    return (name, IDriveType.BR25);

                case "br27":
                    return (name, IDriveType.BR27);

                case "br28":
                    return (name, IDriveType.BR28);

                case "br29":
                    return (name, IDriveType.BR29);

                case "br30":
                    return (name, IDriveType.BR30);

                case "br34":
                    return (name, IDriveType.BR34);

                case "br48":
                    return (name, IDriveType.BR48);

                case "br4":
                    return (name, IDriveType.BR4);

                case "br67":
                    return (name, IDriveType.BR67);

                default:
                    return (name, IDriveType.Unknown);
            }
        }

        public static MediaType GetMediaType(IDriveType iDriveType)
        {
            switch (iDriveType)
            {
                case IDriveType.BR25:
                    return MediaType.AAC;

                case IDriveType.BR27:
                case IDriveType.BR34:
                    return MediaType.MP4;

                case IDriveType.BR28:
                case IDriveType.BR4:
                    return MediaType.MP3;

                case IDriveType.BR29:
                    return MediaType.WMA;

                case IDriveType.BR30:
                    return MediaType.M3U;

                case IDriveType.BR48:
                    return MediaType.FLAC;

                case IDriveType.BR67:
                    return MediaType.JPG;

                default:
                    return MediaType.Unknown;
            }
        }

        public static byte[] Convert(byte[] bytesIn, IDriveType type)
        {
            if (bytesIn.Length < 1) return new byte[] { };

            var bytesOut = new byte[bytesIn.Length];
            for (int i = 0; i < bytesIn.Length; i++)
            {
                // Only first 131072 bytes (128KB) are inverted
                if (type == IDriveType.BR29 || type == IDriveType.BR34 || type == IDriveType.BR48)
                {
                    if (i < 0x20000) bytesOut[i] = (byte)(~bytesIn[i]);
                    else bytesOut[i] = bytesIn[i];
                }
                // Last 3 bytes are not inverted
                else if (type == IDriveType.BR25 || type == IDriveType.BR4)
                {
                    if (bytesIn.Length - i > 3) bytesOut[i] = (byte)(~bytesIn[i]);
                    else bytesOut[i] = bytesIn[i];
                }
                // Last byte is not inverted
                else if (type == IDriveType.BR28 || type == IDriveType.BR30)
                {
                    if (bytesIn.Length - i > 1) bytesOut[i] = (byte)(~bytesIn[i]);
                    else bytesOut[i] = bytesIn[i];
                }
                else
                {
                    bytesOut[i] = (byte)(~bytesIn[i]);
                }
            }

            return bytesOut;
        }
    }
}
