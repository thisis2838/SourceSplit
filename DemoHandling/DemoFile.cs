using System.IO;
using static System.Text.Encoding;
using System.Text.RegularExpressions;

namespace LiveSplit.SourceSplit.DemoHandling
{
    public class DemoFile
    {
        public string Name = "";
        public string MapName = "";
        public string PlayerName = "";
        public string GameName = "";
        public int Index = 0;
        public long TotalTicks = 0;
        public string FilePath = "";

        public DemoFile(string filePath)
        {
            filePath = Path.GetFullPath(filePath);
            if (!File.Exists(filePath))
                return;

            FilePath = filePath;
            Name = Path.GetFileNameWithoutExtension(filePath);

            using (var fs = new FileStream(filePath, FileMode.Open, FileAccess.Read))
            using (var br = new BinaryReader(fs))
            {
                br.BaseStream.Seek(8 + 4 + 4 + 260, SeekOrigin.Current);
                PlayerName = ASCII.GetString(br.ReadBytes(260)).TrimEnd('\0');
                MapName = ASCII.GetString(br.ReadBytes(260)).TrimEnd('\0');
                GameName = ASCII.GetString(br.ReadBytes(260)).TrimEnd('\0');

                br.BaseStream.Seek(4 * 3, SeekOrigin.Current);
                var signOnLen = br.ReadInt32();

                byte command = 0x0;
                while (command != 0x07)
                {
                    command = br.ReadByte();

                    if (command == 0x07) // dem_stop
                        break;

                    var tick = br.ReadInt32();
                    if (tick >= 0)
                        TotalTicks = tick;

                    switch (command)
                    {
                        case 0x01:
                            br.BaseStream.Seek(signOnLen, SeekOrigin.Current);
                            break;
                        case 0x02:
                            {
                                br.BaseStream.Seek(4 + 4 * 3 + 68L, SeekOrigin.Current);
                                var packetLen = br.ReadInt32();
                                br.BaseStream.Seek(packetLen, SeekOrigin.Current);
                            }
                            break;
                        case 0x04: // console commands
                            {
                                var concmdLen = br.ReadInt32();
                                br.BaseStream.Seek(concmdLen, SeekOrigin.Current);
                            }
                            break;
                        case 0x05: // user commands
                            {
                                br.BaseStream.Seek(4, SeekOrigin.Current); // skip sequence
                                var userCmdLen = br.ReadInt32();
                                br.BaseStream.Seek(userCmdLen, SeekOrigin.Current);
                            }
                            break;
                        case 0x08:
                            {
                                var stringTableLen = br.ReadInt32();
                                br.BaseStream.Seek(stringTableLen, SeekOrigin.Current);
                            }
                            break;
                    }
                }
            }

            string name = Path.GetFileNameWithoutExtension(filePath);
            var match = Regex.Match(name, $@"^(?:{MapName}_)([0-9]+)$", RegexOptions.IgnoreCase);
            if (match.Success)
                int.TryParse(match.Groups[1].Value, out Index);
        }

        public static bool FromFilePath(string filePath, out DemoFile demo)
        {
            demo = null;

            if (!File.Exists(filePath))
                return false;

            try { demo = new DemoFile(filePath); return true; }
            catch { return false; }
        }
    }
}
