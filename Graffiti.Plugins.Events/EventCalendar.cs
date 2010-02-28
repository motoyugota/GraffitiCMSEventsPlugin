using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Graffiti.Core;
using System.Collections.Specialized;
using System.Web;

namespace Graffiti.Plugins.Events
{
	[Serializable()]
	[WidgetInfo("1EA1C8BE-6A49-4B6D-8240-5F18D70C1E54", "Event Calendar", "Displays a monthly calendar, showing any posts in the Event category that have an Event Date in the displayed month")]
	public class EventCalendar : Widget
	{
		private const string defaultTitle = "Events";

		public bool ShowIndividualEvents { get; set; }
		public Post CurrentPost { get; set; }

		public override string Name
		{
			get { return this.Title + " (Event Calendar)"; }
		}

		public override string Title
		{
			get { return base.Title; }
			set
			{
				if (string.IsNullOrEmpty(value))
					base.Title = defaultTitle;
				else
					base.Title = value;
			}
		}

		protected override System.Collections.Specialized.NameValueCollection DataAsNameValueCollection()
		{
			NameValueCollection nvc = base.DataAsNameValueCollection();
			nvc["showIndividualEvents"] = this.ShowIndividualEvents.ToString();
			return nvc;
		}

		public override StatusType SetValues(HttpContext context, NameValueCollection nvc)
		{
			StatusType result = base.SetValues(context, nvc);
			if (result == StatusType.Success)
			{
				this.ShowIndividualEvents = CalendarFunctions.ConvertStringToBool(nvc["showIndividualEvents"]);
			}
			return result;
		}

		protected override FormElementCollection AddFormElements()
		{
			FormElementCollection fec = base.AddFormElements();

			fec.Add(new CheckFormElement("showIndividualEvents", "Show Individual Events", "If selected, show individual events in the calendar, otherwise just identify days that have events", false));

			return fec;
		}

		public override string RenderData()
		{
			int year = 0;
			int month = 0;

			CalendarFunctions.GetMonth(ref year, ref month);

			DateTime currentMonth = new DateTime(year, month, 1);
			DateTime previousMonth = currentMonth.AddMonths(-1);
			DateTime nextMonth = currentMonth.AddMonths(1);

			string ret = String.Format("<a class=\"previousMonth\" href=\"?d={0}{1}\">{2}</a>", previousMonth.Year.ToString().PadLeft(4, '0'), previousMonth.Month.ToString().PadLeft(2, '0'), previousMonth.ToString("MMMM yyyy"));
			ret += String.Format("<a class=\"nextMonth\" href=\"?d={0}{1}\">{2}</a>", nextMonth.Year.ToString().PadLeft(4, '0'), nextMonth.Month.ToString().PadLeft(2, '0'), nextMonth.ToString("MMMM yyyy"));
			ret += CalendarFunctions.BuildCalendar(true, year, month, null);

			return ret;
		}
	}
}
