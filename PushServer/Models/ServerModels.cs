using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace PushServer.Models
{
    public class ServerModels
    {
        public string ServerId = string.Empty;
        public string ModuleId = string.Empty;
        public string GroupId = string.Empty;
        public string ConnectionId = string.Empty;
        public string ConnectionName = string.Empty;
        public string ConnectionType = string.Empty;
        public string ProjectId = string.Empty;
        public string IsCache = "0";
        public string CreateTime = string.Empty;
        public List<JObject> listMessage = new List<JObject>();

    }
}