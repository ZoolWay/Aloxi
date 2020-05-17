﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace ZoolWay.Aloxi.Bridge.Loxone.Data
{
    public class LoxAppModel
    {
        public class UserModel
        {
            public String Name { get; set; }
            public Guid Uuid { get; set; }
            public bool IsAdmin { get; set; }
            public bool ChangePassword { get; set; }
            public int UserRights { get; set; }
        }

        public class MsInfoModel
        {
            public string SerialNr { get; set; }
            public string MsName { get; set; }
            public string ProjectName { get; set; }
            public string LocalUrl { get; set; }
            public string RemoteUrl { get; set; }
            public int TempUnit { get; set; }
            public string Currency { get; set; }
            public string SquareMeasure { get; set; }
            public string Location { get; set; }
            public string LanguageCode { get; set; }
            public string CatTitle { get; set; }
            public string RoomTitle { get; set; }
            public UserModel CurrentUser { get; set; }
        }

        public class PartnerInfoModel
        {
            public string Name { get; set; }
        }

        public class RoomModel
        {
            public Guid Uuid { get; set; }
            public string Name { get; set; }
            public string Image { get; set; }
            public RoomType Type { get; set; }
        }

        public class CategoryModel
        {
            public Guid Uuid { get; set; }
            public string Name { get; set; }
            public string Image { get; set; }
            public string Type { get; set; }
        }

        public enum RoomType
        {
            Unknown,
            Bedroom,
            Recreational = 2,
            Thorughfare = 3,
            Central,
            Other
        }

        [JsonConverter(typeof(StringEnumConverter))]
        public enum ControlTypeModel // EnumMember(Value = "")
        {
            Switch,
            Dimmer,
            Jalousie,
            InfoOnlyDigital,
            InfoOnlyAnalog,
            Pushbutton,
            ValueSelector,
            LightController,
            Daytimer,
            CentralJalousie,
        }

        public class ControlModel
        {
            public string Name { get; set; }
            public ControlTypeModel Type { get; set; }
            public Guid UuidAction { get; set; }
            public Guid Room { get; set; }
            public Guid Cat { get; set; }
            public Dictionary<string, Guid> States { get; set; }
            public Dictionary<string, Guid> SubControls { get; set; }
        }

        public DateTime LastModified { get; set; }
        public MsInfoModel MsInfo { get; set; }
        public PartnerInfoModel PartnerInfo { get; set; }
        public Dictionary<string, RoomModel> Rooms { get; set; }
        public Dictionary<string, CategoryModel> Cats { get; set; }
        public Dictionary<string, ControlModel> Controls { get; set; }
    }
}
