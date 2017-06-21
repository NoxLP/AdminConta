using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AdConta.Models;
using ModuloGestion.ObjModels;
using ModuloContabilidad.ObjModels;

//namespace AdConta.ModelControl
//{
//    /// <summary>
//    /// Control and store ALL object models. Just for internal use, to add object models use static AppModelControlMessenger.
//    /// </summary>
//    public class AppModelControl
//    {
//        public AppModelControl()
//        {
//            AppModelControlMessenger.ModelAddedEvent += OnModelAddedEvent;
//            AppModelControlMessenger.ObjModelAskedEvent += OnObjModelAskedEvent;

//            this._Comunidades = new Dictionary<int, Comunidad>();
//            this._Personas = new Dictionary<int, Persona>();
//            this._Conceptos = new Dictionary<int, Concepto>();
//        }

//        #region fields
//        private Dictionary<int, Comunidad> _Comunidades;
//        private Dictionary<int, Persona> _Personas;
//        private Dictionary<int, Concepto> _Conceptos;
//        #endregion

//        #region public methods
//        public void UnsubscribeModelControlEvents()
//        {
//            AppModelControlMessenger.ModelAddedEvent -= OnModelAddedEvent;
//            AppModelControlMessenger.ObjModelAskedEvent -= OnObjModelAskedEvent;
//        }
//        #endregion

//        #region events
//        /// <summary>
//        /// Add object e.Model to the corresponding dictionary, WITHOUT checking if owners exists. The model have to be asked first with 
//        /// AppModelControlMessenger.AskForModel
//        /// </summary>
//        /// <param name="sender"></param>
//        /// <param name="e"></param>
//        private void OnModelAddedEvent(object sender, ModelControlEventArgs e)
//        {
//            //TypeSwitch.Case<>(x=>)
//            TypeSwitch.Do(e.ObjectModel,
//                TypeSwitch.Case<Comunidad>(x => 
//                {
//                    Comunidad model = (Comunidad)e.ObjectModel;
//                    this._Comunidades.Add(model.Id, model);
//                }),
//                TypeSwitch.Case<Persona>(x => 
//                {
//                    Persona model = (Persona)e.ObjectModel;
//                    this._Personas.Add(model.Id, model);
//                }),
//                TypeSwitch.Case<Concepto>(x => 
//                {
//                    Concepto model = (Concepto)e.ObjectModel;
//                    this._Conceptos.Add(model.Id, model);
//                })
//#if (MGESTION)
//                ,
//                TypeSwitch.Case<ComunidadGestion>(x =>
//                {
//                    ComunidadGestion model = (ComunidadGestion)e.ObjectModel;

//                    this._Comunidades[model.IdOwnerComunidad].SetCdadGestion(ref model);
//                }),
//                TypeSwitch.Case<Finca>(x =>
//                {
//                    Finca model = (Finca)e.ObjectModel;

//                    this._Comunidades[model.IdOwnerComunidad]._Fincas.Add(model.Id, model);
//                }),
//                /*TypeSwitch.Case<Cuota>(x =>
//                {
//                    Cuota model = (Cuota)e.Model;

//                    this._Comunidades[model.OwnerIdCdad]._Fincas[model.OwnerIdFinca].Cuotas.Add(model.Id, model);
//                }),*/
//                TypeSwitch.Case<Recibo>(x =>
//                {

//                })
//#endif
//#if (MCONTABILIDAD)
//                ,
//                TypeSwitch.Case<ComunidadContabilidad>(x =>
//                {
//                    ComunidadContabilidad model = (ComunidadContabilidad)e.ObjectModel;

//                    this._Comunidades[model.IdOwnerComunidad].SetCdadContabilidad(ref model);
//                })
//#endif
//            );
//        }
//        private void OnObjModelAskedEvent(ref object sender, ModelControlEventArgs e)
//        {
//            object objModel = null;
//            int id;

//            /*TypeSwitch.Do(e.ObjectModel,
//                TypeSwitch.Case<Comunidad>(x => ModelExists = this._Comunidades.ContainsKey(((Comunidad)e.ObjectModel).Id)),
//                TypeSwitch.Case<Persona>(x => ModelExists = this._Personas.ContainsKey(((Persona)e.ObjectModel).Id)),
//                TypeSwitch.Case<Concepto>(x => ModelExists = this._Conceptos.ContainsKey(((Concepto)e.ObjectModel).Id))
//                );*/

//            TypeSwitch.Do(e.ObjectModel,
//                TypeSwitch.Case<Comunidad>(x =>
//                {
//                    id = ((Comunidad)e.ObjectModel).Id;
//                    if (this._Comunidades.ContainsKey(id)) objModel = this._Comunidades[id];
//                    else objModel = null;
//                }),
//                TypeSwitch.Case<Persona>(x => 
//                {
//                    id = ((Persona)e.ObjectModel).Id;
//                    if (this._Personas.ContainsKey(id)) objModel = this._Personas[id];
//                    else objModel = null;
//                }),
//                TypeSwitch.Case<Concepto>(x =>
//                {
//                    id = ((Concepto)e.ObjectModel).Id;
//                    if (this._Conceptos.ContainsKey(id)) objModel = this._Conceptos[id];
//                    else objModel = null;
//                })
//#if (MGESTION)
//                ,
//                TypeSwitch.Case<ComunidadGestion>(x =>
//                {
//                    id = ((ComunidadGestion)e.ObjectModel).IdOwnerComunidad;
//                    if (this._Comunidades.ContainsKey(id) && this._Comunidades[id].CdadGestion.IdOwnerComunidad == id)
//                        objModel = this._Comunidades[id].CdadGestion;
//                    else objModel = null;
//                })
//#endif
//#if (MCONTABILIDAD)
//                ,
//                TypeSwitch.Case<ComunidadContabilidad>(x =>
//                {
//                    id = ((ComunidadContabilidad)e.ObjectModel).IdOwnerComunidad;
//                    if (this._Comunidades.ContainsKey(id) && this._Comunidades[id].CdadContabilidad.IdOwnerComunidad == id)
//                        objModel = this._Comunidades[id].CdadContabilidad;
//                    else objModel = null;
//                })
//#endif
//                );

//            AppModelControlMessenger.SetMsgFromAppModelcontrol(ref sender, ref objModel);
//        }
//        #endregion
//    }

//}
