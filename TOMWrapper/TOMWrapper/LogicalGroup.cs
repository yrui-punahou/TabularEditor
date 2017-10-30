﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing.Design;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TabularEditor.PropertyGridUI;

namespace TabularEditor.TOMWrapper
{
    public sealed class LogicalGroups: IEnumerable<LogicalGroup>
    {
        public static readonly LogicalGroups Singleton = new LogicalGroups();

        public const string TABLES = "Tables";
        public const string ROLES = "Roles";
        public const string PERSPECTIVES = "Perspectives";
        public const string TRANSLATIONS = "Translations";
        public const string RELATIONSHIPS = "Relationships";
        public const string DATASOURCES = "Data Sources";
        public const string TABLEPARTITIONS = "Table Partitions";
        public const string EXPRESSIONS = "Shared Expressions";

        public readonly LogicalGroup DataSources = new LogicalGroup(DATASOURCES);
        public readonly LogicalGroup Perspectives = new LogicalGroup(PERSPECTIVES);
        public readonly LogicalGroup Relationships = new LogicalGroup(RELATIONSHIPS);
        public readonly LogicalGroup Roles = new LogicalGroup(ROLES);
        public readonly LogicalGroup Expressions = new LogicalGroup(EXPRESSIONS);
        public readonly LogicalGroup Partitions = new LogicalGroup(TABLEPARTITIONS);
        public readonly LogicalGroup Tables = new LogicalGroup(TABLES);
        public readonly LogicalGroup Translations = new LogicalGroup(TRANSLATIONS);

        private IEnumerable<LogicalGroup> Groups()
        {
            yield return DataSources;
            yield return Perspectives;
            yield return Relationships;
            yield return Roles;
            if(TabularModelHandler.Singleton.CompatibilityLevel >= 1400) yield return Expressions;
            yield return Partitions;
            yield return Tables;
            yield return Translations;
        }

        public IEnumerator<LogicalGroup> GetEnumerator()
        {
            if (TabularModelHandler.Singleton.UsePowerBIGovernance)
                return Groups().Where(grp => PowerBI.PowerBIGovernance.AllowGroup(grp.Name)).GetEnumerator();

            return Groups().GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }

    [TypeConverter(typeof(DynamicPropertyConverter))]
    public sealed class LogicalGroup: ITabularNamedObject, ITabularObjectContainer, IDynamicPropertyObject
    {

        [ReadOnly(true)]
        public string Name { get; set; }
        public TranslationIndexer TranslatedNames { get { return null; } }
        public ObjectType ObjectType { get { return ObjectType.Group; } }
        public Model Model { get { return TabularModelHandler.Singleton.Model; } }
        public event PropertyChangedEventHandler PropertyChanged;

        public IEnumerable<ITabularNamedObject> GetChildren()
        {
            switch (Name)
            {
                case LogicalGroups.TABLES: return Model.Tables;
                case LogicalGroups.ROLES: return Model.Roles;
                case LogicalGroups.EXPRESSIONS: return Model.Expressions;
                case LogicalGroups.PERSPECTIVES: return Model.Perspectives;
                case LogicalGroups.TRANSLATIONS: return Model.Cultures;
                case LogicalGroups.RELATIONSHIPS: return Model.Relationships;
                case LogicalGroups.DATASOURCES: return Model.DataSources;
                case LogicalGroups.TABLEPARTITIONS: return Model.Tables.Where(t => !(t is CalculatedTable)).Select(t => t.PartitionViewTable);
            }
            return Enumerable.Empty<TabularNamedObject>();
        }



        public bool CanDelete()
        {
            return false;
        }

        public bool CanDelete(out string message)
        {
            message = Messages.CannotDeleteObject;
            return false;
        }

        public void Delete()
        {
            throw new NotSupportedException();
        }

        [Editor(typeof(TabularEditor.PropertyGridUI.ClonableObjectCollectionEditor<Perspective>), typeof(UITypeEditor)), TypeConverter(typeof(StringConverter))]
        public PerspectiveCollection Perspectives { get { return Model.Perspectives; } }

        [Editor(typeof(TabularEditor.PropertyGridUI.CultureCollectionEditor), typeof(UITypeEditor)), TypeConverter(typeof(StringConverter))]
        public CultureCollection Cultures { get { return Model.Cultures; } }

        [Editor(typeof(TabularEditor.PropertyGridUI.ClonableObjectCollectionEditor<ModelRole>), typeof(UITypeEditor)), TypeConverter(typeof(StringConverter))]
        public ModelRoleCollection Roles { get { return Model.Roles; } }

        public bool Browsable(string propertyName)
        {
            switch(Name)
            {
                case LogicalGroups.PERSPECTIVES: return propertyName == "Perspectives";
                case LogicalGroups.TRANSLATIONS: return propertyName == "Cultures";
                case LogicalGroups.ROLES: return propertyName == "Roles";
                default:
                    return propertyName == "Name"; // For all other groups, only show the name.
            }
        }

        public bool Editable(string propertyName)
        {
            return false;
        }

        [Browsable(false)]
        public int MetadataIndex {
            get {
                return -1;
            }
        }

        internal LogicalGroup(string name)
        {
            Name = name;
        }
    }
}
