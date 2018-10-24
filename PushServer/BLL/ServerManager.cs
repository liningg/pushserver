using Newtonsoft.Json.Linq;
using NLog;
using PushServer.Models;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Web;

namespace PushServer.BLL
{
    public class ServerManager
    {
        private static Logger logger = LogManager.GetCurrentClassLogger();
        public static List<ServerModels> listServer = new List<ServerModels>();
        public static List<ProjectModels> listProject = new List<ProjectModels>();
        private static bool isInited = false;
        public  ServerManager()
        {
            if(!isInited)
            {
                string [] array = ConfigurationManager.AppSettings["ProjectId"].ToString().Split(',');
                foreach (var item in array)
                {
                    ProjectModels project = new ProjectModels();
                    project.ProjectId = item;
                    listProject.Add(project);
                    isInited = true;
                }
            }
        }
        public ServerModels IsExsitServerModels(string projectId, string connectionName, string connectionType,string moduleId, string groupId)
        {
            return listServer.FirstOrDefault(x => x.ProjectId == projectId && x.ModuleId == moduleId && x.GroupId == groupId && x.ConnectionName == connectionName && x.ConnectionType == connectionType);
        }
        public bool IsExsitProjectModels(string projectId)
        {
            return listProject.FirstOrDefault(x => x.ProjectId == projectId) == null ? false : true;
        }
        public ServerModels GetModelsByConnectionId(string connectionId)
        {
            return listServer.FirstOrDefault(x => x.ConnectionId == connectionId);
        }
        /// <summary>
        /// 添加链接到数据库
        /// </summary>
        /// <param name="projectCode"></param>
        /// <param name="connectionId"></param>
        /// <param name="connectionName"></param>
        /// <param name="moduleName"></param>
        /// <param name="groupName"></param>
        /// <param name="moduleGroupType"></param>
        /// <returns></returns>
        public List<ServerModels> AddConnection(string projectId,string connectionId,string connectionName,string connectionType,string moduleId,string groupId,string isCache)
        {
            logger.Info(string.Format("projectCode:{0},connectionId:{1},connectionName:{2},moduleId:{3},groupId:{4}",
                    projectId, connectionId, connectionName, moduleId, groupId));
            List<ServerModels> list = new List<ServerModels>();
            try
            {
                //先查看是否存在这个项目
                if(!IsExsitProjectModels(projectId))
                {
                    throw new Exception("没有找到这个项目");
                }
                ServerModels models = IsExsitServerModels(projectId, connectionName, connectionType, moduleId, groupId);
                ServerModels server = new ServerModels();
                server.ServerId = Guid.NewGuid().ToString();
                if (models != null)
                {
                    server.listMessage = models.listMessage;
                    listServer.RemoveAll(x => x.ProjectId == projectId && x.ModuleId == moduleId && x.GroupId == groupId && x.ConnectionName == connectionName && x.ConnectionType == connectionType);
                    server.ServerId = models.ServerId;
                }
                server.ModuleId = moduleId;
                server.GroupId = groupId;
                server.ConnectionName = connectionName;
                server.ConnectionId = connectionId;
                server.ConnectionType = connectionType;
                server.ProjectId = projectId;
                server.IsCache = isCache;
                server.CreateTime = DateTime.Now.ToString();
                listServer.Add(server);

                list = GetRecvConnections(connectionId, moduleId, groupId);
            }
            catch(Exception e)
            {
                logger.Error(e.Message);
            }
            logger.Info(list);
            return list;
        }
        public void AddMessageToModels(JObject content,string serverId)
        {
            ServerModels server = listServer.FirstOrDefault(x => x.ServerId == serverId);
            if(server != null)
            {
                listServer.FirstOrDefault(x => x.ServerId == serverId).listMessage.Add(content);
               
            }
        }

        public void ClearMessage(string serverId)
        {
            listServer.FirstOrDefault(x => x.ServerId == serverId).listMessage.Clear();
        }

        public List<JObject> GetMessageFromModels(string serverId)
        {
            return listServer.FirstOrDefault(x => x.ServerId == serverId).listMessage;
        }
        public void DelMessageFromModels(string serverId)
        {
            listServer.FirstOrDefault(x => x.ServerId == serverId).listMessage.Clear();
        }
        /// <summary>
        /// 发送消息
        /// </summary>
        /// <param name="projectCode"></param>
        /// <param name="connectionId"></param>
        /// <param name="buissinessName"></param>
        /// <param name="groupName"></param>
        /// <returns></returns>
        public List<ServerModels> GetRecvConnections(string connectionId,string moduleId,string groupId,string connectionName = "")
        {
            logger.Info(string.Format("recvConnectionIds:{0},moduleName:{1},groupName:{2}",
                       connectionName, moduleId, groupId));
            List<ServerModels> list = new List<ServerModels>();
            try
            {
                string projectId = listServer.FirstOrDefault(x => x.ConnectionId == connectionId).ProjectId;
                list = listServer.Where(x => x.ProjectId == projectId).ToList<ServerModels>();
                if(!string.IsNullOrEmpty(moduleId))
                {
                    list = list.Where(x => x.ModuleId == moduleId).ToList<ServerModels>();
                }
                if(!string.IsNullOrEmpty(groupId))
                {
                    list = list.Where(x => x.GroupId == groupId).ToList<ServerModels>();
                }
                if(!string.IsNullOrEmpty(connectionName))
                {
                    list = list.Where(x => x.ConnectionName == connectionName).ToList<ServerModels>();
                }

            }
            catch (Exception e)
            {
                logger.Error(e.Message);
            }
            logger.Info(list);
            return list;
        }
        /// <summary>
        /// 清楚单个链接
        /// </summary>
        /// <param name="projectCode"></param>
        /// <param name="connectionId"></param>
        /// <returns></returns>
        public List<ServerModels> DelConnection(string connectionId, string moduleId, string groupId)
        {
            logger.Info(string.Format("connectionId:{0},moduleName:{1},groupName:{2}",
                      connectionId, moduleId, groupId));
            List<ServerModels> list = new List<ServerModels>();
            try
            {
                list = GetRecvConnections(connectionId,moduleId, groupId);
               
                if(!string.IsNullOrEmpty(moduleId))
                {
                    if(!string.IsNullOrEmpty(groupId))
                    {
                        listServer.RemoveAll(x => x.ConnectionId == connectionId && x.ModuleId == moduleId && x.GroupId == groupId && x.IsCache != "1");
                    }
                    else
                    {
                        listServer.RemoveAll(x => x.ConnectionId == connectionId && x.ModuleId == moduleId && x.IsCache != "1");
                    }
                }
                else
                {
                    listServer.RemoveAll(x => x.ConnectionId == connectionId  && x.IsCache != "1");
                }
            }
            catch(Exception e)
            {
                logger.Error(e.Message);
            }
            logger.Info(list);
            return list;
        }
        
    }
}