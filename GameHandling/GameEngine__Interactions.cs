using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Threading;
using System.IO;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using LiveSplit.ComponentUtil;
using LiveSplit.SourceSplit.GameSpecific;
using LiveSplit.SourceSplit.Utilities;
using System.Collections.Generic;
using System.Runtime.Serialization.Formatters.Binary;
using System.Reflection;
using System.ComponentModel;
using static LiveSplit.SourceSplit.GameHandling.GameMemory;

namespace LiveSplit.SourceSplit.GameHandling
{
    public abstract partial class GameEngine
    {

        #region  RETRIEVAL FUNCTIONS
        public virtual HostState GetHostState()
        {
            return (HostState)GameProcess.ReadValue<int>(HostStatePtr);
        }
        public virtual SignOnState GetSignOnState()
        {
            return (SignOnState)GameProcess.ReadValue<int>(SignOnStatePtr);
        }
        public virtual ServerState GetServerState()
        {
            return (ServerState)GameProcess.ReadValue<int>(ServerStatePtr);
        }

        internal const int _maxEnts = 2048;
        /// <summary>
        /// Gets the entity info of an entity using its index. 
        /// </summary>
        /// <param name="index">The index of the entity</param>
        public virtual CEntInfoV2 GetEntInfoByIndex(int index)
        {
            Debug.Assert(EntInfoSize > 0);

            if (index < 0)
                return new CEntInfoV2();

            IntPtr addr = GlobalEntityListPtr + ((int)EntInfoSize * index);

            this.GameProcess.ReadValue(addr, out CEntInfoV1 v1);
            return CEntInfoV2.FromV1(v1);
        }
        // warning: expensive -  7ms on i5
        // do not call frequently!
        /// <summary>
        /// Gets the entity pointer of the entity with matching name
        /// </summary>
        /// <param name="name">The name of the entity</param>
        public virtual IntPtr GetEntityByName(string name, params string[] ignore)
        {
            CEntInfoV2 nextPtr = this.GetEntInfoByIndex(0);
            var ret = IntPtr.Zero;
            do
            {
                if (nextPtr.EntityPtr == IntPtr.Zero)
                {
                    ret = IntPtr.Zero;
                    goto end;
                }

                IntPtr namePtr;
                this.GameProcess.ReadPointer(nextPtr.EntityPtr + BaseEntityTargetNameOffset, false, out namePtr);
                if (namePtr != IntPtr.Zero)
                {
                    this.GameProcess.ReadString(namePtr, ReadStringType.ASCII, 32, out string n);  // TODO: find real max len
                    if (!(ignore?.Contains(n) ?? false) && name.CompareWildcard(n))
                    {
                        ret = nextPtr.EntityPtr;
                        goto end;
                    }
                }
                nextPtr = GameProcess.ReadValue<CEntInfoV2>((IntPtr)nextPtr.m_pNext);
            }
            while (nextPtr.EntityPtr != IntPtr.Zero);

            end:
#if DEBUG
            Debug.WriteLine($"found entity \"{name}\" ptr : {ret.ToString("X")}");
#endif
            return ret;
        }
        /// <summary>
        /// Gets the entity index of the entity with matching name
        /// </summary>
        /// <param name="name">The name of the entity</param>
        public virtual int GetEntIndexByName(string name, params string[] ignore)
        {
            int ret = -1;
            for (int i = 0; i < _maxEnts; i++)
            {
                CEntInfoV2 info = this.GetEntInfoByIndex(i);
                if (info.EntityPtr == IntPtr.Zero)
                    continue;

                IntPtr namePtr;
                this.GameProcess.ReadPointer(info.EntityPtr + BaseEntityTargetNameOffset, false, out namePtr);
                if (namePtr == IntPtr.Zero)
                    continue;

                string n;
                this.GameProcess.ReadString(namePtr, ReadStringType.ASCII, 32, out n);  // TODO: find real max len
                if (!(ignore?.Contains(n) ?? false) && n.CompareWildcard(name))
                {
                    ret = i;
                    break;
                }
            }
#if DEBUG
            Debug.WriteLine($"found entity \"{name}\" index : {ret}");
#endif
            return ret;

        }
        /// <summary>
        /// Gets the entity index of the entity with matching position
        /// </summary>
        /// <param name="x">The x coordinate of the entity</param>
        /// <param name="y">The y coordinate of the entity</param>
        /// <param name="z">The z coordinate of the entity</param>
        /// <param name="d">The maximum allowed distance away from the specified position</param>
        /// <param name="xy">Whether to ignore the z component when evaluating positions</param>
        public virtual int GetEntIndexByPos(float x, float y, float z, float d = 0f, bool xy = false)
        {
            Vector3f pos = new Vector3f(x, y, z);

            for (int i = 0; i < _maxEnts; i++)
            {
                CEntInfoV2 info = this.GetEntInfoByIndex(i);
                if (info.EntityPtr == IntPtr.Zero)
                    continue;

                Vector3f newpos;
                if (!this.GameProcess.ReadValue(info.EntityPtr + BaseEntityAbsOriginOffset, out newpos))
                    continue;

                if (d == 0f)
                {
                    // not equal 1 becase the player might be in the same exact position
                    if (newpos.BitEquals(pos) && i != 1)
                    {
#if DEBUG
                        Debug.WriteLine($"found entity with pos = {pos} @ {i}");
#endif
                        return i;
                    }
                }
                else // check for distance if it's a non-static entity like an npc or a prop
                {
                    if (xy && newpos.DistanceXY(pos) <= d && i != 1)
                    {
#if DEBUG
                        Debug.WriteLine($"found entity with pos xy dist <= {d} from {pos} @ {i}");
#endif
                        return i;

                    }
                    else if (newpos.Distance(pos) <= d && i != 1)
                    {
#if DEBUG
                        Debug.WriteLine($"found entity with pos dist <= {d} from {pos} @ {i}");
#endif
                        return i;

                    }
                }
            }

            return -1;
        }
        public int GetEntIndexByPos(Vector3f vec, float d = 0f, bool xy = false)
            => GetEntIndexByPos(vec.X, vec.Y, vec.Z, d, xy);

