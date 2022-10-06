using LiveSplit.SourceSplit.Utilities;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace LiveSplit.SourceSplit.ComponentHandling
{
    public partial class SessionsForm : Form
    {
        private static SessionsForm _instance;
        private List<MapTime> _mapTimes = new List<MapTime>();

        public static SessionsForm Instance
        {
            get
            {
                if (_instance == null)
                    _instance = new SessionsForm();
                return _instance;
            }
        }

        public SessionsForm()
        {
            InitializeComponent();

            listSessions.SetColumns(
                "#", 40, 
                "Map", 200, 
                "Total Time", 100, 
                "Active Time", 100, 
                "Paused Time", 100,
                "Offset Time", 100
                );

            listMaps.SetColumns(
                "Map", 200,
                "Times Entered", 80,
                "Total Time", 100,
                "Active Time", 100,
                "Paused Time", 100);
        }

        public void Add(SessionList list)
        {
            var cur = list.Last();
            listSessions.Add(new string[]
            {
                list.Count.ToString(),
                list.Select(x => x.MapName).Distinct().Count().ToString() + " maps",
                chkTickTime.Checked ? list.Sum(x => x.TotalTicks).ToString() : TimeSpan.FromTicks(list.Sum(x => x.TotalTime.Ticks)).ToStringCustom(),
                chkTickTime.Checked ? list.Sum(x => x.ActiveTicks).ToString() : TimeSpan.FromTicks(list.Sum(x => x.ActiveTime.Ticks)).ToStringCustom(),
                chkTickTime.Checked ? list.Sum(x => x.PausedTicks).ToString() : TimeSpan.FromTicks(list.Sum(x => x.PausedTime.Ticks)).ToStringCustom(),
                chkTickTime.Checked ? list.Sum(x => x.OffsetTicks).ToString() : TimeSpan.FromTicks(list.Sum(x => x.OffsetTime.Ticks)).ToStringCustom(),
            },
            new string[]
            {
                list.Count.ToString(),
                cur.MapName,
                chkTickTime.Checked ? cur.TotalTicks.ToString() : cur.TotalTime.ToStringCustom(),
                chkTickTime.Checked ? cur.ActiveTicks.ToString() : cur.ActiveTime.ToStringCustom(),
                chkTickTime.Checked ? cur.PausedTicks.ToString() : cur.PausedTime.ToStringCustom(),
                chkTickTime.Checked ? cur.OffsetTicks.ToString() : cur.OffsetTime.ToStringCustom(),
            });

            //-----------------

            listMaps.Clear();

            var item = _mapTimes.FirstOrDefault(x => x.Name == cur.MapName);
            if (item == null)
            {
                _mapTimes.Add(new MapTime 
                { 
                    Name = cur.MapName, 
                    TimesEntered = 1,
                    TotalTime = cur.TotalTime,
                    ActiveTime = cur.ActiveTime,
                    PausedTime = cur.PausedTime
                });
                item = _mapTimes.Last();
            }
            else
            {
                item.TotalTime += cur.TotalTime;
                item.ActiveTime += cur.ActiveTime;
                item.PausedTime += cur.PausedTime;

                if (list.Count > 2 && cur.MapName != list[list.Count - 2].MapName)
                    item.TimesEntered++;
            }

            listMaps.Add(new string[]
            {
                _mapTimes.Count.ToString()+ " maps",
                _mapTimes.Sum(x => x.TimesEntered).ToString() + " transitions",
                TimeSpan.FromTicks(_mapTimes.Sum(x => x.TotalTime.Ticks)).ToStringCustom(),
                TimeSpan.FromTicks(_mapTimes.Sum(x => x.ActiveTime.Ticks)).ToStringCustom(),
                TimeSpan.FromTicks(_mapTimes.Sum(x => x.PausedTime.Ticks)).ToStringCustom(),
            },
            _mapTimes.ConvertAll(x => new string[]
            {
                x.Name,
                x.TimesEntered.ToString(),
                x.TotalTime.ToStringCustom(),
                x.ActiveTime.ToStringCustom(),
                x.PausedTime.ToStringCustom(),
            }).ToArray());
        }

        private class MapTime
        {
            public string Name;
            public int TimesEntered;
            public TimeSpan TotalTime = TimeSpan.Zero;
            public TimeSpan ActiveTime = TimeSpan.Zero;
            public TimeSpan PausedTime = TimeSpan.Zero;
        }

        public void Clear()
        {
            listMaps.Clear();
            listSessions.Clear();
            _mapTimes = new List<MapTime>();
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            this.Hide();
            e.Cancel = true;
        }
    }
}
