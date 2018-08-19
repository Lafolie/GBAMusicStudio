using BrightIdeasSoftware;
using GBAMusicStudio.Core;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;

namespace GBAMusicStudio.UI
{
    [System.ComponentModel.DesignerCategory("")]
    internal class TrackEditor : ThemedForm
    {
        int currentTrack = 0;
        List<SongEvent> events;

        readonly ObjectListView trackListView;
        readonly ThemedLabel[] labels = new ThemedLabel[3];
        readonly ThemedNumeric[] args = new ThemedNumeric[3];

        readonly ComboBox tracksBox, commandsBox;
        readonly ThemedButton changeVoicesButton, addEventButton, removeEventButton;
        readonly ThemedNumeric[] trackVoiceDetails = new ThemedNumeric[2]; //repurposed [0] ("from") to show original value

        readonly ComboBox globalRemapsBox;
        readonly ThemedButton globalRemapFromButton, remapToButton, globalChangeVoicesButton;
        readonly ThemedNumeric[] gvArgs = new ThemedNumeric[2];

        internal TrackEditor()
        {
            //configure window
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            Text = "GBA Music Studio ― Track Editor";
            ClientSize = new Size(600, 400);

            int paneWidth = 300 - 12 - 6, panelHeight = 400 - 24;

            //setup component panels
            trackListView = new ObjectListView
            {
                FullRowSelect = true,
                HeaderStyle = ColumnHeaderStyle.Nonclickable,
                HideSelection = false,
                Location = new Point(12, 12),
                MultiSelect = false,
                RowFormatter = RowFormatter,
                ShowGroups = false,
                Size = new Size(paneWidth, panelHeight),
                UseFiltering = true,
                UseFilterIndicator = true
            };

            OLVColumn c1, c2, c3, c4;
            c1 = new OLVColumn("Event", "Command.Name");
            c2 = new OLVColumn("Arguments", "Command.Arguments") { UseFiltering = false };
            c3 = new OLVColumn("Offset", "Offset") { AspectToStringFormat = "0x{0:X}", UseFiltering = false };
            c4 = new OLVColumn("Ticks", "AbsoluteTicks") { UseFiltering = false };
            c1.Width = c2.Width = c3.Width = 72;
            c4.Width = 45;
            c1.Hideable = c2.Hideable = c3.Hideable = c4.Hideable = false;
            c1.TextAlign = c2.TextAlign = c3.TextAlign = c4.TextAlign = HorizontalAlignment.Center;
            trackListView.AllColumns.AddRange(new OLVColumn[] { c1, c2, c3, c4 });
            trackListView.RebuildColumns();
            trackListView.SelectedIndexChanged += SelectedIndexChanged;
            trackListView.ItemActivate += ListView_ItemActivate;

            int h2 = panelHeight / 3 - 4;
            var eventEditPanel = new ThemedPanel { Location = new Point(306, 12), Size = new Size(paneWidth, h2) };
            var trackEditPanel = new ThemedPanel { Location = new Point(306, 140), Size = new Size(paneWidth, h2 - 1) };
            var globalEditPanel = new ThemedPanel { Location = new Point(306, 267), Size = new Size(paneWidth, h2) };

            // Arguments numericals
            for (int i = 0; i < 3; i++)
            {
                int y = 17 + (33 * i);
                labels[i] = new ThemedLabel
                {
                    AutoSize = true,
                    Location = new Point(52, y + 3),
                    Text = "Arg. " + (i + 1).ToString(),
                    Visible = false,
                };

                args[i] = new ThemedNumeric
                {
                    Location = new Point(paneWidth - 152, y),
                    Maximum = int.MaxValue,
                    Minimum = int.MinValue,
                    Size = new Size(100, 25),
                    Visible = false
                };
                args[i].ValueChanged += ArgumentChanged;
                eventEditPanel.Controls.AddRange(new Control[] { labels[i], args[i] });
            }

            // Track controls
            tracksBox = new ComboBox
            {
                Location = new Point(4, 4),
                Size = new Size(100, 21)
            };
            tracksBox.SelectedIndexChanged += TracksBox_SelectedIndexChanged;

            changeVoicesButton = new ThemedButton
            {
                Location = new Point(13, 30),
                Text = "Change Voices"
            };
            changeVoicesButton.Click += ChangeEvents;

            addEventButton = new ThemedButton
            {
                Location = new Point(13, 30 + 25 + 5),
                Text = "Add Event"
            };
            addEventButton.Click += AddEvent;

            commandsBox = new ComboBox
            {
                Location = new Point(115, 30 + 25 + 5 + 2),
                Size = new Size(100, 21)
            };
            removeEventButton = new ThemedButton
            {
                Location = new Point(13, 30 + 25 + 5 + 25 + 5),
                Text = "Remove Event"
            };
            removeEventButton.Click += RemoveEvent;

            //Track controls box preparation
            tracksBox.Enabled = changeVoicesButton.Enabled = addEventButton.Enabled = removeEventButton.Enabled = commandsBox.Enabled = false;
            changeVoicesButton.Size = addEventButton.Size = removeEventButton.Size = new Size(95, 25);
            var originalVoiceLabel = new ThemedLabel { Location = new Point(115, 30 + 2 + 3), Text = "From" };
            trackVoiceDetails[0] = new ThemedNumeric { Location = new Point(149, 30 + 2) };
            var newVoiceIDLabel = new ThemedLabel { Location = new Point(204, 30 + 2 + 3), Text = "To" };
            trackVoiceDetails[1] = new ThemedNumeric { Location = new Point(224, 30 + 2) };
            trackVoiceDetails[0].Maximum = trackVoiceDetails[1].Maximum = 0xFF;
            trackVoiceDetails[0].Size = trackVoiceDetails[1].Size = new Size(45, 23);
            trackVoiceDetails[0].TextAlign = trackVoiceDetails[1].TextAlign = HorizontalAlignment.Center;
            originalVoiceLabel.AutoSize = newVoiceIDLabel.AutoSize = true;

            trackEditPanel.Controls.AddRange(new Control[] { tracksBox, changeVoicesButton, originalVoiceLabel, newVoiceIDLabel, trackVoiceDetails[0], trackVoiceDetails[1], addEventButton, commandsBox, removeEventButton });

            // Global controls
            globalRemapsBox = new ComboBox
            {
                DataSource = Config.InstrumentRemaps.Keys.ToArray(),
                Location = new Point(4, 4),
                Size = new Size(100, 21)
            };

            globalRemapFromButton = new ThemedButton
            {
                Location = new Point(116, 3),
                Text = "From"
            };
            globalRemapFromButton.Click += (s, e) => ApplyRemap(true);

            remapToButton = new ThemedButton
            {
                Location = new Point(203, 3),
                Text = "To"
            };
            remapToButton.Click += (s, e) => ApplyRemap(false);

            globalChangeVoicesButton = new ThemedButton
            {
                Location = new Point(13, 30),
                Size = new Size(95, 25),
                Text = "Change Voices"
            };
            globalChangeVoicesButton.Click += ChangeAllEvents;

            //Global remap controls box preparation
            var gvFrom = new ThemedLabel { Location = new Point(115, 30 + 2 + 3), Text = "From" };
            gvArgs[0] = new ThemedNumeric { Location = new Point(149, 30 + 2) };
            var gvTo = new ThemedLabel { Location = new Point(204, 30 + 2 + 3), Text = "To" };
            gvArgs[1] = new ThemedNumeric { Location = new Point(224, 30 + 2) };
            gvArgs[0].Maximum = gvArgs[1].Maximum = 0xFF;
            gvArgs[0].Size = gvArgs[1].Size = new Size(45, 23);
            gvArgs[0].TextAlign = gvArgs[1].TextAlign = HorizontalAlignment.Center;
            gvFrom.AutoSize = gvTo.AutoSize = true;
            globalRemapsBox.Enabled = globalRemapFromButton.Enabled = remapToButton.Enabled = globalChangeVoicesButton.Enabled = false;

            globalEditPanel.Controls.AddRange(new Control[] { globalRemapsBox, globalRemapFromButton, remapToButton, globalChangeVoicesButton, gvFrom, gvTo, gvArgs[0], gvArgs[1] });

            //fin
            Controls.AddRange(new Control[] { trackListView, eventEditPanel, trackEditPanel, globalEditPanel });

            UpdateTracks();
        }

