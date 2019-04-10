// ReSharper disable ArrangeAccessorOwnerBody
// ReSharper disable InconsistentNaming
// ReSharper disable PartialTypeWithSinglePart
// ReSharper disable UnusedMember.Global

using System;
using System.IO;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Serialization;
// ReSharper disable RedundantNameQualifier

namespace scaffold.Model
{
    public partial class ProjectModel
    {
        public string SetPath { get; set; }

        /// <summary>
        /// 初始化标准库
        /// </summary>
        /// <returns></returns>
        public void InitNetStandard2_0()
        {
            if (PropertyGroup == null)
            {
                PropertyGroup = new ProjectPropertyGroup();
            }
            if (string.IsNullOrWhiteSpace(PropertyGroup.TargetFramework))
            {
                PropertyGroup.TargetFramework = "netstandard2.0";
            }
            if (string.IsNullOrWhiteSpace(Sdk))
            {
                Sdk = "Microsoft.NET.Sdk";
            }
            File.WriteAllText(SetPath, ToXml(this));
        }

        /// <summary>
        /// 初始化应用程序
        /// </summary>
        /// <returns></returns>
        public bool InitNetCoreApp2_2()
        {
            throw new NotImplementedException();
        }

        private T XmlToObject<T>(string xml)
        {
            using (var rdr = new StringReader(xml))
            {
                var serializer = new XmlSerializer(typeof(T));
                return (T)serializer.Deserialize(rdr);
            }
        }

        public string ToXml<T>(T o)
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
    [System.Serializable()]
    [System.ComponentModel.DesignerCategory("code")]
    [XmlType(AnonymousType = true)]
    [XmlRoot(Namespace = "", IsNullable = false)]
    public partial class ProjectModel
    {

        private ProjectPropertyGroup propertyGroupField;

        private ProjectItemGroup[] itemGroupField;

        private string sdkField;

        /// <remarks/>
        public ProjectPropertyGroup PropertyGroup
        {
            get
            {
                return propertyGroupField;
            }
            set
            {
                propertyGroupField = value;
            }
        }

        /// <remarks/>
        [XmlElement("ItemGroup")]
        public ProjectItemGroup[] ItemGroup
        {
            get
            {
                return itemGroupField;
            }
            set
            {
                itemGroupField = value;
            }
        }

        /// <remarks/>
        [XmlAttribute()]
        public string Sdk
        {
            get
            {
                return sdkField;
            }
            set
            {
                sdkField = value;
            }
        }
    }

    /// <remarks/>
    [System.Serializable()]
    [System.ComponentModel.DesignerCategory("code")]
    [XmlType(AnonymousType = true)]
    public partial class ProjectPropertyGroup
    {

        private string targetFrameworkField;

        private string aspNetCoreHostingModelField;

        /// <remarks/>
        public string TargetFramework
        {
            get
            {
                return targetFrameworkField;
            }
            set
            {
                targetFrameworkField = value;
            }
        }

        /// <remarks/>
        public string AspNetCoreHostingModel
        {
            get
            {
                return aspNetCoreHostingModelField;
            }
            set
            {
                aspNetCoreHostingModelField = value;
            }
        }

        /// <remarks/>
        public string IsPackable { get; set; }
    }

    /// <remarks/>
    [System.Serializable()]
    [System.ComponentModel.DesignerCategory("code")]
    [XmlType(AnonymousType = true)]
    public partial class ProjectItemGroup
    {

        private ProjectItemGroupFolder folderField;

        private ProjectItemGroupProjectReference[] projectReferenceField;

        private ProjectItemGroupPackageReference[] packageReferenceField;

        private ProjectItemGroupCompile compileField;

        private ProjectItemGroupContent contentField;

        private ProjectItemGroupEmbeddedResource embeddedResourceField;

        private ProjectItemGroupNone noneField;

        /// <remarks/>
        public ProjectItemGroupFolder Folder
        {
            get
            {
                return folderField;
            }
            set
            {
                folderField = value;
            }
        }

        /// <remarks/>
        [XmlElement("ProjectReference")]
        public ProjectItemGroupProjectReference[] ProjectReference
        {
            get
            {
                return projectReferenceField;
            }
            set
            {
                projectReferenceField = value;
            }
        }

