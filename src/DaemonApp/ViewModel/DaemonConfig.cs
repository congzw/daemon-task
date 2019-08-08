using System;
using System.Collections.Generic;
using Common;

namespace DaemonApp.ViewModel
{
    public class DaemonConfig : SimpleConfig
    {
        public DaemonConfig()
        {
            ProcessInfos = new List<SimpleProcessInfo>();
            EntryForm = "DaemonForm";
        }

        public string EntryForm { get; set; }

        public bool IsEntryForm()
        {
            return "DaemonForm".Equals(EntryForm, StringComparison.OrdinalIgnoreCase);
        }

        public IList<SimpleProcessInfo> ProcessInfos { get; set; }
    }
}