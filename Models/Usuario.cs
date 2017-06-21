using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AdConta.Models
{
    public class Usuario : IObjModelBase
    {
        public Usuario(string nick, int id)
        {
            this.Nick = nick;
            this.Id = id;
        }

        #region fields
        #endregion

        #region properties
        public string Nick { get; private set; }
        public int Id { get; private set; }
        #endregion

        #region helpers
        #endregion

        #region public methods
        #endregion
    }

}
