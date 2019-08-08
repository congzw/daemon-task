using System;
using System.Collections.Generic;
using Common;

namespace DaemonApp.ViewModel
{
    public class MyConfig : SimpleConfig
    {
        public MyConfig()
        {
            ProcessInfos = new List<SimpleProcessInfo>();
            EntryForm = "DaemonForm";
        }

        public string EntryForm { get; set; }

        public bool IsEntryForm()
        {
            return "DaemonForm".Equals(EntryForm, StringComparison.OrdinalIgnoreCase);
        }
        
        public WindowServiceInfo ServiceInfo { get; set; }

        public IList<SimpleProcessInfo> ProcessInfos { get; set; }
    }

    public class WindowServiceInfo
    {
        public string ServiceName { get; set; }
        public string ServicePath { get; set; }
        public string ServiceFriendlyName { get; set; }
    }
}