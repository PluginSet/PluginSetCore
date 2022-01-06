using System;

namespace PluginSet.Core.Editor
{
    public class BuildException: Exception
    {
        public BuildException(string msg)
            :base(msg)
        {
            
        }
        
    }
}