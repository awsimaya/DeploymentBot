using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DeploymentBotWeb.Models
{
    public class CelebrityModel
    {
        public string CelebrityName { get; set; }
        public float Confidence { get; set; }
        public bool IsJeff { get; set; }
    }
}
