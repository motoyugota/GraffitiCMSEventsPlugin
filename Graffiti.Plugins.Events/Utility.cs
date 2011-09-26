using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DDay.iCal;
using Graffiti.Core;
using System.Web.UI.WebControls;
using System.Web.Script.Serialization;
using System.Web;

namespace Graffiti.Plugins.Events
{
	internal static class Utility
	{
		private static iCalendarCollection calendars;
		private static int categoryId;

		static Utility()
		{
			categoryId = new CategoryController().GetUnCategorizedCategory().Id;
			calendars = new iCalendarCollection();

			object calendarFeeds = GraffitiContext.Current["calendarFeeds"];
			if (!String.IsNullOrEmpty(calendarFeeds as String))
			{
				string[] feeds = calendarFeeds.ToString().Split(new char[] { '\n' }, StringSplitOptions.RemoveEmptyEntries);
				foreach (string feed in feeds)
				{
					calendars.AddRange(iCalendar.LoadFromUri(new Uri(feed)));
				}
			}
		}

		// TODO: Figure out how to cache the .ics events for a while
		public static IList<Occurrence> LoadFeedEvents(DateTime date)
		{
			return calendars.GetOccurrences<IEvent>(date);
		}

		public static IList<Occurrence> LoadFeedEvents(DateTime startDate, DateTime endDate)
		{
			return calendars.GetOccurrences<IEvent>(startDate, endDate);
		}

		public static HyperLink ExternalEventLink(Post post)
		{
			if (post.ContentType == "External")
			{
				JavaScriptSerializer serializer = new JavaScriptSerializer();
				var json = new
				{
					Title = post.Title,
					EventDate = post["Event Date"],
					StartTime = post["Start Time"],
					EndTime = post["End Time"],
					ExternalUrl = post["External Url"],
					UID = post["UID"],
					Description = post["Description"],
					Location = post["Location"]
				};
				
				HyperLink eventLink = new HyperLink();
				eventLink.Text = post.Title;
				eventLink.Attributes.Add("Title", json.Description);
				eventLink.Attributes.Add("data", HttpUtility.HtmlEncode(serializer.Serialize(json)));
				eventLink.CssClass = "external-event";
				eventLink.NavigateUrl = "";

				return eventLink;
			}

			return null;
		}

		public static IEnumerable<Post> ToPosts(this IList<Occurrence> occurrences)
		{
			foreach (Occurrence occurrence in occurrences)
			{
				IRecurringComponent rc = occurrence.Source as IRecurringComponent;
				if (rc != null)
				{
					IEvent ev = occurrence.Source as IEvent;

					Post post = new Post();
					post.Title = rc.Summary;
					post.Name = "FAKEPOST";
					post.CategoryId = categoryId;
					post["Event Date"] = occurrence.Period.StartTime.Local.ToShortDateString();
					post["Start Time"] = occurrence.Period.StartTime.Local.ToShortTimeString();
					post["End Time"] = occurrence.Period.EndTime.Local.ToShortTimeString();
					post["External Url"] = rc.Url == null ? null : rc.Url.ToString();
					post["UID"] = rc.UID;
					post["Description"] = rc.Description;

					if (ev != null)
					{
						post["Location"] = ev.Location;
					}

					post.ContentType = "External";

					yield return post;
				}
			}
		}
	}
}
