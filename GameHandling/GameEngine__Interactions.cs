using LiveSplit.ComponentUtil;
using LiveSplit.SourceSplit.Utilities;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;

namespace LiveSplit.SourceSplit.GameHandling
{
    public abstract partial class GameEngine
    {
        #region RETRIEVAL FUNCTIONS

        /// <summary>
        /// Gets the current Host State
        /// </summary>
        /// <returns>The current Host State</returns>
        public virtual HostState GetHostState()
        {
            return (HostState)GameProcess.ReadValue<int>(HostStatePtr);
        }

        /// <summary>
        /// Gets the current Sign On State
        /// </summary>
        /// <returns>The current Sign On State</returns>
        public virtual SignOnState GetSignOnState()
        {
            return (SignOnState)GameProcess.ReadValue<int>(SignOnStatePtr);
        }

        /// <summary>
        /// Gets the current Server State
        /// </summary>
        /// <returns>The current Server State</returns>
        public virtual ServerState GetServerState()
        {
            return (ServerState)GameProcess.ReadValue<int>(ServerStatePtr);
        }

        protected const int MAX_ENTITIES = 2048;

        /// <summary>
        /// Gets the entity info of the entity with the specified index
        /// </summary>
        /// <param name="index">The index of the entity</param>
        /// <returns>The info of the entity with the specified index. If there is no such entity, a new CEntInfoV2 will be returned</returns>
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
        /// Gets the entity pointer to the first entity whose name passes the name condition
        /// </summary>
        /// <param name="nameCond">The name condition</param>
        /// <returns>The first entity whose name passes the name condition. If there is no such entity, IntPtr.Zero will be returned</returns>
        public IntPtr GetEntityByName(string nameCond)
        {
            IntPtr ret = IntPtr.Zero;
            GetEntitiesByName(nameCond).Any(x =>
            {
                ret = x;
                return true;
            });
            return ret;
        }

        /// <summary>
        /// Gets all of the pointers to entities whose name passes the name condition
        /// </summary>
        /// <param name="nameCond">The name condition to compare against</param>
        /// <returns>All of the pointers to entities whose name passes the name condition</returns>
        public virtual IEnumerable<IntPtr> GetEntitiesByName(string nameCond)
        {
            CEntInfoV2 nextPtr = this.GetEntInfoByIndex(0);
            var ret = IntPtr.Zero;
            do
            {
                if (nextPtr.EntityPtr == IntPtr.Zero)
                {
                    yield break;
                }

                IntPtr namePtr;
                this.GameProcess.ReadPointer(nextPtr.EntityPtr + BaseEntityTargetNameOffset, false, out namePtr);
                if (namePtr != IntPtr.Zero)
                {
                    this.GameProcess.ReadString(namePtr, ReadStringType.ASCII, 32, out string n);  // TODO: find real max len
                    if (n.CompareWildcard(nameCond))
                    {
#if DEBUG
                        Debug.WriteLine($"found entity \"{n}\" ptr : {ret.ToString("X")}");
#endif
                        yield return nextPtr.EntityPtr;
                    }
                }
                nextPtr = GameProcess.ReadValue<CEntInfoV2>((IntPtr)nextPtr.m_pNext);
            }
            while (nextPtr.EntityPtr != IntPtr.Zero);

        }

        /// <summary>
        /// Gets the index of the first entity whose name passes the name condition
        /// </summary>
        /// <param name="nameCond">The name condition</param>
        /// <returns>The index of the first entity whose name passes the name condition. If no such entity exists, -1 will be returned.</returns>
        public int GetEntIndexByName(string nameCond)
        {
            int ret = -1;
            GetEntIndexesByName(nameCond).Any(x =>
            {
                ret = x;
                return true;
            });
            return ret;
        }

        /// <summary>
        /// Gets all of the indexes of the entities whose name passes the name condition
        /// </summary>
        /// <param name="nameCond">The name condition</param>
        /// <returns>All of the indexes of the entities whose name passes the name condition</returns>
        public virtual IEnumerable<int> GetEntIndexesByName(string nameCond)
        {
            int ret = -1;
            for (int i = 0; i < MAX_ENTITIES; i++)
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
                if (n.CompareWildcard(nameCond))
                {
#if DEBUG
                    Debug.WriteLine($"found entity \"{n}\" index : {ret}");
#endif
                    yield return i;
                }
            }
        }

