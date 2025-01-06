using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CrossPlatformProject2
{
    public static class CategoryManager
    {
        // Define the Categories dictionary
        public static readonly Dictionary<string, int> Categories = new Dictionary<string, int>
    {
        { "General Knowledge", 9 },
        { "Science and Nature", 17 },
        { "Entertainment: Video Games", 15 },
        { "Entertainment: Film", 11 },
        { "Music", 12 },
        { "Books", 10 },
        { "Art", 25 }
    };
    }
}
