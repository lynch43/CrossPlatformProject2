using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CrossPlatformProject2.Models
{
    class Root
    {
        public int response_code { get; set; }//response code
        public List<Result> results { get; set; }//question list
    }
}