        void ListView_ItemActivate(object sender, EventArgs e)
        {
            SongPlayer.SetPosition(((SongEvent)trackListView.SelectedItem.RowObject).AbsoluteTicks);
        }

        void AddEvent(object sender, EventArgs e)
        {
            var cmd = (ICommand)Activator.CreateInstance(Engine.GetCommands()[commandsBox.SelectedIndex].GetType());
            var ev = new SongEvent(0xFFFFFFFF, cmd);
            int index = trackListView.SelectedIndex + 1;
            SongPlayer.Song.InsertEvent(ev, currentTrack, index);
            SongPlayer.RefreshSong();
            LoadTrack(currentTrack);
            SelectItem(index);
        }

        void RemoveEvent(object sender, EventArgs e)
        {
            if (trackListView.SelectedIndex == -1)
                return;
            SongPlayer.Song.RemoveEvent(currentTrack, trackListView.SelectedIndex);
            SongPlayer.RefreshSong();
            LoadTrack(currentTrack);
        }

        void RowFormatter(OLVListItem item)
        {
            var e = (SongEvent)item.RowObject;
            if (e.Command is GoToCommand || e.Command is CallCommand || e.Command is ReturnCommand || e.Command is FinishCommand)
                item.BackColor = Color.MediumSpringGreen;
            else if (e.Command is VoiceCommand)
                item.BackColor = Color.DarkSalmon;
            else if (e.Command is RestCommand)
                item.BackColor = Color.PaleVioletRed;
            else if (e.Command is KeyShiftCommand || e.Command is NoteCommand || e.Command is EndOfTieCommand || e.Command is FreeNoteCommand)
                item.BackColor = Color.SkyBlue;
            else if (e.Command is ModDepthCommand || e.Command is ModTypeCommand)
                item.BackColor = Color.LightSteelBlue;
            else if (e.Command is TuneCommand || e.Command is BendCommand || e.Command is BendRangeCommand)
                item.BackColor = Color.MediumPurple;
            else if (e.Command is PanpotCommand || e.Command is LFODelayCommand || e.Command is LFOSpeedCommand)
                item.BackColor = Color.GreenYellow;
            else if (e.Command is TempoCommand)
                item.BackColor = Color.DeepSkyBlue;
            else
                item.BackColor = Color.SteelBlue;
        }