        /// <summary>
        /// Gets the index of the first entity whose position is near or exactly at a given location
        /// </summary>
        /// <param name="x">Target X coordinate</param>
        /// <param name="y">Target Y coordinate</param>
        /// <param name="z">Target Z coordinate</param>
        /// <param name="d">Distance to the target location</param>
        /// <param name="xy">Whether to ignore the Z coordinate</param>
        /// <returns>The index of the first entity whose position is near or exactly at a given location. If no such entity exists, -1 will be returned.</returns>
        public int GetEntIndexByPos(float x, float y, float z, float d = 0f, bool xy = false)
        {
            int ret = -1;
            GetEntIndexesByPos(x, y, z, d, xy).Any(x =>
            {
                ret = x;
                return true;
            });
            return ret;
        }

        /// <summary>
        /// Gets all of the indexes of the entities whose position is near or exactly at a given location
        /// </summary>
        /// <param name="x">Target X coordinate</param>
        /// <param name="y">Target Y coordinate</param>
        /// <param name="z">Target Z coordinate</param>
        /// <param name="d">Distance to the target location</param>
        /// <param name="xy">Whether to ignore the Z coordinate</param>
        /// <returns>All of the indexes of the entities whose position is near or exactly at the given location</returns>
        public virtual IEnumerable<int> GetEntIndexesByPos(float x, float y, float z, float d = 0f, bool xy = false)
        {
            Vector3f pos = new Vector3f(x, y, z);

            for (int i = 0; i < MAX_ENTITIES; i++)
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
                        yield return i;
                    }
                }
                else // check for distance if it's a non-static entity like an npc or a prop
                {
                    if (xy && newpos.DistanceXY(pos) <= d && i != 1)
                    {
#if DEBUG
                        Debug.WriteLine($"found entity with pos xy dist <= {d} from {pos} @ {i}");
#endif
                        yield return i;

                    }
                    else if (newpos.Distance(pos) <= d && i != 1)
                    {
#if DEBUG
                        Debug.WriteLine($"found entity with pos dist <= {d} from {pos} @ {i}");
#endif
                        yield return i;

                    }
                }
            }
        }

        public int GetEntIndexByPos(Vector3f vec, float d = 0f, bool xy = false) => GetEntIndexByPos(vec.X, vec.Y, vec.Z, d, xy);
        public IEnumerable<int> GetEntIndexesByPos(Vector3f vec, float d = 0f, bool xy = false) => GetEntIndexesByPos(vec.X, vec.Y, vec.Z, d, xy);

        /// <summary>
        /// Gets the position of the entity with the provided index
        /// </summary>
        /// <param name="i">Index of the entity</param>
        /// <returns>The position of the entity with the provided index. If no such entity exists, (0, 0, 0) will be returned.</returns>
        public virtual Vector3f GetEntityPos(int index)
        {
            Vector3f pos;
            var ent = GetEntInfoByIndex(index);
            GameProcess.ReadValue(ent.EntityPtr + BaseEntityAbsOriginOffset, out pos);
            return pos;
        }

        // env_fades don't hold any live fade information and instead they network over fade infos to the client which add it to a list
        /// <summary>
        /// Finds the end time of a fade in or out with the specified speed
        /// </summary>
        /// <param name="speed">The speed of the fade</param>
        /// <returns>The end time of a fade in or out with the specified speed. If no such fade exists, 0 will be returned.</returns>
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
        /// Finds the end time of a fade in or out with the specified speed and RGB color value
        /// </summary>
        /// <param name="speed">The speed of the fade</param>
        /// <param name="r">The red color value of the fade's color</param>
        /// <param name="g">The green color value of the fade's color</param>
        /// <param name="b">The blue color value of the fade's color</param>
        /// <returns>The end time of a fade in or out with the specified speed and RGB color value. If no such fade exists, 0 will be returned.</returns>
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
        /// Finds the fire time of a queued output whose targetname matches the name condition.
        /// </summary>
        /// <param name="targetNameCondition">The name condition</param>
        /// <param name="clamp">The maximum number of inputs to check</param>
        /// <returns>The fire time of a queued output whose name matches the name condition. If no such output exists, 0 will be returned.</returns>
        public virtual float GetOutputFireTime(string targetNameCondition, int clamp = 100)
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
                if (tempName.CompareWildcard(targetNameCondition))
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
        /// Finds the fire time of a queued output whose targetname, command, and param all pass their respective string conditions
        /// </summary>
        /// <param name="targetName">The string condtion for targetname</param>
        /// <param name="command">The string condtion for command</param>
        /// <param name="param">The string condtion for param</param>
        /// <param name="clamp">The maximum number of inputs to check</param>
        /// <returns>Tthe fire time of a queued output whose targetname, command, and param all pass their respective string conditions. If no such output exists, 0 will be returned.</returns>
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
