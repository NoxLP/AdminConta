using System;
using System.Collections.Generic;
using System.Linq;
using System.Security;
using System.Text;
using System.Threading.Tasks;
using QBuilder;
using Dapper;
using System.Data.SqlClient;

namespace AdConta.Models
{
    public class Usuario
    {
        #region properties
        public int Id { get; private set; }
        public string Nick { get; private set; }
        public bool Logged { get; private set; }
        #endregion

        #region helpers
        private bool IsCorrectPassword(string password)
        {
            //TODO: 
            // - Numero de caracteres
            // - ¿numeros y letras y signos?
            // - ¿contraseñas comunes (tablas)?
            return true;
        }
        #endregion

        #region public methods
        public bool NuevoUsuarioYLoguea(string nick, string password)
        {
            if (this.Logged || !IsCorrectPassword(password))
                return false;

            string salt = BCrypt.Net.BCrypt.GenerateSalt();
            string hash = BCrypt.Net.BCrypt.HashPassword(password, salt);

            QueryBuilder qBuilder = new QueryBuilder();
            string[] columns = new string[2] { "Nick", "Contraseña" };
            qBuilder
                .InsertInto("usuario")
                .InsertFirstColumns(columns)
                .InsertValues()
                .InsertValues(columns)
                .CloseBrackets();
            qBuilder.StoreParameter("Nick", nick);
            qBuilder.StoreParameter("Contraseña", password);

            string connectionString = GlobalSettings.Properties.Settings.Default.conta1ConnectionString;
            bool newUserOk = false;
            using (SqlConnection con = new SqlConnection(connectionString))
            {
                con.Open();

                var result = con.Execute(qBuilder.Query, qBuilder.Parameters);

                if (result == 1)
                {
                    newUserOk = true;
                    this.Nick = nick;
                    this.Logged = true;

                    result = con.Query<int>("SELECT LAST_INSERT_ID").Single();
                    this.Id = result;
                }

                con.Close();
            }

            return newUserOk;
        }
        public bool ChangePassword(string newPassword)
        {
            if (!this.Logged || !IsCorrectPassword(newPassword))
                return false;

            string salt = BCrypt.Net.BCrypt.GenerateSalt();
            string hash = BCrypt.Net.BCrypt.HashPassword(newPassword, salt);

            QueryBuilder qBuilder = new QueryBuilder();
            string[] columns = new string[1] { "Contraseña" };
            qBuilder
                .Update("usuario")
                .UpdateSet(columns)
                .Where(new SQLCondition("Id", "@id"));
            qBuilder.StoreParameter("Id", this.Id);
            qBuilder.StoreParameter("Contraseña", newPassword);

            string connectionString = GlobalSettings.Properties.Settings.Default.conta1ConnectionString;
            bool passwUpdated = false;
            using (SqlConnection con = new SqlConnection(connectionString))
            {
                con.Open();

                var result = con.Execute(qBuilder.Query, qBuilder.Parameters);

                if (result == 1) passwUpdated = true;

                con.Close();
            }

            return passwUpdated;
        }
        public bool CheckPasswordYLoguea(string nick, string password)
        {
            if (this.Logged || !IsCorrectPassword(password))
                return false;

            QueryBuilder qBuilder = new QueryBuilder();
            SQLCondition condition = new SQLCondition("Nick", "@nick");
            qBuilder
                .Select(new string[] { "Id", "Contraseña" })
                .From("usuario")
                .Where(condition);
            qBuilder.StoreParameter("nick", nick);

            string connectionString = GlobalSettings.Properties.Settings.Default.conta1ConnectionString;
            string hash;
            int id;
            using (SqlConnection con = new SqlConnection(connectionString))
            {
                con.Open();
                var result = con.Query(qBuilder.Query, qBuilder.Parameters).SingleOrDefault();
                hash = result.Contraseña;
                id = result.Id;
                con.Close();
            }

            if (!BCrypt.Net.BCrypt.Verify(password, hash))
                return false;

            this.Nick = nick;
            this.Logged = true;
            this.Id = id;
            return true;
        }
        #endregion
    }
}
