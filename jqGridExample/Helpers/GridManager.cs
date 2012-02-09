using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using jqGridExample.Models;
using System.Xml.Serialization;
using System.IO;

namespace jqGridExample.Helpers
{
    public class GridManager
    {
        private string GetFilename(string name, string appDataPath)
        {
            return string.Format("{0}\\Grids\\{1}.xml", appDataPath, name);
        }
        public Grid GetGridDefinition(string name, string appDataPath)
        {
            Grid grid = null;
            XmlSerializer deserializer = new XmlSerializer(typeof(Grid));
            string filename = GetFilename(name, appDataPath);

            if (File.Exists(filename))
            {
                using (TextReader textReader = new StreamReader(filename))
                {
                    grid = (Grid)deserializer.Deserialize(textReader);
                    textReader.Close();
                }
            }
            return grid;
        }
        public void SaveGridDefinition(string name, string appDataPath, Grid grid)
        {
            XmlSerializer serializer = new XmlSerializer(typeof(Grid));
            string filename = GetFilename(name, appDataPath);
            using (TextWriter textWriter = new StreamWriter(filename))
            {
                serializer.Serialize(textWriter, grid);
                textWriter.Close();
            }
        }

        public void SyncGridDefinitions(string appDataPath)
        {
            //Sync the list of grids
            using (jqGridExampleDbContext db = new jqGridExampleDbContext())
            {
                IEnumerable<string> gridProcedures = db.GetGridProcedures();
                foreach (string procedure in gridProcedures)
                {
                    Grid savedGrid = GetGridDefinition(procedure, appDataPath);
                    IEnumerable<GridParameter> parameters = db.DeriveParameters(procedure);
                    Dictionary<string, object> parameterList = new Dictionary<string, object>();
                    foreach (GridParameter p in parameters)
                    {
                        parameterList.Add(p.Name, string.Empty);
                    }
                    IEnumerable<GridColumn> columns = db.DeriveColumnList(procedure, parameterList);
                    if (savedGrid == null)
                    {
                        //There is no existing file so build the entire grid model and persist it
                        savedGrid = new Grid();
                        savedGrid.Name = procedure;
                        savedGrid.StoredProcedure = procedure;
                        savedGrid.Title = procedure;
                        savedGrid.Columns = columns.ToList();
                        savedGrid.Parameters = parameters.ToList();

                        SaveGridDefinition(procedure, appDataPath, savedGrid);
                    }
                    else
                    {
                        //This is an existing file. Check for new or removed columns and parameters
                        var removedColumns = from col in savedGrid.Columns
                                             where !columns.Any(c => c.Name == col.Name)
                                             select col;
                        foreach (var col in removedColumns)
                        {
                            savedGrid.Columns.Remove(col);
                        }
                        var addedColumns = from col in columns
                                           where !savedGrid.Columns.Any(c => c.Name == col.Name)
                                           select col;
                        foreach (var col in addedColumns)
                        {
                            savedGrid.Columns.Add(col);
                        }
                        var removedParameters = from param in savedGrid.Parameters
                                                where !parameters.Any(p => p.Name == param.Name)
                                                select param;
                        foreach (var param in removedParameters)
                        {
                            savedGrid.Parameters.Remove(param);
                        }
                        var addedParameters = from param in parameters
                                              where !savedGrid.Parameters.Any(p => p.Name == param.Name)
                                              select param;
                        foreach (var param in addedParameters)
                        {
                            savedGrid.Parameters.Add(param);
                        }

                        SaveGridDefinition(procedure, appDataPath, savedGrid);
                    }
                }
            }
        }
    }
}