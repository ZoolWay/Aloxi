using System;
using System.ComponentModel;

namespace ZoolWay.Aloxi.Bridge.Loxone
{
    [TypeConverter(typeof(LoxoneUuidConverter))]
    [ImmutableObject(true)]
    public class LoxoneUuid
    {
        public Guid Guid { get; }
        public string SubPath { get; }

        public LoxoneUuid(string composedUuid)
        {
            var parts = ParseParts(composedUuid);
            this.Guid = parts.guid;
            this.SubPath = parts.subPath;
        }

        public LoxoneUuid(Guid guid, string subPath)
        {
            this.Guid = guid;
            this.SubPath = subPath;
        }

        public override string ToString()
        {
            string guidPart = this.Guid.ToString("D").Remove(23, 1);
            if (String.IsNullOrWhiteSpace(SubPath)) return guidPart;
            return $"{guidPart}/{SubPath}";
        }

        public static LoxoneUuid Parse(string s)
        {
            if (String.IsNullOrWhiteSpace(s)) return null;
            return new LoxoneUuid(s);
        }

        public static LoxoneUuid From(Guid guid)
        {
            return new LoxoneUuid(guid, null);
        }

        public override int GetHashCode()
        {
            return ToString().GetHashCode();
        }

        public override bool Equals(object obj)
        {
            if (obj == null) return false;
            if (Object.ReferenceEquals(obj, this)) return true;
            if (!(obj is LoxoneUuid)) return false;

            LoxoneUuid other = obj as LoxoneUuid;
            return (this.Guid.Equals(other.Guid) && String.Equals(this.SubPath, other.SubPath));
        }

        private static (Guid guid, string subPath) ParseParts(string composedUuid)
        {
            if (composedUuid == null) return (Guid.Empty, null);
            string uuidPart = composedUuid;
            string subPath = null;
            int idxSeperator = composedUuid.IndexOf("/");
            if (idxSeperator >= 0)
            {
                uuidPart = composedUuid.Substring(0, idxSeperator);
                subPath = composedUuid.Substring(idxSeperator + 1);
            }
            return (ParseUuid(uuidPart), subPath);
        }

        private static Guid ParseUuid(string uuid)
        {
            if (String.IsNullOrWhiteSpace(uuid)) return Guid.Empty;
            if ((uuid.Length == 35) && (uuid[8] == '-') && (uuid[13] == '-') && (uuid[18] == '-') && (uuid[23] != '-'))
            {
                uuid = uuid.Insert(23, "-");
            }
            return Guid.Parse(uuid);
        }
    }
}
