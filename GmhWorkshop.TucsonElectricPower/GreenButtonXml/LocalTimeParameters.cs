namespace Pw02.GreenButtonXml;

/// <remarks/>
[Serializable()]
[System.ComponentModel.DesignerCategoryAttribute("code")]
[System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true, Namespace = "http://naesb.org/espi")]
[System.Xml.Serialization.XmlRootAttribute(Namespace = "http://naesb.org/espi", IsNullable = false)]
public partial class LocalTimeParameters
{

    private string dstEndRuleField;

    private short dstOffsetField;

    private string dstStartRuleField;

    private short tzOffsetField;

    /// <remarks/>
    public string dstEndRule
    {
        get
        {
            return this.dstEndRuleField;
        }
        set
        {
            this.dstEndRuleField = value;
        }
    }

    /// <remarks/>
    public short dstOffset
    {
        get
        {
            return this.dstOffsetField;
        }
        set
        {
            this.dstOffsetField = value;
        }
    }

    /// <remarks/>
    public string dstStartRule
    {
        get
        {
            return this.dstStartRuleField;
        }
        set
        {
            this.dstStartRuleField = value;
        }
    }

    /// <remarks/>
    public short tzOffset
    {
        get
        {
            return this.tzOffsetField;
        }
        set
        {
            this.tzOffsetField = value;
        }
    }
}