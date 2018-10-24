using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Microsoft.AspNet.SignalR;
using System.Threading.Tasks;
using System.Diagnostics;
using NLog;
using PushServer.BLL;
using Newtonsoft.Json.Linq;
using System.Text;
using Newtonsoft.Json;
using PushServer.Models;

namespace PushServer
{
    public class ServerHub : Hub
    {
        private Logger logger = LogManager.GetCurrentClassLogger();
        private static ServerManager severManager = new ServerManager();
        public void Hello()
        {
            Clients.All.hello();
        }
        public void Test(object o)
        {
            logger.Info(JsonConvert.SerializeObject(o));
            Clients.Client(Context.ConnectionId).RecvMessage(Context.ConnectionId, JsonConvert.SerializeObject(o));

        }
        /// <summary>
        /// 供客户端调用的服务器端代码
        /// </summary>
        /// <param name="message"></param>
        public void SendMessage(object data)
        {
            string dt = JsonConvert.SerializeObject(data);
            logger.Info(dt);
            try
            {
                JObject o = JObject.Parse(dt);
                string moduleId = o["ModuleId"].ToString();
                string[] groupIds = o["GroupId"].ToString().Split(',');
                string connectionName = o["RecvName"].ToString();
                string content = o["Data"].ToString();
                ServerModels send = severManager.GetModelsByConnectionId(Context.ConnectionId);
                string type = o["Type"].ToString();
                foreach (string group in groupIds)
                {
                    if (string.IsNullOrEmpty(group))
                    {
                        continue;
                    }
                    List<ServerModels> list = severManager.GetRecvConnections(Context.ConnectionId, moduleId, group, connectionName);
                    foreach (var item in list)
                    {
                        try
                        {
                            //封装消息
                            JObject obj = new JObject();
                            obj["ServerId"] = item.ServerId;
                            obj["SendName"] = send.ConnectionName;
                            obj["SendType"] = send.ConnectionType;
                            obj["RecvName"] = item.ConnectionName;
                            obj["RecvType"] = item.ConnectionType;
                            obj["ModuleId"] = moduleId;
                            obj["GroupId"] = group;
                            obj["Type"] = type;
                            obj["Content"] = content;
                            //添加消息
                            severManager.AddMessageToModels(obj, item.ServerId);
                            //发送消息
                            Clients.Client(item.ConnectionId).RecvMessage( JsonConvert.SerializeObject(severManager.GetMessageFromModels(item.ServerId)));
                            logger.Info(JsonConvert.SerializeObject(obj));
                        }
                        catch (Exception e)
                        {
                            severManager.DelConnection(Context.ConnectionId, "", "");
                            logger.Error(e.Message);
                        }

                    }
                }
            }
            catch(Exception e)
            {
                logger.Error(e.Message);
            }
        }
        public void JoinGroup(object data)
        {
            string dt = JsonConvert.SerializeObject(data);
            logger.Info(dt);
            try
            {
                JObject o = JObject.Parse(dt);
                string projectId = o["ProjectId"].ToString();
                string connectionName = o["SendName"].ToString();
                string connectionType = o["SendType"].ToString();
                string moduleId = o["ModuleId"].ToString();
                string[] groupIds = o["GroupId"].ToString().Split(',');
                string isCache = o["Config"]["IsCache"].ToString();
                string content = "JoinGroup";// o["Data"].ToString();
                string type = MessageType.JOINGROUP;
                foreach (string group in groupIds)
                {
                    if (string.IsNullOrEmpty(group))
                    {
                        continue;
                    }
                    List<ServerModels> list = severManager.AddConnection(projectId, this.Context.ConnectionId, connectionName, connectionType, moduleId, group, isCache);
                    foreach (var item in list)
                    {
                        try
                        {
                            JObject obj = new JObject();
                            obj["ServerId"] = item.ServerId;
                            obj["SendName"] = connectionName;
                            obj["SendType"] = connectionType;
                            obj["RecvName"] = item.ConnectionName;
                            obj["RecvType"] = item.ConnectionType;
                            obj["ModuleId"] = moduleId;
                            obj["GroupId"] = group;
                            obj["Type"] = type;
                            obj["Content"] = content;
                            //添加消息
                            severManager.AddMessageToModels(obj, item.ServerId);
                            Clients.Client(item.ConnectionId).RecvMessage(JsonConvert.SerializeObject(severManager.GetMessageFromModels(item.ServerId)));
                            logger.Info(JsonConvert.SerializeObject(obj));
                        }
                        catch (Exception e)
                        {
                            severManager.DelConnection(Context.ConnectionId, "", "");
                            logger.Error(e.Message);
                        }
                    }
                }
            }
            catch(Exception e)
            {
                logger.Error(e.Message);
            }
           
            
        }
        public void LeavelGroup(object data)
        {
            string dt = JsonConvert.SerializeObject(data);
            logger.Info(dt);
            try
            {
                JObject o = JObject.Parse(dt); ;
                string moduleId = o["ModuleId"].ToString();
                string[] groupIds = o["GroupId"].ToString().Split(',');
                string content = "LeavelGroup";// o["Data"].ToString();
                string type = MessageType.LEVEALGROUP;
                ServerModels send = severManager.GetModelsByConnectionId(Context.ConnectionId);
                foreach (string group in groupIds)
                {
                    if (string.IsNullOrEmpty(group))
                    {
                        continue;
                    }
                    List<ServerModels> list = severManager.DelConnection(Context.ConnectionId, moduleId, group);
                    foreach (var item in list)
                    {
                        try
                        {
                            JObject obj = new JObject();
                            obj["ServerId"] = item.ServerId;
                            obj["SendName"] = send.ConnectionName;
                            obj["SendType"] = send.ConnectionType;
                            obj["RecvName"] = item.ConnectionName;
                            obj["RecvType"] = item.ConnectionType;
                            obj["ModuleId"] = moduleId;
                            obj["GroupId"] = group;
                            obj["Type"] = type;
                            obj["Content"] = content;
                            Clients.Client(item.ConnectionId).RecvMessage( JsonConvert.SerializeObject(severManager.GetMessageFromModels(item.ServerId)));
                            logger.Info(JsonConvert.SerializeObject(obj));
                        }
                        catch (Exception e)
                        {
                            severManager.DelConnection(Context.ConnectionId, "", "");
                            logger.Error(e.Message);
                        }
                    }
                }
            }
            catch(Exception e)
            {
                logger.Error(e.Message);
            }
            
               
        }
        public void ReplyMessage(string data)
        {
            logger.Info(data);
            try
            {
                severManager.DelMessageFromModels(data);
            }
            catch(Exception e)
            {
                logger.Error(e.Message);
            }
        }
        /// <summary>
        /// 客户端连接的时候调用
        /// </summary>
        /// <returns></returns>
        public override Task OnConnected()
        {
            logger.Info(Context.ConnectionId);
            return base.OnConnected();
        }
        public override Task OnDisconnected(bool stopCalled)
        {
            logger.Info(Context.ConnectionId);
            severManager.DelConnection(Context.ConnectionId, "", "");
            return base.OnDisconnected(true);
        }
        public override Task OnReconnected()
        {
            logger.Info(Context.ConnectionId);
            return base.OnReconnected();
        }

    }
}