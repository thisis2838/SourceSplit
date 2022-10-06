using System;
using System.Diagnostics;
using System.Linq;
using System.Media;
using System.Runtime.InteropServices;
using LiveSplit.ComponentUtil;
using LiveSplit.SourceSplit.GameSpecific;
using System.Windows.Forms;
using System.Collections.Generic;
using System.Threading;
using System.Text;
using LiveSplit.SourceSplit.GameHandling;
using LiveSplit.SourceSplit.ComponentHandling;
using LiveSplit.SourceSplit.Utilities;

namespace LiveSplit.SourceSplit.GameHandling
{
    // custom command system for games that need their own specific settings
    // usually set through monitoring a buffer for invalid console command inputs
    class CustomCommand
    {
        public string Name;
        public string Description;
        public bool Archived { get; set; }

        public bool BValue { get; set; }
        public string Value { get; set; }
        public int IValue { get; set; }
        public float FValue { get; set; }
        private Action _callback = null;

        private static string[] _noVars = new string[] { "no", "0", "false" };

        public CustomCommand(string name, string def, string description = "", Action callback = null, bool archived = false)
        {
            Name = name;
            Parse(def);
            Description = description;
            _callback = callback;
            Archived = archived;
        }

        public bool Update(string input)
        {
            string[] elems = input.Split(' ');

            if (elems.Count() == 0 || elems[0] != Name)
                return false;

            input = input.Substring(Name.Length + 1);
            Parse(input);
            _callback?.Invoke();
            SystemSounds.Asterisk.Play();

            return true;
        }

        public void Parse(string input)
        {
            Value = input;
            BValue = !_noVars.Contains(input.Trim().ToLower()) && !string.IsNullOrWhiteSpace(input.Trim(' ', '\"'));
            if (int.TryParse(input, out int tmpI))
                IValue = tmpI;
            else IValue = 0;
            if (float.TryParse(input, out float tmpF))
                FValue = tmpF;
            else FValue = 0;
        }

        public override string ToString() => this.ToString(true);

        public string ToString(bool showDesc = true)
        {
            string vals = $"{Name} (archived: {Archived}) s[{Value}] b[{BValue}] i[{IValue}] f[{FValue}]";
            string desc = "";
            if (showDesc && !string.IsNullOrEmpty(Description))
            {
                var lines = Description.Split('\n');
                for (int i = 0; i < lines.Count(); i++)
                {
                    if (i == 0)
                        desc += " - " + lines[i] + "\n";
                    else
                        desc += "   " + lines[i] + "\n";
                }
            }

            return $"{vals}\n{desc}\n".Replace("\n\n", "\n").Replace("\t", "\"   ").TrimEnd('\n');
        }
    }

    class CustomCommandHandler
    {
        public CustomCommand[] Commands { get; set; }
        private IntPtr _cmdBufferPtr = IntPtr.Zero;
        private Process _game;
        private byte[] _cmdBuffer;
        private const int BUFFER_SIZE = 512;
        private string _gameDir;

        public CustomCommandHandler(params CustomCommand[] commands)
        {
            Commands = commands;
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
                        Debug.WriteLine("Command buffer found at 0x" + _cmdBufferPtr.ToString("X"));
                        break;
                    }
                }
            }

            _game = state.GameEngine.GameProcess;
            Update(state);
            SendConsoleMessage($@"

SourceSplit Custom Commands are present, enter ""ss_list"" to list them, or ""ss_help"" for help!
There are {Commands.Count()} command(s) available.

");
            return;

            fail:
            _cmdBufferPtr = IntPtr.Zero;
            Debug.WriteLine("Failed to initialize custom command handler!");
            return;
        }


        // allow disabling and enabling of features through monitoring specific console input
        public void Update(GameState state)
        {
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
                            string cmd = Encoding.Default.GetString(newBuffer.Skip(tmp).Take(count).ToArray());

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
                Trace.WriteLine(ex);
            }

        }

        private bool ProcessCommand(string input)
        {
            string cleanedCmd = input.ToLower().Trim(' ', '\0');

            switch (cleanedCmd)
            {
                case "ss_list":
                    ListAllCommands();
                    return true;
                case "ss_help":
                    PrintHelp();
                    return true;

            }
            foreach (CustomCommand cmd in Commands)
            {
                if (cleanedCmd.Contains(cmd.Name))
                {
                    if (cleanedCmd.Length > cmd.Name.Length && cmd.Update(cleanedCmd))
                    {
                        if (cmd.Archived)
                        {
                            SourceSplitComponent.Settings.SetMiscSetting($"{_gameDir}__{cmd.Name}", cmd.Value);
                        }

                        SendConsoleMessage($"{cmd.Name} set to \"{cmd.Value}\"!\n");
                        return true;
                    }
                    if (cleanedCmd == cmd.Name)
                    {
                        SendConsoleMessage(cmd.ToString());
                        return true;
                    }
                }
            }

            return false;
        }

        public void SendConsoleMessage(string input)
        {
            List<string> commands = input.Split('\n').ToList();
            if (commands.Count() == 0)
                commands.Add(input);

            foreach (string command in commands)
                WinUtils.SendMessage(_game, $"echo " + command + "\0");
        }

        private void ListAllCommands()
        {
            SendConsoleMessage($@"

Listing {Commands.Count()} command(s):
{Commands.Aggregate("", (a, b) => a + "\t- " + b.ToString(false) + "\n").TrimEnd('\n')}

");
        }

        private void PrintHelp()
        {
            SendConsoleMessage(@"

SourceSplit Custom Commands help: 
    - Enter in commands as if they are normal game commands!
    - Type ""ss_list"" for a list of available commands!
    - Enter a command without arguments for a description of it.

");
        }
    }
}