        /// <summary>
        /// Gets the entity position of the entity with matching index
        /// </summary>
        /// <param name="i">The index of the entity</param>
        public virtual Vector3f GetEntityPos(int i)
        {
            Vector3f pos;
            var ent = GetEntInfoByIndex(i);
            GameProcess.ReadValue(ent.EntityPtr + BaseEntityAbsOriginOffset, out pos);
            return pos;
        }
        // env_fades don't hold any live fade information and instead they network over fade infos to the client which add it to a list
        /// <summary>
        /// Finds the time when a fade in or out finishes. Returns 0 on failure to find a fade with matching description.
        /// </summary>
        /// <param name="speed">The speed of the fade</param>
        public virtual float GetFadeEndTime(float speed)
        {
            float ret = 0;

            if (FadeListPtr == IntPtr.Zero)
                goto end;

            int fadeListSize = GameProcess.ReadValue<int>(FadeListPtr + 0x10);
            if (fadeListSize == 0) goto end;

            ScreenFadeInfo fadeInfo;
            uint fadeListHeader = GameProcess.ReadValue<uint>(FadeListPtr + 0x4);
            for (int i = 0; i < fadeListSize; i++)
            {
                fadeInfo = GameProcess.ReadValue<ScreenFadeInfo>(GameProcess.ReadPointer((IntPtr)fadeListHeader) + 0x4 * i);
                if (fadeInfo.Speed != speed)
                    continue;
                else
                {
                    ret = fadeInfo.End;
                    goto end;
                }
            }

            end:

#if false
            Debug.WriteLine($"found fade with speed {speed} end time : {ret}");
#endif
            return ret;
        }
        /// <summary>
        /// Finds the time when a fade in or out finishes. Returns 0 on failure to find a fade with matching description.
        /// </summary>
        /// <param name="speed">The speed of the fade</param>
        /// <param name="r">Red value of the color of the game</param>
        /// <param name="g">Green value of the color of the game</param>
        /// <param name="b">Blue value of the color of the game</param>
        public virtual float GetFadeEndTime(float speed, byte r, byte g, byte b)
        {
            float ret = 0;

            if (FadeListPtr == IntPtr.Zero)
                goto end;

            int fadeListSize = GameProcess.ReadValue<int>(FadeListPtr + 0x10);
            if (fadeListSize == 0) goto end;

            ScreenFadeInfo fadeInfo;
            byte[] targColor = { r, g, b };
            uint fadeListHeader = GameProcess.ReadValue<uint>(FadeListPtr + 0x4);
            for (int i = 0; i < fadeListSize; i++)
            {
                fadeInfo = GameProcess.ReadValue<ScreenFadeInfo>(GameProcess.ReadPointer((IntPtr)fadeListHeader) + 0x4 * i);
                byte[] color = { fadeInfo.R, fadeInfo.G, fadeInfo.B };
                if (fadeInfo.Speed != speed && !targColor.SequenceEqual(color))
                    continue;
                else
                {
                    ret = fadeInfo.End;
                    goto end;
                }
            }

            end:
#if false
            Debug.WriteLine($"found fade with speed {speed} and color {r}, {g}, {b} end time : {ret}");
#endif
            return ret;
        }
        // ioEvents are stored in a non-contiguous list where every ioEvent contain pointers to the next or previous event 
        // todo: add more input types and combinations to ensure the correct result
        /// <summary>
        /// Finds the fire time of an output. Returns 0 on failure to find an output with matching description.
        /// </summary>
        /// <param name="targetName">The name of the targeted entity</param>
        /// <param name="clamp">The maximum number of inputs to check</param>
        /// <param name="inclusive">If the name specified is a substring of the target name</param>
        public virtual float GetOutputFireTime(string targetName, int clamp = 100)
        {
            float ret = 0;

            if (EventQueuePtr == IntPtr.Zero)
                goto end;

            EventQueuePrioritizedEvent ioEvent;
            GameProcess.ReadValue(GameProcess.ReadPointer(EventQueuePtr), out ioEvent);

            // clamp the number of items to go through the list to save performance
            // the list is automatically updated once an output is fired
            for (int i = 0; i < clamp; i++)
            {
                string tempName = GameProcess.ReadString((IntPtr)ioEvent.m_iTarget, 256) ?? "";
                if (tempName.CompareWildcard(targetName))
                {
                    ret = ioEvent.m_flFireTime;
                    goto end;
                }
                else
                {
                    IntPtr nextPtr = (IntPtr)ioEvent.m_pNext;
                    if (nextPtr != IntPtr.Zero)
                    {
                        GameProcess.ReadValue(nextPtr, out ioEvent);
                        continue;
                    }
                    else goto end; // end early if we've hit the end of the list
                }
            }

            end:
#if false
            Debug.WriteLine($"found output with target name \"{targetName}\" fire time : {ret}");
#endif
            return ret;
        }
        /// <summary>
        /// Finds the fire time of an output. Returns 0 on failure to find an output with matching description.
        /// </summary>
        /// <param name="targetName">The name of the targeted entity</param>
        /// <param name="command">The command of the output</param>
        /// <param name="param">The parameters of the command</param>
        /// <param name="clamp">The maximum number of inputs to check</param>
        public virtual unsafe float GetOutputFireTime(string targetName, string command, string param, int clamp = 100)
        {
            float ret = 0;

            if (EventQueuePtr == IntPtr.Zero)
                goto end;

            EventQueuePrioritizedEvent ioEvent;
            GameProcess.ReadValue(GameProcess.ReadPointer(EventQueuePtr), out ioEvent);

            for (int i = 0; i < clamp; i++)
            {
                string tempName = GameProcess.ReadString((IntPtr)ioEvent.m_iTarget, 256) ?? "";
                string tempCommand = GameProcess.ReadString((IntPtr)ioEvent.m_iTargetInput, 256) ?? "";
                string tempParam = GameProcess.ReadString((IntPtr)ioEvent.m_VariantValue[0], 256) ?? "";

                if (tempName.CompareWildcard(targetName)
                    && tempCommand.ToLower().CompareWildcard(command.ToLower())
                    && tempParam.CompareWildcard(param))
                {
                    ret = ioEvent.m_flFireTime;
                    goto end;
                }
                else
                {
                    IntPtr nextPtr = (IntPtr)ioEvent.m_pNext;
                    if (nextPtr != IntPtr.Zero)
                    {
                        GameProcess.ReadValue(nextPtr, out ioEvent);
                        continue;
                    }
                    else goto end; // end early if we've hit the end of the list
                }
            }

            end:
#if false
            Debug.WriteLine($"found output with target name \"{targetName}\" command \"{command}\" param \"{param}\" fire time : {ret}");
#endif
            return ret;
        }
#endregion
    }

#region GAME STRUCTS

