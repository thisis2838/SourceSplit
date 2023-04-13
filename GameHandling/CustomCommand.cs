using LiveSplit.ComponentUtil;
using LiveSplit.SourceSplit.ComponentHandling;
using LiveSplit.SourceSplit.Utilities;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Media;
using System.Text;
using System.Text.RegularExpressions;

namespace LiveSplit.SourceSplit.GameHandling
{
    // custom command system for games that need their own specific settings
    // usually set through monitoring a buffer for invalid console command inputs
    class CustomCommand
    {
        private string _name;
        public string Name
        {
            get => _name;
            set => _name = value.ToLower();
        }
        public string Description;
        private string _longDesc = null;
        public string LongDescription
        {
            get => _longDesc ?? Description;
            set => _longDesc = value;
        }

        public bool Archived { get; set; } = true;
        public bool Hidden { get; set; } = false;

        public bool Boolean { get; set; }
        public string String { get; set; }
        public int Integer { get; set; }
        public float Float { get; set; }

        public Action<string> Callback = null;
        private static string[] _noVars = new string[] { "0", "false", "" };

        public CustomCommand(
            string name, 
            string def, string description = "", string longDescription = null, 
            Action<string> callback = null, 
            bool archived = false)
        {
            Name = name;
            Parse(def);
            Description = description;
            LongDescription = longDescription;
            Callback = callback;
            Archived = archived;
        }

        public bool Update(string input)
        {
            if (string.IsNullOrWhiteSpace(input)) 
                return false;

            Parse(input);
            Callback?.Invoke(input);

            if (!Hidden) SystemSounds.Asterisk.Play();
            return true;
        }

        public void Parse(string input)
        {
            input = input.Trim(' ', '\"');

            String = input;
            Boolean = !_noVars.Contains(input.Trim().ToLower());
            if (int.TryParse(input, out int tmpI)) Integer = tmpI; else Integer = 0;
            if (float.TryParse(input, out float tmpF)) Float = tmpF; else Float = 0;
            if (Boolean) Float = Integer = 1;
        }

        public override string ToString() 
        {
            return  $"{Name} (archived: {Archived}) (value: s[{String}] b[{Boolean}] i[{Integer}] f[{Float}])";
        }
    }

    class CustomCommandHandler
    {
        public List<CustomCommand> Commands = new List<CustomCommand>();
        private IntPtr _cmdBufferPtr = IntPtr.Zero;
        private IntPtr _conMsgPtr = IntPtr.Zero;
        private Process _game;
        private byte[] _cmdBuffer;
        private const int BUFFER_SIZE = 512;
        private string _gameDir;
        private bool _dontProcess = false;

        public CustomCommandHandler(params CustomCommand[] commands)
        {
            Commands = commands.ToList();
            _cmdBuffer = new byte[BUFFER_SIZE];
        }

