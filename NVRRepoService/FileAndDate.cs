/*  Copyright (C) 2015 NAVERTICA a.s. http://www.navertica.com 

    This program is free software; you can redistribute it and/or modify
    it under the terms of the GNU General Public License as published by
    the Free Software Foundation; either version 2 of the License, or
    (at your option) any later version.

    This program is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    GNU General Public License for more details.

    You should have received a copy of the GNU General Public License along
    with this program; if not, write to the Free Software Foundation, Inc.,
    51 Franklin Street, Fifth Floor, Boston, MA 02110-1301 USA.  */
using System;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;

namespace Navertica.SharePoint.RepoService
{
    /// <summary>
    /// Data structure to store binary data from a (script) file together with its last modified datetime
    /// </summary>
    [DataContract]
    public class FileAndDate : IComparable<FileAndDate>, IComparable<DateTime>
    {
        public FileAndDate(byte[] fileBytes, DateTime fileModified)
        {
            data = fileBytes;
            lastUpdate = fileModified;
        }

        public override bool Equals(object obj)
        {
            var date = obj as FileAndDate;
            if (date != null)
            {
                return BinaryData.SequenceEqual(date.BinaryData);
            }
            var bytes = obj as byte[];
            if (bytes != null)
            {
                return BinaryData.SequenceEqual(bytes);
            }
            if (obj is string)
                return StringUnicode.Equals(obj);

            return false;
        }
        
        [DataMember] private byte[] data;
        [DataMember] private DateTime lastUpdate;

        public byte[] BinaryData
        {
            get { return data; }
        }

        public string StringUnicode
        {
            get { return Encoding.UTF8.GetString(data); }
        }

        public DateTime LastUpdate
        {
            get { return lastUpdate; }
        }

        public int CompareTo(FileAndDate other)
        {
            if (other == null) return 1;
            if (LastUpdate > other.LastUpdate) return 1;
            if (LastUpdate == other.LastUpdate) return 0;
            return -1;
        }

        public int CompareTo(DateTime dt)
        {
            if (LastUpdate > dt) return 1;
            if (LastUpdate == dt) return 0;
            return -1;
        }

        public override string ToString()
        {
            return StringUnicode;
        }

        public override int GetHashCode()
        {
            return data.GetHashCode() * 7 * lastUpdate.GetHashCode();
        }
    }
}
