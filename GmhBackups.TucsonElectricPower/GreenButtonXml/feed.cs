
// NOTE: Generated code may require at least .NET Framework 4.5 or .NET Core/Standard 2.0.
namespace Pw02.GreenButtonXml;

/// <remarks/>
[Serializable()]
[System.ComponentModel.DesignerCategoryAttribute("code")]
[System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true, Namespace = "http://www.w3.org/2005/Atom")]
[System.Xml.Serialization.XmlRootAttribute(Namespace = "http://www.w3.org/2005/Atom", IsNullable = false)]
public partial class feed
{

    private string idField;

    private feedLink linkField;

    private string titleField;

    private System.DateTimeOffset updatedField;

    private feedEntry[] entryField;

    /// <remarks/>
    public string id
    {
        get
        {
            return this.idField;
        }
        set
        {
            this.idField = value;
        }
    }

    /// <remarks/>
    public feedLink link
    {
        get
        {
            return this.linkField;
        }
        set
        {
            this.linkField = value;
        }
    }

    /// <remarks/>
    public string title
    {
        get
        {
            return this.titleField;
        }
        set
        {
            this.titleField = value;
        }
    }

    /// <remarks/>
    public System.DateTimeOffset updated
    {
        get
        {
            return this.updatedField;
        }
        set
        {
            this.updatedField = value;
        }
    }

    /// <remarks/>
    [System.Xml.Serialization.XmlElementAttribute("entry")]
    public feedEntry[] entry
    {
        get
        {
            return this.entryField;
        }
        set
        {
            this.entryField = value;
        }
    }
}