        /// <remarks/>
        [XmlElement("PackageReference")]
        public ProjectItemGroupPackageReference[] PackageReference
        {
            get
            {
                return packageReferenceField;
            }
            set
            {
                packageReferenceField = value;
            }
        }

        /// <remarks/>
        public ProjectItemGroupCompile Compile
        {
            get
            {
                return compileField;
            }
            set
            {
                compileField = value;
            }
        }

        /// <remarks/>
        public ProjectItemGroupContent Content
        {
            get
            {
                return contentField;
            }
            set
            {
                contentField = value;
            }
        }

        /// <remarks/>
        public ProjectItemGroupEmbeddedResource EmbeddedResource
        {
            get
            {
                return embeddedResourceField;
            }
            set
            {
                embeddedResourceField = value;
            }
        }

        /// <remarks/>
        public ProjectItemGroupNone None
        {
            get
            {
                return noneField;
            }
            set
            {
                noneField = value;
            }
        }
    }

    /// <remarks/>
    [System.Serializable()]
    [System.ComponentModel.DesignerCategory("code")]
    [XmlType(AnonymousType = true)]
    public partial class ProjectItemGroupFolder
    {

        private string includeField;

        /// <remarks/>
        [XmlAttribute()]
        public string Include
        {
            get
            {
                return includeField;
            }
            set
            {
                includeField = value;
            }
        }
    }

    /// <remarks/>
    [System.Serializable()]
    [System.ComponentModel.DesignerCategory("code")]
    [XmlType(AnonymousType = true)]
    public partial class ProjectItemGroupProjectReference
    {

        private string includeField;

        /// <remarks/>
        [XmlAttribute()]
        public string Include
        {
            get
            {
                return includeField;
            }
            set
            {
                includeField = value;
            }
        }
    }

    /// <remarks/>
    [System.Serializable()]
    [System.ComponentModel.DesignerCategory("code")]
    [XmlType(AnonymousType = true)]
    public partial class ProjectItemGroupPackageReference
    {

        private string includeField;

        private string versionField;

        private string privateAssetsField;

        /// <remarks/>
        [XmlAttribute()]
        public string Include
        {
            get
            {
                return includeField;
            }
            set
            {
                includeField = value;
            }
        }

        /// <remarks/>
        [XmlAttribute()]
        public string Version
        {
            get
            {
                return versionField;
            }
            set
            {
                versionField = value;
            }
        }

        /// <remarks/>
        [XmlAttribute()]
        public string PrivateAssets
        {
            get
            {
                return privateAssetsField;
            }
            set
            {
                privateAssetsField = value;
            }
        }
    }

    /// <remarks/>
    [System.Serializable()]
    [System.ComponentModel.DesignerCategory("code")]
    [XmlType(AnonymousType = true)]
    public partial class ProjectItemGroupCompile
    {

        private string removeField;

        /// <remarks/>
        [XmlAttribute()]
        public string Remove
        {
            get
            {
                return removeField;
            }
            set
            {
                removeField = value;
            }
        }
    }

    /// <remarks/>
    [System.Serializable()]
    [System.ComponentModel.DesignerCategory("code")]
    [XmlType(AnonymousType = true)]
    public partial class ProjectItemGroupContent
    {

        private string removeField;

        /// <remarks/>
        [XmlAttribute()]
        public string Remove
        {
            get
            {
                return removeField;
            }
            set
            {
                removeField = value;
            }
        }
    }

    /// <remarks/>
    [System.Serializable()]
    [System.ComponentModel.DesignerCategory("code")]
    [XmlType(AnonymousType = true)]
    public partial class ProjectItemGroupEmbeddedResource
    {

        private string removeField;

        /// <remarks/>
        [XmlAttribute()]
        public string Remove
        {
            get
            {
                return removeField;
            }
            set
            {
                removeField = value;
            }
        }
    }

    /// <remarks/>
    [System.Serializable()]
    [System.ComponentModel.DesignerCategory("code")]
    [XmlType(AnonymousType = true)]
    public partial class ProjectItemGroupNone
    {

        private string removeField;

        /// <remarks/>
        [XmlAttribute()]
        public string Remove
        {
            get
            {
                return removeField;
            }
            set
            {
                removeField = value;
            }
        }
    }
}
