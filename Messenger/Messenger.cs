using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Messenger
{
    static public class Messenger
    {
        static private Dictionary<string, object> _msgDict = new Dictionary<string, object>();

        static public void RegisterMsg(string title, object Msg)
        {
            if (!_msgDict.ContainsKey(title))
                _msgDict.Add(title, Msg);
            else
                _msgDict[title] = Msg;
        }

        static public object SearchMsg(string title)
        {
            if (_msgDict.ContainsKey(title))
            {
                object Msg = _msgDict[title];
                _msgDict.Remove(title);
                return Msg;
            }
            else return null;
        }
    }
}
