using System.Collections.Generic;

namespace SpinCore.Translation
{
    public struct CustomLanguage
    {
        public string Name { get; set; }
        public string Id { get; set; }
        public Dictionary<string, string> Keys { get; set; }
    }
}
