#region Copyright (C) 2007-2012 Team MediaPortal

/*
    Copyright (C) 2007-2012 Team MediaPortal
    http://www.team-mediaportal.com

    This file is part of MediaPortal 2

    MediaPortal 2 is free software: you can redistribute it and/or modify
    it under the terms of the GNU General Public License as published by
    the Free Software Foundation, either version 3 of the License, or
    (at your option) any later version.

    MediaPortal 2 is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
    GNU General Public License for more details.

    You should have received a copy of the GNU General Public License
    along with MediaPortal 2. If not, see <http://www.gnu.org/licenses/>.
*/

#endregion

using System;
using System.IO;
using System.Xml;
using System.Xml.Serialization;
using MediaPortal.Plugins.SlimTv.Interfaces;
using MediaPortal.Plugins.SlimTv.Interfaces.Items;

namespace MediaPortal.Plugins.SlimTv.UPnP.Items
{
  public class Program : IProgramRecordingStatus
  {
    private static XmlSerializer _xmlSerializer;

    public Program()
    {}

    //public Program(WebProgramDetailed webProgram, int serverIndex)
    //{
    //  ServerIndex = serverIndex;
    //  Description = webProgram.Description;
    //  StartTime = webProgram.StartTime;
    //  EndTime = webProgram.EndTime;
    //  Genre = webProgram.Genre;
    //  Title = webProgram.Title;
    //  ChannelId = webProgram.IdChannel;
    //  ProgramId = webProgram.Id;
    //  RecordingStatus = GetRecordingStatus(webProgram);
    //}

    //public static RecordingStatus GetRecordingStatus(WebProgramBasic programDetailed)
    //{
    //  RecordingStatus recordingStatus = RecordingStatus.None;
    //  if (programDetailed.IsScheduled)
    //    recordingStatus |= RecordingStatus.Scheduled;
    //  return recordingStatus;
    //}

    //public static RecordingStatus GetRecordingStatus(WebProgramDetailed programDetailed)
    //{
    //  RecordingStatus recordingStatus = RecordingStatus.None;
    //  if (programDetailed.IsRecording || programDetailed.IsRecordingOnce || programDetailed.IsRecordingSeries)
    //    recordingStatus |= RecordingStatus.Recording;
    //  if (programDetailed.IsScheduled || programDetailed.IsRecordingOncePending)
    //    recordingStatus |= RecordingStatus.Scheduled;
    //  if (programDetailed.IsRecordingSeriesPending)
    //    recordingStatus |= RecordingStatus.SeriesScheduled;
    //  return recordingStatus;
    //}

    #region IProgram Member

    public int ServerIndex { get; set; }
    public int ProgramId { get; set; }
    public int ChannelId { get; set; }
    public string Title { get; set; }
    public string Description { get; set; }
    public string Genre { get; set; }
    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }
    public RecordingStatus RecordingStatus { get; set; }

    #endregion

    /// <summary>
    /// Serializes this Program instance to the given <paramref name="writer"/>.
    /// </summary>
    /// <param name="writer">Writer to write the XML serialization to.</param>
    public void Serialize(XmlWriter writer)
    {
      XmlSerializer xs = GetOrCreateXMLSerializer();
      xs.Serialize(writer, this);
    }

    /// <summary>
    /// Deserializes a Program instance from a given XML fragment.
    /// </summary>
    /// <param name="str">XML fragment containing a serialized user profile instance.</param>
    /// <returns>Deserialized instance.</returns>
    public static Program Deserialize(string str)
    {
      XmlSerializer xs = GetOrCreateXMLSerializer();
      using (StringReader reader = new StringReader(str))
        return xs.Deserialize(reader) as Program;
    }

    /// <summary>
    /// Deserializes a Program instance from a given <paramref name="reader"/>.
    /// </summary>
    /// <param name="reader">XML reader containing a serialized user profile instance.</param>
    /// <returns>Deserialized instance.</returns>
    public static Program Deserialize(XmlReader reader)
    {
      XmlSerializer xs = GetOrCreateXMLSerializer();
      return xs.Deserialize(reader) as Program;
    }

    protected static XmlSerializer GetOrCreateXMLSerializer()
    {
      return _xmlSerializer ?? (_xmlSerializer = new XmlSerializer(typeof(Program)));
    }
  }
}