        public void Init(GameState state)
        {
            _gameDir = state.GameDir;

            Commands.Where(x => x.Archived).ToList().ForEach(x =>
            {
                if (SourceSplitComponent.Settings.GetMiscSetting($"{_gameDir}__{x.Name}") is var setting && setting != null)
                    x.Parse(setting);
            });

            _cmdBufferPtr = IntPtr.Zero;
            ProcessModuleWow64Safe engine = state.GetModule("engine.dll");

            var scanner = new SignatureScanner(state.GameProcess, engine.BaseAddress, engine.ModuleMemorySize);
            IntPtr ptr = scanner.Scan(new SigScanTarget("68" + scanner.Scan(new SigScanTarget("execing %s\n".ConvertToHex())).GetByteString()));

            if (ptr == IntPtr.Zero)
                goto fail;

            byte[] bytes = state.GameProcess.ReadBytes(ptr, 100);
            for (int i = 0; i < 100; i++)
            {
                byte e = bytes[i];
                if (e == 0xA1 || (bytes[i] >= 0xB8 && bytes[i] <= 0xBF))
                {
                    uint val = state.GameProcess.ReadValue<uint>(ptr + i + 1);
                    if (scanner.IsWithin(val))
                    {
                        _cmdBufferPtr = (IntPtr)val;
                        Logging.WriteLine("Command buffer found at 0x" + _cmdBufferPtr.ToString("X"));
                        break;
                    }
                }
            }

            _game = state.GameEngine.GameProcess;
            GetExecPtr(state);
            Update(state);
            SetAliases();

            if (Commands.Where(x => !x.Hidden).Count() == 0) return;
            SendConsoleMessage(
@$"//////////////////////////////////////////////////////////////////////////////////

SourceSplit Custom Commands are present, enter ""ss_list"" to list them, or ""ss_help"" for help!
There are {Commands.Count()} command(s) available.

////////////////////////////////////////////////////////////////////////////////////");
            return;

            fail:
            _cmdBufferPtr = IntPtr.Zero;
            Logging.WriteLine("Failed to initialize custom command handler!");
            return;
        }

        private void SetAliases()
        {
            _game.SendMessage("alias ss_list \0");
            _game.SendMessage("alias ss_help \0");

            Commands.ForEach(x => _game.SendMessage("alias " + x.Name + "\0"));
        }

        private void GetExecPtr(GameState state)
        {
            try
            {
                var tier0 = state.GetModule("tier0.dll");
                var tier0Symbols = WinUtils.AllSymbols(state.GameProcess, tier0);

                _conMsgPtr = (IntPtr)tier0Symbols.Where(x => x.Name == "ConMsg").FirstOrDefault().Address;
            } 
            catch (Exception ex)
            {
                Logging.WriteLine($"Couldn't find ConMsg pointer: {ex}");
                _conMsgPtr = IntPtr.Zero;   
            }

            if (_conMsgPtr != IntPtr.Zero) 
                Logging.WriteLine($"ConMsg found at {_conMsgPtr.ToString("X")}");
        }

        public void Update(GameState state)
        {
            if (Commands.Count() == 0) return;

            if (_cmdBufferPtr == IntPtr.Zero)
                return;

            byte[] newBuffer = state.GameProcess.ReadBytes(_cmdBufferPtr, BUFFER_SIZE);
            if (newBuffer == null)
                return;

            try
            {
                if (!newBuffer.SequenceEqual(_cmdBuffer))
                {
                    int tmp = 0;
                    for (int i = 0; i < BUFFER_SIZE - 1; i++)
                    {
                        // null byte, we've hit the end of a command
                        if (newBuffer[i] == 0x00)
                        {
                            int count = i - tmp + 1;
                            string cmd = Encoding.ASCII.GetString(newBuffer.Skip(tmp).Take(count).ToArray());

                            byte[] prevBytes = state.GameProcess.ReadBytes(_cmdBufferPtr + tmp, count);

                            if (ProcessCommand(cmd))
                                // don't modify the buffer if this section has changed
                                if (prevBytes.SequenceEqual(state.GameProcess.ReadBytes(_cmdBufferPtr + tmp, count)))
                                    // remove the command from the buffer, replacing it with null bytes so we don't encounter
                                    // it in the next update loop
                                    state.GameProcess.WriteBytes(_cmdBufferPtr + tmp, new byte[count]);

                            tmp = i;

                            // 2nd null byte, we've hit the effective end of the buffer
                            if (newBuffer[i + 1] == 0x00)
                                break;
                        }
                    }

                    state.GameProcess.ReadBytes(_cmdBufferPtr, BUFFER_SIZE).CopyTo(_cmdBuffer, 0);
                }
            }
            catch (ArgumentNullException ex)
            {
                Logging.WriteLine(ex);
            }

        }

        private bool ProcessCommand(string input)
        {
            if (_dontProcess) return false;

            string cleanedCmd = input.ToLower().Trim(' ', '\0');
            var match = Regex.Match(cleanedCmd, @"^(?<command>[A-z0-9-_]+)(?:$|(?: +)(?<arg>.+)$)");
            string command = match.Groups["command"].Value.ToLower();
            string arg = match.Groups["arg"].Value;

            switch (command)
            {
                case "ss_list": ListAllCommands(); return true;
                case "ss_help": 
                    {
                        if (string.IsNullOrWhiteSpace(arg))
                        {
                            PrintHelp(); 
                            return true;
                        }

                        var cmd = Commands.FirstOrDefault(x => x.Name == arg.ToLower());
                        if (cmd != null)
                        {
                            SendConsoleMessage($"{cmd}\n{new string('-', cmd.ToString().Length)}\n{cmd.LongDescription}");
                        }

                        return true;
                    }
            }

            foreach (CustomCommand cmd in Commands)
            {
                if (command != cmd.Name)
                    continue;

                if (cmd.Update(arg))
                {
                    if (cmd.Archived)
                    {
                        SourceSplitComponent.Settings.SetMiscSetting($"{_gameDir}__{cmd.Name}", cmd.String);
                    }

                    if (!cmd.Hidden)
                    {
                        SendConsoleMessage($"{cmd.Name} set to \"{cmd.String}\"!");
                    }
                }

                return true;
            }

            return false;
        }

        public void SendConsoleMessage(string input)
        {
            _dontProcess = true;

            input = input.Replace("\r", "").Trim(' ', '\t', '\n');
            input = "\n\n" + input + "\n\n";

            if (_conMsgPtr != IntPtr.Zero)
            {
                input = input.Replace("%", "%%");
                input = input.Replace("\t", "    ");

                const int MAX_LEN = 0x100;
                for (int i = 0; i < input.Length; i += MAX_LEN)
                {
                    var send = input.Substring(i, MAX_LEN.Bounded(0, input.Length - i));
                    _game.CallFunctionString(send, _conMsgPtr);
                }
            }
            else
            {
                input = input.Replace('\"', '\'').Replace("\t", "    ").Replace(' ', '\"');
                input.Split('\n').ToList().ForEach(x => _game.SendMessage("echo " + x + "\0"));
            }

            _dontProcess = false;
        }

        private void ListAllCommands()
        {
            SendConsoleMessage
            (
                $"Listing {Commands.Where(x => !x.Hidden).Count()} command(s):\n" +
                Commands.Where(x => !x.Hidden).Aggregate("", (a, b) => $"{a}\t- {b}\n\t  {b.Description}\n").TrimEnd('\n')
            );
        }

        private void PrintHelp()
        {
            SendConsoleMessage
            (
                "SourceSplit Custom Commands help: \n" +
                "\t- Enter in commands as if they are normal game commands. \n" +
                "\t- Type \"ss_list\" for a list of available commands. \n" +
                "\t- Enter \"ss_help <command>\" to show information about a command"
            );
        }
    }
}
