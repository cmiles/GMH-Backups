namespace Pw02.GreenButtonXml;

/// <remarks/>
[Serializable()]
[System.ComponentModel.DesignerCategoryAttribute("code")]
[System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true, Namespace = "http://naesb.org/espi")]
public partial class UsagePointServiceCategory
{

    private byte kindField;

    /// <remarks/>
    public byte kind
    {
        get
        {
            return this.kindField;
        }
        set
        {
            this.kindField = value;
        }
    }
}