namespace Pw02.GreenButtonXml;

/// <remarks/>
[Serializable()]
[System.ComponentModel.DesignerCategoryAttribute("code")]
[System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true, Namespace = "http://naesb.org/espi")]
[System.Xml.Serialization.XmlRootAttribute(Namespace = "http://naesb.org/espi", IsNullable = false)]
public partial class IntervalBlock
{

    private IntervalBlockInterval intervalField;

    private IntervalBlockIntervalReading[] intervalReadingField;

    /// <remarks/>
    public IntervalBlockInterval interval
    {
        get
        {
            return this.intervalField;
        }
        set
        {
            this.intervalField = value;
        }
    }

    /// <remarks/>
    [System.Xml.Serialization.XmlElementAttribute("IntervalReading")]
    public IntervalBlockIntervalReading[] IntervalReading
    {
        get
        {
            return this.intervalReadingField;
        }
        set
        {
            this.intervalReadingField = value;
        }
    }
}