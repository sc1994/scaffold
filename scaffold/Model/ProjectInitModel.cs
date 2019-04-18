// ReSharper disable ArrangeAccessorOwnerBody
// ReSharper disable InconsistentNaming
// ReSharper disable PartialTypeWithSinglePart
// ReSharper disable UnusedMember.Global

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Serialization;
// ReSharper disable RedundantNameQualifier

namespace scaffold.Model
{
    public partial class ProjectInitModel
    {
        [XmlIgnore]
        public string SetPath { get; set; }

        public static void AddNuget(string path, string nuget, string version)
        {
            var projectXML = File.ReadAllText(path);
            var project = XmlToObject(projectXML);
            var item = project.ItemGroup.FirstOrDefault(x => x.PackageReference?.ToList().Count > 0);

            if (item == null)
            {
                var tempList = project.ItemGroup.ToList();
                tempList.Add(new ProjectItemGroup
                {
                    PackageReference = new[]
                   {
                        new ProjectItemGroupPackageReference
                        {
                            Include = nuget,
                            Version = version
                        }
                    }
                });
                project.ItemGroup = tempList.ToArray();
                File.WriteAllText(path, ToXml(project));
            }
            else
            {
                if (item.PackageReference.All(x => x.Include == nuget))
                {
                    var tempList = item.PackageReference.ToList();
                    tempList.Add(new ProjectItemGroupPackageReference
                    {
                        Include = nuget,
                        Version = version
                    });
                    item.PackageReference = tempList.ToArray();
                    File.WriteAllText(path, ToXml(project));
                }
            }
        }

        /// <summary>
        /// 初始化标准库
        /// </summary>
        /// <returns></returns>
        public void InitNetStandard2_0()
        {
            //if (PropertyGroup == null)
            //{
            //    PropertyGroup = new ProjectPropertyGroup();
            //}
            //if (string.IsNullOrWhiteSpace(PropertyGroup.TargetFramework))
            //{
            //    PropertyGroup.TargetFramework = "netstandard2.0";
            //}
            //if (string.IsNullOrWhiteSpace(Sdk))
            //{
            //    Sdk = "Microsoft.NET.Sdk";
            //}
            //File.WriteAllText(SetPath, ToXml(this));
        }

        /// <summary>
        /// 初始化应用程序
        /// </summary>
        /// <returns></returns>
        public bool InitNetCoreApp2_2()
        {
            throw new NotImplementedException();
        }

        private static Project XmlToObject(string xml)
        {
            using (var rdr = new StringReader(xml))
            {
                var serializer = new XmlSerializer(typeof(Project));
                return (Project)serializer.Deserialize(rdr);
            }
        }

        public static string ToXml<T>(T o)
        {
            var xsSubmit = new XmlSerializer(typeof(T));
            using (var sww = new StringWriter())
            {
                using (var writer = XmlWriter.Create(sww))
                {
                    xsSubmit.Serialize(writer, o);
                    return XElement.Parse(sww.ToString()).ToString();
                }
            }
        }
    }


    // 注意: 生成的代码可能至少需要 .NET Framework 4.5 或 .NET Core/Standard 2.0。
    /// <remarks/>
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    [System.Xml.Serialization.XmlRootAttribute(Namespace = "", IsNullable = false)]
    public partial class Project
    {

        private ProjectPropertyGroup[] propertyGroupField;

        private ProjectItemGroup[] itemGroupField;

        private string sdkField;

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute("PropertyGroup")]
        public ProjectPropertyGroup[] PropertyGroup
        {
            get
            {
                return this.propertyGroupField;
            }
            set
            {
                this.propertyGroupField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute("ItemGroup")]
        public ProjectItemGroup[] ItemGroup
        {
            get
            {
                return this.itemGroupField;
            }
            set
            {
                this.itemGroupField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string Sdk
        {
            get
            {
                return this.sdkField;
            }
            set
            {
                this.sdkField = value;
            }
        }
    }

    /// <remarks/>
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    public partial class ProjectPropertyGroup
    {

        private string documentationFileField;

        private string targetFrameworkField;

        private string conditionField;

        /// <remarks/>
        public string DocumentationFile
        {
            get
            {
                return this.documentationFileField;
            }
            set
            {
                this.documentationFileField = value;
            }
        }

        /// <remarks/>
        public string TargetFramework
        {
            get
            {
                return this.targetFrameworkField;
            }
            set
            {
                this.targetFrameworkField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string Condition
        {
            get
            {
                return this.conditionField;
            }
            set
            {
                this.conditionField = value;
            }
        }
    }

    /// <remarks/>
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    public partial class ProjectItemGroup
    {

        private ProjectItemGroupReference[] referenceField;

        private ProjectItemGroupProjectReference[] projectReferenceField;

        private ProjectItemGroupPackageReference[] packageReferenceField;

        /// <remarks/>
        public ProjectItemGroupReference[] Reference
        {
            get
            {
                return this.referenceField;
            }
            set
            {
                this.referenceField = value;
            }
        }

        /// <remarks/>
        public ProjectItemGroupProjectReference[] ProjectReference
        {
            get
            {
                return this.projectReferenceField;
            }
            set
            {
                this.projectReferenceField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute("PackageReference")]
        public ProjectItemGroupPackageReference[] PackageReference
        {
            get
            {
                return this.packageReferenceField;
            }
            set
            {
                this.packageReferenceField = value;
            }
        }
    }

    /// <remarks/>
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    public partial class ProjectItemGroupReference
    {

        private string hintPathField;

        private string includeField;

        /// <remarks/>
        public string HintPath
        {
            get
            {
                return this.hintPathField;
            }
            set
            {
                this.hintPathField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string Include
        {
            get
            {
                return this.includeField;
            }
            set
            {
                this.includeField = value;
            }
        }
    }

    /// <remarks/>
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    public partial class ProjectItemGroupProjectReference
    {

        private string includeField;

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string Include
        {
            get
            {
                return this.includeField;
            }
            set
            {
                this.includeField = value;
            }
        }
    }

    /// <remarks/>
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    public partial class ProjectItemGroupPackageReference
    {

        private string includeField;

        private string versionField;

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string Include
        {
            get
            {
                return this.includeField;
            }
            set
            {
                this.includeField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string Version
        {
            get
            {
                return this.versionField;
            }
            set
            {
                this.versionField = value;
            }
        }
    }


}