        void ApplyRemap(bool from)
        {
            bool changed = false;
            string remap = (string)globalRemapsBox.SelectedItem;
            foreach (var track in SongPlayer.Song.Commands)
                foreach (var ev in track)
                    if (ev.Command is VoiceCommand voice)
                    {
                        voice.Voice = Config.GetRemap(voice.Voice, remap, from);
                        changed = true;
                    }
            if (changed)
            {
                SongPlayer.RefreshSong();
                LoadTrack(currentTrack);
            }
        }

        void ChangeEvents(object sender, EventArgs e)
        {
            bool changed = false;
            foreach (var ev in events)
                if (sender == changeVoicesButton && ev.Command is VoiceCommand voice && voice.Voice == trackVoiceDetails[0].Value)
                {
                    voice.Voice = (byte)trackVoiceDetails[1].Value;
                    changed = true;
                }
            if (changed)
            {
                SongPlayer.RefreshSong();
                LoadTrack(currentTrack);
            }
        }

        void ChangeAllEvents(object sender, EventArgs e)
        {
            bool changed = false;
            foreach (var track in SongPlayer.Song.Commands)
                foreach (var ev in track)
                    if (sender == globalChangeVoicesButton && ev.Command is VoiceCommand voice && voice.Voice == gvArgs[0].Value)
                    {
                        voice.Voice = (byte)gvArgs[1].Value;
                        changed = true;
                    }
            if (changed)
            {
                SongPlayer.RefreshSong();
                LoadTrack(currentTrack);
            }
        }

