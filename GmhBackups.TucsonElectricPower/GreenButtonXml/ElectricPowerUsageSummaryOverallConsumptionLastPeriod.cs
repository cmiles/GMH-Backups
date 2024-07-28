namespace Pw02.GreenButtonXml;

/// <remarks/>
[Serializable()]
[System.ComponentModel.DesignerCategoryAttribute("code")]
[System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true, Namespace = "http://naesb.org/espi")]
public partial class ElectricPowerUsageSummaryOverallConsumptionLastPeriod
{

    private byte powerOfTenMultiplierField;

    private uint timeStampField;

    /// <remarks/>
    public byte powerOfTenMultiplier
    {
        get
        {
            return this.powerOfTenMultiplierField;
        }
        set
        {
            this.powerOfTenMultiplierField = value;
        }
    }

    /// <remarks/>
    public uint timeStamp
    {
        get
        {
            return this.timeStampField;
        }
        set
        {
            this.timeStampField = value;
        }
    }
}