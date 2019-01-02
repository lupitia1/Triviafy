using System.Collections.Generic;

namespace TriviaFy.JsonModels
{
    public class Album
    {
        public string href { get; set; }
        public List<Image> images { get; set; }
        public string name { get; set; }
    }
}