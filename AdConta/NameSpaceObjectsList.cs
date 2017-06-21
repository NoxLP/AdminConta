using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Collections;
using OfficeOpenXml;
using System.IO;
using Extensions;
using ModuloGestion.ObjModels;

namespace NameSpaceObjectsList
{
    public class NamespaceObjectsList
    {
        public NamespaceObjectsList(string[] namespaceNames)
        {
            _NamespaceNames = namespaceNames;
            GetAllTypes();
        }

        #region fields
        private string[] _NamespaceNames;
        private Assembly[] _DomainAssemblies;
        #endregion

        #region properties
        public IEnumerable<Type> Types { get; private set; }
        public string[] NameSpaceNames { get { return this._NamespaceNames; } }
        #endregion

        #region helpers
        private void GetAllTypes()
        {
            _DomainAssemblies = AppDomain.CurrentDomain.GetAssemblies();
            Types = _DomainAssemblies
                .SelectMany(a => a.GetTypes())
                .Where(t => 
                    (t.IsValueType || t.IsClass) &&
                    _NamespaceNames.Contains(t.Namespace) &&
                    !t.IsAbstract);
        }
        #endregion

        #region public methods
        public string PrintTypesWithPropsFields(string filePath, bool onlySettableProperties = false, bool backingFields = false, bool toConsole = false)
        {
            if (File.Exists(filePath)) File.Delete(filePath);
            var file = new FileInfo(filePath);
            var package = new ExcelPackage(file);
            ExcelWorksheet wSheet = package.Workbook.Worksheets.Add("prueba1");

            string result = "";
            int i = 1;
            int j = 1;
            int c = 1;
            int fila = 1;
            int ultimoMaximo = 1;
            foreach(Type t in Types)
            {
                //clase o struct
                c = (j % 2) == 0 ? 3 : 1;//(i * 2) - 1;
                
                string clas;
                if (t.IsClass) clas = "class";
                else clas = "struct";

                result = result.Append(Environment.NewLine, i.ToString(), ".: ", clas, " " , t.Name, Environment.NewLine);
                wSheet.Cells[fila, c].Style.Border.Top.Style = OfficeOpenXml.Style.ExcelBorderStyle.Thick;
                wSheet.Cells[fila, c].Style.Border.Bottom.Style = OfficeOpenXml.Style.ExcelBorderStyle.Thick;
                wSheet.Cells[fila, c].Value = string.Concat(i.ToString(), ".: ", clas, " ", t.Name);
                wSheet.Cells[fila, c].Style.Font.Bold = true;
                wSheet.Cells[fila, c].Style.Font.Size = 18.0f;

                //properties y fields
                var pInfos = t.GetProperties(BindingFlags.Public | BindingFlags.Static | BindingFlags.Instance | BindingFlags.FlattenHierarchy).ToList();
                if(onlySettableProperties)
                    pInfos = pInfos.Where(pInfo => pInfo.GetSetMethod() != null).ToList();

                var fInfos = t.GetFields(BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Instance | BindingFlags.FlattenHierarchy).ToList();
                if(!backingFields)
                    //http://stackoverflow.com/questions/40820102/reflection-returns-backing-fields-of-read-only-properties
                    fInfos = fInfos.Where(fInfo => fInfo.GetCustomAttribute<CompilerGeneratedAttribute>() == null).ToList();

                //Get all inherited fields up the hierarchy => BindingFlags.FlattenHierarchy only works with public members
                bool inheritance = t.BaseType != null;
                Type inheritedT = t;
                Type baseT;
                while (inheritance)
                {
                    baseT = inheritedT.BaseType;
                    var baseFInfos = baseT.GetFields(BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.FlattenHierarchy).ToList();
                    if(!backingFields)
                        baseFInfos = baseFInfos.Where(fInfo => fInfo.GetCustomAttribute<CompilerGeneratedAttribute>() == null).ToList();

                    fInfos = fInfos.Union(baseFInfos).ToList();
                    inheritance = baseT.BaseType != null;
                    inheritedT = baseT;
                }

                string tab4 = "    ";
                string tab5 = "     ";
                string tab6 = "      ";
                string fields = Environment.NewLine + tab4 + "-Fields:" + Environment.NewLine;
                int f = fila + 1;
                wSheet.Cells[f, c].Value = "-Fields:";
                wSheet.Cells[f, c].Style.Font.Bold = true;
                wSheet.Cells[f, c].Style.Font.Size = 14.0f;
                wSheet.Cells[f, c].Style.Border.Bottom.Style = OfficeOpenXml.Style.ExcelBorderStyle.Thick;
                
                foreach (FieldInfo fInfo in fInfos)
                {
                    Type fType = fInfo.FieldType;
                    fields = fields.Append(tab4, tab5, "+Name: ", fInfo.Name, Environment.NewLine);
                    wSheet.Cells[f + 1, c].Value = "+Name: " + fInfo.Name;

                    if (fType.IsGenericType && typeof(IEnumerable).IsAssignableFrom(fType) && !typeof(string).IsAssignableFrom(fType))
                    {
                        if (typeof(IDictionary).IsAssignableFrom(fType))
                        {
                            Type keysType = fType.GenericTypeArguments[0]; //keys types of dummy dict
                            Type valuesType = fType.GenericTypeArguments[1]; //values types of dummy dict
                            fields = fields.Append(tab4, tab6, "Type: ", fType.Name, "<", keysType.Name, ",", valuesType.Name, ">", Environment.NewLine);
                            wSheet.Cells[f + 2, c].Value = string.Concat("Type: ", fType.Name, "<", keysType.Name, ",", valuesType.Name, ">");
                        }
                        else
                        {
                            Type genericType = fType.GenericTypeArguments[0];
                            fields = fields.Append(tab4, tab6, "Type: ", fType.Name, "<", genericType.Name, ">", Environment.NewLine);
                            wSheet.Cells[f + 2, c].Value = string.Concat("Type: ", fType.Name, "<", genericType.Name, ">");
                        }
                    }
                    else
                    {
                        fields = fields.Append(tab4, tab6, "Type: ", fType.Name, Environment.NewLine);
                        wSheet.Cells[f + 2, c].Value = "Type: " + fType.Name;
                    }
                    wSheet.Cells[f + 2, c].Style.Border.Bottom.Style = OfficeOpenXml.Style.ExcelBorderStyle.Thin;
                    f = f + 2;
                }

                string props = Environment.NewLine + tab4 + "-Properties:" + Environment.NewLine;
                int p = f + 2 + 1;
                wSheet.Cells[p, c].Style.Border.Top.Style = OfficeOpenXml.Style.ExcelBorderStyle.Thin;
                wSheet.Cells[p, c].Value = "-Properties:";
                wSheet.Cells[p, c].Style.Font.Size = 14.0f;
                wSheet.Cells[p, c].Style.Font.Bold = true;
                wSheet.Cells[p, c].Style.Border.Bottom.Style = OfficeOpenXml.Style.ExcelBorderStyle.Thick;
                wSheet.Cells[p, c].Style.Border.Top.Style = OfficeOpenXml.Style.ExcelBorderStyle.Thick;

                foreach (PropertyInfo pInfo in pInfos)
                {
                    Type pType = pInfo.PropertyType;
                    props = props.Append(tab4, tab5, "+Name: ", pInfo.Name, Environment.NewLine);
                    wSheet.Cells[p + 1, c].Value = "+Name: " + pInfo.Name;

                    if (pType.IsGenericType && typeof(IEnumerable).IsAssignableFrom(pType) && !typeof(string).IsAssignableFrom(pType))
                    {
                        if (typeof(IDictionary).IsAssignableFrom(pType))
                        {
                            Type keysType = pType.GenericTypeArguments[0]; //keys types of dummy dict
                            Type valuesType = pType.GenericTypeArguments[1]; //values types of dummy dict
                            props = props.Append(tab4, tab6, "Type: ", pType.Name, "<", keysType.Name, ",", valuesType.Name, ">", Environment.NewLine);
                            wSheet.Cells[p + 2, c].Value = string.Concat("Type: ", pType.Name, "<", keysType.Name, ",", valuesType.Name, ">");
                        }
                        else
                        {
                            Type genericType = pType.GenericTypeArguments[0];
                            props = props.Append(tab4, tab6, "Type: ", pType.Name, "<", genericType.Name, ">", Environment.NewLine);
                            wSheet.Cells[p + 2, c].Value = string.Concat("Type: ", pType.Name, "<", genericType.Name, ">");
                        }
                    }
                    else
                    {
                        props = props.Append(tab4, tab6, "Type: ", pType.Name, Environment.NewLine);
                        wSheet.Cells[p + 2, c].Value = "Type: " + pType.Name;
                    }
                    wSheet.Cells[p + 2, c].Style.Border.Bottom.Style = OfficeOpenXml.Style.ExcelBorderStyle.Thin;
                    p = p + 2;
                }

                result = result.Append(fields, props, Environment.NewLine, "---------------", Environment.NewLine);

                wSheet.Column(c).AutoFit();
                ExcelRange typeRange = wSheet.Cells[1, c, p + 2, c];
                typeRange.Style.Border.Left.Style = OfficeOpenXml.Style.ExcelBorderStyle.Thin;
                typeRange.Style.Border.Right.Style = OfficeOpenXml.Style.ExcelBorderStyle.Thin;

                if ((j % 2) == 0)
                {
                    fila = Math.Max(ultimoMaximo, p + 1);
                    ultimoMaximo = fila;
                }
                else
                    ultimoMaximo = p + 1;
                i++;
                j++;
            }

            package.Save();

            if (toConsole)
            {
                Console.WriteLine(result);
                Console.ReadLine();
            }
            return result;
        }
        #endregion
    }

}
