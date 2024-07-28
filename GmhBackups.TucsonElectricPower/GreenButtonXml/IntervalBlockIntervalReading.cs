namespace Pw02.GreenButtonXml;

/// <remarks/>
[Serializable()]
[System.ComponentModel.DesignerCategoryAttribute("code")]
[System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true, Namespace = "http://naesb.org/espi")]
public partial class IntervalBlockIntervalReading
{

    private IntervalBlockIntervalReadingTimePeriod timePeriodField;

    private ushort valueField;

    /// <remarks/>
    public IntervalBlockIntervalReadingTimePeriod timePeriod
    {
        get
        {
            return this.timePeriodField;
        }
        set
        {
            this.timePeriodField = value;
        }
    }

    /// <remarks/>
    public ushort value
    {
        get
        {
            return this.valueField;
        }
        set
        {
            this.valueField = value;
        }
    }
}