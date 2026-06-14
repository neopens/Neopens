using Neopens.FrameworkLite.Interfaces;
using System;

namespace Neopens.FrameworkLite.Core
{
    public class ActionResult : IActionResult
    {
        public int State { get; set; }

        public  object Content { get; set; }

        public string Message { get; set; }

        public DateTime Time { get; set; }

        public ActionResult()
        {
            Time  = DateTime.Now;
            Message =  string.Empty;
        }
        public override string ToString()
        {
            return $"[{Time.ToString("yyyy-MM-dd HH:mm:ss:fff")}] State:{State} | Message:{Message} | Content:{Content?.ToString() ?? "null"}";
        }
    }

    public class ActionResult<T> : ActionResult, IActionResult<T>
    {
        public new T Content { get => (T)base.Content; set => base.Content = value; }
    }
}