    [StructLayout(LayoutKind.Sequential)]
    public struct CEntInfoV1
    {
        public uint m_pEntity;
        public int m_SerialNumber;
        public uint m_pPrev;
        public uint m_pNext;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct CEntInfoV2
    {
        public uint m_pEntity;
        public int m_SerialNumber;
        public uint m_pPrev;
        public uint m_pNext;
        public int m_targetname;
        public int m_classname;

        public IntPtr EntityPtr => (IntPtr)this.m_pEntity;

        public static CEntInfoV2 FromV1(CEntInfoV1 v1)
        {
            var ret = new CEntInfoV2();
            ret.m_pEntity = v1.m_pEntity;
            ret.m_SerialNumber = v1.m_SerialNumber;
            ret.m_pPrev = v1.m_pPrev;
            ret.m_pNext = v1.m_pNext;
            return ret;
        }
    }

    // taken from source sdk
    [StructLayout(LayoutKind.Sequential)]
    public struct ScreenFadeInfo
    {
        public float Speed;            // How fast to fade (tics / second) (+ fade in, - fade out)
        public float End;              // When the fading hits maximum
        public float Reset;            // When to reset to not fading (for fadeout and hold)
        public byte R, G, B, Alpha;    // Fade color
        public uint Flags;              // Fading flags
    };

    // todo: figure out a way to utilize ehandles
    [StructLayout(LayoutKind.Sequential)]
    public unsafe struct EventQueuePrioritizedEvent
    {
        public float m_flFireTime;
        public uint m_iTarget;
        public uint m_iTargetInput;
        public uint m_pActivator;       // EHANDLE
        public uint m_pCaller;          // EHANDLE
        public int m_iOutputID;
        public uint m_pEntTarget;       // EHANDLE
        // variant_t m_VariantValue, class, only relevant members
        // most notable is v_union which stores the parameters of the i/o event
        public fixed uint m_VariantValue[5];
        public uint m_pNext;
        public uint m_pPrev;
        /*
        // the aformentioned union has a different size for se2003, so we'll have to hack our way around
        public fixed uint m_Other[15];
        public uint m_pNext => GameMemory.IsSource2003 ? m_Other[13] : m_Other[0];
        public uint m_pPrev => GameMemory.IsSource2003 ? m_Other[14] : m_Other[1];
        */
    };

    public enum CEntInfoSize
    {
        Source2003 = 4 * 3,
        HL2 = 4 * 4,
        Portal2 = 4 * 6
    }
#endregion
}
