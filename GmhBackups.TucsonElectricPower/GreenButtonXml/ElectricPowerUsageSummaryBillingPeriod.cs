namespace Pw02.GreenButtonXml;

/// <remarks/>
[Serializable()]
[System.ComponentModel.DesignerCategoryAttribute("code")]
[System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true, Namespace = "http://naesb.org/espi")]
public partial class ElectricPowerUsageSummaryBillingPeriod
{

    private uint durationField;

    private uint startField;

    /// <remarks/>
    public uint duration
    {
        get
        {
            return this.durationField;
        }
        set
        {
            this.durationField = value;
        }
    }

    /// <remarks/>
    public uint start
    {
        get
        {
            return this.startField;
        }
        set
        {
            this.startField = value;
        }
    }
}