        void LoadTrack(int track)
        {
            currentTrack = track;
            events = SongPlayer.Song.Commands[track];
            trackListView.SetObjects(events);
            SelectedIndexChanged(null, null);
        }

        void TracksBox_SelectedIndexChanged(object sender, EventArgs e) => LoadTrack(tracksBox.SelectedIndex);
        internal void UpdateTracks()
        {
            bool tracks = SongPlayer.NumTracks > 0;
            tracksBox.Enabled = changeVoicesButton.Enabled = addEventButton.Enabled = removeEventButton.Enabled = commandsBox.Enabled = globalChangeVoicesButton.Enabled = tracks;

            tracksBox.DataSource = Enumerable.Range(1, SongPlayer.NumTracks).Select(i => $"Track {i}").ToList();
            globalRemapsBox.Enabled = globalRemapFromButton.Enabled = remapToButton.Enabled = tracks && globalRemapsBox.Items.Count > 0;

            commandsBox.DataSource = Engine.GetCommands().Select(c => c.Name).ToList();

            if (!tracks)
                trackListView.Items.Clear();
        }

        void SelectItem(int index)
        {
            trackListView.Items[index].Selected = true;
            trackListView.Select();
            trackListView.EnsureVisible(index);
        }

        void ArgumentChanged(object sender, EventArgs e)
        {
            for (int i = 0; i < 3; i++)
            {
                if (sender == args[i])
                {
                    var se = events[trackListView.SelectedIndices[0]];
                    object value = args[i].Value;
                    var m = se.Command.GetType().GetMember(labels[i].Text)[0];
                    if (m is FieldInfo f)
                        f.SetValue(se.Command, Convert.ChangeType(value, f.FieldType));
                    else if (m is PropertyInfo p)
                        p.SetValue(se.Command, Convert.ChangeType(value, p.PropertyType));
                    SongPlayer.RefreshSong();

                    var control = ActiveControl;
                    int index = trackListView.SelectedIndex;
                    LoadTrack(currentTrack);
                    SelectItem(index);
                    control.Select();

                    return;
                }
            }
        }

        void SelectedIndexChanged(object sender, EventArgs e)
        {
            if (trackListView.SelectedIndices.Count != 1)
            {
                labels[0].Visible = labels[1].Visible = labels[2].Visible =
                    args[0].Visible = args[1].Visible = args[2].Visible = false;
            }
            else
            {
                var se = (SongEvent)trackListView.SelectedObject;
                var ignore = typeof(ICommand).GetMembers();
                var mi = se.Command == null ? new MemberInfo[0] : se.Command.GetType().GetMembers().Where(m => !ignore.Any(a => m.Name == a.Name) && (m is FieldInfo || m is PropertyInfo)).ToArray();
                for (int i = 0; i < 3; i++)
                {
                    labels[i].Visible = args[i].Visible = i < mi.Length;
                    if (args[i].Visible)
                    {
                        labels[i].Text = mi[i].Name;

                        args[i].ValueChanged -= ArgumentChanged;

                        dynamic m = mi[i];

                        args[i].Hexadecimal = se.Command is CallCommand || se.Command is GoToCommand;

                        TypeInfo valueType;
                        if (mi[i].MemberType == MemberTypes.Field)
                            valueType = m.FieldType;
                        else
                            valueType = m.PropertyType;
                        args[i].Maximum = valueType.DeclaredFields.Single(f => f.Name == "MaxValue").GetValue(m);
                        args[i].Minimum = valueType.DeclaredFields.Single(f => f.Name == "MinValue").GetValue(m);

                        object value = m.GetValue(se.Command);
                        args[i].Value = (decimal)Convert.ChangeType(value, TypeCode.Decimal);

                        args[i].ValueChanged += ArgumentChanged;
                    }
                }
            }
            labels[0].Parent.Refresh();
        }
    }
}
