using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace AdConta
{
    /// <summary>
    /// Static class for adding objectModels so they don't get duplicated.
    /// Use AppModelControlMessenger.AskForObjModel(ref object sender, ref object objModel)
    /// </summary>
    public static class AppModelControlMessenger
    {
        #region fields
        /// <summary>
        /// key = sender, value = objModelAsked
        /// </summary>
        private static Dictionary<object, object> _MsgDict = new Dictionary<object, object>();
        #endregion

        #region events
        public delegate void ModelAddedEventHandler(object sender, ModelControlEventArgs e);
        public static event ModelAddedEventHandler ModelAddedEvent = delegate { };
        /// <summary>
        /// Add object Model to the corresponding dictionary, WITHOUT checking if owners exists. The model have to be asked first with
        /// AppModelControlMessenger.AskForModel
        /// </summary>
        /// <param name="objModel"></param>
        internal static void AddModel(ref object objModel)
        {
            ModelControlEventArgs e = new ModelControlEventArgs(ref objModel);

            ModelAddedEvent(null, e);
        }

        public delegate void ObjModelAskedEventHandler(ref object sender, ModelControlEventArgs e);
        public static event ObjModelAskedEventHandler ObjModelAskedEvent = delegate { };
        /// <summary>
        /// Ask AppModelControl if objModel exists. Return true if objModel exists(and vv) and, if objModel exists, it assign objModel via 
        /// ref parameter to the existing object (objModel = existingObject)
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="objModel"></param>
        /// <returns></returns>
        public static bool AskForObjModel(ref object sender, ref object objModel)
        {
            ModelControlEventArgs e = new ModelControlEventArgs(ref objModel);

            if (!_MsgDict.ContainsKey(sender)) _MsgDict.Add(sender, null);
            ObjModelAskedEvent(ref sender, e);

            if (_MsgDict[sender] == null)
            {
                _MsgDict.Remove(sender);
                AddModel(ref objModel);
                return false;
            }

            objModel = _MsgDict[sender];
            _MsgDict.Remove(sender);
            return true;

            /*bool ret =(_MsgDict[sender] == null ? false : (bool)_MsgDict[sender]);
            _MsgDict.Remove(sender);
            return ret;*/
        }
        #endregion

        #region public methods
        /// <summary>
        /// For internal use only. Don't use it unless you are modifying AppModelControl class.
        /// Set result the of a ModelAskedEvent ing the messages static dictionary.
        /// </summary>
        /// <param name="key"></param>
        /// <param name="objModel"></param>
        public static void SetMsgFromAppModelcontrol(ref object key, ref object objModel)
        {
            if (!_MsgDict.ContainsKey(key)) return;

            _MsgDict[key] = objModel;
        }
        #endregion
    }

    public class ModelControlEventArgs : EventArgs
    {
        public ModelControlEventArgs(ref object objectModel)
        {
            this._ObjectModel = objectModel;
        }

        private object _ObjectModel;
        public object ObjectModel { get { return this._ObjectModel; } }
    }
}
