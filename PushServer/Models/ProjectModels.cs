using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace PushServer.Models
{
    public class ProjectModels
    {
        private string _projectId = string.Empty;
        public string ProjectId
        {
            get
            {
                return _projectId;
            }
            set
            {
                _projectId = value;
            }
        }
        private string _projectCode = string.Empty;
        public string ProjectCode
        {
            get
            {
                return _projectCode;
            }
            set
            {
                _projectCode = value;
            }
        }
        private string _createTime = string.Empty;
        public string CreateTime
        {
            get
            {
                return _createTime;
            }
            set
            {
                _createTime = value;
            }
        }
    }
}