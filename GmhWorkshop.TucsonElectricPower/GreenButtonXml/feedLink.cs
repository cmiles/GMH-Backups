namespace Pw02.GreenButtonXml;

/// <remarks/>
[Serializable()]
[System.ComponentModel.DesignerCategoryAttribute("code")]
[System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true, Namespace = "http://www.w3.org/2005/Atom")]
public partial class feedLink
{

    private string hrefField;

    private string relField;

    /// <remarks/>
    [System.Xml.Serialization.XmlAttributeAttribute()]
    public string href
    {
        get
        {
            return this.hrefField;
        }
        set
        {
            this.hrefField = value;
        }
    }

    /// <remarks/>
    [System.Xml.Serialization.XmlAttributeAttribute()]
    public string rel
    {
        get
        {
            return this.relField;
        }
        set
        {
            this.relField = value;
        }
    }
}