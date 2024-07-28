namespace Pw02.GreenButtonXml;

/// <remarks/>
[Serializable()]
[System.ComponentModel.DesignerCategoryAttribute("code")]
[System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true, Namespace = "http://www.w3.org/2005/Atom")]
public partial class feedEntry
{

    private string idField;

    private feedEntryLink[] linkField;

    private string titleField;

    private feedEntryContent contentField;

    private System.DateTimeOffset publishedField;

    private System.DateTimeOffset updatedField;

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
    [System.Xml.Serialization.XmlElementAttribute("link")]
    public feedEntryLink[] link
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
    public feedEntryContent content
    {
        get
        {
            return this.contentField;
        }
        set
        {
            this.contentField = value;
        }
    }

    /// <remarks/>
    public System.DateTimeOffset published
    {
        get
        {
            return this.publishedField;
        }
        set
        {
            this.publishedField = value;
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
}